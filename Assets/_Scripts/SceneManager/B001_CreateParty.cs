using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class B001_CreateParty : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text sessionCodeText;
    public Button backButton;
    public Button startGameButton;
    public FadeController fadeController;

    [Header("Player Slots (4 slots)")]
    public PlayerSlot[] playerSlots; // Inspector에서 4개 할당

    [Header("Alert Popup")]
    public GameObject hostLeftPopup;
    public TMP_Text hostLeftMessage;
    public Button hostLeftConfirmButton;

    private bool isHost = false;
    private const int MAX_PLAYERS = 4;
    private string myUserId = "";
    private bool hasAutoReadied = false; // 자동 준비 완료 플래그

    [System.Serializable]
    public class PlayerSlot
    {
        public TMP_Text nicknameText;
        public Image loginImage; // 로그인 이미지
        public Color occupiedColor = Color.green; // 플레이어 있음 (초록색)
        public Color emptyColor = Color.white; // 빈 슬롯 (흰색)
    }

    private void Start()
    {
        // Get my userId
        if (UserSession.Instance != null)
        {
            myUserId = UserSession.Instance.UserID;
        }

        // Register button listeners
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClick);

        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClick);

        if (hostLeftConfirmButton != null)
            hostLeftConfirmButton.onClick.AddListener(OnHostLeftConfirm);

        // Hide host left popup
        if (hostLeftPopup != null)
            hostLeftPopup.SetActive(false);

        // Subscribe to multiplayer events
        SubscribeMultiplayEvents();

        // Check SocketIOManager exists
        if (SocketIOManager.Instance == null)
        {
            Debug.LogError("SocketIOManager is not in scene!");
            return;
        }

        // WebSocket should already be connected from MainMenu
        if (!SocketIOManager.Instance.IsConnected || !SocketIOManager.Instance.IsAuthenticated)
        {
            Debug.LogWarning("WebSocket not connected - returning to main menu");
            OnBackButtonClick();
            return;
        }

        // Initialize lobby
        InitializeLobby();
    }

    private void InitializeLobby()
    {
        // Check MultiplaySession
        if (MultiplaySession.Instance == null)
        {
            Debug.LogError("MultiplaySession is not in scene!");
            return;
        }

        // Get info from MultiplaySession
        isHost = MultiplaySession.Instance.IsHost;

        // Set up UI
        UpdateUI();

        // Update player list (for both host and client)
        UpdatePlayerList();

        // Auto ready if not host
        if (!isHost && !hasAutoReadied)
        {
            Debug.Log("[Lobby] I'm not host - auto readying...");
            StartCoroutine(AutoReadyCoroutine());
        }
    }

    private void UpdateUI()
    {
        // Display session code
        if (sessionCodeText != null && MultiplaySession.Instance != null && MultiplaySession.Instance.CurrentRoom != null)
        {
            string code = MultiplaySession.Instance.CurrentRoom.gameCode;
            sessionCodeText.text = string.IsNullOrEmpty(code) ? "0000" : code;
            Debug.Log($"[UI] Session code updated: {sessionCodeText.text}");
        }
        else
        {
            Debug.LogWarning("[UI] Cannot display session code");
        }

        // Start game button (only active for host)
        if (startGameButton != null)
            startGameButton.gameObject.SetActive(isHost);
    }

    #region Auto Ready
    private IEnumerator AutoReadyCoroutine()
    {
        if (hasAutoReadied)
        {
            Debug.Log("[AutoReady] Already readied - skipping");
            yield break;
        }

        // Wait a bit for room to stabilize after joining
        yield return new WaitForSeconds(1f);

        Debug.Log("[AutoReady] Sending ready request...");

        ReadyToggleRequest request = new ReadyToggleRequest
        {
            gameCode = MultiplaySession.Instance.CurrentRoom.gameCode
        };

        bool requestCompleted = false;

        yield return APIManager.Instance.Post(
            "/games/multiplay/rooms/ready",
            request,
            onSuccess: (response) =>
            {
                Debug.Log($"[AutoReady] Success response: {response}");

                try
                {
                    ReadyToggleResponseWrapper wrapper = JsonUtility.FromJson<ReadyToggleResponseWrapper>(response);

                    if (wrapper != null && wrapper.result != null)
                    {
                        Debug.Log($"[AutoReady] Ready state: {wrapper.result.isReady}");
                        hasAutoReadied = true;

                        // Update local player ready state
                        if (MultiplaySession.Instance != null && MultiplaySession.Instance.CurrentRoom != null)
                        {
                            var myPlayer = MultiplaySession.Instance.CurrentRoom.players?.Find(p => p.userId == myUserId);
                            if (myPlayer != null)
                            {
                                myPlayer.isReady = wrapper.result.isReady == 1;
                                Debug.Log($"[AutoReady] My ready state updated: {myPlayer.isReady}");
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[AutoReady] Failed to parse response: {e.Message}");
                }

                requestCompleted = true;
            },
            onError: (error) =>
            {
                Debug.LogError($"[AutoReady] Failed: {error}");
                requestCompleted = true;
            }
        );

        while (!requestCompleted)
        {
            yield return null;
        }
    }
    #endregion

    #region Player List UI
    private void UpdatePlayerList()
    {
        if (MultiplaySession.Instance == null || MultiplaySession.Instance.CurrentRoom == null)
        {
            Debug.LogWarning("[UpdatePlayerList] CurrentRoom is null");
            return;
        }

        var players = MultiplaySession.Instance.CurrentRoom.players;

        if (players == null)
        {
            Debug.LogWarning("[UpdatePlayerList] Players list is null");
            return;
        }

        Debug.Log($"[UpdatePlayerList] Updating with {players.Count} players");

        // Sort players: Host first, then others
        if (players.Count > 0)
        {
            players.Sort((a, b) =>
            {
                if (a.isHost && !b.isHost) return -1;
                if (!a.isHost && b.isHost) return 1;
                return 0;
            });

            // Log all players
            foreach (var p in players)
            {
                Debug.Log($"[UpdatePlayerList] Player: {p.nickname}, Host: {p.isHost}, Ready: {p.isReady}");
            }
        }

        // Update all 4 slots
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (playerSlots == null || playerSlots.Length <= i || playerSlots[i] == null)
            {
                Debug.LogWarning($"[UpdatePlayerList] PlayerSlot {i} is not assigned");
                continue;
            }

            if (i < players.Count)
            {
                // Slot occupied
                var player = players[i];
                SetPlayerSlot(i, player.nickname, true, player.isHost);
            }
            else
            {
                // Empty slot
                SetPlayerSlot(i, "", false, false);
            }
        }

        Debug.Log($"[UpdatePlayerList] UI update complete");
    }
    private void SetPlayerSlot(int slotIndex, string nickname, bool isOccupied, bool isHost)
    {
        if (playerSlots == null || slotIndex >= playerSlots.Length || playerSlots[slotIndex] == null)
            return;

        var slot = playerSlots[slotIndex];

        // Update nickname text
        if (slot.nicknameText != null)
        {
            if (isOccupied)
            {
                slot.nicknameText.text = nickname + (isHost ? " [Host]" : "");
            }
            else
            {
                slot.nicknameText.text = ""; // Empty slot
            }
        }

        // Update login image color
        if (slot.loginImage != null)
        {
            if (isOccupied)
            {
                // Occupied: Green (모든 플레이어 자동 준비 상태이므로 항상 초록색)
                slot.loginImage.color = slot.occupiedColor;
            }
            else
            {
                // Empty: White
                slot.loginImage.color = slot.emptyColor;
            }
        }
    }
    #endregion

    #region Button Handlers
    public void OnBackButtonClick()
    {
        // Leave room
        if (MultiplaySession.Instance != null && MultiplaySession.Instance.CurrentRoom != null)
        {
            string gameCode = MultiplaySession.Instance.CurrentRoom.gameCode;

            // Send leave event via WebSocket (only if connected)
            if (SocketIOManager.Instance != null && SocketIOManager.Instance.IsConnected)
            {
                SocketIOManager.Instance.LeaveRoom(gameCode);
            }
        }

        // Clear session
        if (MultiplaySession.Instance != null)
        {
            MultiplaySession.Instance.ClearRoomData();
        }

        // Disconnect WebSocket (only if connected)
        if (SocketIOManager.Instance != null && SocketIOManager.Instance.IsConnected)
        {
            SocketIOManager.Instance.UnregisterMultiplayEvents();
            SocketIOManager.Instance.Disconnect();
        }

        // Navigate to main menu
        if (fadeController != null)
            fadeController.FadeToScene("000_MainMenu");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("000_MainMenu");
    }

    public void OnStartGameClick()
    {
        if (!isHost)
        {
            Debug.LogWarning("Only host can start game");
            return;
        }

        // Check max 4 players
        var players = MultiplaySession.Instance.CurrentRoom?.players;
        if (players != null && players.Count > 4)
        {
            Debug.LogWarning("Exceeded maximum player count");
            ShowError("Maximum 4 players allowed");
            return;
        }

        // Host can start with any number of players (1-4)
        int playerCount = players?.Count ?? 0;
        Debug.Log($"Starting game with {playerCount} player(s)...");

        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        StartGameRequest request = new StartGameRequest
        {
            gameCode = MultiplaySession.Instance.CurrentRoom.gameCode
        };

        yield return APIManager.Instance.Post(
            "/games/multiplay/rooms/start",
            request,
            onSuccess: (response) =>
            {
                Debug.Log($"[API] Game start response: {response}");

                StartGameResponseWrapper wrapper = JsonUtility.FromJson<StartGameResponseWrapper>(response);

                if (wrapper != null && wrapper.result != null)
                {
                    Debug.Log($"[API] Game started successfully");

                    // Navigate to game scene
                    StartMultiplayGame();
                }
                else
                {
                    ShowError("Failed to parse game start response");
                }
            },
            onError: (error) =>
            {
                Debug.LogError($"[StartGame] Error: {error}");

                // Parse error message
                string errorMessage = "Failed to start game";

                if (error.Contains("준비 상태가 아닙니다") || error.Contains("not ready"))
                {
                    errorMessage = "Not all players are ready.\nPlease wait a moment and try again.";
                }
                else if (error.Contains("400"))
                {
                    errorMessage = "Cannot start game.\nPlease check all players are ready.";
                }

                ShowError(errorMessage);
            }
        );
    }

    private void ShowError(string message)
    {
        Debug.LogWarning($"[Error] {message}");
        // TODO: Show error popup to user
    }
    #endregion

    #region Multiplayer Game Start
    private void StartMultiplayGame()
    {
        // Auto-generate tags
        List<string> randomTags = GenerateRandomTags();

        // Save tags to UserSession
        UserSession.Instance.Tags = randomTags;

        // Save to GameData (for system compatibility)
        for (int i = 0; i < randomTags.Count && i < 4; i++)
        {
            GameData.tags[i] = randomTags[i];
        }
        GameData.difficulty = 3; // Default 3x3
        GameData.isMultiplay = true; // Mark as multiplayer

        // Save game code
        GameData.gameCode = MultiplaySession.Instance.CurrentRoom.gameCode;
        GameData.imageUrl = MultiplaySession.Instance.CurrentRoom.imageUrl;

        Debug.Log("[Multiplay] Auto-generated tags: " + string.Join(", ", randomTags));
        Debug.Log("[Multiplay] Game code: " + GameData.gameCode);

        // Navigate to G002_Game
        if (fadeController != null)
            fadeController.FadeToScene("G002_Game");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("G002_Game");
    }

    private List<string> GenerateRandomTags()
    {
        List<string> tags = new List<string>();

        string[] categories = { "Style", "Subject", "Mood", "Background" };

        foreach (var category in categories)
        {
            var options = LoadTagOptions($"TagRandom/{category}");
            if (options.Count > 0)
            {
                string randomTag = options[Random.Range(0, options.Count)];
                tags.Add(randomTag);
            }
            else
            {
                tags.Add($"Default{category}");
            }
        }

        return tags;
    }

    private List<string> LoadTagOptions(string path)
    {
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset != null)
        {
            var lines = textAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            var result = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    result.Add(trimmed);
            }

            return result;
        }

        Debug.LogWarning("Tag file not found: " + path);
        return new List<string>();
    }
    #endregion

    #region WebSocket Events
    private void SubscribeMultiplayEvents()
    {
        if (MultiplaySession.Instance == null) return;

        MultiplaySession.Instance.OnRoomDataUpdated += OnRoomDataUpdated;
        MultiplaySession.Instance.OnPlayerJoined += OnPlayerJoined;
        MultiplaySession.Instance.OnPlayerLeft += OnPlayerLeft;
        MultiplaySession.Instance.OnGameStarted += OnGameStarted;
        MultiplaySession.Instance.OnHostLeft += OnHostLeft;
    }

    private void UnsubscribeMultiplayEvents()
    {
        if (MultiplaySession.Instance == null) return;

        MultiplaySession.Instance.OnRoomDataUpdated -= OnRoomDataUpdated;
        MultiplaySession.Instance.OnPlayerJoined -= OnPlayerJoined;
        MultiplaySession.Instance.OnPlayerLeft -= OnPlayerLeft;
        MultiplaySession.Instance.OnGameStarted -= OnGameStarted;
        MultiplaySession.Instance.OnHostLeft -= OnHostLeft;
    }

    private void OnRoomDataUpdated(RoomData roomData)
    {
        Debug.Log("[Event] Room data updated");
        UpdatePlayerList();
    }

    private void OnPlayerJoined(PlayerData player)
    {
        Debug.Log($"[Event] {player.username} joined");
        UpdatePlayerList();
    }

    private void OnPlayerLeft(PlayerData player)
    {
        Debug.Log($"[Event] {player.username} left");

        // Check if the leaving player is the host
        if (player.isHost && player.userId != myUserId)
        {
            Debug.Log("[Event] Host has left - triggering host left event");
            OnHostLeft();
            return;
        }

        UpdatePlayerList();
    }

    private void OnGameStarted()
    {
        if (!isHost) // Client only
        {
            Debug.Log("[Event] Game started by host");
            StartMultiplayGame();
        }
    }

    private void OnHostLeft()
    {
        // Don't show popup if I am the host leaving
        if (isHost)
        {
            Debug.Log("[Event] I am the host - ignoring host left event");
            return;
        }

        Debug.Log("[Event] Host has left the room");
        ShowHostLeftPopup();
    }

    private void ShowHostLeftPopup()
    {
        if (hostLeftPopup != null)
        {
            hostLeftPopup.SetActive(true);

            if (hostLeftMessage != null)
                hostLeftMessage.text = "Host has left the room.\nThe room has been closed.";
        }
        else
        {
            // No popup - return to main menu immediately
            ForceReturnToMainMenu();
        }
    }

    private void OnHostLeftConfirm()
    {
        // Hide popup and return to main menu
        if (hostLeftPopup != null)
            hostLeftPopup.SetActive(false);

        ForceReturnToMainMenu();
    }

    private void ForceReturnToMainMenu()
    {
        // Clear session
        if (MultiplaySession.Instance != null)
        {
            MultiplaySession.Instance.ClearRoomData();
        }

        // Disconnect WebSocket
        if (SocketIOManager.Instance != null && SocketIOManager.Instance.IsConnected)
        {
            SocketIOManager.Instance.UnregisterMultiplayEvents();
            SocketIOManager.Instance.Disconnect();
        }

        // Navigate to main menu
        if (fadeController != null)
            fadeController.FadeToScene("000_MainMenu");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("000_MainMenu");
    }
    #endregion

    private void OnDestroy()
    {
        // Unsubscribe from events
        UnsubscribeMultiplayEvents();

        // Remove button listeners
        if (backButton != null)
            backButton.onClick.RemoveAllListeners();

        if (startGameButton != null)
            startGameButton.onClick.RemoveAllListeners();

        if (hostLeftConfirmButton != null)
            hostLeftConfirmButton.onClick.RemoveAllListeners();
    }
}