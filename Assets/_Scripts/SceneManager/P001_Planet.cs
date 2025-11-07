using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class P001_Planet : MonoBehaviour
{
    [Header("UI")]
    public Transform planetListContainer;
    public GameObject planetCardPrefab;
    public TMP_InputField searchInput;
    public TMP_Text sortButtonText;

    [Header("TR 버튼 그룹")]
    public GameObject[] loginOnlyButtons;
    public GameObject settingsButton;

    [Header("패널들")]
    public GameObject settingsPanel;
    public GameObject profilePanel;
    public GameObject loadingPanel; // 로딩 패널 추가

    public FadeController fadeController;

    private List<PlanetListItem> allPlanets = new();
    private List<PlanetListItem> favoritePlanets = new();
    private bool isAscending = true; // true: ABC, false: CBA

    private void Awake()
    {
        fadeController = FindObjectOfType<FadeController>();
    }

    private void Start()
    {
        UpdateTopRightButtons();
        UpdateSortButtonText();
        LoadPlanetList();

        // 로딩 패널 초기화
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    private void OnEnable()
    {
        UpdateTopRightButtons();
    }

    #region 행성 목록 로드
    private void LoadPlanetList()
    {
        StartCoroutine(LoadPlanetListCoroutine());
    }

    private IEnumerator LoadPlanetListCoroutine()
    {
        Debug.Log("[P001_Planet] 행성 목록 로드 시작");

        // 전체 행성 목록 가져오기
        yield return APIManager.Instance.Get(
            "/planets",
            onSuccess: (response) =>
            {
                Debug.Log($"[P001_Planet] API 응답: {response}");

                PlanetListResponse planetResponse = JsonUtility.FromJson<PlanetListResponse>(response);

                if (planetResponse != null && planetResponse.result != null)
                {
                    allPlanets = new List<PlanetListItem>(planetResponse.result);
                    Debug.Log($"[P001_Planet] 행성 목록 로드 성공: {allPlanets.Count}개");

                    // 각 행성 정보 출력
                    foreach (var planet in allPlanets)
                    {
                        Debug.Log($"  - id: {planet.id}, username: {planet.username}, title: {planet.title}");
                    }
                }
                else
                {
                    Debug.LogError("[P001_Planet] 행성 목록 파싱 실패");
                    allPlanets = new List<PlanetListItem>();
                }

                // 로그인 상태면 즐겨찾기 목록도 가져오기
                if (UserSession.Instance != null && UserSession.Instance.IsLoggedIn)
                {
                    StartCoroutine(LoadFavoriteListCoroutine());
                }
                else
                {
                    Debug.Log("[P001_Planet] 비로그인 상태 - 즐겨찾기 스킵");
                    RefreshPlanetList();
                }
            },
            onError: (error) =>
            {
                Debug.LogError($"[P001_Planet] 행성 목록 로드 실패: {error}");
                allPlanets = new List<PlanetListItem>();
                RefreshPlanetList();
            }
        );
    }

    private IEnumerator LoadFavoriteListCoroutine()
    {
        Debug.Log("[P001_Planet] 즐겨찾기 목록 로드 시작");

        // 즐겨찾기 목록 가져오기
        yield return APIManager.Instance.Get(
            "/planets/favorites/me",
            onSuccess: (response) =>
            {
                Debug.Log($"[P001_Planet] 즐겨찾기 응답: {response}");

                FavoriteListResponse favoriteResponse = JsonUtility.FromJson<FavoriteListResponse>(response);

                if (favoriteResponse != null && favoriteResponse.result != null)
                {
                    favoritePlanets = new List<PlanetListItem>(favoriteResponse.result);
                    Debug.Log($"[P001_Planet] 즐겨찾기 목록 로드 성공: {favoritePlanets.Count}개");
                }
                else
                {
                    favoritePlanets = new List<PlanetListItem>();
                }

                RefreshPlanetList();
            },
            onError: (error) =>
            {
                Debug.LogError($"[P001_Planet] 즐겨찾기 목록 로드 실패: {error}");
                favoritePlanets = new List<PlanetListItem>();
                RefreshPlanetList();
            }
        );
    }
    #endregion

    #region 행성 카드 표시
    private void RefreshPlanetList()
    {
        Debug.Log($"[P001_Planet] RefreshPlanetList 시작 - 전체: {allPlanets.Count}개");

        // 기존 카드 삭제
        foreach (Transform child in planetListContainer)
        {
            Destroy(child.gameObject);
        }

        if (allPlanets.Count == 0)
        {
            Debug.LogWarning("[P001_Planet] 표시할 행성이 없습니다");
            return;
        }

        // Guest_ 필터링 + 검색어 필터링
        string searchKeyword = searchInput != null ? searchInput.text.ToLower() : "";
        List<PlanetListItem> filteredPlanets = allPlanets.FindAll(p =>
        {
            // Guest_ 제외
            if (!string.IsNullOrEmpty(p.username) && p.username.StartsWith("guest_"))
            {
                return false;
            }

            // 검색어 필터링
            if (string.IsNullOrEmpty(searchKeyword))
            {
                return true;
            }

            return p.username.ToLower().Contains(searchKeyword) ||
                   p.title.ToLower().Contains(searchKeyword);
        });

        Debug.Log($"[P001_Planet] Guest 필터링 + 검색 후: {filteredPlanets.Count}개 (검색어: '{searchKeyword}')");

        // 정렬: ABC(오름차순) / CBA(내림차순)
        if (isAscending)
        {
            // ABC: 오름차순
            filteredPlanets.Sort((a, b) => string.Compare(a.username, b.username, System.StringComparison.Ordinal));
            Debug.Log("[P001_Planet] 정렬: ABC (오름차순)");
        }
        else
        {
            // CBA: 내림차순
            filteredPlanets.Sort((a, b) => string.Compare(b.username, a.username, System.StringComparison.Ordinal));
            Debug.Log("[P001_Planet] 정렬: CBA (내림차순)");
        }

        // 카드 생성
        Debug.Log($"[P001_Planet] 카드 생성 시작");
        foreach (var planet in filteredPlanets)
        {
            GameObject cardObj = Instantiate(planetCardPrefab, planetListContainer);
            PlanetCard card = cardObj.GetComponent<PlanetCard>();

            if (card != null)
            {
                bool isFavorite = favoritePlanets.Exists(f => f.username == planet.username);
                card.Init(planet, isFavorite, this);
                Debug.Log($"[P001_Planet] 카드 생성 완료: {planet.username}");
            }
            else
            {
                Debug.LogError("[P001_Planet] PlanetCard 컴포넌트를 찾을 수 없습니다!");
            }
        }

        Debug.Log($"[P001_Planet] RefreshPlanetList 완료 - {filteredPlanets.Count}개 카드 생성");
    }

    public void OnSearchInputChanged()
    {
        RefreshPlanetList();
    }
    #endregion

    #region 정렬
    public void ToggleSort()
    {
        isAscending = !isAscending;
        UpdateSortButtonText();
        RefreshPlanetList();

        Debug.Log($"[P001_Planet] 정렬 토글: {(isAscending ? "ABC" : "CBA")}");
    }

    private void UpdateSortButtonText()
    {
        if (sortButtonText != null)
        {
            sortButtonText.text = isAscending ? "ABC" : "CBA";
        }
    }
    #endregion

    #region 즐겨찾기
    public void ToggleFavorite(string ownerUsername, bool currentState)
    {
        if (UserSession.Instance == null || !UserSession.Instance.IsLoggedIn)
        {
            Debug.LogWarning("로그인이 필요합니다.");
            return;
        }

        StartCoroutine(ToggleFavoriteCoroutine(ownerUsername, currentState));
    }

    private IEnumerator ToggleFavoriteCoroutine(string ownerUsername, bool currentState)
    {
        if (currentState)
        {
            // 즐겨찾기 해제
            yield return APIManager.Instance.Delete(
                $"/planets/{ownerUsername}/favorite",
                onSuccess: (response) =>
                {
                    Debug.Log("즐겨찾기 해제 성공");
                    favoritePlanets.RemoveAll(p => p.username == ownerUsername);
                    RefreshPlanetList();
                },
                onError: (error) =>
                {
                    Debug.LogError("즐겨찾기 해제 실패: " + error);
                }
            );
        }
        else
        {
            // 즐겨찾기 추가
            yield return APIManager.Instance.Post(
                $"/planets/{ownerUsername}/favorite",
                new { },
                onSuccess: (response) =>
                {
                    Debug.Log("즐겨찾기 추가 성공");
                    StartCoroutine(LoadFavoriteListCoroutine());
                },
                onError: (error) =>
                {
                    Debug.LogError("즐겨찾기 추가 실패: " + error);
                }
            );
        }
    }
    #endregion

    #region 로딩 패널
    /// <summary>
    /// 로딩 패널 표시/숨김
    /// </summary>
    public void ShowLoadingPanel(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
            Debug.Log($"[P001_Planet] 로딩 패널: {(show ? "표시" : "숨김")}");
        }
        else
        {
            Debug.LogWarning("[P001_Planet] 로딩 패널이 연결되지 않았습니다!");
        }
    }
    #endregion

    #region UI 버튼 핸들러
    public void Back()
    {
        fadeController.FadeToScene("000_MainMenu");
    }

    public void OnClick_UserInfo()
    {
        OpenPanel(profilePanel);
    }

    public void OnClick_Friend()
    {
        fadeController.FadeToScene("F001_Friend");
    }

    public void Logout()
    {
        UserSession.Instance.Logout();
        fadeController.FadeToScene("000_MainMenu");
        Debug.Log("로그아웃 완료, 메인 메뉴로 이동");
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

    public void UpdateTopRightButtons()
    {
        bool isLoggedIn = UserSession.Instance != null && UserSession.Instance.IsLoggedIn;

        foreach (var go in loginOnlyButtons)
            go.SetActive(isLoggedIn);

        settingsButton.SetActive(true);
    }

    public void OnClick_OpenSettings()
    {
        OpenPanel(settingsPanel);
    }
    #endregion
}