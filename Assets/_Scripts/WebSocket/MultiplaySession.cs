using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiplaySession : MonoBehaviour
{
    public static MultiplaySession Instance { get; private set; }

    // Current room info
    public RoomData CurrentRoom { get; private set; }
    public bool IsHost { get; private set; }
    public string SharedImageUrl { get; set; }

    public string MyUserId => UserSession.Instance?.UserID;

    // Guest info (for non-logged-in multiplayer access)
    private string guestNickname = null;
    public string GuestNickname => guestNickname;

    // Events
    public event Action<RoomData> OnRoomDataUpdated;
    public event Action<PlayerData> OnPlayerJoined;
    public event Action<PlayerData> OnPlayerLeft;
    public event Action OnGameStarted;
    public event Action OnHostLeft;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region Guest Management
    /// <summary>
    /// Set guest nickname (for non-logged-in multiplayer entry)
    /// </summary>
    public void SetGuestNickname(string nickname)
    {
        guestNickname = nickname;
        Debug.Log($"[MultiplaySession] Guest nickname set: {nickname}");
    }

    /// <summary>
    /// Clear guest nickname
    /// </summary>
    public void ClearGuestNickname()
    {
        guestNickname = null;
        Debug.Log("[MultiplaySession] Guest nickname cleared");
    }

    /// <summary>
    /// Get current user nickname (logged-in or guest)
    /// </summary>
    public string GetCurrentNickname()
    {
        // Get from UserSession if logged in
        if (UserSession.Instance != null && UserSession.Instance.IsLoggedIn)
        {
            return UserSession.Instance.Nickname;
        }

        // Use guest nickname if not logged in
        return guestNickname ?? "Unknown";
    }

    /// <summary>
    /// Check if current user is in guest mode
    /// </summary>
    public bool IsGuestMode()
    {
        return !string.IsNullOrEmpty(guestNickname) &&
               (UserSession.Instance == null || !UserSession.Instance.IsLoggedIn);
    }
    #endregion

    /// <summary>
    /// Set initial room info on room creation/join
    /// </summary>
    public void SetRoomInfo(string roomId, string sessionCode, bool isHost)
    {
        if (CurrentRoom == null)
            CurrentRoom = new RoomData();

        CurrentRoom.roomId = roomId;
        CurrentRoom.sessionCode = sessionCode;
        this.IsHost = isHost;

        if (isHost)
            CurrentRoom.hostId = MyUserId;

        Debug.Log($"[MultiplaySession] Room info set - RoomId: {roomId}, Code: {sessionCode}, IsHost: {isHost}");
    }

    /// <summary>
    /// Update entire room data from websocket
    /// </summary>
    public void UpdateRoomData(RoomData roomData)
    {
        CurrentRoom = roomData;
        OnRoomDataUpdated?.Invoke(roomData);
        Debug.Log($"[MultiplaySession] Room data updated - Players: {roomData.players?.Count ?? 0}");
    }

    /// <summary>
    /// Add player
    /// </summary>
    public void AddPlayer(PlayerData player)
    {
        if (CurrentRoom.players == null)
            CurrentRoom.players = new List<PlayerData>();

        CurrentRoom.players.Add(player);
        OnPlayerJoined?.Invoke(player);
        OnRoomDataUpdated?.Invoke(CurrentRoom);

        Debug.Log($"[MultiplaySession] Player joined: {player.nickname}");
    }

    /// <summary>
    /// Remove player
    /// </summary>
    public void RemovePlayer(string userId)
    {
        if (CurrentRoom.players == null) return;

        var player = CurrentRoom.players.Find(p => p.userId == userId);
        if (player != null)
        {
            CurrentRoom.players.Remove(player);
            OnPlayerLeft?.Invoke(player);
            OnRoomDataUpdated?.Invoke(CurrentRoom);

            Debug.Log($"[MultiplaySession] Player left: {player.nickname}");

            // Check if host left
            if (userId == CurrentRoom.hostId)
            {
                Debug.Log("[MultiplaySession] Host has left the room!");
                OnHostLeft?.Invoke();
            }
        }
    }

    public void TriggerHostLeft()
    {
        OnHostLeft?.Invoke();
    }

    /// <summary>
    /// Update specific player's ready state
    /// </summary>
    public void UpdatePlayerReady(string userId, bool isReady)
    {
        if (CurrentRoom.players == null) return;

        var player = CurrentRoom.players.Find(p => p.userId == userId);
        if (player != null)
        {
            player.isReady = isReady;
            OnRoomDataUpdated?.Invoke(CurrentRoom);
            Debug.Log($"[MultiplaySession] {player.nickname} ready state: {isReady}");
        }
    }

    /// <summary>
    /// Set game started flag
    /// </summary>
    public void StartGame()
    {
        if (CurrentRoom != null)
        {
            CurrentRoom.isGameStarted = true;
            OnGameStarted?.Invoke();
            Debug.Log("[MultiplaySession] Game started!");
        }
    }

    /// <summary>
    /// Update player clear time
    /// </summary>
    public void UpdatePlayerClearTime(string userId, float clearTime)
    {
        if (CurrentRoom.players == null) return;

        var player = CurrentRoom.players.Find(p => p.userId == userId);
        if (player != null)
        {
            player.clearTime = clearTime;
            Debug.Log($"[MultiplaySession] {player.nickname} clear time: {clearTime}s");

            // Recalculate ranks
            CalculateRanks();
            OnRoomDataUpdated?.Invoke(CurrentRoom);
        }
    }

    /// <summary>
    /// Calculate ranks based on clear time
    /// </summary>
    private void CalculateRanks()
    {
        if (CurrentRoom.players == null) return;

        // Filter cleared players only
        var clearedPlayers = CurrentRoom.players.FindAll(p => p.clearTime > 0);

        // Sort by time
        clearedPlayers.Sort((a, b) => a.clearTime.CompareTo(b.clearTime));

        // Assign ranks
        for (int i = 0; i < clearedPlayers.Count; i++)
        {
            clearedPlayers[i].rank = i + 1;
        }
    }

    /// <summary>
    /// Clear room data (on game end)
    /// </summary>
    public void ClearRoomData()
    {
        CurrentRoom = null;
        IsHost = false;
        SharedImageUrl = null;
        ClearGuestNickname();
        Debug.Log("[MultiplaySession] Room data cleared");
    }

    /// <summary>
    /// Get current player count in room
    /// </summary>
    public int GetPlayerCount()
    {
        return CurrentRoom?.players?.Count ?? 0;
    }

    /// <summary>
    /// Check if room is full
    /// </summary>
    public bool IsRoomFull()
    {
        return GetPlayerCount() >= (CurrentRoom?.maxPlayers ?? 4);
    }
}