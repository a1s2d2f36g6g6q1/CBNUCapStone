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

    private void Start()
    {
        LoadFriendList();
        UpdateTopRightButtons();
    }

    private void OnEnable()
    {
        UpdateTopRightButtons();
    }

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) CloseAllPanels();
    }

    private void LoadFriendList()
    {
        List<string> dummyNames = new();

        for (int i = 1; i <= 30; i++)
        {
            dummyNames.Add($"Test Friend {i}");
        }

        foreach (var name in dummyNames)
        {
            var card = Instantiate(friendCardPrefab, friendListContainer);
            var friendCard = card.GetComponent<FriendCard>();
            friendCard.Init(name);
        }
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



}