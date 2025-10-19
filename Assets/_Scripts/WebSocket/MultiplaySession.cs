using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiplaySession : MonoBehaviour
{
    public static MultiplaySession Instance { get; private set; }

    // 현재 방 정보
    public RoomData CurrentRoom { get; private set; }
    public bool IsHost { get; private set; }
    public string MyUserId => UserSession.Instance?.UserID;

    // 이벤트
    public event Action<RoomData> OnRoomDataUpdated;
    public event Action<PlayerData> OnPlayerJoined;
    public event Action<PlayerData> OnPlayerLeft;
    public event Action OnGameStarted;
    public event Action OnHostLeft; // 호스트 퇴장

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
    /// 방 생성/입장 시 초기 정보 설정
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

        Debug.Log($"방 정보 설정 완료 - RoomId: {roomId}, Code: {sessionCode}, IsHost: {isHost}");
    }

    /// <summary>
    /// 웹소켓으로 받은 방 데이터 전체 업데이트
    /// </summary>
    public void UpdateRoomData(RoomData roomData)
    {
        CurrentRoom = roomData;
        OnRoomDataUpdated?.Invoke(roomData);
        Debug.Log($"방 데이터 업데이트 - 플레이어 수: {roomData.players?.Count ?? 0}");
    }

    /// <summary>
    /// 플레이어 추가
    /// </summary>
    public void AddPlayer(PlayerData player)
    {
        if (CurrentRoom.players == null)
            CurrentRoom.players = new List<PlayerData>();

        CurrentRoom.players.Add(player);
        OnPlayerJoined?.Invoke(player);
        OnRoomDataUpdated?.Invoke(CurrentRoom);

        Debug.Log($"플레이어 입장: {player.nickname}");
    }

    /// <summary>
    /// 플레이어 제거
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

            Debug.Log($"플레이어 퇴장: {player.nickname}");

            // 호스트가 나간 경우
            if (userId == CurrentRoom.hostId)
            {
                Debug.Log("호스트가 퇴장했습니다!");
                OnHostLeft?.Invoke();
            }
        }
    }
    public void TriggerHostLeft()
    {
        OnHostLeft?.Invoke();
    }

    /// <summary>
    /// 특정 플레이어의 준비 상태 업데이트
    /// </summary>
    public void UpdatePlayerReady(string userId, bool isReady)
    {
        if (CurrentRoom.players == null) return;

        var player = CurrentRoom.players.Find(p => p.userId == userId);
        if (player != null)
        {
            player.isReady = isReady;
            OnRoomDataUpdated?.Invoke(CurrentRoom);
            Debug.Log($"{player.nickname} 준비 상태: {isReady}");
        }
    }

    /// <summary>
    /// 게임 시작 플래그 설정
    /// </summary>
    public void StartGame()
    {
        if (CurrentRoom != null)
        {
            CurrentRoom.isGameStarted = true;
            OnGameStarted?.Invoke();
            Debug.Log("게임 시작!");
        }
    }

    /// <summary>
    /// 플레이어 클리어 시간 업데이트
    /// </summary>
    public void UpdatePlayerClearTime(string userId, float clearTime)
    {
        if (CurrentRoom.players == null) return;

        var player = CurrentRoom.players.Find(p => p.userId == userId);
        if (player != null)
        {
            player.clearTime = clearTime;
            Debug.Log($"{player.nickname} 클리어 시간: {clearTime}초");

            // 순위 재계산
            CalculateRanks();
            OnRoomDataUpdated?.Invoke(CurrentRoom);
        }
    }

    /// <summary>
    /// 순위 계산 (클리어 시간 기준)
    /// </summary>
    private void CalculateRanks()
    {
        if (CurrentRoom.players == null) return;

        // 클리어한 플레이어만 필터링
        var clearedPlayers = CurrentRoom.players.FindAll(p => p.clearTime > 0);

        // 시간 순으로 정렬
        clearedPlayers.Sort((a, b) => a.clearTime.CompareTo(b.clearTime));

        // 순위 부여
        for (int i = 0; i < clearedPlayers.Count; i++)
        {
            clearedPlayers[i].rank = i + 1;
        }
    }

    /// <summary>
    /// 방 정보 초기화 (게임 종료 시)
    /// </summary>
    public void ClearRoomData()
    {
        CurrentRoom = null;
        IsHost = false;
        Debug.Log("방 정보 초기화");
    }

    /// <summary>
    /// 현재 방에 있는 플레이어 수
    /// </summary>
    public int GetPlayerCount()
    {
        return CurrentRoom?.players?.Count ?? 0;
    }

    /// <summary>
    /// 방이 가득 찼는지 확인
    /// </summary>
    public bool IsRoomFull()
    {
        return GetPlayerCount() >= (CurrentRoom?.maxPlayers ?? 4);
    }
}