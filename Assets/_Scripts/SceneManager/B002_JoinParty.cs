using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class B002_JoinParty : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField sessionCodeInput;
    public Button joinButton;
    public Button backButton;
    public TMP_Text errorMessageText;
    public GameObject loadingPanel;
    public FadeController fadeController;

    private bool isConnecting = false;

    private void Start()
    {
        if (joinButton != null)
            joinButton.onClick.AddListener(OnJoinButtonClick);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClick);

        if (errorMessageText != null)
            errorMessageText.text = "";

        SetLoadingState(false);
    }

    public void OnJoinButtonClick()
    {
        string gameCode = sessionCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(gameCode))
        {
            ShowError("Please enter a game code");
            return;
        }

        if (isConnecting)
        {
            Debug.Log("Already attempting to connect");
            return;
        }

        // Start WebSocket connection
        StartCoroutine(ConnectAndJoinRoom(gameCode));
    }

    private IEnumerator ConnectAndJoinRoom(string gameCode)
    {
        isConnecting = true;
        SetLoadingState(true);

        // Check if SocketIOManager exists
        if (SocketIOManager.Instance == null)
        {
            ShowError("SocketIOManager not initialized");
            SetLoadingState(false);
            isConnecting = false;
            yield break;
        }

        // Connect to WebSocket
        bool socketConnected = false;
        bool socketAuthenticated = false;
        string connectionError = null;

        SocketIOManager.Instance.OnConnected += () => socketConnected = true;
        SocketIOManager.Instance.OnAuthenticated += () => socketAuthenticated = true;
        SocketIOManager.Instance.OnConnectionError += (error) => connectionError = error;

        if (!SocketIOManager.Instance.IsConnected)
        {
            Debug.Log("Connecting to WebSocket...");
            SocketIOManager.Instance.Connect();
        }
        else
        {
            socketConnected = true;
            socketAuthenticated = SocketIOManager.Instance.IsAuthenticated;
        }

        // Wait for connection and authentication (max 10 seconds)
        float elapsed = 0f;
        while ((!socketConnected || !socketAuthenticated) && connectionError == null && elapsed < 10f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Unsubscribe from events
        SocketIOManager.Instance.OnConnected -= () => socketConnected = true;
        SocketIOManager.Instance.OnAuthenticated -= () => socketAuthenticated = true;
        SocketIOManager.Instance.OnConnectionError -= (error) => connectionError = error;

        if (connectionError != null)
        {
            ShowError("WebSocket connection failed: " + connectionError);
            SetLoadingState(false);
            isConnecting = false;
            yield break;
        }

        if (!socketConnected)
        {
            ShowError("WebSocket connection timeout");
            SetLoadingState(false);
            isConnecting = false;
            yield break;
        }

        if (!socketAuthenticated)
        {
            ShowError("WebSocket authentication timeout");
            SetLoadingState(false);
            isConnecting = false;
            yield break;
        }

        Debug.Log("WebSocket connected and authenticated, joining room...");

        // Register multiplayer events
        SocketIOManager.Instance.RegisterMultiplayEvents();

        // Call room join API
        yield return JoinRoomCoroutine(gameCode);

        isConnecting = false;
    }

    private IEnumerator JoinRoomCoroutine(string gameCode)
    {
        JoinRoomRequest request = new JoinRoomRequest { gameCode = gameCode };

        yield return APIManager.Instance.Post(
            "/games/multiplay/rooms/join",
            request,
            onSuccess: (response) =>
            {
                Debug.Log($"[API] Join room response: {response}");

                JoinRoomResponseWrapper wrapper = JsonUtility.FromJson<JoinRoomResponseWrapper>(response);

                if (wrapper.result != null)
                {
                    Debug.Log($"[API] Joined room - RoomId: {wrapper.result.roomId}, Code: {wrapper.result.gameCode}");

                    // Set room info in MultiplaySession (client)
                    MultiplaySession.Instance.SetRoomInfo(
                        wrapper.result.roomId.ToString(),
                        wrapper.result.gameCode,
                        false // Client
                    );

                    // Store image and tags
                    if (MultiplaySession.Instance.CurrentRoom != null)
                    {
                        MultiplaySession.Instance.CurrentRoom.imageUrl = wrapper.result.imageUrl;
                        MultiplaySession.Instance.CurrentRoom.tags = wrapper.result.tags;
                        MultiplaySession.Instance.CurrentRoom.hostUsername = wrapper.result.hostUsername;
                    }

                    // Convert participants to players
                    if (wrapper.result.participants != null)
                    {
                        var players = new List<PlayerData>();
                        foreach (var p in wrapper.result.participants)
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
                        MultiplaySession.Instance.CurrentRoom.players = players;
                    }

                    // Send join_room event via WebSocket
                    SocketIOManager.Instance.JoinRoom(gameCode);

                    Debug.Log($"Room joined successfully! RoomId: {wrapper.result.roomId}");

                    SetLoadingState(false);

                    // Navigate to lobby
                    if (fadeController != null)
                        fadeController.FadeToScene("B001_CreateParty");
                    else
                        UnityEngine.SceneManagement.SceneManager.LoadScene("B001_CreateParty");
                }
                else
                {
                    ShowError("Failed to parse room join response");
                    SetLoadingState(false);
                }
            },
            onError: (error) =>
            {
                ShowError("Failed to join room: " + error);
                SetLoadingState(false);

                // Disconnect WebSocket
                SocketIOManager.Instance.Disconnect();
            }
        );
    }

    public void OnBackButtonClick()
    {
        // Disconnect WebSocket if connected
        if (SocketIOManager.Instance != null && SocketIOManager.Instance.IsConnected)
        {
            SocketIOManager.Instance.Disconnect();
        }

        if (fadeController != null)
            fadeController.FadeToScene("000_MainMenu");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("000_MainMenu");
    }

    private void SetLoadingState(bool isLoading)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(isLoading);

        if (joinButton != null)
            joinButton.interactable = !isLoading;

        if (sessionCodeInput != null)
            sessionCodeInput.interactable = !isLoading;
    }

    private void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.color = Color.red;

            StartCoroutine(ClearErrorMessageAfterDelay(3f));
        }

        Debug.LogWarning(message);
    }

    private IEnumerator ClearErrorMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorMessageText != null)
            errorMessageText.text = "";
    }

    private void OnDestroy()
    {
        if (joinButton != null)
            joinButton.onClick.RemoveAllListeners();

        if (backButton != null)
            backButton.onClick.RemoveAllListeners();
    }
}