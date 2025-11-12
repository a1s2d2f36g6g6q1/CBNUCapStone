using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiplaySession : MonoBehaviour
{
    // ===== Singleton =====
    public static MultiplaySession Instance { get; private set; }

    // ===== Properties =====
    public RoomData CurrentRoom { get; private set; }
    public bool IsHost { get; private set; }
    public string MyUserId => UserSession.Instance?.UserID;

    // ===== Events =====
    public event Action<RoomData> OnRoomDataUpdated;
    public event Action<PlayerData> OnPlayerJoined;
    public event Action<PlayerData> OnPlayerLeft;
    public event Action OnGameStarted;
    public event Action OnHostLeft;

    // ===== Unity Lifecycle =====
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

    // ===== Public Methods - Room Management =====
    public void SetRoomInfo(string roomId, string gameCode, bool isHost)
    {
        if (CurrentRoom == null)
            CurrentRoom = new RoomData();

        CurrentRoom.roomId = roomId;
        CurrentRoom.gameCode = gameCode;
        this.IsHost = isHost;

        if (isHost)
            CurrentRoom.hostId = MyUserId;

        Debug.Log($"[MultiplaySession] Room info set - RoomId: {roomId}, Code: {gameCode}, IsHost: {isHost}");
    }

    public void UpdateRoomData(RoomData roomData)
    {
        CurrentRoom = roomData;
        TriggerRoomDataUpdated();
        Debug.Log($"[MultiplaySession] Room data updated - Player count: {roomData.players?.Count ?? 0}");
    }

    public void ClearRoomData()
    {
        CurrentRoom = null;
        IsHost = false;
        Debug.Log("[MultiplaySession] Room data cleared");
    }

    // ===== Public Methods - Player Management =====
    public void AddPlayer(PlayerData player)
    {
        if (CurrentRoom.players == null)
            CurrentRoom.players = new List<PlayerData>();

        CurrentRoom.players.Add(player);
        OnPlayerJoined?.Invoke(player);
        TriggerRoomDataUpdated();

        Debug.Log($"[MultiplaySession] Player joined: {player.nickname}");
    }

    public void RemovePlayer(string userId)
    {
        if (CurrentRoom.players == null) return;

        var player = CurrentRoom.players.Find(p => p.userId == userId);
        if (player != null)
        {
            bool wasHost = player.isHost;
            string playerName = player.nickname;

            CurrentRoom.players.Remove(player);
            OnPlayerLeft?.Invoke(player);

            Debug.Log($"[MultiplaySession] Player left: {playerName}");

            if (wasHost)
            {
                Debug.Log("[MultiplaySession] Host has left - Triggering OnHostLeft event");
                CurrentRoom.hostId = null;
                OnHostLeft?.Invoke();
            }
            else
            {
                TriggerRoomDataUpdated();
            }
        }
    }

    public void UpdatePlayerReady(string userId, bool isReady)
    {
        if (CurrentRoom.players == null) return;

        var player = CurrentRoom.players.Find(p => p.userId == userId);
        if (player != null)
        {
            player.isReady = isReady;
            TriggerRoomDataUpdated();
            Debug.Log($"[MultiplaySession] {player.nickname} ready status: {isReady}");
        }
    }

    public void UpdatePlayerClearTime(string userId, float clearTime)
    {
        if (CurrentRoom.players == null) return;

        var player = CurrentRoom.players.Find(p => p.userId == userId);
        if (player != null)
        {
            player.clearTime = clearTime;
            Debug.Log($"[MultiplaySession] {player.nickname} clear time: {clearTime}s");

            CalculateRanks();
            TriggerRoomDataUpdated();
        }
    }

    // ===== Public Methods - Game Management =====
    public void StartGame()
    {
        if (CurrentRoom != null)
        {
            CurrentRoom.isGameStarted = true;
            OnGameStarted?.Invoke();
            Debug.Log("[MultiplaySession] Game started");
        }
    }

    // ===== Public Methods - Utility ===== 
    public int GetPlayerCount()
    {
        return CurrentRoom?.players?.Count ?? 0;
    }

    public bool IsRoomFull()
    {
        return GetPlayerCount() >= (CurrentRoom?.maxPlayers ?? 4);
    }

    public void TriggerHostLeft()
    {
        OnHostLeft?.Invoke();
    }

    public void TriggerRoomDataUpdated()
    {
        OnRoomDataUpdated?.Invoke(CurrentRoom);
    }

    // ===== Private Methods =====
    private void CalculateRanks()
    {
        if (CurrentRoom.players == null) return;

        var clearedPlayers = CurrentRoom.players.FindAll(p => p.clearTime > 0);

        clearedPlayers.Sort((a, b) => a.clearTime.CompareTo(b.clearTime));

        for (int i = 0; i < clearedPlayers.Count; i++)
        {
            clearedPlayers[i].rank = i + 1;
        }
    }
}