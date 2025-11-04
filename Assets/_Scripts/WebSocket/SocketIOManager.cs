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
            Debug.Log("[SocketIO] Already connected to websocket");
            return;
        }

        Debug.Log("[SocketIO] Starting connection...");

        try
        {
            var uri = new Uri(serverUrl);
            Debug.Log($"[SocketIO] Server URI: {uri}");

            socket = new SocketIOClient.SocketIO(uri, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                ConnectionTimeout = TimeSpan.FromSeconds(10),
                Reconnection = false,
                ExtraHeaders = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {APIManager.Instance.GetToken()}" }
                }
            });

            Debug.Log("[SocketIO] Socket object created");

            socket.OnConnected += (sender, e) =>
            {
                IsConnected = true;
                Debug.Log("[SocketIO] Websocket connected successfully!");

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnConnected?.Invoke();
                });
            };

            socket.OnDisconnected += (sender, e) =>
            {
                IsConnected = false;
                Debug.Log("[SocketIO] Websocket disconnected");

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnDisconnected?.Invoke();
                });
            };

            socket.OnError += (sender, e) =>
            {
                Debug.LogError($"[SocketIO] Websocket error: {e}");

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnConnectionError?.Invoke(e);
                });
            };

            Debug.Log("[SocketIO] Calling ConnectAsync...");

            // Timeout handling
            var connectTask = socket.ConnectAsync();
            var timeoutTask = Task.Delay(15000); // 15 second timeout

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                // Timeout occurred
                Debug.LogError("[SocketIO] Connection timeout (15 seconds)");
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnConnectionError?.Invoke("Connection timeout: Server not responding");
                });

                // Cleanup socket
                if (socket != null)
                {
                    await socket.DisconnectAsync();
                    socket.Dispose();
                    socket = null;
                }
            }
            else
            {
                // Connection completed
                await connectTask;
                Debug.Log("[SocketIO] ConnectAsync completed");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SocketIO] Websocket connection failed: {e.Message}");
            Debug.LogError($"[SocketIO] Stack Trace: {e.StackTrace}");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnConnectionError?.Invoke($"Server connection failed: {e.Message}");
            });

            // Cleanup socket
            if (socket != null)
            {
                try
                {
                    await socket.DisconnectAsync();
                    socket.Dispose();
                }
                catch { }
                socket = null;
            }
        }
    }

    public async void Disconnect()
    {
        if (socket != null && IsConnected)
        {
            Debug.Log("[SocketIO] Disconnecting...");
            await socket.DisconnectAsync();
            IsConnected = false;
            Debug.Log("[SocketIO] Disconnected");
        }
    }

    public void On(string eventName, Action<SocketIOResponse> callback)
    {
        if (socket == null)
        {
            Debug.LogWarning($"[SocketIO] Socket is null. Event registration failed: {eventName}");
            return;
        }

        socket.On(eventName, (response) =>
        {
            // Execute callback on Unity main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                callback?.Invoke(response);
            });
        });

        Debug.Log($"[SocketIO] Event registered: {eventName}");
    }

    public void Off(string eventName)
    {
        if (socket == null) return;
        socket.Off(eventName);
        Debug.Log($"[SocketIO] Event unregistered: {eventName}");
    }

    public async void Emit(string eventName, object data)
    {
        if (socket == null || !IsConnected)
        {
            Debug.LogWarning("[SocketIO] Websocket not connected");
            return;
        }

        Debug.Log($"[SocketIO] Emitting event: {eventName}");
        await socket.EmitAsync(eventName, data);
    }

    private void OnDestroy()
    {
        Debug.Log("[SocketIO] OnDestroy called");
        Disconnect();
    }

    // FIXED: Event names changed to match API spec (Line 1382-1391)
    public void RegisterMultiplayEvents()
    {
        Debug.Log("[SocketIO] Registering multiplayer events");

        // FIXED: user_joined (was player-joined)
        On("user_joined", (response) =>
        {
            try
            {
                var playerData = response.GetValue<PlayerData>();
                Debug.Log($"[SocketIO] user_joined: {playerData.nickname}");
                if (MultiplaySession.Instance != null)
                {
                    MultiplaySession.Instance.AddPlayer(playerData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketIO] user_joined parse error: {e.Message}");
            }
        });

        // FIXED: user_left (was player-left)
        On("user_left", (response) =>
        {
            try
            {
                var data = response.GetValue<Dictionary<string, string>>();
                if (data.ContainsKey("userId") && MultiplaySession.Instance != null)
                {
                    Debug.Log($"[SocketIO] user_left: {data["userId"]}");
                    MultiplaySession.Instance.RemovePlayer(data["userId"]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketIO] user_left parse error: {e.Message}");
            }
        });

        // FIXED: room_updated (was player-ready)
        On("room_updated", (response) =>
        {
            try
            {
                var data = response.GetValue<Dictionary<string, object>>();
                if (data.ContainsKey("userId") && data.ContainsKey("isReady") && MultiplaySession.Instance != null)
                {
                    string userId = data["userId"].ToString();
                    bool isReady = Convert.ToBoolean(data["isReady"]);
                    Debug.Log($"[SocketIO] room_updated: {userId} -> {isReady}");
                    MultiplaySession.Instance.UpdatePlayerReady(userId, isReady);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketIO] room_updated parse error: {e.Message}");
            }
        });

        // game_started - OK (matches API spec)
        On("game_started", (response) =>
        {
            Debug.Log("[SocketIO] game_started event received");
            if (MultiplaySession.Instance != null)
            {
                MultiplaySession.Instance.StartGame();
            }
        });

        // FIXED: game_completed (was player-completed)
        On("game_completed", (response) =>
        {
            try
            {
                var data = response.GetValue<Dictionary<string, object>>();
                if (data.ContainsKey("userId") && data.ContainsKey("clearTime") && MultiplaySession.Instance != null)
                {
                    string userId = data["userId"].ToString();
                    float clearTime = Convert.ToSingle(data["clearTime"]);
                    Debug.Log($"[SocketIO] game_completed: {userId} -> {clearTime}s");
                    MultiplaySession.Instance.UpdatePlayerClearTime(userId, clearTime);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketIO] game_completed parse error: {e.Message}");
            }
        });

        // FIXED: user_disconnected (was room-closed)
        On("user_disconnected", (response) =>
        {
            Debug.Log("[SocketIO] user_disconnected event received");
            if (MultiplaySession.Instance != null)
            {
                MultiplaySession.Instance.TriggerHostLeft();
            }
        });

        // Image URL sharing (keeping this as is - not in API spec but might be custom)
        On("image-url-shared", (response) =>
        {
            try
            {
                var data = response.GetValue<Dictionary<string, string>>();
                if (data.ContainsKey("imageUrl"))
                {
                    string imageUrl = data["imageUrl"];
                    Debug.Log($"[SocketIO] Received shared image URL: {imageUrl}");

                    if (MultiplaySession.Instance != null)
                    {
                        MultiplaySession.Instance.SharedImageUrl = imageUrl;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SocketIO] image-url-shared parse error: {e.Message}");
            }
        });

        Debug.Log("[SocketIO] Multiplayer events registered");
    }

    public void UnregisterMultiplayEvents()
    {
        Debug.Log("[SocketIO] Unregistering multiplayer events");

        // FIXED: Updated event names
        Off("user_joined");
        Off("user_left");
        Off("room_updated");
        Off("game_started");
        Off("game_completed");
        Off("user_disconnected");
        Off("image-url-shared");

        Debug.Log("[SocketIO] Multiplayer events unregistered");
    }
}