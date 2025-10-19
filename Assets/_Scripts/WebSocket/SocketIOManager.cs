using UnityEngine;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SocketIOManager : MonoBehaviour
{
    public static SocketIOManager Instance { get; private set; }

    private SocketIOClient.SocketIO socket;
    public bool IsConnected { get; private set; }

    private string serverUrl = "ws://13.209.33.42:3000/socket.io/?EIO=4&transport=websocket";

    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnConnectionError;

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

    public async void Connect()
    {
        if (IsConnected)
        {
            Debug.Log("이미 웹소켓에 연결되어 있습니다.");
            return;
        }

        try
        {
            var uri = new Uri(serverUrl);
            socket = new SocketIOClient.SocketIO(uri, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                ExtraHeaders = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {APIManager.Instance.GetToken()}" }
                }
            });

            socket.OnConnected += (sender, e) =>
            {
                IsConnected = true;
                Debug.Log("웹소켓 연결 성공");

                // Unity 메인 스레드에서 실행
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnConnected?.Invoke();
                });
            };

            socket.OnDisconnected += (sender, e) =>
            {
                IsConnected = false;
                Debug.Log("웹소켓 연결 해제");

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnDisconnected?.Invoke();
                });
            };

            socket.OnError += (sender, e) =>
            {
                Debug.LogError($"웹소켓 에러: {e}");

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnConnectionError?.Invoke(e);
                });
            };

            await socket.ConnectAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"웹소켓 연결 실패: {e.Message}");
            OnConnectionError?.Invoke(e.Message);
        }
    }

    public async void Disconnect()
    {
        if (socket != null && IsConnected)
        {
            await socket.DisconnectAsync();
            IsConnected = false;
        }
    }

    public void On(string eventName, Action<SocketIOResponse> callback)
    {
        if (socket == null) return;

        socket.On(eventName, (response) =>
        {
            // Unity 메인 스레드에서 콜백 실행
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                callback?.Invoke(response);
            });
        });
    }

    public void Off(string eventName)
    {
        if (socket == null) return;
        socket.Off(eventName);
    }

    public async void Emit(string eventName, object data)
    {
        if (socket == null || !IsConnected)
        {
            Debug.LogWarning("웹소켓이 연결되지 않았습니다.");
            return;
        }

        await socket.EmitAsync(eventName, data);
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    public void RegisterMultiplayEvents()
    {
        // 플레이어 입장
        On("player-joined", (response) =>
        {
            try
            {
                var playerData = response.GetValue<PlayerData>();
                if (MultiplaySession.Instance != null)
                {
                    MultiplaySession.Instance.AddPlayer(playerData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"player-joined 파싱 오류: {e.Message}");
            }
        });

        // 플레이어 퇴장
        On("player-left", (response) =>
        {
            try
            {
                var data = response.GetValue<Dictionary<string, string>>();
                if (data.ContainsKey("userId") && MultiplaySession.Instance != null)
                {
                    MultiplaySession.Instance.RemovePlayer(data["userId"]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"player-left 파싱 오류: {e.Message}");
            }
        });

        // 준비 상태 변경
        On("player-ready", (response) =>
        {
            try
            {
                var data = response.GetValue<Dictionary<string, object>>();
                if (data.ContainsKey("userId") && data.ContainsKey("isReady") && MultiplaySession.Instance != null)
                {
                    string userId = data["userId"].ToString();
                    bool isReady = Convert.ToBoolean(data["isReady"]);
                    MultiplaySession.Instance.UpdatePlayerReady(userId, isReady);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"player-ready 파싱 오류: {e.Message}");
            }
        });

        // 게임 시작
        On("game-started", (response) =>
        {
            if (MultiplaySession.Instance != null)
            {
                MultiplaySession.Instance.StartGame();
            }
        });

        // 플레이어 완료
        On("player-completed", (response) =>
        {
            try
            {
                var data = response.GetValue<Dictionary<string, object>>();
                if (data.ContainsKey("userId") && data.ContainsKey("clearTime") && MultiplaySession.Instance != null)
                {
                    string userId = data["userId"].ToString();
                    float clearTime = Convert.ToSingle(data["clearTime"]);
                    MultiplaySession.Instance.UpdatePlayerClearTime(userId, clearTime);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"player-completed 파싱 오류: {e.Message}");
            }
        });

        // 방 폭파 (호스트 퇴장)
        On("room-closed", (response) =>
        {
            if (MultiplaySession.Instance != null)
            {
                MultiplaySession.Instance.TriggerHostLeft();
            }
        });
    }

    public void UnregisterMultiplayEvents()
    {
        Off("player-joined");
        Off("player-left");
        Off("player-ready");
        Off("game-started");
        Off("player-completed");
        Off("room-closed");
    }
}