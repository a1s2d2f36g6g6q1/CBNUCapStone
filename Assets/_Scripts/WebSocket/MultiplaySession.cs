using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiplaySession : MonoBehaviour
{
    public static MultiplaySession Instance { get; private set; }

    // Current room info
    public RoomData CurrentRoom { get; private set; }
    public bool IsHost { get; private set; }
    public string MyUserId => UserSession.Instance?.UserID;

    // Events
    public event Action<RoomData> OnRoomDataUpdated;
    public event Action<PlayerData> OnPlayerJoined;
    public event Action<PlayerData> OnPlayerLeft;
    public event Action OnGameStarted;
    public event Action OnHostLeft; // Host left

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

    /// <summary>
    /// Set initial room info when creating/joining room
    /// Changed parameter name from sessionCode to gameCode for clarity
    /// </summary>
    public void SetRoomInfo(string roomId, string gameCode, bool isHost)
    {
        if (CurrentRoom == null)
            CurrentRoom = new RoomData();

        CurrentRoom.roomId = roomId;
        CurrentRoom.gameCode = gameCode; // Changed from sessionCode
        this.IsHost = isHost;

        if (isHost)
            CurrentRoom.hostId = MyUserId;

        Debug.Log($"Room info set - RoomId: {roomId}, Code: {gameCode}, IsHost: {isHost}");
    }

    /// <summary>
    /// Update entire room data from WebSocket
    /// </summary>
    public void UpdateRoomData(RoomData roomData)
    {
        CurrentRoom = roomData;
        TriggerRoomDataUpdated();
        Debug.Log($"Room data updated - Player count: {roomData.players?.Count ?? 0}");
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
        TriggerRoomDataUpdated();

        Debug.Log($"Player joined: {player.nickname}");
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
            TriggerRoomDataUpdated();

            Debug.Log($"Player left: {player.nickname}");

            // If host left
            if (userId == CurrentRoom.hostId)
            {
                Debug.Log("Host has left!");
                OnHostLeft?.Invoke();
            }
        }
    }

    public void TriggerHostLeft()
    {
        OnHostLeft?.Invoke();
    }

    /// <summary>
    /// Update specific player's ready status
    /// </summary>
    public void UpdatePlayerReady(string userId, bool isReady)
    {
        if (CurrentRoom.players == null) return;

        var player = CurrentRoom.players.Find(p => p.userId == userId);
        if (player != null)
        {
            player.isReady = isReady;
            TriggerRoomDataUpdated();
            Debug.Log($"{player.nickname} ready status: {isReady}");
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
            Debug.Log("Game started!");
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
            Debug.Log($"{player.nickname} clear time: {clearTime}s");

            // Recalculate ranks
            CalculateRanks();
            TriggerRoomDataUpdated();
        }
    }

    /// <summary>
    /// Calculate ranks (based on clear time)
    /// </summary>
    private void CalculateRanks()
    {
        if (CurrentRoom.players == null) return;

        // Filter only cleared players
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
    /// Clear room info (when game ends)
    /// </summary>
    public void ClearRoomData()
    {
        CurrentRoom = null;
        IsHost = false;
        Debug.Log("Room info cleared");
    }

    /// <summary>
    /// Get player count in current room
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

    /// <summary>
    /// Public method to trigger room data updated event
    /// This is needed because SocketIOManager can't invoke the event directly
    /// </summary>
    public void TriggerRoomDataUpdated()
    {
        OnRoomDataUpdated?.Invoke(CurrentRoom);
    }
}