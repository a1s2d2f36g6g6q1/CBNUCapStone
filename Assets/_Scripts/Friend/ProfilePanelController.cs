using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ProfilePanelController : MonoBehaviour
{
    public TMP_Text idText;
    public TMP_InputField nicknameField;

    [Header("Password Fields")]
    public TMP_InputField currentPasswordField;  // Current password
    public TMP_InputField newPasswordField;      // New password

    public Button changeNicknameButton;
    public Button changePasswordButton;
    public Button closeButtonX;
    public Button closeButtonConfirm;

    [Header("Nickname Status")]
    public GameObject nicknameState_Default;
    public GameObject nicknameState_Checked;
    public GameObject nicknameState_Error;

    [Header("Password Status")]
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

    #region Load Profile
    private void LoadUserProfile()
    {
        idText.text = $"ID : {UserSession.Instance?.UserID ?? "N/A"}";
        nicknameField.text = UserSession.Instance?.Nickname ?? "";

        // Clear password fields
        if (currentPasswordField != null) currentPasswordField.text = "";
        if (newPasswordField != null) newPasswordField.text = "";
    }
    #endregion

    #region Change Nickname
    public void ChangeNickname()
    {
        string newNickname = nicknameField.text.Trim();

        if (string.IsNullOrEmpty(newNickname))
        {
            Debug.LogWarning("[Profile] Nickname is empty");
            ShowNicknameError();
            return;
        }

        if (newNickname == UserSession.Instance.Nickname)
        {
            Debug.LogWarning("[Profile] Same as current nickname");
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
                UserSession.Instance.UpdateNickname(newNickname);
                Debug.Log($"[Profile] Nickname changed successfully: {newNickname}");
                ShowNicknameSuccess();
            },
            onError: (error) =>
            {
                Debug.LogError("[Profile] Nickname change failed: " + error);
                ShowNicknameError();
            }
        );
    }

    private void ShowNicknameSuccess()
    {
        nicknameState_Default.SetActive(false);
        nicknameState_Checked.SetActive(true);
        nicknameState_Error.SetActive(false);
        StartCoroutine(ResetNicknameStateAfterDelay(3f));
    }

    private void ShowNicknameError()
    {
        nicknameState_Default.SetActive(false);
        nicknameState_Checked.SetActive(false);
        nicknameState_Error.SetActive(true);
        StartCoroutine(ResetNicknameStateAfterDelay(3f));
    }

    private IEnumerator ResetNicknameStateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetNicknameState();
    }
    #endregion

    #region Change Password
    public void ChangePassword()
    {
        string currentPassword = currentPasswordField != null ? currentPasswordField.text : "";
        string newPassword = newPasswordField != null ? newPasswordField.text : "";

        // Validation
        if (string.IsNullOrEmpty(currentPassword))
        {
            Debug.LogWarning("[Profile] Current password is empty");
            ShowPasswordError();
            return;
        }

        if (string.IsNullOrEmpty(newPassword))
        {
            Debug.LogWarning("[Profile] New password is empty");
            ShowPasswordError();
            return;
        }

        if (currentPassword == newPassword)
        {
            Debug.LogWarning("[Profile] New password is same as current password");
            ShowPasswordError();
            return;
        }

        StartCoroutine(ChangePasswordCoroutine(currentPassword, newPassword));
    }

    private IEnumerator ChangePasswordCoroutine(string oldPassword, string newPassword)
    {
        // ✅ Backend expects "oldPassword" and "newPassword"
        PasswordUpdateRequest requestData = new PasswordUpdateRequest
        {
            oldPassword = oldPassword,     // ✅ "oldPassword" key in JSON
            newPassword = newPassword
        };

        Debug.Log("[Profile] Sending password change request");

        yield return APIManager.Instance.Put(
            "/users/password",
            requestData,
            onSuccess: (response) =>
            {
                Debug.Log("[Profile] Password changed successfully");
                ShowPasswordSuccess();

                // Clear input fields
                if (currentPasswordField != null) currentPasswordField.text = "";
                if (newPasswordField != null) newPasswordField.text = "";
            },
            onError: (error) =>
            {
                Debug.LogError("[Profile] Password change failed: " + error);
                ShowPasswordError();
            }
        );
    }

    private void ShowPasswordSuccess()
    {
        passwordState_Default.SetActive(false);
        passwordState_Checked.SetActive(true);
        passwordState_Error.SetActive(false);
        StartCoroutine(ResetPasswordStateAfterDelay(3f));
    }

    private void ShowPasswordError()
    {
        passwordState_Default.SetActive(false);
        passwordState_Checked.SetActive(false);
        passwordState_Error.SetActive(true);
        StartCoroutine(ResetPasswordStateAfterDelay(3f));
    }

    private IEnumerator ResetPasswordStateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetPasswordState();
    }
    #endregion

    #region UI State Management
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