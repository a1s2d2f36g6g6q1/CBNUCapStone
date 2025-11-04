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
    public TMP_Text[] playerSlots; // 4개 고정 슬롯
    public FadeController fadeController;

    [Header("알림창")]
    public GameObject hostLeftPopup;
    public TMP_Text hostLeftMessage;

    private bool isHost = false;
    private const int MAX_PLAYERS = 4;

    private void Start()
    {
        // 버튼 리스너 등록
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClick);

        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClick);

        // 호스트 퇴장 팝업 숨김
        if (hostLeftPopup != null)
            hostLeftPopup.SetActive(false);

        // 멀티플레이 이벤트 구독
        SubscribeMultiplayEvents();

        // SocketIOManager 확인
        if (SocketIOManager.Instance == null)
        {
            Debug.LogError("SocketIOManager가 씬에 없습니다!");
            return;
        }

        // 웹소켓 연결 확인
        if (!SocketIOManager.Instance.IsConnected)
        {
            Debug.Log("웹소켓 연결 시도 중...");
            SocketIOManager.Instance.Connect();
            StartCoroutine(WaitForConnectionAndInitialize());
        }
        else
        {
            InitializeLobby();
        }
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
            Debug.Log("웹소켓 연결 성공");
            InitializeLobby();
        }
        else
        {
            Debug.LogWarning("웹소켓 연결 실패, 로비는 계속 진행");
            // 웹소켓 없이도 로비 초기화
            InitializeLobby();
        }
    }

    private void InitializeLobby()
    {
        // MultiplaySession 확인
        if (MultiplaySession.Instance == null)
        {
            Debug.LogError("MultiplaySession이 씬에 없습니다!");
            return;
        }

        // MultiplaySession에서 정보 가져오기
        isHost = MultiplaySession.Instance.IsHost;

        // UI 설정
        UpdateUI();

        // 플레이어 목록 업데이트 (호스트/클라이언트 모두)
        UpdatePlayerList();
    }

    private void UpdateUI()
    {
        // 세션 코드 표시
        if (sessionCodeText != null && MultiplaySession.Instance != null && MultiplaySession.Instance.CurrentRoom != null)
        {
            string code = MultiplaySession.Instance.CurrentRoom.sessionCode;
            sessionCodeText.text = string.IsNullOrEmpty(code) ? "0000" : code;
            Debug.Log($"[UI] 세션 코드 업데이트: {sessionCodeText.text}");
        }
        else
        {
            Debug.LogWarning("[UI] 세션 코드를 표시할 수 없음");
        }

        // 게임 시작 버튼 (호스트만 활성화)
        if (startGameButton != null)
            startGameButton.gameObject.SetActive(isHost);
    }

    #region 플레이어 목록 UI
    private void UpdatePlayerList()
    {
        if (MultiplaySession.Instance.CurrentRoom == null) return;

        var players = MultiplaySession.Instance.CurrentRoom.players;

        // 4개 슬롯 모두 업데이트
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (playerSlots == null || playerSlots.Length <= i || playerSlots[i] == null)
                continue;

            if (players != null && i < players.Count)
            {
                // 플레이어가 있으면 닉네임 표시
                string displayName = string.IsNullOrEmpty(players[i].nickname) ? "Guest" : players[i].nickname;
                playerSlots[i].text = displayName;
            }
            else
            {
                // 빈 슬롯은 "Empty" 표시
                playerSlots[i].text = "Empty";
            }
        }
    }
    #endregion

    #region 버튼 핸들러
    public void OnBackButtonClick()
    {
        // 방 나가기
        if (MultiplaySession.Instance != null && MultiplaySession.Instance.CurrentRoom != null)
        {
            string roomId = MultiplaySession.Instance.CurrentRoom.roomId;

            // 웹소켓으로 나가기 이벤트 전송 (연결되어 있을 때만)
            if (SocketIOManager.Instance != null && SocketIOManager.Instance.IsConnected)
            {
                SocketIOManager.Instance.Emit("leave-room", new { roomId });
            }
        }

        // 세션 초기화
        if (MultiplaySession.Instance != null)
        {
            MultiplaySession.Instance.ClearRoomData();
        }

        // 웹소켓 연결 해제 (연결되어 있을 때만)
        if (SocketIOManager.Instance != null && SocketIOManager.Instance.IsConnected)
        {
            SocketIOManager.Instance.UnregisterMultiplayEvents();
            SocketIOManager.Instance.Disconnect();
        }

        // 메인 메뉴로 이동
        if (fadeController != null)
            fadeController.FadeToScene("000_MainMenu");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("000_MainMenu");
    }

    public void OnStartGameClick()
    {
        if (!isHost) return;

        // 최대 4명 확인
        var players = MultiplaySession.Instance.CurrentRoom?.players;
        if (players != null && players.Count > 4)
        {
            Debug.LogWarning("최대 플레이어 수를 초과했습니다.");
            return;
        }

        // 웹소켓으로 게임 시작 이벤트 전송
        SocketIOManager.Instance.Emit("start-game", new
        {
            roomId = MultiplaySession.Instance.CurrentRoom.roomId
        });

        // 게임 씬으로 이동
        StartMultiplayGame();
    }
    #endregion

    #region 멀티플레이 게임 시작
    private void StartMultiplayGame()
    {
        List<string> randomTags = GenerateRandomTags();
        UserSession.Instance.Tags = randomTags;

        for (int i = 0; i < randomTags.Count && i < 4; i++)
        {
            GameData.tags[i] = randomTags[i];
        }
        GameData.difficulty = 3;

        Debug.Log("[Multiplay] Tags: " + string.Join(", ", randomTags));

        if (MultiplaySession.Instance.IsHost)
        {
            StartCoroutine(GenerateAndShareImageUrl(randomTags));
        }
        else
        {
            Debug.Log("[Multiplay] Client waiting for image URL...");

            if (fadeController != null)
                fadeController.FadeToScene("G002_Game");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("G002_Game");
        }
    }

    private IEnumerator GenerateAndShareImageUrl(List<string> tags)
    {
        Debug.Log("[Multiplay] Host generating image...");

        bool imageReady = false;

        if (AIImageService.Instance != null)
        {
            AIImageService.Instance.GenerateImage(
                tags,
                onSuccess: (texture) =>
                {
                    imageReady = true;
                },
                onError: (error) =>
                {
                    Debug.LogError($"[Multiplay] Image gen failed: {error}");
                    imageReady = true;
                }
            );

            float elapsed = 0f;
            while (!imageReady && elapsed < 10f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        string imageUrl = MultiplaySession.Instance?.SharedImageUrl;

        if (!string.IsNullOrEmpty(imageUrl))
        {
            Debug.Log($"[Multiplay] Sharing URL: {imageUrl}");

            if (SocketIOManager.Instance != null && SocketIOManager.Instance.IsConnected)
            {
                SocketIOManager.Instance.Emit("share-image-url", new
                {
                    roomId = MultiplaySession.Instance.CurrentRoom.roomId,
                    imageUrl = imageUrl
                });
            }
        }
        else
        {
            Debug.LogWarning("[Multiplay] No URL to share. Using fallback.");
        }

        yield return new WaitForSeconds(0.5f);

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

        Debug.LogWarning("태그 파일을 찾을 수 없음: " + path);
        return new List<string>();
    }
    #endregion

    #region 웹소켓 이벤트
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
        Debug.Log($"{player.nickname} 입장");
        UpdatePlayerList();
    }

    private void OnPlayerLeft(PlayerData player)
    {
        Debug.Log($"{player.nickname} 퇴장");
        UpdatePlayerList();
    }

    private void OnGameStarted()
    {
        if (!isHost) // 클라이언트만
        {
            StartMultiplayGame();
        }
    }

    private void OnHostLeft()
    {
        // 호스트 퇴장 팝업 표시
        if (hostLeftPopup != null)
        {
            hostLeftPopup.SetActive(true);

            if (hostLeftMessage != null)
                hostLeftMessage.text = "호스트가 나갔습니다.\n방이 닫혔습니다.";

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
        // 이벤트 구독 해제
        UnsubscribeMultiplayEvents();

        // 버튼 리스너 제거
        if (backButton != null)
            backButton.onClick.RemoveAllListeners();

        if (startGameButton != null)
            startGameButton.onClick.RemoveAllListeners();
    }
}