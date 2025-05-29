using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

public class MainMenuUIController : MonoBehaviour
{
    [Header("Fade")] public FadeController fadeController;

    [Header("패널들")] public GameObject loginPanel;
    public GameObject signupPanel;
    public GameObject settingsPanel;
    public GameObject profilePanel;

    [Header("입력 필드 - 로그인")] public TMP_InputField loginIdField;
    public TMP_InputField loginPwField;

    [Header("입력 필드 - 회원가입")] public TMP_InputField signupIdField;
    public TMP_InputField signupPw1Field;
    public TMP_InputField signupPw2Field;
    public TMP_InputField signupNicknameField;

    [Header("중복확인 관련")] public TMP_Text idCheckResultText;

    [Header("ID 체크 상태 텍스트들")] public GameObject idCheck_Default; // '_'
    public GameObject idCheck_Checked; // '✔'
    public GameObject idCheck_Duplicated; // 'X'

    [Header("회원가입 버튼")] public Button signupButton;

    [Header("TR 버튼 그룹")] public GameObject[] loginOnlyButtons; // 친구, 유저정보 버튼 (로그인 후) + 로그아웃버튼
    public GameObject[] guestOnlyButtons; // 로그인, 회원가입 버튼 (비로그인 상태)
    public GameObject settingsButton; // 항상 보이는 설정 버튼

    private void Start()
    {
        UpdateTopRightButtons();

        signupIdField.onValueChanged.AddListener(OnIDInputChanged);
        SetIDCheckState_Default();
        UpdateSignupButtonInteractable();
    }

    private void OnEnable()
    {
        UpdateTopRightButtons(); // 씬 돌아올 때 로그인 상태 갱신
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) CloseAllPanels();
    }

    // ========================
    // ID 중복 검사
    // ========================
    private void OnIDInputChanged(string newText)
    {
        SetIDCheckState_Default();
        UpdateSignupButtonInteractable();
    }

    public void CheckDuplicateID()
    {
        var id = signupIdField.text;

        if (id == "test123")
            SetIDCheckState_Duplicated();
        else
            SetIDCheckState_Checked();

        UpdateSignupButtonInteractable();
    }

    private void SetIDCheckState_Default()
    {
        idCheck_Default.SetActive(true);
        idCheck_Checked.SetActive(false);
        idCheck_Duplicated.SetActive(false);
    }

    private void SetIDCheckState_Checked()
    {
        idCheck_Default.SetActive(false);
        idCheck_Checked.SetActive(true);
        idCheck_Duplicated.SetActive(false);
        idCheckResultText.text = "ID available";
    }

    private void SetIDCheckState_Duplicated()
    {
        idCheck_Default.SetActive(false);
        idCheck_Checked.SetActive(false);
        idCheck_Duplicated.SetActive(true);
        idCheckResultText.text = "ID already in use";
    }

    private void UpdateSignupButtonInteractable()
    {
        signupButton.interactable = idCheck_Checked.activeSelf;
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
    public void OnClick_Friend()
    {
        fadeController.FadeToScene("F001_Friend");
    }

    public void OnClick_UserInfo()
    {
        OpenPanel(profilePanel);
    }

    public void OnClick_OpenLogin()
    {
        OpenPanel(loginPanel);
    }

    public void OnClick_OpenSignup()
    {
        OpenPanel(signupPanel);
    }

    public void OnClick_OpenSettings()
    {
        OpenPanel(settingsPanel);
    }

    private bool isLoggedIn = UserSession.Instance != null && UserSession.Instance.IsLoggedIn;
    public void UpdateTopRightButtons()
    {
        foreach (var go in guestOnlyButtons)
            go.SetActive(!isLoggedIn);

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
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        settingsPanel.SetActive(false);
        profilePanel.SetActive(false);
    }

    public void Logout()
    {
        UserSession.Instance.Logout();
        UpdateTopRightButtons();
        CloseAllPanels();
        Debug.Log("로그아웃 완료!");
    }

    // ========================
    // 로그인
    // ========================
    public void TryLogin()
    {
        StartCoroutine(SendLoginRequest());
    }

    private IEnumerator SendLoginRequest()
    {
        string id = loginIdField.text;
        string password = loginPwField.text;

        string json = $"{{\"id\":\"{id}\",\"password\":\"{password}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest("http://localhost:3000/api/login", "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("로그인 성공: " + request.downloadHandler.text);
            isLoggedIn = true;
            UpdateTopRightButtons();
            CloseAllPanels();
        }
        else
        {
            Debug.Log("❌ 로그인 실패: " + request.error);
            Debug.Log("서버 응답: " + request.downloadHandler.text);
        }
    }

    // ========================
    // 회원가입
    // ========================
    public void TrySignup()
    {
        StartCoroutine(SendSignupRequest());
    }

    private IEnumerator SendSignupRequest()
    {
        string id = signupIdField.text;
        string pw = signupPw1Field.text;
        string pw2 = signupPw2Field.text;
        string nickname = signupNicknameField.text;

        if (pw != pw2)
        {
            Debug.LogWarning("비밀번호 불일치");
            yield break;
        }

        string json = $"{{\"user_id\":\"{id}\",\"password\":\"{pw}\",\"nickname\":\"{nickname}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest("http://localhost:3000/api/auth/signup", "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("회원가입 성공: " + request.downloadHandler.text);
            CloseAllPanels();
        }
        else
        {
            Debug.Log("회원가입 실패: " + request.error);
            Debug.Log("서버 응답: " + request.downloadHandler.text);
        }
    }
}