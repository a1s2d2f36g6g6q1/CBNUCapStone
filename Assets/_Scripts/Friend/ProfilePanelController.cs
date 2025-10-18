using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ProfilePanelController : MonoBehaviour
{
    public TMP_Text idText;
    public TMP_InputField nicknameField;
    public TMP_InputField passwordField;
    public TMP_InputField passwordConfirmField;

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
        idText.text = $"ID : {UserSession.Instance?.UserID ?? "N/A"}";
        nicknameField.text = UserSession.Instance?.Nickname ?? "N/A";
        passwordField.text = "";
        passwordConfirmField.text = "";

        ResetNicknameState();
        ResetPasswordState();
    }

    private void Start()
    {
        changeNicknameButton.onClick.AddListener(ChangeNickname);
        changePasswordButton.onClick.AddListener(ChangePassword);
        closeButtonX.onClick.AddListener(ClosePanel);
        closeButtonConfirm.onClick.AddListener(ClosePanel);
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

    private void ChangeNickname()
    {
        var newNickname = nicknameField.text;
        if (!string.IsNullOrEmpty(newNickname))
        {
            UserSession.Instance.UpdateNickname(newNickname);
            Debug.Log($"닉네임 변경 완료: {newNickname}");
            nicknameState_Default.SetActive(false);
            nicknameState_Checked.SetActive(true);
            nicknameState_Error.SetActive(false);
        }
        else
        {
            Debug.LogWarning("닉네임 입력이 비어있음");
            nicknameState_Default.SetActive(false);
            nicknameState_Checked.SetActive(false);
            nicknameState_Error.SetActive(true);
        }
    }

    private void ChangePassword()
    {
        if (passwordField.text == passwordConfirmField.text && !string.IsNullOrEmpty(passwordField.text))
        {
            Debug.Log($"비밀번호 변경 완료: {passwordField.text}");
            passwordState_Default.SetActive(false);
            passwordState_Checked.SetActive(true);
            passwordState_Error.SetActive(false);
        }
        else
        {
            Debug.LogWarning("비밀번호 불일치");
            passwordState_Default.SetActive(false);
            passwordState_Checked.SetActive(false);
            passwordState_Error.SetActive(true);
        }
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}