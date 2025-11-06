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
    public bool IsAuthenticated { get; private set; }

    private string serverUrl = "ws://13.209.33.42:3000";

    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnConnectionError;
    public event Action OnAuthenticated;

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
            Debug.Log("[SocketIO] Already connected to WebSocket");
            return;
        }

        Debug.Log("[SocketIO] Starting connection process...");

        try
        {
            var uri = new Uri(serverUrl);
            socket = new SocketIOClient.SocketIO(uri, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
            });

            Debug.Log("[SocketIO] Socket instance created");

            socket.OnConnected += async (sender, e) =>
            {
                IsConnected = true;
                Debug.Log("[SocketIO] WebSocket connected successfully");

                var dispatcher = UnityMainThreadDispatcher.Instance();
                if (dispatcher != null)
                {
                    dispatcher.Enqueue(() =>
                    {
                        OnConnected?.Invoke();
                    });
                }

                Debug.Log("[SocketIO] Setting up authentication listener...");
                SetupAuthenticationListener();

                Debug.Log("[SocketIO] Waiting 100ms before authentication...");
                await Task.Delay(100);

                Debug.Log("[SocketIO] Starting authentication...");
                await AuthenticateAsync();
            };

            socket.OnDisconnected += (sender, e) =>
            {
                IsConnected = false;
                IsAuthenticated = false;
                Debug.Log("[SocketIO] WebSocket disconnected");

                var dispatcher = UnityMainThreadDispatcher.Instance();
                if (dispatcher != null)
                {
                    dispatcher.Enqueue(() =>
                    {
                        OnDisconnected?.Invoke();
                    });
                }
            };

            socket.OnError += (sender, e) =>
            {
                Debug.LogError($"[SocketIO] WebSocket error: {e}");

                var dispatcher = UnityMainThreadDispatcher.Instance();
                if (dispatcher != null)
                {
                    dispatcher.Enqueue(() =>
                    {
                        OnConnectionError?.Invoke(e);
                    });
                }
            };

            Debug.Log("[SocketIO] Attempting to connect to server...");
            await socket.ConnectAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SocketIO] WebSocket connection failed: {e.Message}");
            OnConnectionError?.Invoke(e.Message);
        }
    }

    private async Task AuthenticateAsync()
    {
        Debug.Log($"[SocketIO] AuthenticateAsync called - IsConnected: {IsConnected}, IsAuthenticated: {IsAuthenticated}");

        if (!IsConnected)
        {
            Debug.LogError("[SocketIO] Cannot authenticate: Not connected");
            return;
        }

        if (IsAuthenticated)
        {
            Debug.Log("[SocketIO] Already authenticated");
            return;
        }

        Debug.Log("[SocketIO] Checking for APIManager instance...");
        if (APIManager.Instance == null)
        {
            Debug.LogError("[SocketIO] APIManager.Instance is NULL!");
            return;
        }

        Debug.Log("[SocketIO] Getting token from APIManager...");
        string token = APIManager.Instance.GetToken();

        Debug.Log($"[SocketIO] Token retrieved - IsNull: {token == null}, IsEmpty: {string.IsNullOrEmpty(token)}");

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("[SocketIO] Cannot authenticate: No token available");
            Debug.LogError("[SocketIO] User must be logged in to use multiplayer features");
            return;
        }

        Debug.Log($"[SocketIO] Sending authentication with token: {token.Substring(0, Math.Min(20, token.Length))}...");

        try
        {
            await socket.EmitAsync("authenticate", new { token });
            Debug.Log("[SocketIO] Authentication request sent successfully, waiting for response...");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SocketIO] Failed to send authentication: {e.Message}");
        }
    }

    private void SetupAuthenticationListener()
    {
        Debug.Log("[SocketIO] Setting up authentication listener");

        socket.On("authenticated", (response) =>
        {
            try
            {
                Debug.Log($"[SocketIO] Received authentication response: {response}");

                var authResponse = response.GetValue<WS_AuthResponse>();

                if (authResponse != null)
                {
                    Debug.Log($"[SocketIO] Auth response parsed - isSuccess: {authResponse.isSuccess}, code: {authResponse.code}, message: {authResponse.message}");

                    if (authResponse.isSuccess)
                    {
                        IsAuthenticated = true;
                        Debug.Log("[SocketIO] WebSocket authentication successful!");

                        var dispatcher = UnityMainThreadDispatcher.Instance();
                        if (dispatcher != null)
                        {
                            dispatcher.Enqueue(() =>
                            {
                                OnAuthenticated?.Invoke();
                            });
                        }
                    }
                    else
                    {
                        Debug.LogError($"[SocketIO] Authentication failed: {authResponse.message}");
                    }
                }
                else
                {
                    Debug.LogError("[SocketIO] Authentication response is null");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketIO] Failed to parse authentication response: {e.Message}\n{e.StackTrace}");
            }
        });

        Debug.Log("[SocketIO] Authentication listener set up successfully");
    }

    public async void Disconnect()
    {
        if (socket != null && IsConnected)
        {
            await socket.DisconnectAsync();
            IsConnected = false;
            IsAuthenticated = false;
        }
    }

    public void On(string eventName, Action<SocketIOResponse> callback)
    {
        if (socket == null) return;

        socket.On(eventName, (response) =>
        {
            var dispatcher = UnityMainThreadDispatcher.Instance();
            if (dispatcher != null)
            {
                dispatcher.Enqueue(() =>
                {
                    callback?.Invoke(response);
                });
            }
            else
            {
                callback?.Invoke(response);
            }
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
            Debug.LogWarning("[SocketIO] WebSocket is not connected");
            return;
        }

        Debug.Log($"[SocketIO] Emitting event: {eventName}");
        await socket.EmitAsync(eventName, data);
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    public void RegisterMultiplayEvents()
    {
        Debug.Log("[SocketIO] Registering multiplayer WebSocket events");

        On("user_joined", (response) =>
        {
            try
            {
                Debug.Log($"[WS] user_joined event: {response}");

                var evt = response.GetValue<WS_UserJoinedEvent>();

                if (evt != null && evt.result != null && evt.result.participants != null)
                {
                    var players = new List<PlayerData>();
                    foreach (var p in evt.result.participants)
                    {
                        players.Add(new PlayerData
                        {
                            userId = p.userId,
                            username = p.username,
                            nickname = p.username,
                            isReady = p.isReady == 1,
                            isHost = p.isHost
                        });
                    }

                    if (MultiplaySession.Instance != null && MultiplaySession.Instance.CurrentRoom != null)
                    {
                        MultiplaySession.Instance.CurrentRoom.players = players;
                        MultiplaySession.Instance.TriggerRoomDataUpdated();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WS] Failed to parse user_joined event: {e.Message}");
            }
        });

        On("user_left", (response) =>
        {
            try
            {
                Debug.Log($"[WS] user_left event: {response}");

                var evt = response.GetValue<WS_UserLeftEvent>();

                if (evt != null && evt.result != null && !string.IsNullOrEmpty(evt.result.userId))
                {
                    if (MultiplaySession.Instance != null)
                    {
                        MultiplaySession.Instance.RemovePlayer(evt.result.userId);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WS] Failed to parse user_left event: {e.Message}");
            }
        });

        On("room_updated", (response) =>
        {
            try
            {
                Debug.Log($"[WS] room_updated event: {response}");

                var evt = response.GetValue<WS_RoomUpdatedEvent>();

                if (evt != null && evt.result != null && evt.result.participants != null)
                {
                    if (MultiplaySession.Instance != null && MultiplaySession.Instance.CurrentRoom != null)
                    {
                        foreach (var p in evt.result.participants)
                        {
                            var player = MultiplaySession.Instance.CurrentRoom.players?.Find(pl => pl.userId == p.userId);
                            if (player != null)
                            {
                                player.isReady = p.isReady == 1;
                            }
                        }

                        MultiplaySession.Instance.TriggerRoomDataUpdated();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WS] Failed to parse room_updated event: {e.Message}");
            }
        });

        On("game_started", (response) =>
        {
            try
            {
                Debug.Log($"[WS] game_started event: {response}");

                if (MultiplaySession.Instance != null)
                {
                    MultiplaySession.Instance.StartGame();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WS] Failed to parse game_started event: {e.Message}");
            }
        });

        On("game_completed", (response) =>
        {
            try
            {
                Debug.Log($"[WS] game_completed event: {response}");

                var evt = response.GetValue<WS_GameCompletedEvent>();

                if (evt != null && evt.result != null && evt.result.winner != null)
                {
                    string userId = evt.result.winner.userId;
                    int clearTimeMs = evt.result.winner.clearTimeMs;
                    float clearTime = clearTimeMs / 1000f;

                    if (!string.IsNullOrEmpty(userId) && MultiplaySession.Instance != null)
                    {
                        MultiplaySession.Instance.UpdatePlayerClearTime(userId, clearTime);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WS] Failed to parse game_completed event: {e.Message}");
            }
        });

        On("user_disconnected", (response) =>
        {
            try
            {
                Debug.Log($"[WS] user_disconnected event: {response}");

                var evt = response.GetValue<WS_UserDisconnectedEvent>();

                if (evt != null && evt.result != null && !string.IsNullOrEmpty(evt.result.userId))
                {
                    if (MultiplaySession.Instance != null)
                    {
                        MultiplaySession.Instance.RemovePlayer(evt.result.userId);

                        if (evt.result.isHost)
                        {
                            MultiplaySession.Instance.TriggerHostLeft();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[WS] Failed to parse user_disconnected event: {e.Message}");
            }
        });

        Debug.Log("[SocketIO] Multiplayer WebSocket events registered successfully");
    }

    public void UnregisterMultiplayEvents()
    {
        Debug.Log("[SocketIO] Unregistering multiplayer WebSocket events");

        Off("user_joined");
        Off("user_left");
        Off("room_updated");
        Off("game_started");
        Off("game_completed");
        Off("user_disconnected");
    }

    public async void JoinRoom(string gameCode)
    {
        if (!IsConnected || !IsAuthenticated)
        {
            Debug.LogError("[SocketIO] Cannot join room: Not connected or authenticated");
            return;
        }

        Debug.Log($"[SocketIO] Joining room with gameCode: {gameCode}");
        await socket.EmitAsync("join_room", new { gameCode });
    }

    public async void LeaveRoom(string gameCode)
    {
        if (!IsConnected)
        {
            Debug.LogWarning("[SocketIO] Cannot leave room: Not connected");
            return;
        }

        Debug.Log($"[SocketIO] Leaving room with gameCode: {gameCode}");
        await socket.EmitAsync("leave_room", new { gameCode });
    }
}

[Serializable]
public class WS_AuthResponse
{
    public bool isSuccess;
    public string code;
    public string message;
}

[Serializable]
public class WS_UserLeftResult
{
    public string userId;
    public string username;
    public string gameCode;
}

[Serializable]
public class WS_UserLeftEvent
{
    public bool isSuccess;
    public string code;
    public string message;
    public WS_UserLeftResult result;
}

[Serializable]
public class WS_WinnerData
{
    public string userId;
    public string username;
    public int clearTimeMs;
}

[Serializable]
public class WS_GameCompletedResult
{
    public string gameId;
    public string gameCode;
    public WS_WinnerData winner;
    public string gameStatus;
}

[Serializable]
public class WS_GameCompletedEvent
{
    public bool isSuccess;
    public string code;
    public string message;
    public WS_GameCompletedResult result;
}

[Serializable]
public class WS_UserDisconnectedResult
{
    public string userId;
    public string username;
    public bool isHost;
}

[Serializable]
public class WS_UserDisconnectedEvent
{
    public bool isSuccess;
    public string code;
    public string message;
    public WS_UserDisconnectedResult result;
}