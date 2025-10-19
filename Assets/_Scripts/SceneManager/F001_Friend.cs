using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class F001_Friend : MonoBehaviour
{
    public FadeController fadeController;

    [Header("UI 연결")]
    public Transform friendListContainer;
    public GameObject friendCardPrefab;

    [Header("패널들")]
    public GameObject settingsPanel;
    public GameObject profilePanel;

    [Header("TR 버튼 그룹")]
    public GameObject[] loginOnlyButtons;
    public GameObject settingsButton;

    [Header("더미 데이터 사용 여부")]
    public bool useDummyData = true;  // Inspector에서 설정 가능

    private List<FriendItem> friendList = new();

    private void Start()
    {
        UpdateTopRightButtons();
        LoadFriendList();
    }

    private void OnEnable()
    {
        UpdateTopRightButtons();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) CloseAllPanels();
    }

    #region 친구 목록 로드
    private void LoadFriendList()
    {
        if (useDummyData)
        {
            LoadDummyFriendList();
        }
        else
        {
            StartCoroutine(LoadFriendListFromAPI());
        }
    }

    private void LoadDummyFriendList()
    {
        // 더미 친구 데이터 생성
        friendList = new List<FriendItem>
        {
            new FriendItem { username = "user001", nickname = "Alice", planetId = "planet-uuid-001" },
            new FriendItem { username = "user002", nickname = "Bob", planetId = "planet-uuid-002" },
            new FriendItem { username = "user003", nickname = "Charlie", planetId = "planet-uuid-003" },
            new FriendItem { username = "user004", nickname = "David", planetId = "planet-uuid-004" },
            new FriendItem { username = "user005", nickname = "Eve", planetId = "planet-uuid-005" },
            new FriendItem { username = "user006", nickname = "Frank", planetId = "planet-uuid-006" },
            new FriendItem { username = "user007", nickname = "Grace", planetId = "planet-uuid-007" },
            new FriendItem { username = "user008", nickname = "Henry", planetId = "planet-uuid-008" }
        };

        Debug.Log($"더미 친구 목록 로드: {friendList.Count}명");
        RefreshFriendListUI();
    }

    private IEnumerator LoadFriendListFromAPI()
    {
        // TODO: 백엔드 API 준비되면 활성화
        yield return APIManager.Instance.Get(
            "/friends",  // 또는 실제 엔드포인트
            onSuccess: (response) =>
            {
                FriendListResponse friendResponse = JsonUtility.FromJson<FriendListResponse>(response);
                friendList = new List<FriendItem>(friendResponse.result);

                Debug.Log($"친구 목록 로드 성공: {friendList.Count}명");
                RefreshFriendListUI();
            },
            onError: (error) =>
            {
                Debug.LogError("친구 목록 로드 실패: " + error);

                // API 실패 시 더미 데이터 사용
                LoadDummyFriendList();
            }
        );
    }

    private void RefreshFriendListUI()
    {
        // 기존 카드 삭제
        foreach (Transform child in friendListContainer)
        {
            Destroy(child.gameObject);
        }

        // 새 카드 생성
        foreach (var friend in friendList)
        {
            var card = Instantiate(friendCardPrefab, friendListContainer);
            var friendCard = card.GetComponent<FriendCard>();
            friendCard.Init(friend);
        }
    }
    #endregion

    #region UI 관리
    public void UpdateTopRightButtons()
    {
        bool isLoggedIn = UserSession.Instance != null && UserSession.Instance.IsLoggedIn;

        foreach (var go in loginOnlyButtons)
            go.SetActive(isLoggedIn);

        settingsButton.SetActive(true);
    }

    public void OpenPanel(GameObject panel)
    {
        CloseAllPanels();
        if (panel != null)
            panel.SetActive(true);
        else
            Debug.LogWarning("⚠ panel is NULL");
    }

    public void CloseAllPanels()
    {
        settingsPanel.SetActive(false);
        profilePanel.SetActive(false);
    }

    public void Back()
    {
        fadeController.FadeToScene("000_MainMenu");
    }

    public void OnClick_UserInfo()
    {
        OpenPanel(profilePanel);
    }

    public void OnClick_OpenSettings()
    {
        OpenPanel(settingsPanel);
    }

    public void Logout()
    {
        UserSession.Instance.Logout();
        fadeController.FadeToScene("000_MainMenu");
        Debug.Log("로그아웃 완료, 메인 메뉴로 이동");
    }
    #endregion
}