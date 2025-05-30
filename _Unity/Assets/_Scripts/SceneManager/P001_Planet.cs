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

    private List<string> allPlanets = new();
    private bool isRecentSort = true;

    private void Awake()
    {
        fadeController = FindObjectOfType<FadeController>();
    }

    private void Start()
    {
        LoadPlanetList();
        UpdateSortButtonText();
        UpdateTopRightButtons();
    }

    private void OnEnable()
    {
        UpdateTopRightButtons();
    }

    private void LoadPlanetList()
    {
        allPlanets.Clear();
        allPlanets.AddRange(new[] { "User A", "User B", "User C", "Test 1", "Test 2", "Test 3", "Test 4", "Test 5" });
        RefreshPlanetList(allPlanets);
    }

    private void RefreshPlanetList(List<string> data)
    {
        foreach (Transform child in planetListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var name in data)
        {
            var card = Instantiate(planetCardPrefab, planetListContainer);
            card.GetComponent<PlanetCard>().Init(name);
        }
    }

    public void ToggleSort()
    {
        isRecentSort = !isRecentSort;
        UpdateSortButtonText();
        SortAndRefresh();
    }

    private void UpdateSortButtonText()
    {
        sortButtonText.text = isRecentSort ? "Sort : Recent" : "Sort : Recommended";
    }

    public void SortAndRefresh()
    {
        var cards = planetListContainer.GetComponentsInChildren<PlanetCard>(true);
        var sorted = new List<PlanetCard>(cards);

        sorted.Sort((a, b) =>
        {
            if (a.IsBookmarked && !b.IsBookmarked) return -1;
            if (!a.IsBookmarked && b.IsBookmarked) return 1;

            if (isRecentSort)
                return string.Compare(a.PlanetOwner, b.PlanetOwner);
            else
                return string.Compare(b.PlanetOwner, a.PlanetOwner);
        });

        foreach (var card in sorted)
        {
            card.transform.SetAsLastSibling();
        }
    }

    public void Search()
    {
        string query = searchInput.text.Trim().ToLower();
        var filtered = allPlanets.FindAll(p => p.ToLower().Contains(query));
        RefreshPlanetList(filtered);
    }

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
}