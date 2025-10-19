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

    [Header("패널들")]
    public GameObject loginPanel;
    public GameObject signupPanel;
    public GameObject settingsPanel;
    public GameObject profilePanel;

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

    [Header("ID 체크 상태 텍스트들")]
    public GameObject idCheck_Default;
    public GameObject idCheck_Checked;
    public GameObject idCheck_Duplicated;

    [Header("회원가입 버튼")]
    public Button signupButton;

    [Header("TR 버튼 그룹")]
    public GameObject[] loginOnlyButtons;
    public GameObject[] guestOnlyButtons;
    public GameObject settingsButton;

    [Header("로딩/에러 메시지")]
    public GameObject loadingPanel; // 로딩 UI (선택사항)
    public TMP_Text errorMessageText; // 에러 메시지 표시용
    private Coroutine currentCheckCoroutine; // 이거 추가


    private void Start()
    {
        // SocketIOManager 자동 생성
        if (SocketIOManager.Instance == null)
        {
            GameObject socketObj = new GameObject("SocketIOManager");
            socketObj.AddComponent<SocketIOManager>();
            Debug.Log("SocketIOManager 자동 생성");
        }

        // MultiplaySession 자동 생성
        if (MultiplaySession.Instance == null)
        {
            GameObject sessionObj = new GameObject("MultiplaySession");
            sessionObj.AddComponent<MultiplaySession>();
            Debug.Log("MultiplaySession 자동 생성");
        }
        signupIdField.onValueChanged.AddListener(OnIDInputChanged);
        SetIDCheckState_Default();
        UpdateSignupButtonInteractable();
    }

    private void OnEnable()
    {
        // UserSession 이벤트 구독
        if (UserSession.Instance != null)
        {
            UserSession.Instance.OnLoginStateChanged += UpdateTopRightButtons;
        }

        UpdateTopRightButtons();
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
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

        // 기존 중복 체크 결과 초기화
        if (idCheckResultText != null)
            idCheckResultText.text = "";
    }

    #region ID 중복 확인
    public void CheckDuplicateID()
    {
        string username = signupIdField.text.Trim();

        if (string.IsNullOrEmpty(username))
        {
            ShowError("아이디를 입력해주세요.");
            return;
        }

        // 기존 체크 중단
        if (currentCheckCoroutine != null)
        {
            StopCoroutine(currentCheckCoroutine);
        }

        currentCheckCoroutine = StartCoroutine(CheckUsernameCoroutine(username));
    }

    private IEnumerator CheckUsernameCoroutine(string username)
    {
        SetLoadingState(true);
        Debug.Log($"중복 체크 시작: {username}");

        yield return APIManager.Instance.Get(
            $"/users/check-username?username={username}",
            onSuccess: (response) =>
            {
                Debug.Log($"서버 응답: {response}");

                // CheckUsernameResponse 사용 (ApiResponse 아님!)
                CheckUsernameResponse apiResponse = JsonUtility.FromJson<CheckUsernameResponse>(response);
                Debug.Log($"파싱 결과 - available: {apiResponse.available}");

                if (apiResponse.available) // available == true → 사용 가능
                {
                    SetIDCheckState_Checked();
                    Debug.Log("사용 가능한 아이디입니다.");
                }
                else // available == false → 중복
                {
                    SetIDCheckState_Duplicated();
                    Debug.Log("이미 사용 중인 아이디입니다.");
                }

                SetLoadingState(false);
            },
            onError: (error) =>
            {
                SetIDCheckState_Default();
                ShowError("ID 중복 확인 실패: " + error);
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

    #region 회원가입
    public void TrySignup()
    {
        string username = signupIdField.text.Trim();
        string password = signupPw1Field.text;
        string passwordConfirm = signupPw2Field.text;
        string nickname = signupNicknameField.text.Trim();

        // 유효성 검사
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(nickname))
        {
            ShowError("모든 필드를 입력해주세요.");
            return;
        }

        if (password != passwordConfirm)
        {
            ShowError("비밀번호가 일치하지 않습니다.");
            return;
        }

        if (!idCheck_Checked.activeSelf)
        {
            ShowError("ID 중복 확인을 먼저 진행해주세요.");
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
                Debug.Log("회원가입 성공!");
                ShowError("회원가입 성공! 로그인해주세요.", false);

                // 입력 필드 초기화
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
                ShowError("회원가입 실패: " + error);
                SetLoadingState(false);
            }
        );
    }
    #endregion

    #region 로그인
    public void TryLogin()
    {
        string username = loginIdField.text.Trim();
        string password = loginPwField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowError("ID와 비밀번호를 입력해주세요.");
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
                Debug.Log($"원본 서버 응답: {response}");

                LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(response);
                APIManager.Instance.SetToken(loginResponse.token);

                // 토큰 저장 후 프로필 불러오기
                StartCoroutine(LoadUserProfileAfterLogin());
            },
            onError: (error) =>
            {
                ShowError("로그인 실패: " + error);
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
                UserSession.Instance.SetUserInfo(userData.username, userData.nickname);

                Debug.Log($"로그인 성공! 환영합니다, {userData.nickname}님");

                UpdateTopRightButtons();
                CloseAllPanels();

                loginIdField.text = "";
                loginPwField.text = "";

                SetLoadingState(false);
            },
            onError: (error) =>
            {
                ShowError("프로필 로드 실패: " + error);
                SetLoadingState(false);
            }
        );
    }
    #endregion

    #region 로그아웃
    public void Logout()
    {
        UserSession.Instance.Logout();
        UpdateTopRightButtons();
        CloseAllPanels();
        Debug.Log("로그아웃 완료!");
    }
    #endregion

    #region UI 헬퍼 메서드
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

            // 3초 후 메시지 자동 삭제
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

        Debug.Log($"UpdateTopRightButtons 호출 - isLoggedIn: {isLoggedIn}");
        Debug.Log($"guestOnlyButtons 길이: {guestOnlyButtons?.Length ?? 0}");
        Debug.Log($"loginOnlyButtons 길이: {loginOnlyButtons?.Length ?? 0}");

        if (guestOnlyButtons != null)
        {
            foreach (var go in guestOnlyButtons)
            {
                if (go != null)
                {
                    go.SetActive(!isLoggedIn);
                    Debug.Log($"Guest 버튼 '{go.name}' → Active: {!isLoggedIn}");
                }
                else
                {
                    Debug.LogWarning("guestOnlyButtons에 null 오브젝트 있음!");
                }
            }
        }

        if (loginOnlyButtons != null)
        {
            foreach (var go in loginOnlyButtons)
            {
                if (go != null)
                {
                    go.SetActive(isLoggedIn);
                    Debug.Log($"Login 버튼 '{go.name}' → Active: {isLoggedIn}");
                }
                else
                {
                    Debug.LogWarning("loginOnlyButtons에 null 오브젝트 있음!");
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
            Debug.LogWarning("⚠ panel is NULL");
    }

    public void CloseAllPanels()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        settingsPanel.SetActive(false);
        profilePanel.SetActive(false);
    }
    #endregion

    #region 씬 전환 버튼들 (기존 코드 유지)
    public void OnClick_SinglePlay()
    {
        fadeController.FadeToScene("G001_TagInput");
    }

    // MainMenuUIController.cs에 추가/수정

    public void OnClick_CreateParty()
    {
        SetLoadingState(true);

        if (SocketIOManager.Instance != null)
        {
            SocketIOManager.Instance.OnConnected += OnSocketConnectedForCreate;
            SocketIOManager.Instance.OnConnectionError += OnSocketConnectionError;
            SocketIOManager.Instance.Connect();
        }
        else
        {
            Debug.LogError("SocketIOManager가 없습니다.");
            SetLoadingState(false);
        }
    }

    private void OnSocketConnectedForCreate()
    {
        SocketIOManager.Instance.OnConnected -= OnSocketConnectedForCreate;
        SocketIOManager.Instance.OnConnectionError -= OnSocketConnectionError;

        Debug.Log("웹소켓 연결 성공, 방 생성 중...");

        // 멀티플레이 이벤트 등록
        SocketIOManager.Instance.RegisterMultiplayEvents();

        // 방 생성 API 호출
        StartCoroutine(CreateRoomCoroutine());
    }

    private void OnSocketConnectionError(string error)
    {
        SocketIOManager.Instance.OnConnected -= OnSocketConnectedForCreate;
        SocketIOManager.Instance.OnConnectionError -= OnSocketConnectionError;

        ShowError("웹소켓 연결 실패: " + error);
        SetLoadingState(false);
    }

    private IEnumerator CreateRoomCoroutine()
    {
        CreateRoomRequest request = new CreateRoomRequest();

        yield return APIManager.Instance.Post(
            "/games/multiplay/rooms/create",
            request,
            onSuccess: (response) =>
            {
                CreateRoomResponse roomResponse = JsonUtility.FromJson<CreateRoomResponse>(response);

                // MultiplaySession에 방 정보 저장 (호스트)
                MultiplaySession.Instance.SetRoomInfo(
                    roomResponse.roomId,
                    roomResponse.sessionCode,
                    true // 호스트
                );

                Debug.Log($"방 생성 성공! RoomId: {roomResponse.roomId}, Code: {roomResponse.sessionCode}");

                SetLoadingState(false);
                fadeController.FadeToScene("B001_CreateParty");
            },
            onError: (error) =>
            {
                ShowError("방 생성 실패: " + error);
                SetLoadingState(false);
                SocketIOManager.Instance.Disconnect();
            }
        );
    }

    public void OnClick_JoinParty()
    {
        fadeController.FadeToScene("B002_JoinParty");
    }

    public void OnClick_MyPlanet()
    {
        fadeController.FadeToScene("P002_MyPlanet");
    }

    public void OnClick_PlanetTravel()
    {
        fadeController.FadeToScene("P001_PlanetTravel");
    }

    public void OnClick_Exit()
    {
        Application.Quit();
        Debug.Log("게임 종료!");
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