using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ProfilePanelController : MonoBehaviour
{
    public TMP_Text idText;
    public TMP_InputField nicknameField;
    public TMP_InputField passwordField;         // 새 비밀번호
    public TMP_InputField passwordConfirmField;  // 새 비밀번호 확인

    public Button changeNicknameButton;
    public Button changePasswordButton;
    public Button closeButtonX;
    public Button closeButtonConfirm;

    [Header("닉네임 상태 표시")]
    public GameObject nicknameState_Default;
    public GameObject nicknameState_Checked;
    public GameObject nicknameState_Error;

    [Header("비밀번호 상태 표시")]
    public GameObject passwordState_Default;
    public GameObject passwordState_Checked;
    public GameObject passwordState_Error;

    private void OnEnable()
    {
        LoadUserProfile();
        ResetAllStates();
    }

    private void Start()
    {
        changeNicknameButton.onClick.AddListener(ChangeNickname);
        changePasswordButton.onClick.AddListener(ChangePassword);
        closeButtonX.onClick.AddListener(ClosePanel);
        closeButtonConfirm.onClick.AddListener(ClosePanel);
    }

    #region 프로필 로드
    private void LoadUserProfile()
    {
        // UserSession에서 기본 정보 표시
        idText.text = $"ID : {UserSession.Instance?.UserID ?? "N/A"}";
        nicknameField.text = UserSession.Instance?.Nickname ?? "";

        // 비밀번호 필드 초기화
        passwordField.text = "";
        passwordConfirmField.text = "";
    }
    #endregion

    #region 닉네임 변경
    public void ChangeNickname()
    {
        string newNickname = nicknameField.text.Trim();

        if (string.IsNullOrEmpty(newNickname))
        {
            Debug.LogWarning("닉네임을 입력해주세요.");
            ShowNicknameError();
            return;
        }

        if (newNickname == UserSession.Instance.Nickname)
        {
            Debug.LogWarning("현재 닉네임과 동일합니다.");
            ShowNicknameError();
            return;
        }

        StartCoroutine(ChangeNicknameCoroutine(newNickname));
    }

    private IEnumerator ChangeNicknameCoroutine(string newNickname)
    {
        NicknameUpdateRequest requestData = new NicknameUpdateRequest
        {
            nickname = newNickname
        };

        yield return APIManager.Instance.Put(
            "/users/nickname",
            requestData,
            onSuccess: (response) =>
            {
                // 세션 업데이트
                UserSession.Instance.UpdateNickname(newNickname);

                Debug.Log($"닉네임 변경 성공: {newNickname}");
                ShowNicknameSuccess();
            },
            onError: (error) =>
            {
                Debug.LogError("닉네임 변경 실패: " + error);
                ShowNicknameError();
            }
        );
    }

    private void ShowNicknameSuccess()
    {
        nicknameState_Default.SetActive(false);
        nicknameState_Checked.SetActive(true);
        nicknameState_Error.SetActive(false);

        // 3초 후 상태 초기화
        StartCoroutine(ResetNicknameStateAfterDelay(3f));
    }

    private void ShowNicknameError()
    {
        nicknameState_Default.SetActive(false);
        nicknameState_Checked.SetActive(false);
        nicknameState_Error.SetActive(true);

        // 3초 후 상태 초기화
        StartCoroutine(ResetNicknameStateAfterDelay(3f));
    }

    private IEnumerator ResetNicknameStateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetNicknameState();
    }
    #endregion

    #region 비밀번호 변경
    public void ChangePassword()
    {
        string newPassword = passwordField.text;
        string confirmPassword = passwordConfirmField.text;

        // 유효성 검사
        if (string.IsNullOrEmpty(newPassword))
        {
            Debug.LogWarning("새 비밀번호를 입력해주세요.");
            ShowPasswordError();
            return;
        }

        if (newPassword != confirmPassword)
        {
            Debug.LogWarning("새 비밀번호가 일치하지 않습니다.");
            ShowPasswordError();
            return;
        }

        StartCoroutine(ChangePasswordCoroutine(newPassword));
    }

    private IEnumerator ChangePasswordCoroutine(string newPassword)
    {
        PasswordUpdateRequest requestData = new PasswordUpdateRequest
        {
            currentPassword = "",  // 백엔드에서 사용 안 함
            newPassword = newPassword
        };

        yield return APIManager.Instance.Put(
            "/users/password",
            requestData,
            onSuccess: (response) =>
            {
                Debug.Log("비밀번호 변경 성공");
                ShowPasswordSuccess();

                // 입력 필드 초기화
                passwordField.text = "";
                passwordConfirmField.text = "";
            },
            onError: (error) =>
            {
                Debug.LogError("비밀번호 변경 실패: " + error);
                ShowPasswordError();
            }
        );
    }

    private void ShowPasswordSuccess()
    {
        passwordState_Default.SetActive(false);
        passwordState_Checked.SetActive(true);
        passwordState_Error.SetActive(false);

        // 3초 후 상태 초기화
        StartCoroutine(ResetPasswordStateAfterDelay(3f));
    }

    private void ShowPasswordError()
    {
        passwordState_Default.SetActive(false);
        passwordState_Checked.SetActive(false);
        passwordState_Error.SetActive(true);

        // 3초 후 상태 초기화
        StartCoroutine(ResetPasswordStateAfterDelay(3f));
    }

    private IEnumerator ResetPasswordStateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetPasswordState();
    }
    #endregion

    #region UI 상태 관리
    private void ResetAllStates()
    {
        ResetNicknameState();
        ResetPasswordState();
    }

    private void ResetNicknameState()
    {
        nicknameState_Default.SetActive(true);
        nicknameState_Checked.SetActive(false);
        nicknameState_Error.SetActive(false);
    }

    private void ResetPasswordState()
    {
        passwordState_Default.SetActive(true);
        passwordState_Checked.SetActive(false);
        passwordState_Error.SetActive(false);
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
    #endregion
}