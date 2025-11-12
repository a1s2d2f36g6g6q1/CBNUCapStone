using UnityEngine;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SocketIOManager : MonoBehaviour
{
    // ===== Singleton =====
    public static SocketIOManager Instance { get; private set; }

    // ===== Properties =====
    public bool IsConnected { get; private set; }
    public bool IsAuthenticated { get; private set; }

    // ===== Private Fields =====
    private SocketIOClient.SocketIO socket;
    private string serverUrl = "ws://13.209.33.42:3000";
    private TaskCompletionSource<bool> connectionTcs;
    private TaskCompletionSource<bool> authenticationTcs;

    // ===== Events =====
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnConnectionError;
    public event Action OnAuthenticated;

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

        EnsureMainThreadDispatcher();
    }

    private void OnDestroy()
    {
        Debug.Log("[SocketIO] OnDestroy - Cleaning up socket connection");

        if (socket != null && IsConnected)
        {
            socket.DisconnectAsync().Wait();
        }
    }

    // ===== Public Methods - Connection =====
    public async Task<bool> ConnectAndAuthenticateAsync()
    {
        if (IsConnected && IsAuthenticated)
        {
            Debug.Log("[SocketIO] Already connected and authenticated");
            return true;
        }

        if (IsConnected && !IsAuthenticated)
        {
            Debug.Log("[SocketIO] Already connected - Attempting authentication");
            return await AuthenticateAsync();
        }

        Debug.Log("[SocketIO] Starting connection process...");

        try
        {
            connectionTcs = new TaskCompletionSource<bool>();
            authenticationTcs = new TaskCompletionSource<bool>();

            var uri = new Uri(serverUrl);
            socket = new SocketIOClient.SocketIO(uri, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                Reconnection = false,
                ConnectionTimeout = TimeSpan.FromSeconds(15)
            });

            Debug.Log("[SocketIO] Socket instance created");

            SetupAuthenticationListener();

            socket.OnConnected += OnSocketConnected;
            socket.OnDisconnected += OnSocketDisconnected;
            socket.OnError += OnSocketError;

            Debug.Log("[SocketIO] Connecting to server...");
            await socket.ConnectAsync();

            var connectionTask = await Task.WhenAny(connectionTcs.Task, Task.Delay(15000));

            if (connectionTask != connectionTcs.Task)
            {
                Debug.LogError("[SocketIO] Connection timeout (15s)");
                return false;
            }

            if (!await connectionTcs.Task)
            {
                Debug.LogError("[SocketIO] Connection failed");
                return false;
            }

            Debug.Log("[SocketIO] Connection successful - Waiting for authentication");

            var authTask = await Task.WhenAny(authenticationTcs.Task, Task.Delay(15000));

            if (authTask != authenticationTcs.Task)
            {
                Debug.LogError("[SocketIO] Authentication timeout (15s)");
                return false;
            }

            bool authResult = await authenticationTcs.Task;
            Debug.Log($"[SocketIO] Authentication result: {authResult}");

            return authResult;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SocketIO] Connection error: {e.Message}");
            Debug.LogError($"[SocketIO] Stack trace: {e.StackTrace}");
            return false;
        }
    }

    public async void Disconnect()
    {
        if (socket != null && IsConnected)
        {
            Debug.Log("[SocketIO] Disconnecting...");
            await socket.DisconnectAsync();
        }
    }

    // ===== Public Methods - Event Management =====
    public void On(string eventName, Action<SocketIOResponse> callback)
    {
        if (socket == null)
        {
            Debug.LogError("[SocketIO] Cannot register event: Socket is null");
            return;
        }

        socket.On(eventName, (response) =>
        {
            var dispatcher = UnityMainThreadDispatcher.Instance();
            if (dispatcher != null)
            {
                dispatcher.Enqueue(() => callback?.Invoke(response));
            }
        });

        Debug.Log($"[SocketIO] Registered event listener: {eventName}");
    }

    public void Off(string eventName)
    {
        socket?.Off(eventName);
    }

    public async void Emit(string eventName, object data)
    {
        if (socket == null || !IsConnected)
        {
            Debug.LogError("[SocketIO] Cannot emit: Not connected");
            return;
        }

        await socket.EmitAsync(eventName, data);
        Debug.Log($"[SocketIO] Emitted event: {eventName}");
    }

    // ===== Public Methods - Multiplayer =====
    public void RegisterMultiplayEvents()
    {
        Debug.Log("[SocketIO] Registering multiplayer WebSocket events");

        On("user_joined", (response) =>
        {
            try
            {
                Debug.Log($"[WS] user_joined event received: {response}");

                string responseStr = response.ToString();
                WS_UserJoinedEvent evt = null;

                if (responseStr.StartsWith("["))
                {
                    int firstBracket = responseStr.IndexOf('{');
                    int lastBracket = responseStr.LastIndexOf('}') + 1;
                    if (firstBracket >= 0 && lastBracket > firstBracket)
                    {
                        string jsonObject = responseStr.Substring(firstBracket, lastBracket - firstBracket);
                        evt = JsonUtility.FromJson<WS_UserJoinedEvent>(jsonObject);
                    }
                }
                else
                {
                    evt = response.GetValue<WS_UserJoinedEvent>();
                }

                if (evt != null && evt.result != null && evt.result.participants != null)
                {
                    Debug.Log($"[WS] New user joined: {evt.result.username}");

                    if (MultiplaySession.Instance != null && MultiplaySession.Instance.CurrentRoom != null)
                    {
                        foreach (var p in evt.result.participants)
                        {
                            var existingPlayer = MultiplaySession.Instance.CurrentRoom.players?.Find(pl => pl.userId == p.userId);
                            if (existingPlayer == null)
                            {
                                var newPlayer = new PlayerData
                                {
                                    userId = p.userId,
                                    username = p.username,
                                    nickname = p.username,
                                    isReady = p.isReady == 1,
                                    isHost = p.isHost
                                };
                                MultiplaySession.Instance.AddPlayer(newPlayer);
                            }
                        }
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
                Debug.Log($"[WS] user_left event received: {response}");

                string responseStr = response.ToString();
                WS_UserLeftEvent evt = null;

                if (responseStr.StartsWith("["))
                {
                    int firstBracket = responseStr.IndexOf('{');
                    int lastBracket = responseStr.LastIndexOf('}') + 1;
                    if (firstBracket >= 0 && lastBracket > firstBracket)
                    {
                        string jsonObject = responseStr.Substring(firstBracket, lastBracket - firstBracket);
                        evt = JsonUtility.FromJson<WS_UserLeftEvent>(jsonObject);
                    }
                }
                else
                {
                    evt = response.GetValue<WS_UserLeftEvent>();
                }

                if (evt != null && evt.result != null && !string.IsNullOrEmpty(evt.result.userId))
                {
                    Debug.Log($"[WS] User left: {evt.result.username} (ID: {evt.result.userId})");

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
                Debug.Log($"[WS] room_updated event received: {response}");

                string responseStr = response.ToString();
                WS_RoomUpdatedEvent evt = null;

                if (responseStr.StartsWith("["))
                {
                    int firstBracket = responseStr.IndexOf('{');
                    int lastBracket = responseStr.LastIndexOf('}') + 1;
                    if (firstBracket >= 0 && lastBracket > firstBracket)
                    {
                        string jsonObject = responseStr.Substring(firstBracket, lastBracket - firstBracket);
                        evt = JsonUtility.FromJson<WS_RoomUpdatedEvent>(jsonObject);
                    }
                }
                else
                {
                    evt = response.GetValue<WS_RoomUpdatedEvent>();
                }

                if (evt != null && evt.result != null && evt.result.participants != null)
                {
                    Debug.Log($"[WS] Room updated with {evt.result.participants.Length} participants");

                    if (MultiplaySession.Instance != null && MultiplaySession.Instance.CurrentRoom != null)
                    {
                        foreach (var p in evt.result.participants)
                        {
                            var player = MultiplaySession.Instance.CurrentRoom.players?.Find(pl => pl.userId == p.userId);
                            if (player != null)
                            {
                                player.isReady = p.isReady == 1;
                                Debug.Log($"[WS] Updated {player.username} ready state: {player.isReady}");
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
                Debug.Log($"[WS] game_started event received: {response}");

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
                Debug.Log($"[WS] game_completed event received: {response}");

                string responseStr = response.ToString();
                WS_GameCompletedEvent evt = null;

                if (responseStr.StartsWith("["))
                {
                    int firstBracket = responseStr.IndexOf('{');
                    int lastBracket = responseStr.LastIndexOf('}') + 1;
                    if (firstBracket >= 0 && lastBracket > firstBracket)
                    {
                        string jsonObject = responseStr.Substring(firstBracket, lastBracket - firstBracket);
                        evt = JsonUtility.FromJson<WS_GameCompletedEvent>(jsonObject);
                    }
                }
                else
                {
                    evt = response.GetValue<WS_GameCompletedEvent>();
                }

                if (evt != null && evt.result != null && evt.result.winner != null)
                {
                    string userId = evt.result.winner.userId;
                    int clearTimeMs = evt.result.winner.clearTimeMs;
                    float clearTime = clearTimeMs / 1000f;

                    Debug.Log($"[WS] Player completed: {evt.result.winner.username}, Time: {clearTime}s");

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
                Debug.Log($"[WS] user_disconnected event received: {response}");

                string responseStr = response.ToString();
                WS_UserDisconnectedEvent evt = null;

                if (responseStr.StartsWith("["))
                {
                    int firstBracket = responseStr.IndexOf('{');
                    int lastBracket = responseStr.LastIndexOf('}') + 1;
                    if (firstBracket >= 0 && lastBracket > firstBracket)
                    {
                        string jsonObject = responseStr.Substring(firstBracket, lastBracket - firstBracket);
                        evt = JsonUtility.FromJson<WS_UserDisconnectedEvent>(jsonObject);
                    }
                }
                else
                {
                    evt = response.GetValue<WS_UserDisconnectedEvent>();
                }

                if (evt != null && evt.result != null && !string.IsNullOrEmpty(evt.result.userId))
                {
                    Debug.Log($"[WS] User disconnected: {evt.result.username}");

                    if (MultiplaySession.Instance != null)
                    {
                        MultiplaySession.Instance.RemovePlayer(evt.result.userId);

                        if (evt.result.isHost)
                        {
                            Debug.Log("[WS] Disconnected user was host - Triggering host left event");
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

    // ===== Private Methods - Connection Handlers =====
    private async void OnSocketConnected(object sender, EventArgs e)
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

        connectionTcs?.TrySetResult(true);

        await Task.Delay(200);

        Debug.Log("[SocketIO] Starting authentication...");
        bool authResult = await AuthenticateAsync();

        if (!authResult)
        {
            Debug.LogError("[SocketIO] Authentication failed after connection");
            authenticationTcs?.TrySetResult(false);
        }
    }

    private void OnSocketDisconnected(object sender, string e)
    {
        IsConnected = false;
        IsAuthenticated = false;
        Debug.Log($"[SocketIO] WebSocket disconnected: {e}");

        var dispatcher = UnityMainThreadDispatcher.Instance();
        if (dispatcher != null)
        {
            dispatcher.Enqueue(() =>
            {
                OnDisconnected?.Invoke();
            });
        }

        connectionTcs?.TrySetResult(false);
        authenticationTcs?.TrySetResult(false);
    }

    private void OnSocketError(object sender, string e)
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

        connectionTcs?.TrySetResult(false);
        authenticationTcs?.TrySetResult(false);
    }

    // ===== Private Methods - Authentication =====
    private async Task<bool> AuthenticateAsync()
    {
        Debug.Log($"[SocketIO] AuthenticateAsync called - IsConnected: {IsConnected}");

        if (!IsConnected)
        {
            Debug.LogError("[SocketIO] Cannot authenticate: Not connected");
            return false;
        }

        if (IsAuthenticated)
        {
            Debug.Log("[SocketIO] Already authenticated");
            return true;
        }

        if (APIManager.Instance == null)
        {
            Debug.LogError("[SocketIO] APIManager.Instance is NULL");
            return false;
        }

        string token = APIManager.Instance.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("[SocketIO] No JWT token available");
            return false;
        }

        Debug.Log($"[SocketIO] Sending authentication with token: {token.Substring(0, Math.Min(20, token.Length))}...");

        try
        {
            await socket.EmitAsync("authenticate", new { token });
            Debug.Log("[SocketIO] Authentication request sent successfully");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SocketIO] Failed to send authentication: {e.Message}");
            authenticationTcs?.TrySetResult(false);
            return false;
        }
    }

    private void SetupAuthenticationListener()
    {
        Debug.Log("[SocketIO] Setting up authentication listener");

        On("authenticated", (response) =>
        {
            try
            {
                Debug.Log($"[SocketIO] Authentication response received: {response}");

                string responseStr = response.ToString();
                WS_AuthResponse authResponse = null;

                if (responseStr.StartsWith("["))
                {
                    int firstBracket = responseStr.IndexOf('{');
                    int lastBracket = responseStr.LastIndexOf('}') + 1;
                    if (firstBracket >= 0 && lastBracket > firstBracket)
                    {
                        string jsonObject = responseStr.Substring(firstBracket, lastBracket - firstBracket);
                        authResponse = JsonUtility.FromJson<WS_AuthResponse>(jsonObject);
                    }
                }
                else
                {
                    authResponse = response.GetValue<WS_AuthResponse>();
                }

                if (authResponse != null && authResponse.isSuccess)
                {
                    IsAuthenticated = true;
                    Debug.Log("[SocketIO] Authentication successful");

                    var dispatcher = UnityMainThreadDispatcher.Instance();
                    if (dispatcher != null)
                    {
                        dispatcher.Enqueue(() =>
                        {
                            OnAuthenticated?.Invoke();
                        });
                    }

                    authenticationTcs?.TrySetResult(true);
                }
                else
                {
                    Debug.LogError($"[SocketIO] Authentication failed: {authResponse?.message ?? "Unknown error"}");
                    authenticationTcs?.TrySetResult(false);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketIO] Failed to parse authentication response: {e.Message}");
                authenticationTcs?.TrySetResult(false);
            }
        });
    }

    // ===== Private Methods - Utility =====
    private void EnsureMainThreadDispatcher()
    {
        if (UnityMainThreadDispatcher.Instance() == null)
        {
            Debug.Log("[SocketIO] Creating UnityMainThreadDispatcher");
        }
    }
}