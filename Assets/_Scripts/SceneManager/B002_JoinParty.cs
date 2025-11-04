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
        string sessionCode = sessionCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(sessionCode))
        {
            ShowError("Please enter session code");
            return;
        }

        if (isConnecting)
        {
            Debug.Log("Already connecting");
            return;
        }

        // Start websocket connection
        StartCoroutine(ConnectAndJoinRoom(sessionCode));
    }

    private IEnumerator ConnectAndJoinRoom(string sessionCode)
    {
        isConnecting = true;
        SetLoadingState(true);

        // Connect websocket
        bool socketConnected = false;
        string connectionError = null;

        SocketIOManager.Instance.OnConnected += () => socketConnected = true;
        SocketIOManager.Instance.OnConnectionError += (error) => connectionError = error;

        SocketIOManager.Instance.Connect();

        // Wait for connection (max 5 seconds)
        float elapsed = 0f;
        while (!socketConnected && connectionError == null && elapsed < 5f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Unsubscribe events
        SocketIOManager.Instance.OnConnected -= () => socketConnected = true;
        SocketIOManager.Instance.OnConnectionError -= (error) => connectionError = error;

        if (connectionError != null)
        {
            ShowError("Websocket connection failed: " + connectionError);
            SetLoadingState(false);
            isConnecting = false;
            yield break;
        }

        if (!socketConnected)
        {
            ShowError("Websocket connection timeout");
            SetLoadingState(false);
            isConnecting = false;
            yield break;
        }

        Debug.Log("Websocket connected, attempting to join room...");

        // Register multiplayer events
        SocketIOManager.Instance.RegisterMultiplayEvents();

        // Call join room API
        yield return JoinRoomCoroutine(sessionCode);

        isConnecting = false;
    }

    private IEnumerator JoinRoomCoroutine(string sessionCode)
    {
        // FIXED: Use gameCode field instead of sessionCode
        JoinRoomRequest request = new JoinRoomRequest { gameCode = sessionCode };

        yield return APIManager.Instance.Post(
            "/games/multiplay/rooms/join",
            request,
            onSuccess: (response) =>
            {
                Debug.Log($"[JoinParty] API response: {response}");

                // FIXED: Parse response with wrapper structure
                JoinRoomResponse joinResponse = JsonUtility.FromJson<JoinRoomResponse>(response);

                if (joinResponse.isSuccess && joinResponse.result != null)
                {
                    // FIXED: Convert JoinRoomResult to RoomData format
                    RoomData roomData = ConvertJoinResultToRoomData(joinResponse.result);

                    // Save room info to MultiplaySession (client)
                    MultiplaySession.Instance.SetRoomInfo(
                        roomData.roomId,
                        roomData.sessionCode,
                        false // Client
                    );

                    // Update room data
                    MultiplaySession.Instance.UpdateRoomData(roomData);

                    Debug.Log($"Room joined successfully! RoomId: {roomData.roomId}");

                    SetLoadingState(false);

                    // Navigate to lobby
                    if (fadeController != null)
                        fadeController.FadeToScene("B001_CreateParty");
                    else
                        UnityEngine.SceneManagement.SceneManager.LoadScene("B001_CreateParty");
                }
                else
                {
                    ShowError("Room join failed: " + joinResponse.message);
                    SetLoadingState(false);

                    // Disconnect websocket
                    SocketIOManager.Instance.Disconnect();
                }
            },
            onError: (error) =>
            {
                ShowError("Room join failed: " + error);
                SetLoadingState(false);

                // Disconnect websocket
                SocketIOManager.Instance.Disconnect();
            }
        );
    }

    /// <summary>
    /// FIXED: Convert JoinRoomResult to RoomData format
    /// </summary>
    private RoomData ConvertJoinResultToRoomData(JoinRoomResult result)
    {
        RoomData roomData = new RoomData
        {
            roomId = result.roomId.ToString(),  // Convert int to string
            sessionCode = result.gameCode,
            hostId = result.hostUsername,  // Use hostUsername as hostId
            players = ConvertParticipantsToPlayers(result.participants),
            isGameStarted = false,
            maxPlayers = 4
        };

        return roomData;
    }

    /// <summary>
    /// FIXED: Convert ParticipantData[] to List<PlayerData>
    /// </summary>
    private List<PlayerData> ConvertParticipantsToPlayers(ParticipantData[] participants)
    {
        List<PlayerData> players = new List<PlayerData>();

        if (participants != null)
        {
            foreach (var p in participants)
            {
                players.Add(new PlayerData
                {
                    userId = p.userId.ToString(),
                    nickname = p.nickname,
                    isReady = p.isReady == 1,  // Convert int to bool (0 or 1)
                    isHost = false  // Will be determined by comparing with hostUsername
                });
            }
        }

        return players;
    }

    public void OnBackButtonClick()
    {
        // Disconnect websocket if connected
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