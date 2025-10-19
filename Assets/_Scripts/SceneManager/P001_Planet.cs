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

    public FadeController fadeController;

    private List<PlanetListItem> allPlanets = new();
    private List<PlanetListItem> favoritePlanets = new();
    private bool isRecentSort = true;

    private void Awake()
    {
        fadeController = FindObjectOfType<FadeController>();
    }

    private void Start()
    {
        UpdateTopRightButtons();
        UpdateSortButtonText();
        LoadPlanetList();
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
        // 1. 전체 행성 목록 가져오기
        yield return APIManager.Instance.Get(
            "/planets",
            onSuccess: (response) =>
            {
                PlanetListResponse planetResponse = JsonUtility.FromJson<PlanetListResponse>(response);
                allPlanets = new List<PlanetListItem>(planetResponse.result);
                Debug.Log($"행성 목록 로드 성공: {allPlanets.Count}개");

                // 2. 로그인 상태면 즐겨찾기 목록도 가져오기
                if (UserSession.Instance.IsLoggedIn)
                {
                    StartCoroutine(LoadFavoriteListCoroutine());
                }
                else
                {
                    RefreshPlanetList();
                }
            },
            onError: (error) =>
            {
                Debug.LogError("행성 목록 로드 실패: " + error);
            }
        );
    }

    private IEnumerator LoadFavoriteListCoroutine()
    {
        yield return APIManager.Instance.Get(
            "/planets/favorites/me",
            onSuccess: (response) =>
            {
                FavoriteListResponse favoriteResponse = JsonUtility.FromJson<FavoriteListResponse>(response);
                favoritePlanets = new List<PlanetListItem>(favoriteResponse.result);
                Debug.Log($"즐겨찾기 목록 로드 성공: {favoritePlanets.Count}개");

                RefreshPlanetList();
            },
            onError: (error) =>
            {
                Debug.LogError("즐겨찾기 목록 로드 실패: " + error);
                RefreshPlanetList();
            }
        );
    }

    private void RefreshPlanetList()
    {
        // 기존 카드 삭제
        foreach (Transform child in planetListContainer)
        {
            Destroy(child.gameObject);
        }

        // 정렬된 리스트 생성
        List<PlanetListItem> sortedList = new List<PlanetListItem>(allPlanets);

        if (isRecentSort)
        {
            // 최신순 정렬 (planetId 기준)
            sortedList.Sort((a, b) => string.Compare(b.planetId, a.planetId));
        }
        else
        {
            // 추천순 정렬 (방문자 수 기준)
            sortedList.Sort((a, b) => b.visitCount.CompareTo(a.visitCount));
        }

        // 즐겨찾기를 맨 앞으로
        List<PlanetListItem> favoriteFirst = new();
        List<PlanetListItem> others = new();

        foreach (var planet in sortedList)
        {
            if (IsFavorite(planet.planetId))
                favoriteFirst.Add(planet);
            else
                others.Add(planet);
        }

        favoriteFirst.AddRange(others);

        // 카드 생성
        foreach (var planet in favoriteFirst)
        {
            var card = Instantiate(planetCardPrefab, planetListContainer);
            var planetCard = card.GetComponent<PlanetCard>();
            bool isFav = IsFavorite(planet.planetId);
            planetCard.Init(planet, isFav, this);
        }
    }

    private bool IsFavorite(string planetId)
    {
        return favoritePlanets.Exists(p => p.planetId == planetId);
    }
    #endregion

    #region 정렬/검색
    public void ToggleSort()
    {
        isRecentSort = !isRecentSort;
        UpdateSortButtonText();
        RefreshPlanetList();
    }

    private void UpdateSortButtonText()
    {
        sortButtonText.text = isRecentSort ? "Sort : Recent" : "Sort : Recommended";
    }

    public void Search()
    {
        string query = searchInput.text.Trim().ToLower();

        if (string.IsNullOrEmpty(query))
        {
            RefreshPlanetList();
            return;
        }

        // 기존 카드 삭제
        foreach (Transform child in planetListContainer)
        {
            Destroy(child.gameObject);
        }

        // 검색 필터링
        var filtered = allPlanets.FindAll(p =>
            p.ownerNickname.ToLower().Contains(query) ||
            p.ownerUsername.ToLower().Contains(query)
        );

        // 카드 생성
        foreach (var planet in filtered)
        {
            var card = Instantiate(planetCardPrefab, planetListContainer);
            var planetCard = card.GetComponent<PlanetCard>();
            bool isFav = IsFavorite(planet.planetId);
            planetCard.Init(planet, isFav, this);
        }
    }
    #endregion

    #region 즐겨찾기 토글 (PlanetCard에서 호출)
    public void ToggleFavorite(string planetId, bool currentState)
    {
        if (!UserSession.Instance.IsLoggedIn)
        {
            Debug.LogWarning("로그인이 필요합니다.");
            return;
        }

        StartCoroutine(ToggleFavoriteCoroutine(planetId, currentState));
    }

    private IEnumerator ToggleFavoriteCoroutine(string planetId, bool currentState)
    {
        if (currentState)
        {
            // 즐겨찾기 해제
            yield return APIManager.Instance.Delete(
                $"/planets/{planetId}/favorite",
                onSuccess: (response) =>
                {
                    Debug.Log("즐겨찾기 해제 성공");
                    favoritePlanets.RemoveAll(p => p.planetId == planetId);
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
                $"/planets/{planetId}/favorite",
                new { },
                onSuccess: (response) =>
                {
                    Debug.Log("즐겨찾기 추가 성공");
                    LoadFavoriteListCoroutine();
                },
                onError: (error) =>
                {
                    Debug.LogError("즐겨찾기 추가 실패: " + error);
                }
            );
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