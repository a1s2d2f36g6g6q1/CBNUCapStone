using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUIController : MonoBehaviour
{
    [Header("Fade")]
    public FadeController fadeController;

    [Header("패널들")]
    public GameObject loginPanel;
    public GameObject signupPanel;
    public GameObject settingsPanel;

    [Header("입력 필드 - 로그인")]
    public TMP_InputField loginIdField;
    public TMP_InputField loginPwField;

    [Header("입력 필드 - 회원가입")]
    public TMP_InputField signupIdField;
    public TMP_InputField signupPw1Field;
    public TMP_InputField signupPw2Field;
    public TMP_InputField signupNicknameField;

    [Header("중복확인 관련")]
    public TMP_Text idCheckResultText;

    [Header("TR 버튼 그룹")]
    public GameObject[] loginOnlyButtons;    // 친구, 유저정보 버튼 (로그인 후)
    public GameObject[] guestOnlyButtons;    // 로그인, 회원가입 버튼 (비로그인 상태)
    public GameObject settingsButton;        // 항상 보이는 설정 버튼

    private bool isLoggedIn = false;

    void Start()
    {
        UpdateTopRightButtons();
    }

    // ========================
    // Static 버튼 동작
    // ========================
    public void OnClick_SinglePlay()
    {
        fadeController.FadeToScene("G001_TagInput");
    }

    public void OnClick_CreateParty()
    {
        fadeController.FadeToScene("B001_CreateParty");
    }

    public void OnClick_JoinParty()
    {
        fadeController.FadeToScene("B002_JoinParty");
    }

    public void OnClick_MyPlanet()
    {
        fadeController.FadeToScene("P001_MyPlanet");
    }

    public void OnClick_PlanetTravel()
    {
        fadeController.FadeToScene("P002_PlanetTravel");
    }

    public void OnClick_Exit()
    {
        Application.Quit();
        Debug.Log("게임 종료!");
    }

    // ========================
    // TR 버튼 관련
    // ========================

    public void OnClick_OpenLogin()
    {
        OpenPanel(loginPanel);
    }
    public void OnClick_OpenSignup()
    {
        OpenPanel(signupPanel);
    }
    public void UpdateTopRightButtons()
    {
        foreach (GameObject go in guestOnlyButtons)
            go.SetActive(!isLoggedIn);

        foreach (GameObject go in loginOnlyButtons)
            go.SetActive(isLoggedIn);

        settingsButton.SetActive(true);
    }

    public void OpenPanel(GameObject panel)
    {
        CloseAllPanels();
        panel.SetActive(true);
    }

    public void CloseAllPanels()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void Logout()
    {
        isLoggedIn = false;
        UpdateTopRightButtons();
        CloseAllPanels();
        Debug.Log("로그아웃 완료!");
    }

    // ========================
    // 로그인 / 회원가입
    // ========================
    public void TryLogin()
    {
        string id = loginIdField.text;
        string pw = loginPwField.text;

        if (id == "admin" && pw == "1234")
        {
            isLoggedIn = true;
            UpdateTopRightButtons();
            CloseAllPanels();
            Debug.Log("로그인 성공!");
        }
        else
        {
            Debug.Log("로그인 실패");
        }
    }

    public void TrySignup()
    {
        string id = signupIdField.text;
        string pw1 = signupPw1Field.text;
        string pw2 = signupPw2Field.text;
        string nickname = signupNicknameField.text;

        if (pw1 != pw2)
        {
            Debug.Log("비밀번호 불일치");
            return;
        }

        Debug.Log($"회원가입 요청: {id}, {nickname}");
        signupPanel.SetActive(false);
    }

    public void CheckDuplicateID()
    {
        string id = signupIdField.text;

        if (id == "test123")
        {
            idCheckResultText.text = "❌ 이미 사용 중입니다.";
        }
        else
        {
            idCheckResultText.text = "✅ 사용 가능합니다!";
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllPanels();
        }
    }
}