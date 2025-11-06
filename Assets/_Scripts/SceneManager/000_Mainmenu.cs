using System.Collections.Generic;
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

    [Header("ID Check Status Texts")]
    public GameObject idCheck_Default;
    public GameObject idCheck_Checked;
    public GameObject idCheck_Duplicated;

    [Header("Signup Button")]
    public Button signupButton;

    [Header("TR Button Groups")]
    public GameObject[] loginOnlyButtons;
    public GameObject[] guestOnlyButtons;
    public GameObject settingsButton;

    [Header("Loading/Error Messages")]
    public GameObject loadingPanel;
    public TMP_Text errorMessageText;

    private Coroutine currentCheckCoroutine;

    private void Start()
    {
        // SocketIOManager auto-create
        if (SocketIOManager.Instance == null)
        {
            GameObject socketObj = new GameObject("SocketIOManager");
            socketObj.AddComponent<SocketIOManager>();
            Debug.Log("SocketIOManager auto-created");
        }

        // MultiplaySession auto-create
        if (MultiplaySession.Instance == null)
        {
            GameObject sessionObj = new GameObject("MultiplaySession");
            sessionObj.AddComponent<MultiplaySession>();
            Debug.Log("MultiplaySession auto-created");
        }

        signupIdField.onValueChanged.AddListener(OnIDInputChanged);
        SetIDCheckState_Default();
        UpdateSignupButtonInteractable();
    }

    private void OnEnable()
    {
        // Subscribe to UserSession events
        if (UserSession.Instance != null)
        {
            UserSession.Instance.OnLoginStateChanged += UpdateTopRightButtons;
        }

        UpdateTopRightButtons();
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

        if (idCheckResultText != null)
            idCheckResultText.text = "";
    }

    #region ID Duplicate Check
    public void CheckDuplicateID()
    {
        string username = signupIdField.text.Trim();

        if (string.IsNullOrEmpty(username))
        {
            ShowError("Please enter a username.");
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
        Debug.Log($"Starting duplicate ID check: {username}");

        yield return APIManager.Instance.Get(
            $"/users/check-username?username={username}",
            onSuccess: (response) =>
            {
                Debug.Log($"Server response: {response}");

                CheckUsernameResponse apiResponse = JsonUtility.FromJson<CheckUsernameResponse>(response);
                Debug.Log($"Parsed result - available: {apiResponse.available}");

                if (apiResponse.available)
                {
                    SetIDCheckState_Checked();
                    Debug.Log("Username is available.");
                }
                else
                {
                    SetIDCheckState_Duplicated();
                    Debug.Log("Username is already taken.");
                }

                SetLoadingState(false);
            },
            onError: (error) =>
            {
                SetIDCheckState_Default();
                ShowError("ID duplication check failed: " + error);
                SetLoadingState(false);
            }
        );

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
        idCheckResultText.text = "ID Available";
    }

    private void SetIDCheckState_Duplicated()
    {
        idCheck_Default.SetActive(false);
        idCheck_Checked.SetActive(false);
        idCheck_Duplicated.SetActive(true);
        idCheckResultText.text = "ID Already Taken";
    }

    private void UpdateSignupButtonInteractable()
    {
        signupButton.interactable = idCheck_Checked.activeSelf;
    }
    #endregion

    #region Signup
    public void TrySignup()
    {
        string username = signupIdField.text.Trim();
        string password = signupPw1Field.text;
        string passwordConfirm = signupPw2Field.text;
        string nickname = signupNicknameField.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(nickname))
        {
            ShowError("Please fill in all fields.");
            return;
        }

        if (password != passwordConfirm)
        {
            ShowError("Passwords do not match.");
            return;
        }

        if (!idCheck_Checked.activeSelf)
        {
            ShowError("Please check ID duplication first.");
            return;
        }

        StartCoroutine(SignupCoroutine(username, password, nickname));
    }

    private IEnumerator SignupCoroutine(string username, string password, string nickname)
    {
        SetLoadingState(true);

        SignupRequest signupData = new SignupRequest
        {
            username = username,
            password = password,
            nickname = nickname
        };

        yield return APIManager.Instance.Post(
            "/users/signup",
            signupData,
            onSuccess: (response) =>
            {
                Debug.Log("Sign-up successful!");
                ShowError("Sign-up successful! Please log in.", false);

                signupIdField.text = "";
                signupPw1Field.text = "";
                signupPw2Field.text = "";
                signupNicknameField.text = "";
                SetIDCheckState_Default();

                CloseAllPanels();
                OpenPanel(loginPanel);
                SetLoadingState(false);
            },
            onError: (error) =>
            {
                ShowError("Sign-up failed: " + error);
                SetLoadingState(false);
            }
        );
    }
    #endregion

    #region Login
    public void TryLogin()
    {
        string username = loginIdField.text.Trim();
        string password = loginPwField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter ID and password.");
            return;
        }

        StartCoroutine(LoginCoroutine(username, password));
    }

    private IEnumerator LoginCoroutine(string username, string password)
    {
        SetLoadingState(true);

        LoginRequest loginData = new LoginRequest { username = username, password = password };

        yield return APIManager.Instance.Post("/users/login", loginData,
            onSuccess: (response) =>
            {
                Debug.Log($"Raw server response: {response}");

                LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(response);
                APIManager.Instance.SetToken(loginResponse.token);

                StartCoroutine(LoadUserProfileAfterLogin());
            },
            onError: (error) =>
            {
                ShowError("Login failed: " + error);
                SetLoadingState(false);
            }
        );
    }

    private IEnumerator LoadUserProfileAfterLogin()
    {
        yield return APIManager.Instance.Get("/users/profile",
            onSuccess: (response) =>
            {
                UserData userData = JsonUtility.FromJson<UserData>(response);

                // Upgrade from guest to real account
                if (UserSession.Instance.IsGuest)
                {
                    UserSession.Instance.UpgradeFromGuest(userData.username, userData.nickname);
                }
                else
                {
                    UserSession.Instance.SetUserInfo(userData.username, userData.nickname, false);
                }

                Debug.Log($"Login successful! Welcome, {userData.nickname}");

                UpdateTopRightButtons();
                CloseAllPanels();

                loginIdField.text = "";
                loginPwField.text = "";

                SetLoadingState(false);
            },
            onError: (error) =>
            {
                ShowError("Failed to load profile: " + error);
                SetLoadingState(false);
            }
        );
    }
    #endregion

    #region Logout
    public void Logout()
    {
        UserSession.Instance.Logout();
        UpdateTopRightButtons();
        CloseAllPanels();
        Debug.Log("Logged out.");
    }
    #endregion

    #region UI Helper Methods
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

        Debug.Log(message);
    }

    private IEnumerator ClearErrorMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorMessageText != null)
            errorMessageText.text = "";
    }

    public void UpdateTopRightButtons()
    {
        bool isLoggedIn = UserSession.Instance != null && UserSession.Instance.IsLoggedIn;
        bool isGuest = UserSession.Instance != null && UserSession.Instance.IsGuest;

        Debug.Log($"UpdateTopRightButtons called - isLoggedIn: {isLoggedIn}, isGuest: {isGuest}");

        if (guestOnlyButtons != null)
        {
            foreach (var go in guestOnlyButtons)
            {
                if (go != null)
                {
                    // Guest or not logged in: show login/signup buttons
                    go.SetActive(!isLoggedIn || isGuest);
                }
            }
        }

        if (loginOnlyButtons != null)
        {
            foreach (var go in loginOnlyButtons)
            {
                if (go != null)
                {
                    // Only show for real users (not guests)
                    go.SetActive(isLoggedIn && !isGuest);
                }
            }
        }

        if (settingsButton != null)
            settingsButton.SetActive(true);
    }

    public void OpenPanel(GameObject panel)
    {
        CloseAllPanels();
        if (panel != null)
            panel.SetActive(true);
        else
            Debug.LogWarning("Panel is NULL");
    }

    public void CloseAllPanels()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        settingsPanel.SetActive(false);
        profilePanel.SetActive(false);
    }
    #endregion

    #region Scene Transition Buttons
    public void OnClick_SinglePlay()
    {
        fadeController.FadeToScene("G001_TagInput");
    }

    public void OnClick_CreateParty()
    {
        Debug.Log("===== CreateParty button clicked =====");

        if (APIManager.Instance == null)
        {
            Debug.LogError("APIManager is null!");
            return;
        }

        if (MultiplaySession.Instance == null)
        {
            Debug.LogError("MultiplaySession is null!");
            return;
        }

        Debug.Log("Starting room creation API call");
        SetLoadingState(true);

        StartCoroutine(CreateRoomCoroutine());
    }

    private void OnSocketConnectedForCreate()
    {
        SocketIOManager.Instance.OnConnected -= OnSocketConnectedForCreate;
        SocketIOManager.Instance.OnConnectionError -= OnSocketConnectionError;

        Debug.Log("WebSocket connected. Creating room...");

        SocketIOManager.Instance.RegisterMultiplayEvents();

        StartCoroutine(CreateRoomCoroutine());
    }

    private void OnSocketConnectionError(string error)
    {
        SocketIOManager.Instance.OnConnected -= OnSocketConnectedForCreate;
        SocketIOManager.Instance.OnConnectionError -= OnSocketConnectionError;

        ShowError("WebSocket connection failed: " + error);
        SetLoadingState(false);
    }

    private IEnumerator CreateRoomCoroutine()
    {
        List<string> randomTags = GenerateRandomTags();

        if (UserSession.Instance != null)
        {
            UserSession.Instance.Tags = randomTags;
        }

        CreateRoomRequest request = new CreateRoomRequest
        {
            tags = randomTags.ToArray()
        };

        yield return APIManager.Instance.Post(
            "/games/multiplay/rooms/create",
            request,
            onSuccess: (response) =>
            {
                Debug.Log($"[Raw API response] {response}");

                CreateRoomResponseWrapper wrapper = JsonUtility.FromJson<CreateRoomResponseWrapper>(response);

                if (wrapper.result != null)
                {
                    Debug.Log($"[Parsed] RoomId: {wrapper.result.roomId}, Code: {wrapper.result.gameCode}");

                    MultiplaySession.Instance.SetRoomInfo(
                        wrapper.result.roomId,
                        wrapper.result.gameCode,
                        true
                    );

                    if (MultiplaySession.Instance.CurrentRoom != null)
                    {
                        MultiplaySession.Instance.CurrentRoom.imageUrl = wrapper.result.imageUrl;
                        MultiplaySession.Instance.CurrentRoom.tags = wrapper.result.tags;
                    }

                    Debug.Log($"Room created! RoomId: {wrapper.result.roomId}, Code: {wrapper.result.gameCode}, ImageUrl: {wrapper.result.imageUrl}");

                    SetLoadingState(false);
                    fadeController.FadeToScene("B001_CreateParty");
                }
                else
                {
                    ShowError("Failed to parse room creation response");
                    SetLoadingState(false);
                }
            },
            onError: (error) =>
            {
                ShowError("Room creation failed: " + error);
                SetLoadingState(false);
            }
        );
    }

    private List<string> GenerateRandomTags()
    {
        List<string> tags = new List<string>();
        string[] categories = { "Style", "Subject", "Mood", "Background" };

        foreach (var category in categories)
        {
            var options = LoadTagOptions($"TagRandom/{category}");
            if (options.Count > 0)
            {
                string randomTag = options[Random.Range(0, options.Count)];
                tags.Add(randomTag);
            }
            else
            {
                tags.Add($"Default{category}");
            }
        }

        Debug.Log("[Multiplay] Auto-generated tags: " + string.Join(", ", tags));
        return tags;
    }

    private List<string> LoadTagOptions(string path)
    {
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset != null)
        {
            var lines = textAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            var result = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    result.Add(trimmed);
            }

            return result;
        }

        Debug.LogWarning("Tag file not found: " + path);
        return new List<string>();
    }

    public void OnClick_JoinParty()
    {
        fadeController.FadeToScene("B002_JoinParty");
    }

    public void OnClick_MyPlanet()
    {
        if (PlanetSession.Instance != null && UserSession.Instance != null)
        {
            string myUsername = UserSession.Instance.UserID;
            PlanetSession.Instance.CurrentPlanetOwnerID = myUsername;
            PlanetSession.Instance.CurrentPlanetId = myUsername;

            Debug.Log($"[MyPlanet] Moving to my planet: {myUsername}");
        }

        fadeController.FadeToScene("P002_MyPlanet");
    }

    public void OnClick_PlanetTravel()
    {
        fadeController.FadeToScene("P001_PlanetTravel");
    }

    public void OnClick_Exit()
    {
        Application.Quit();
        Debug.Log("Quit game!");
    }

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
    #endregion
}