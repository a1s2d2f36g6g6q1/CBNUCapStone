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
    private bool isABCSort = true; // ABC순(true) / CBA순(false)

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
        // 전체 행성 목록 가져오기
        yield return APIManager.Instance.Get(
            "/planets",
            onSuccess: (response) =>
            {
                Debug.Log($"[API 응답] {response}");

                PlanetListResponse planetResponse = JsonUtility.FromJson<PlanetListResponse>(response);

                if (planetResponse != null && planetResponse.result != null)
                {
                    allPlanets = new List<PlanetListItem>(planetResponse.result);
                    Debug.Log($"행성 목록 로드 성공: {allPlanets.Count}개");

                    // 로그인 상태면 즐겨찾기 목록도 가져오기
                    if (UserSession.Instance != null && UserSession.Instance.IsLoggedIn)
                    {
                        StartCoroutine(LoadFavoriteListCoroutine());
                    }
                    else
                    {
                        RefreshPlanetList();
                    }
                }
                else
                {
                    Debug.LogError("행성 목록 파싱 실패");
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
                Debug.Log($"[즐겨찾기 API 응답] {response}");

                // API 응답 구조에 맞게 파싱
                var wrapper = JsonUtility.FromJson<FavoriteListWrapper>(response);

                if (wrapper != null && wrapper.result != null && wrapper.result.favorites != null)
                {
                    favoritePlanets = new List<PlanetListItem>(wrapper.result.favorites);
                    Debug.Log($"즐겨찾기 목록 로드 성공: {favoritePlanets.Count}개");
                }
                else
                {
                    Debug.LogWarning("즐겨찾기 목록이 비어있거나 파싱 실패");
                    favoritePlanets = new List<PlanetListItem>();
                }

                RefreshPlanetList();
            },
            onError: (error) =>
            {
                Debug.LogWarning("즐겨찾기 목록 로드 실패: " + error);
                favoritePlanets = new List<PlanetListItem>();
                RefreshPlanetList();
            }
        );
    }

    // 즐겨찾기 목록을 위한 래퍼 클래스
    [System.Serializable]
    private class FavoriteListWrapper
    {
        public bool isSuccess;
        public int code;
        public string message;
        public FavoriteResult result;
    }

    [System.Serializable]
    private class FavoriteResult
    {
        public PlanetListItem[] favorites;
    }

    private void RefreshPlanetList()
    {
        // 기존 카드 삭제
        foreach (Transform child in planetListContainer)
        {
            Destroy(child.gameObject);
        }

        List<PlanetListItem> sortedList = new List<PlanetListItem>(allPlanets);

        // ABC순 / CBA순 정렬 (title 기준)
        if (isABCSort)
        {
            // ABC순 (오름차순) - title 우선, 없으면 username
            sortedList.Sort((a, b) =>
            {
                string nameA = string.IsNullOrEmpty(a.title) ? a.ownerUsername : a.title;
                string nameB = string.IsNullOrEmpty(b.title) ? b.ownerUsername : b.title;
                return string.Compare(nameA, nameB, System.StringComparison.OrdinalIgnoreCase);
            });
            Debug.Log("[Planet] ABC순 정렬 (title 기준)");
        }
        else
        {
            // CBA순 (내림차순)
            sortedList.Sort((a, b) =>
            {
                string nameA = string.IsNullOrEmpty(a.title) ? a.ownerUsername : a.title;
                string nameB = string.IsNullOrEmpty(b.title) ? b.ownerUsername : b.title;
                return string.Compare(nameB, nameA, System.StringComparison.OrdinalIgnoreCase);
            });
            Debug.Log("[Planet] CBA순 정렬 (title 기준)");
        }

        // 즐겨찾기를 맨 앞으로
        List<PlanetListItem> favoriteFirst = new();
        List<PlanetListItem> others = new();

        foreach (var planet in sortedList)
        {
            if (IsFavorite(planet.ownerUsername))
                favoriteFirst.Add(planet);
            else
                others.Add(planet);
        }

        favoriteFirst.AddRange(others);

        Debug.Log($"[Planet] 총 {favoriteFirst.Count}개 행성 표시 (즐겨찾기: {favoriteFirst.Count - others.Count}개, 일반: {others.Count}개)");

        // 카드 생성
        foreach (var planet in favoriteFirst)
        {
            var card = Instantiate(planetCardPrefab, planetListContainer);
            var planetCard = card.GetComponent<PlanetCard>();
            bool isFav = IsFavorite(planet.ownerUsername);
            planetCard.Init(planet, isFav, this);
        }
    }

    private bool IsFavorite(string ownerUsername)
    {
        return favoritePlanets.Exists(p => p.ownerUsername == ownerUsername);
    }
    #endregion

    #region 정렬/검색
    public void ToggleSort()
    {
        isABCSort = !isABCSort;
        UpdateSortButtonText();
        RefreshPlanetList();
    }

    private void UpdateSortButtonText()
    {
        sortButtonText.text = isABCSort ? "Sort : ABC" : "Sort : CBA";
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

        // 검색 필터링 (title 또는 username에서 검색)
        var filtered = allPlanets.FindAll(p =>
        {
            string title = string.IsNullOrEmpty(p.title) ? "" : p.title.ToLower();
            string username = p.ownerUsername.ToLower();
            return title.Contains(query) || username.Contains(query);
        });

        Debug.Log($"[Planet] 검색 결과: '{query}' - {filtered.Count}개");

        // 정렬 적용
        if (isABCSort)
        {
            filtered.Sort((a, b) =>
            {
                string nameA = string.IsNullOrEmpty(a.title) ? a.ownerUsername : a.title;
                string nameB = string.IsNullOrEmpty(b.title) ? b.ownerUsername : b.title;
                return string.Compare(nameA, nameB, System.StringComparison.OrdinalIgnoreCase);
            });
        }
        else
        {
            filtered.Sort((a, b) =>
            {
                string nameA = string.IsNullOrEmpty(a.title) ? a.ownerUsername : a.title;
                string nameB = string.IsNullOrEmpty(b.title) ? b.ownerUsername : b.title;
                return string.Compare(nameB, nameA, System.StringComparison.OrdinalIgnoreCase);
            });
        }

        // 카드 생성
        foreach (var planet in filtered)
        {
            var card = Instantiate(planetCardPrefab, planetListContainer);
            var planetCard = card.GetComponent<PlanetCard>();
            bool isFav = IsFavorite(planet.ownerUsername);
            planetCard.Init(planet, isFav, this);
        }
    }
    #endregion

    #region 즐겨찾기 토글 (PlanetCard에서 호출)
    public void ToggleFavorite(string ownerUsername, bool currentState)
    {
        if (UserSession.Instance == null || !UserSession.Instance.IsLoggedIn)
        {
            Debug.LogWarning("로그인이 필요합니다.");
            return;
        }

        if (currentState)
        {
            // 즐겨찾기 삭제
            StartCoroutine(RemoveFavoriteCoroutine(ownerUsername));
        }
        else
        {
            // 즐겨찾기 추가
            StartCoroutine(AddFavoriteCoroutine(ownerUsername));
        }
    }

    private IEnumerator AddFavoriteCoroutine(string ownerUsername)
    {
        yield return APIManager.Instance.Post(
            $"/planets/{ownerUsername}/favorite",
            new { },
            onSuccess: (response) =>
            {
                Debug.Log($"{ownerUsername} 즐겨찾기 추가 성공");
                StartCoroutine(LoadFavoriteListCoroutine());
            },
            onError: (error) =>
            {
                Debug.LogError($"즐겨찾기 추가 실패: {error}");
            }
        );
    }

    private IEnumerator RemoveFavoriteCoroutine(string ownerUsername)
    {
        yield return APIManager.Instance.Delete(
            $"/planets/{ownerUsername}/favorite",
            onSuccess: (response) =>
            {
                Debug.Log($"{ownerUsername} 즐겨찾기 삭제 성공");
                StartCoroutine(LoadFavoriteListCoroutine());
            },
            onError: (error) =>
            {
                Debug.LogError($"즐겨찾기 삭제 실패: {error}");
            }
        );
    }
    #endregion

    #region 패널 제어
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        profilePanel.SetActive(false);
    }

    public void OpenProfile()
    {
        profilePanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void CloseAllPanels()
    {
        settingsPanel.SetActive(false);
        profilePanel.SetActive(false);
    }
    #endregion

    #region UI 버튼 핸들러
    public void OnClick_Back()
    {
        fadeController.FadeToScene("000_MainMenu");
    }

    public void OnClick_Friend()
    {
        fadeController.FadeToScene("F001_Friend");
    }

    public void OnClick_Logout()
    {
        UserSession.Instance.Logout();
        fadeController.FadeToScene("000_MainMenu");
        Debug.Log("로그아웃 완료");
    }

    private void UpdateTopRightButtons()
    {
        bool isLoggedIn = UserSession.Instance != null && UserSession.Instance.IsLoggedIn;

        foreach (var go in loginOnlyButtons)
            go.SetActive(isLoggedIn);

        settingsButton.gameObject.SetActive(true);
    }
    #endregion
}