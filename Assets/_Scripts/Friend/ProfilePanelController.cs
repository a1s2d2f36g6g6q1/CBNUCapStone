using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilePanelController : MonoBehaviour
{
    [Header("Profile UI")]
    public TMP_Text idText;
    public TMP_InputField nicknameField;

    [Header("Legacy Password UI (Optional)")]
    public TMP_InputField passwordField;
    public TMP_InputField passwordConfirmField;

    [Header("New Password UI (Preferred)")]
    public TMP_InputField oldPasswordField;
    public TMP_InputField newPasswordField;

    [Header("Buttons (Optional)")]
    public Button changeNicknameButton;
    public Button changePasswordButton;

    [Header("Nickname Feedback Texts")]
    public GameObject nicknameFeedback_Default;
    public GameObject nicknameFeedback_Checked;
    public GameObject nicknameFeedback_Error;

    [Header("Password Feedback Texts")]
    public GameObject passwordFeedback_Default;
    public GameObject passwordFeedback_Checked;
    public GameObject passwordFeedback_Error;

    private void OnEnable()
    {
        TryWireEventsSafely();
        SafeLoadUserProfile();
        ResetAllStates();
    }

    private void TryWireEventsSafely()
    {
        if (changeNicknameButton != null)
        {
            changeNicknameButton.onClick.RemoveListener(ChangeNickname);
            changeNicknameButton.onClick.AddListener(ChangeNickname);
        }

        if (changePasswordButton != null)
        {
            changePasswordButton.onClick.RemoveListener(ChangePassword);
            changePasswordButton.onClick.AddListener(ChangePassword);
        }
    }

    private void SafeLoadUserProfile()
    {
        string uid = (UserSession.Instance != null) ? (UserSession.Instance.UserID ?? "N/A") : "N/A";
        string nick = (UserSession.Instance != null) ? (UserSession.Instance.Nickname ?? "") : "";

        if (idText != null) idText.text = $"ID : {uid}";
        if (nicknameField != null) nicknameField.text = nick;

        if (passwordField != null) passwordField.text = "";
        if (passwordConfirmField != null) passwordConfirmField.text = "";
        if (oldPasswordField != null) oldPasswordField.text = "";
        if (newPasswordField != null) newPasswordField.text = "";
    }

    private void ResetAllStates()
    {
        SetNicknameFeedbackState("default");
        SetPasswordFeedbackState("default");
    }

    #region === Nickname ===
    public void ChangeNickname()
    {
        if (nicknameField == null)
        {
            Debug.LogWarning("Nickname input field is not assigned.");
            SetNicknameFeedbackState("error");
            return;
        }

        string newNickname = nicknameField.text?.Trim() ?? "";
        if (string.IsNullOrEmpty(newNickname))
        {
            Debug.LogWarning("Please enter a nickname.");
            SetNicknameFeedbackState("error");
            return;
        }

        if (UserSession.Instance != null && newNickname == (UserSession.Instance.Nickname ?? ""))
        {
            Debug.LogWarning("The new nickname is the same as the current one.");
            SetNicknameFeedbackState("error");
            return;
        }

        StartCoroutine(ChangeNicknameCoroutine(newNickname));
    }

    private IEnumerator ChangeNicknameCoroutine(string newNickname)
    {
        if (APIManager.Instance == null)
        {
            Debug.LogError("APIManager is null.");
            SetNicknameFeedbackState("error");
            yield break;
        }

        UpdateNicknameRequest req = new UpdateNicknameRequest { nickname = newNickname };

        yield return APIManager.Instance.Put(
            "/users/nickname",
            req,
            onSuccess: (resp) =>
            {
                Debug.Log($"Nickname updated: {newNickname}");
                if (UserSession.Instance != null)
                {
                    UserSession.Instance.SetUserInfo(UserSession.Instance.UserID, newNickname);
                }
                if (nicknameField != null) nicknameField.text = newNickname;
                SetNicknameFeedbackState("checked");
            },
            onError: (err) =>
            {
                Debug.LogError("Failed to update nickname: " + err);
                SetNicknameFeedbackState("error");
            }
        );
    }

    private void SetNicknameFeedbackState(string state)
    {
        if (nicknameFeedback_Default != null) nicknameFeedback_Default.SetActive(state == "default");
        if (nicknameFeedback_Checked != null) nicknameFeedback_Checked.SetActive(state == "checked");
        if (nicknameFeedback_Error != null) nicknameFeedback_Error.SetActive(state == "error");
    }
    #endregion

    #region === Password ===
    public void ChangePassword()
    {
        string oldPw = (oldPasswordField != null) ? (oldPasswordField.text ?? "") : "";
        string newPw = (newPasswordField != null) ? (newPasswordField.text ?? "") : "";

        bool usingNewUI = (oldPasswordField != null || newPasswordField != null);

        if (usingNewUI)
        {
            if (string.IsNullOrEmpty(oldPw))
            {
                Debug.LogWarning("Please enter your current (old) password.");
                SetPasswordFeedbackState("error");
                return;
            }
            if (string.IsNullOrEmpty(newPw))
            {
                Debug.LogWarning("Please enter a new password.");
                SetPasswordFeedbackState("error");
                return;
            }
        }
        else
        {
            string fallbackNew = (passwordField != null) ? (passwordField.text ?? "") : "";
            string fallbackConfirm = (passwordConfirmField != null) ? (passwordConfirmField.text ?? "") : "";

            if (string.IsNullOrEmpty(fallbackNew))
            {
                Debug.LogWarning("Please enter a new password.");
                SetPasswordFeedbackState("error");
                return;
            }
            if (!string.IsNullOrEmpty(fallbackConfirm) && fallbackNew != fallbackConfirm)
            {
                Debug.LogWarning("New password and confirmation do not match.");
                SetPasswordFeedbackState("error");
                return;
            }

            newPw = fallbackNew;

            if (string.IsNullOrEmpty(oldPw))
            {
                Debug.LogWarning("Current (old) password input is required by API.");
                SetPasswordFeedbackState("error");
                return;
            }
        }

        StartCoroutine(ChangePasswordCoroutine(oldPw, newPw));
    }

    private IEnumerator ChangePasswordCoroutine(string oldPassword, string newPassword)
    {
        if (APIManager.Instance == null)
        {
            Debug.LogError("APIManager is null.");
            SetPasswordFeedbackState("error");
            yield break;
        }

        PasswordUpdateRequest req = new PasswordUpdateRequest
        {
            oldPassword = oldPassword,
            newPassword = newPassword
        };

        yield return APIManager.Instance.Put(
            "/users/password",
            req,
            onSuccess: (resp) =>
            {
                Debug.Log("Password updated successfully.");
                SetPasswordFeedbackState("checked");

                if (oldPasswordField != null) oldPasswordField.text = "";
                if (newPasswordField != null) newPasswordField.text = "";
                if (passwordField != null) passwordField.text = "";
                if (passwordConfirmField != null) passwordConfirmField.text = "";
            },
            onError: (err) =>
            {
                Debug.LogError("Failed to update password: " + err);
                SetPasswordFeedbackState("error");
            }
        );
    }

    private void SetPasswordFeedbackState(string state)
    {
        if (passwordFeedback_Default != null) passwordFeedback_Default.SetActive(state == "default");
        if (passwordFeedback_Checked != null) passwordFeedback_Checked.SetActive(state == "checked");
        if (passwordFeedback_Error != null) passwordFeedback_Error.SetActive(state == "error");
    }
    #endregion
}