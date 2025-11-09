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

    // Connection state tracking
    private TaskCompletionSource<bool> connectionTcs;
    private TaskCompletionSource<bool> authenticationTcs;

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

        // Ensure UnityMainThreadDispatcher exists
        EnsureMainThreadDispatcher();
    }

    private void EnsureMainThreadDispatcher()
    {
        if (UnityMainThreadDispatcher.Instance() == null)
        {
            Debug.Log("[SocketIO] Creating UnityMainThreadDispatcher");
        }
    }

    /// <summary>
    /// Connect to WebSocket server with async/await pattern
    /// Returns true if connection and authentication successful
    /// </summary>
    // ConnectAndAuthenticateAsync 메서드 수정
    public async Task<bool> ConnectAndAuthenticateAsync()
    {
        if (IsConnected && IsAuthenticated)
        {
            Debug.Log("[SocketIO] Already connected and authenticated");
            return true;
        }

        if (IsConnected && !IsAuthenticated)
        {
            Debug.Log("[SocketIO] Already connected, attempting authentication");
            return await AuthenticateAsync();
        }

        Debug.Log("[SocketIO] Starting connection process...");

        try
        {
            // Create new TaskCompletionSource for this connection attempt
            connectionTcs = new TaskCompletionSource<bool>();
            authenticationTcs = new TaskCompletionSource<bool>();

            var uri = new Uri(serverUrl);
            socket = new SocketIOClient.SocketIO(uri, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                Reconnection = false,
                ConnectionTimeout = TimeSpan.FromSeconds(15) // 타임아웃 증가
            });

            Debug.Log("[SocketIO] Socket instance created");

            // Setup authentication listener BEFORE connecting
            SetupAuthenticationListener();

            // Setup connection event handlers
            socket.OnConnected += OnSocketConnected;
            socket.OnDisconnected += OnSocketDisconnected;
            socket.OnError += OnSocketError;

            // Connect to server
            Debug.Log("[SocketIO] Attempting to connect to server...");
            await socket.ConnectAsync();

            // Wait for connection to complete (with timeout)
            var connectionTask = await Task.WhenAny(connectionTcs.Task, Task.Delay(15000)); // 15초 타임아웃

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

            Debug.Log("[SocketIO] Connection successful, waiting for authentication...");

            // Wait for authentication to complete (with timeout)
            var authTask = await Task.WhenAny(authenticationTcs.Task, Task.Delay(15000)); // 15초 타임아웃

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

        // Mark connection as successful
        connectionTcs?.TrySetResult(true);

        // Small delay before authentication
        await Task.Delay(200);

        // Start authentication
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

        // Mark connection as failed if waiting
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

        // Mark connection as failed if waiting
        connectionTcs?.TrySetResult(false);
        authenticationTcs?.TrySetResult(false);
    }

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
            return true; // Will be confirmed by listener
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

        socket.On("authenticated", (response) =>
        {
            try
            {
                Debug.Log($"[SocketIO] Received authentication response: {response}");

                string responseStr = response.ToString();
                Debug.Log($"[SocketIO] Response string: {responseStr}");

                WS_AuthResponse authResponse = null;

                // Check if response is array format
                if (responseStr.StartsWith("["))
                {
                    try
                    {
                        // Find the complete JSON object within array
                        int firstBracket = responseStr.IndexOf('{');
                        int bracketCount = 0;
                        int lastBracket = -1;

                        // Count brackets to find matching closing bracket
                        for (int i = firstBracket; i < responseStr.Length; i++)
                        {
                            if (responseStr[i] == '{')
                                bracketCount++;
                            else if (responseStr[i] == '}')
                            {
                                bracketCount--;
                                if (bracketCount == 0)
                                {
                                    lastBracket = i;
                                    break;
                                }
                            }
                        }

                        if (firstBracket >= 0 && lastBracket > firstBracket)
                        {
                            string jsonObject = responseStr.Substring(firstBracket, lastBracket - firstBracket + 1);
                            Debug.Log($"[SocketIO] Extracted JSON: {jsonObject}");
                            authResponse = JsonUtility.FromJson<WS_AuthResponse>(jsonObject);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[SocketIO] Failed to extract from array: {ex.Message}");
                    }
                }

                // Fallback: try direct parsing
                if (authResponse == null)
                {
                    try
                    {
                        authResponse = response.GetValue<WS_AuthResponse>();
                        Debug.Log($"[SocketIO] Parsed directly (not array)");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[SocketIO] Direct parse failed: {ex.Message}");
                    }
                }

                if (authResponse != null)
                {
                    Debug.Log($"[SocketIO] Auth response parsed - isSuccess: {authResponse.isSuccess}, code: {authResponse.code}, message: {authResponse.message}");

                    if (authResponse.isSuccess)
                    {
                        IsAuthenticated = true;
                        Debug.Log("[SocketIO] Authentication successful!");

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
                        Debug.LogError($"[SocketIO] Authentication failed: {authResponse.message} (code: {authResponse.code})");
                        authenticationTcs?.TrySetResult(false);
                    }
                }
                else
                {
                    Debug.LogError("[SocketIO] Authentication response is null after parsing");
                    authenticationTcs?.TrySetResult(false);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketIO] Failed to parse authentication response: {e.Message}");
                Debug.LogError($"[SocketIO] Stack trace: {e.StackTrace}");
                authenticationTcs?.TrySetResult(false);
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
            Debug.Log("[SocketIO] Disconnected from WebSocket");
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
        if (socket != null)
        {
            socket.OnConnected -= OnSocketConnected;
            socket.OnDisconnected -= OnSocketDisconnected;
            socket.OnError -= OnSocketError;
        }
        Disconnect();
    }

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
                        Debug.Log($"[WS] Extracted JSON: {jsonObject}");
                        evt = JsonUtility.FromJson<WS_UserJoinedEvent>(jsonObject);
                    }
                }
                else
                {
                    evt = response.GetValue<WS_UserJoinedEvent>();
                }

                if (evt != null && evt.result != null && evt.result.participants != null)
                {
                    Debug.Log($"[WS] Processing {evt.result.participants.Length} participants");

                    var players = new List<PlayerData>();
                    foreach (var p in evt.result.participants)
                    {
                        Debug.Log($"[WS] Participant: {p.username}, Host: {p.isHost}, Ready: {p.isReady}");

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

                        // Trigger room data updated to refresh UI
                        MultiplaySession.Instance.TriggerRoomDataUpdated();

                        Debug.Log($"[WS] Room data updated with {players.Count} players");
                    }
                }
                else
                {
                    Debug.LogError("[WS] Failed to parse user_joined event");
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
                            Debug.Log("[WS] Disconnected user was host - triggering host left");
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