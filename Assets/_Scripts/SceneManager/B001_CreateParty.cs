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
    public TMP_Text[] playerSlots; // 4 fixed slots
    public FadeController fadeController;

    [Header("Alert Popup")]
    public GameObject hostLeftPopup;
    public TMP_Text hostLeftMessage;

    private bool isHost = false;
    private const int MAX_PLAYERS = 4;

    private void Start()
    {
        // Register button listeners
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClick);

        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClick);

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
    private IEnumerator WaitForConnectionAndInitialize()
    {
        float elapsed = 0f;
        while (!SocketIOManager.Instance.IsConnected && elapsed < 5f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (SocketIOManager.Instance.IsConnected)
        {
            Debug.Log("WebSocket connected successfully");
            InitializeLobby();
        }
        else
        {
            Debug.LogWarning("WebSocket connection failed, but lobby will continue");
            // Continue with lobby initialization even without WebSocket
            InitializeLobby();
        }
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

    #region Player List UI
    private void UpdatePlayerList()
    {
        if (MultiplaySession.Instance.CurrentRoom == null) return;

        var players = MultiplaySession.Instance.CurrentRoom.players;

        // Update all 4 slots
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (playerSlots == null || playerSlots.Length <= i || playerSlots[i] == null)
                continue;

            if (players != null && i < players.Count)
            {
                // If player exists, display nickname
                string displayName = string.IsNullOrEmpty(players[i].username) ? "Guest" : players[i].username;

                // Add ready indicator for non-host players
                if (!players[i].isHost)
                {
                    displayName += players[i].isReady ? " [Ready]" : " [Not Ready]";
                }
                else
                {
                    displayName += " [Host]";
                }

                playerSlots[i].text = displayName;
            }
            else
            {
                // Empty slot displays "Empty"
                playerSlots[i].text = "Empty";
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
        if (!isHost) return;

        // Check max 4 players
        var players = MultiplaySession.Instance.CurrentRoom?.players;
        if (players != null && players.Count > 4)
        {
            Debug.LogWarning("Exceeded maximum player count");
            return;
        }

        // Check if all non-host players are ready
        if (players != null)
        {
            bool allReady = true;
            foreach (var player in players)
            {
                if (!player.isHost && !player.isReady)
                {
                    allReady = false;
                    break;
                }
            }

            if (!allReady)
            {
                Debug.LogWarning("Not all players are ready");
                ShowError("All players must be ready before starting");
                return;
            }
        }

        Debug.Log("Starting game via API...");
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

                if (wrapper.result != null)
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
                ShowError("Failed to start game: " + error);
            }
        );
    }

    private void ShowError(string message)
    {
        Debug.LogWarning(message);
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
        UpdatePlayerList();
    }

    private void OnPlayerJoined(PlayerData player)
    {
        Debug.Log($"{player.username} joined");
        UpdatePlayerList();
    }

    private void OnPlayerLeft(PlayerData player)
    {
        Debug.Log($"{player.username} left");
        UpdatePlayerList();
    }

    private void OnGameStarted()
    {
        if (!isHost) // Client only
        {
            StartMultiplayGame();
        }
    }

    private void OnHostLeft()
    {
        // Show host left popup
        if (hostLeftPopup != null)
        {
            hostLeftPopup.SetActive(true);

            if (hostLeftMessage != null)
                hostLeftMessage.text = "Host has left the room.\nThe room has been closed.";

            StartCoroutine(ReturnToMainMenuAfterDelay(3f));
        }
        else
        {
            OnBackButtonClick();
        }
    }

    private IEnumerator ReturnToMainMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnBackButtonClick();
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
    }
}