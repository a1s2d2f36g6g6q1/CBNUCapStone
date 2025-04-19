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

    private bool isLoggedIn = false;

    // static 고정 버튼
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

    // TR 우측 상단 버튼

    public void OnClick_Settings()
    {
        if (settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
        }
        else
        {
            settingsPanel.SetActive(true);
        }
    }
    public void OnClick_Friends()
    {
        fadeController.FadeToScene("F001_Friend");
    }




    // ========= 패널 열고 닫기 =========
    public void OpenLoginPanel() => loginPanel.SetActive(true);
    public void OpenSignupPanel() => signupPanel.SetActive(true);
    public void OpenSettingsPanel() => settingsPanel.SetActive(true);
    public void ClosePanel(GameObject panel) => panel.SetActive(false);

    // ========= 로그인 / 회원가입 =========
    public void TryLogin()
    {
        string id = loginIdField.text;
        string pw = loginPwField.text;

        // 나중에 서버 인증으로 대체
        if (id == "admin" && pw == "1234")
        {
            isLoggedIn = true;
            Debug.Log("로그인 성공!");
            loginPanel.SetActive(false);
            // 로그인 상태 반영 추가로 구현 가능
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

    // ========= 아이디 중복 확인 (임시 더미) =========
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
}