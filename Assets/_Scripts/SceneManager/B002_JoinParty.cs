using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class B002_JoinParty : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField sessionCodeInput;
    public Button joinButton;
    public Button backButton;
    public TMP_Text errorMessageText;
    public GameObject loadingPanel;
    public FadeController fadeController;

    private bool isConnecting = false;

    private void Start()
    {
        if (joinButton != null)
            joinButton.onClick.AddListener(OnJoinButtonClick);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClick);

        if (errorMessageText != null)
            errorMessageText.text = "";

        SetLoadingState(false);
    }

    public void OnJoinButtonClick()
    {
        string sessionCode = sessionCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(sessionCode))
        {
            ShowError("세션 코드를 입력해주세요.");
            return;
        }

        if (isConnecting)
        {
            Debug.Log("이미 연결 시도 중입니다.");
            return;
        }

        // 웹소켓 연결 시작
        StartCoroutine(ConnectAndJoinRoom(sessionCode));
    }

    private IEnumerator ConnectAndJoinRoom(string sessionCode)
    {
        isConnecting = true;
        SetLoadingState(true);

        // 웹소켓 연결
        bool socketConnected = false;
        string connectionError = null;

        SocketIOManager.Instance.OnConnected += () => socketConnected = true;
        SocketIOManager.Instance.OnConnectionError += (error) => connectionError = error;

        SocketIOManager.Instance.Connect();

        // 연결 대기 (최대 5초)
        float elapsed = 0f;
        while (!socketConnected && connectionError == null && elapsed < 5f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 이벤트 구독 해제
        SocketIOManager.Instance.OnConnected -= () => socketConnected = true;
        SocketIOManager.Instance.OnConnectionError -= (error) => connectionError = error;

        if (connectionError != null)
        {
            ShowError("웹소켓 연결 실패: " + connectionError);
            SetLoadingState(false);
            isConnecting = false;
            yield break;
        }

        if (!socketConnected)
        {
            ShowError("웹소켓 연결 시간 초과");
            SetLoadingState(false);
            isConnecting = false;
            yield break;
        }

        Debug.Log("웹소켓 연결 성공, 방 입장 시도 중...");

        // 멀티플레이 이벤트 등록
        SocketIOManager.Instance.RegisterMultiplayEvents();

        // 방 입장 API 호출
        yield return JoinRoomCoroutine(sessionCode);

        isConnecting = false;
    }

    private IEnumerator JoinRoomCoroutine(string sessionCode)
    {
        JoinRoomRequest request = new JoinRoomRequest { sessionCode = sessionCode };

        yield return APIManager.Instance.Post(
            "/games/multiplay/rooms/join",
            request,
            onSuccess: (response) =>
            {
                JoinRoomResponse joinResponse = JsonUtility.FromJson<JoinRoomResponse>(response);

                // MultiplaySession에 방 정보 저장 (클라이언트)
                MultiplaySession.Instance.SetRoomInfo(
                    joinResponse.roomId,
                    sessionCode,
                    false // 클라이언트
                );

                // 방 데이터 업데이트
                if (joinResponse.roomData != null)
                {
                    MultiplaySession.Instance.UpdateRoomData(joinResponse.roomData);
                }

                Debug.Log($"방 입장 성공! RoomId: {joinResponse.roomId}");

                SetLoadingState(false);

                // 로비로 이동
                if (fadeController != null)
                    fadeController.FadeToScene("B001_CreateParty");
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene("B001_CreateParty");
            },
            onError: (error) =>
            {
                ShowError("방 입장 실패: " + error);
                SetLoadingState(false);

                // 웹소켓 연결 해제
                SocketIOManager.Instance.Disconnect();
            }
        );
    }

    public void OnBackButtonClick()
    {
        // 웹소켓 연결되어 있으면 해제
        if (SocketIOManager.Instance != null && SocketIOManager.Instance.IsConnected)
        {
            SocketIOManager.Instance.Disconnect();
        }

        if (fadeController != null)
            fadeController.FadeToScene("000_MainMenu");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("000_MainMenu");
    }

    private void SetLoadingState(bool isLoading)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(isLoading);

        if (joinButton != null)
            joinButton.interactable = !isLoading;

        if (sessionCodeInput != null)
            sessionCodeInput.interactable = !isLoading;
    }

    private void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.color = Color.red;

            StartCoroutine(ClearErrorMessageAfterDelay(3f));
        }

        Debug.LogWarning(message);
    }

    private IEnumerator ClearErrorMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorMessageText != null)
            errorMessageText.text = "";
    }

    private void OnDestroy()
    {
        if (joinButton != null)
            joinButton.onClick.RemoveAllListeners();

        if (backButton != null)
            backButton.onClick.RemoveAllListeners();
    }
}