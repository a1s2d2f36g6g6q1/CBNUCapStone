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
            Debug.Log("[JoinParty] Already attempting to connect");
            return;
        }

        // Check if user is logged in
        if (UserSession.Instance == null || !UserSession.Instance.IsLoggedIn)
        {
            ShowError("You must be logged in to join a party");
            return;
        }

        // Check token
        if (APIManager.Instance == null || string.IsNullOrEmpty(APIManager.Instance.GetToken()))
        {
            ShowError("Please login first to join a party");
            return;
        }

        // Start connection and join
        StartCoroutine(ConnectAndJoinRoomCoroutine(gameCode));
    }

    private IEnumerator ConnectAndJoinRoomCoroutine(string gameCode)
    {
        isConnecting = true;
        SetLoadingState(true);

        // STEP 1/5: Initialize
        ShowError("(1/5) Initializing connection...", false);
        yield return new WaitForSeconds(0.3f);

        // Check if SocketIOManager exists
        if (SocketIOManager.Instance == null)
        {
            ShowError("SocketIOManager not initialized");
            SetLoadingState(false);
            isConnecting = false;
            yield break;
        }

        // STEP 2/5: WebSocket Connection
        ShowError("(2/5) Connecting to WebSocket server...", false);
        Debug.Log("[JoinParty] Starting WebSocket connection...");

        // Use async method
        var connectionTask = SocketIOManager.Instance.ConnectAndAuthenticateAsync();

        // Wait for connection task to complete (with visual feedback)
        float elapsed = 0f;
        int lastSecond = 0;
        while (!connectionTask.IsCompleted && elapsed < 20f)
        {
            elapsed += Time.deltaTime;
            int currentSecond = (int)elapsed;

            // Update status every second
            if (currentSecond != lastSecond && currentSecond > 0)
            {
                ShowError($"(2/5) Connecting to WebSocket... ({currentSecond}s)", false);
                lastSecond = currentSecond;
            }

            yield return null;
        }

        if (!connectionTask.IsCompleted)
        {
            Debug.LogError("[JoinParty] Connection task timeout");
            ShowError("Error: Connection timeout (20s)");
            SetLoadingState(false);
            isConnecting = false;
            yield break;
        }

        bool connected = connectionTask.Result;

        Debug.Log($"[JoinParty] Connection result: {connected}");

        if (!connected)
        {
            ShowError("Failed to connect to server");
            SetLoadingState(false);
            isConnecting = false;
            yield break;
        }

        // STEP 3/5: Authentication Complete
        ShowError("(3/5) Authentication successful!", false);
        yield return new WaitForSeconds(0.5f);

        Debug.Log("[JoinParty] WebSocket connected and authenticated, registering events");

        // STEP 4/5: Register Events
        ShowError("(4/5) Registering multiplayer events...", false);
        SocketIOManager.Instance.RegisterMultiplayEvents();
        yield return new WaitForSeconds(0.3f);

        // STEP 5/5: Join Room
        ShowError("(5/5) Joining room...", false);

        // Call room join API
        yield return JoinRoomCoroutine(gameCode);

        isConnecting = false;
    }

    private IEnumerator JoinRoomCoroutine(string gameCode)
    {
        Debug.Log($"[JoinParty] Joining room via API - Code: {gameCode}");

        JoinRoomRequest request = new JoinRoomRequest { gameCode = gameCode };

        bool requestCompleted = false;
        bool requestSuccess = false;

        yield return APIManager.Instance.Post(
            "/games/multiplay/rooms/join",
            request,
            onSuccess: (response) =>
            {
                Debug.Log($"[API] Join room response: {response}");

                try
                {
                    JoinRoomResponseWrapper wrapper = JsonUtility.FromJson<JoinRoomResponseWrapper>(response);

                    if (wrapper != null && wrapper.result != null)
                    {
                        Debug.Log($"[API] Joined room - RoomId: {wrapper.result.roomId}, Code: {wrapper.result.gameCode}");

                        // Set room info in MultiplaySession (client)
                        MultiplaySession.Instance.SetRoomInfo(
                            wrapper.result.roomId.ToString(),
                            wrapper.result.gameCode,
                            false // Client (not host)
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

                        Debug.Log($"[JoinParty] Room joined successfully! RoomId: {wrapper.result.roomId}");

                        requestSuccess = true;
                    }
                    else
                    {
                        Debug.LogError("[JoinParty] Failed to parse room join response");
                        ShowError("Failed to parse server response");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[JoinParty] Error parsing response: {e.Message}");
                    ShowError("Error processing server response");
                }

                requestCompleted = true;
            },
            onError: (error) =>
            {
                Debug.LogError($"[JoinParty] Room join failed: {error}");
                ShowError("Failed to join room: " + error);
                requestCompleted = true;
            }
        );

        // Wait for request to complete
        while (!requestCompleted)
        {
            yield return null;
        }

        if (requestSuccess)
        {
            ShowError("(5/5) Successfully joined room!", false);
            yield return new WaitForSeconds(0.8f);

            SetLoadingState(false);
            Debug.Log("[JoinParty] Transitioning to lobby scene");

            // Navigate to lobby (same scene as CreateParty)
            if (fadeController != null)
                fadeController.FadeToScene("B001_CreateParty");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("B001_CreateParty");
        }
        else
        {
            SetLoadingState(false);

            // Disconnect WebSocket on error
            if (SocketIOManager.Instance != null)
            {
                SocketIOManager.Instance.Disconnect();
            }
        }
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

    private void ShowError(string message, bool isError = true)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.color = isError ? Color.red : Color.white;

            if (isError)
            {
                StartCoroutine(ClearErrorMessageAfterDelay(3f));
            }
        }

        Debug.Log($"[JoinParty] {message}");
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