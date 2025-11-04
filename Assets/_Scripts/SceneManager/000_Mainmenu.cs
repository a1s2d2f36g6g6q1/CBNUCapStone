using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

public class MainMenuUIController : MonoBehaviour
{
    [Header("Fade")]
    public FadeController fadeController;

    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject signupPanel;
    public GameObject settingsPanel;
    public GameObject profilePanel;

    [Header("Login Input Fields")]
    public TMP_InputField loginIdField;
    public TMP_InputField loginPwField;

    [Header("Signup Input Fields")]
    public TMP_InputField signupIdField;
    public TMP_InputField signupPw1Field;
    public TMP_InputField signupPw2Field;
    public TMP_InputField signupNicknameField;

    [Header("ID Check")]
    public TMP_Text idCheckResultText;

    [Header("ID Check Status")]
    public GameObject idCheck_Default;
    public GameObject idCheck_Checked;
    public GameObject idCheck_Duplicated;

    [Header("Signup Button")]
    public Button signupButton;

    [Header("TR Button Groups")]
    public GameObject[] loginOnlyButtons;
    public GameObject[] guestOnlyButtons;
    public GameObject settingsButton;

    [Header("Loading/Error - Signup")]
    public GameObject loadingPanel;
    public TMP_Text errorMessageText;
    private Coroutine currentCheckCoroutine;

    [Header("Create Party Panels")]
    public GameObject createPartyLoadingPanel;
    public GameObject createPartyErrorPanel;
    public TMP_Text createPartyErrorText;

    private void Start()
    {
        // Auto-create UnityMainThreadDispatcher (before SocketIO)
        if (UnityMainThreadDispatcher.Instance() == null)
        {
            GameObject dispatcherObj = new GameObject("UnityMainThreadDispatcher");
            dispatcherObj.AddComponent<UnityMainThreadDispatcher>();
            Debug.Log("[MainMenu] UnityMainThreadDispatcher auto-created");
        }

        // Auto-create SocketIOManager
        if (SocketIOManager.Instance == null)
        {
            GameObject socketObj = new GameObject("SocketIOManager");
            socketObj.AddComponent<SocketIOManager>();
            Debug.Log("[MainMenu] SocketIOManager auto-created");
        }

        // Auto-create MultiplaySession
        if (MultiplaySession.Instance == null)
        {
            GameObject sessionObj = new GameObject("MultiplaySession");
            sessionObj.AddComponent<MultiplaySession>();
            Debug.Log("[MainMenu] MultiplaySession auto-created");
        }

        // Clear guest info when returning to main menu
        if (MultiplaySession.Instance != null)
        {
            MultiplaySession.Instance.ClearGuestNickname();
        }

        // Clear single play game data
        if (SinglePlayGameManager.Instance != null)
        {
            SinglePlayGameManager.Instance.ClearGameData();
        }

        signupIdField.onValueChanged.AddListener(OnIDInputChanged);
        SetIDCheckState_Default();
        UpdateSignupButtonInteractable();

        UpdateTopRightButtons();
    }

    private void OnEnable()
    {
        // Subscribe to UserSession events
        if (UserSession.Instance != null)
        {
            UserSession.Instance.OnLoginStateChanged += UpdateTopRightButtons;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (UserSession.Instance != null)
        {
            UserSession.Instance.OnLoginStateChanged -= UpdateTopRightButtons;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) CloseAllPanels();
    }

    private void OnIDInputChanged(string newText)
    {
        SetIDCheckState_Default();
        UpdateSignupButtonInteractable();
    }

    #region ID Duplicate Check
    public void OnClick_CheckID()
    {
        string username = signupIdField.text.Trim();

        if (string.IsNullOrEmpty(username))
        {
            ShowError("Please enter username.");
            return;
        }

        if (currentCheckCoroutine != null)
        {
            StopCoroutine(currentCheckCoroutine);
        }

        currentCheckCoroutine = StartCoroutine(CheckUsernameCoroutine(username));
    }

    private IEnumerator CheckUsernameCoroutine(string username)
    {
        SetLoadingState(true);

        yield return APIManager.Instance.Get(
            $"/users/check-username?username={username}",
            onSuccess: (response) =>
            {
                CheckUsernameResponse checkResponse = JsonUtility.FromJson<CheckUsernameResponse>(response);

                if (checkResponse.available)
                {
                    Debug.Log("[MainMenu] Username available");
                    SetIDCheckState_Checked();
                }
                else
                {
                    Debug.Log("[MainMenu] Username already taken");
                    SetIDCheckState_Duplicated();
                }

                SetLoadingState(false);
                UpdateSignupButtonInteractable();
            },
            onError: (error) =>
            {
                ShowError("ID check failed: " + error);
                SetLoadingState(false);
            }
        );
    }
    #endregion

    #region ID Check State UI
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
    }

    private void SetIDCheckState_Duplicated()
    {
        idCheck_Default.SetActive(false);
        idCheck_Checked.SetActive(false);
        idCheck_Duplicated.SetActive(true);
    }

    private void UpdateSignupButtonInteractable()
    {
        bool isIDChecked = idCheck_Checked.activeSelf;
        signupButton.interactable = isIDChecked;
    }
    #endregion

    #region Signup
    public void OnClick_Signup()
    {
        string username = signupIdField.text.Trim();
        string pw1 = signupPw1Field.text;
        string pw2 = signupPw2Field.text;
        string nickname = signupNicknameField.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pw1) ||
            string.IsNullOrEmpty(pw2) || string.IsNullOrEmpty(nickname))
        {
            ShowError("Please fill all fields.");
            return;
        }

        if (pw1 != pw2)
        {
            ShowError("Passwords do not match.");
            return;
        }

        if (!idCheck_Checked.activeSelf)
        {
            ShowError("Please check ID duplicate first.");
            return;
        }

        SignupRequest request = new SignupRequest
        {
            username = username,
            password = pw1,
            nickname = nickname
        };

        SetLoadingState(true);
        StartCoroutine(SignupCoroutine(request));
    }

    private IEnumerator SignupCoroutine(SignupRequest request)
    {
        yield return APIManager.Instance.Post(
            "/users/signup",
            request,
            onSuccess: (response) =>
            {
                Debug.Log("[MainMenu] Signup success!");
                SetLoadingState(false);
                ShowError("Signup success! Please log in.", false);
                CloseAllPanels();
                OpenPanel(loginPanel);
            },
            onError: (error) =>
            {
                Debug.LogError($"[Signup] Signup failed: {error}");

                if (error.Contains("Connection refused") || error.Contains("Failed to connect"))
                {
                    ShowError("Cannot connect to server.\nServer might be offline or check network.");
                }
                else if (error.Contains("409") || error.Contains("already exists"))
                {
                    ShowError("Username already exists.");
                }
                else
                {
                    ShowError("Signup failed: " + error);
                }

                SetLoadingState(false);
            }
        );
    }
    #endregion

    #region Login
    public void OnClick_Login()
    {
        string username = loginIdField.text.Trim();
        string password = loginPwField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter username and password.");
            return;
        }

        // FIXED: API spec uses "userId" field name, not "username"
        LoginRequest request = new LoginRequest
        {
            userId = username,
            password = password
        };

        SetLoadingState(true);
        StartCoroutine(LoginCoroutine(request));
    }

    private IEnumerator LoginCoroutine(LoginRequest request)
    {
        yield return APIManager.Instance.Post(
            "/users/login",
            request,
            onSuccess: (response) =>
            {
                Debug.Log($"[Login] Server response: {response}");

                LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(response);

                Debug.Log($"[Login] After parsing - token: {loginResponse.token}");

                // Save token first (required for profile fetch)
                if (!string.IsNullOrEmpty(loginResponse.token))
                {
                    APIManager.Instance.SetToken(loginResponse.token);
                    Debug.Log("[Login] Token saved");
                }

                // FIXED: Fetch profile to get username and nickname
                StartCoroutine(LoadProfileAfterLogin());
            },
            onError: (error) =>
            {
                Debug.LogError($"[Login] Login failed: {error}");

                if (error.Contains("Connection refused") || error.Contains("Failed to connect"))
                {
                    ShowError("Cannot connect to server.\nServer might be offline or check network.");
                }
                else if (error.Contains("401") || error.Contains("Unauthorized"))
                {
                    ShowError("Invalid username or password.");
                }
                else
                {
                    ShowError("Login failed: " + error);
                }

                SetLoadingState(false);
            }
        );
    }
    #endregion

    #region Load Profile After Login
    // FIXED: Parse profile response with proper wrapper structure
    private IEnumerator LoadProfileAfterLogin()
    {
        Debug.Log("[Login] Fetching profile...");

        yield return APIManager.Instance.Get(
            "/users/profile",
            onSuccess: (response) =>
            {
                Debug.Log($"[Login] Profile response: {response}");

                // FIXED: Profile response has wrapper structure
                ProfileResponse profileResponse = JsonUtility.FromJson<ProfileResponse>(response);

                if (profileResponse.isSuccess && profileResponse.result != null)
                {
                    // Save nickname to UserSession
                    // Note: We don't have username in profile response, need to get it separately
                    UserSession.Instance.SetUserInfo(
                        profileResponse.result.nickname,  // Using nickname as ID temporarily
                        profileResponse.result.nickname,
                        false  // Not guest
                    );

                    Debug.Log($"[Login] Login success! Nickname: {profileResponse.result.nickname}");

                    SetLoadingState(false);
                    CloseAllPanels();

                    // Trigger login state changed event
                    if (UserSession.Instance != null)
                    {
                        UserSession.Instance.OnLoginStateChanged?.Invoke();
                    }
                }
                else
                {
                    Debug.LogError($"[Login] Profile fetch failed: {profileResponse.message}");
                    ShowError("Login successful but profile fetch failed");
                    SetLoadingState(false);
                    CloseAllPanels();
                }
            },
            onError: (error) =>
            {
                Debug.LogError($"[Login] Profile fetch failed: {error}");
                ShowError("Login successful but profile fetch failed");
                SetLoadingState(false);
                CloseAllPanels();
            }
        );
    }
    #endregion

    #region Loading/Error (Signup)
    private void SetLoadingState(bool isLoading)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(isLoading);
    }

    private void ShowError(string message, bool isError = true)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.color = isError ? Color.red : Color.green;

            StartCoroutine(ClearErrorMessageAfterDelay(3f));
        }

        Debug.Log("[MainMenu] " + message);
    }

    private IEnumerator ClearErrorMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorMessageText != null)
            errorMessageText.text = "";
    }
    #endregion

    #region CreateParty Loading/Error
    private void SetCreatePartyLoadingState(bool isLoading)
    {
        if (createPartyLoadingPanel != null)
            createPartyLoadingPanel.SetActive(isLoading);
    }

    private void ShowCreatePartyError(string message)
    {
        if (createPartyErrorPanel != null && createPartyErrorText != null)
        {
            createPartyErrorText.text = message;
            createPartyErrorPanel.SetActive(true);
        }

        Debug.LogError("[MainMenu] " + message);
    }

    public void OnClick_CloseCreatePartyError()
    {
        if (createPartyErrorPanel != null)
            createPartyErrorPanel.SetActive(false);
    }
    #endregion

    #region Button State Update
    public void UpdateTopRightButtons()
    {
        bool isLoggedIn = UserSession.Instance != null && UserSession.Instance.IsLoggedIn;

        foreach (var go in loginOnlyButtons)
            go.SetActive(isLoggedIn);

        foreach (var go in guestOnlyButtons)
            go.SetActive(!isLoggedIn);

        settingsButton.SetActive(true);

        Debug.Log($"[UpdateTopRightButtons] isLoggedIn: {isLoggedIn}");
    }
    #endregion

    #region Panel Management
    public void OpenPanel(GameObject panel)
    {
        CloseAllPanels();
        if (panel != null)
            panel.SetActive(true);
        else
            Debug.LogWarning("[MainMenu] Panel is NULL");
    }

    public void CloseAllPanels()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        settingsPanel.SetActive(false);
        profilePanel.SetActive(false);

        if (createPartyLoadingPanel != null)
            createPartyLoadingPanel.SetActive(false);
        if (createPartyErrorPanel != null)
            createPartyErrorPanel.SetActive(false);
    }
    #endregion

    #region Panel Open Buttons
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

    public void OnClick_OpenProfile()
    {
        OpenPanel(profilePanel);
    }
    #endregion

    #region Logout
    public void OnClick_Logout()
    {
        if (UserSession.Instance != null)
        {
            UserSession.Instance.Logout();
        }

        CloseAllPanels();

        Debug.Log("[MainMenu] Logged out");
    }
    #endregion
}