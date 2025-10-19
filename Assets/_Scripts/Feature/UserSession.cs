using UnityEngine;
using System.Collections.Generic;

public class UserSession : MonoBehaviour
{
    public static UserSession Instance { get; private set; }

    public string UserID { get; private set; } = "";
    public string Nickname { get; private set; } = "";
    public bool IsLoggedIn { get; private set; } = false;
    public List<string> Tags { get; set; } = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 앱 시작 시 자동 로그인 체크
        CheckAutoLogin();
    }

    private void CheckAutoLogin()
    {
        if (APIManager.Instance != null && APIManager.Instance.HasToken())
        {
            // 토큰이 있으면 프로필 정보 불러오기
            StartCoroutine(LoadUserProfile());
        }
        else
        {
            // 토큰 없으면 즉시 UI 업데이트 트리거
            BroadcastLoginStateChanged();
        }
    }

    private System.Collections.IEnumerator LoadUserProfile()
    {
        yield return APIManager.Instance.Get("/users/profile",
            onSuccess: (response) =>
            {
                UserData userData = JsonUtility.FromJson<UserData>(response);
                SetUserInfo(userData.username, userData.nickname);
                Debug.Log("자동 로그인 성공");

                BroadcastLoginStateChanged(); // 추가
            },
            onError: (error) =>
            {
                Debug.LogWarning("자동 로그인 실패: " + error);
                Logout();
                BroadcastLoginStateChanged(); // 추가
            }
        );
    }

    public System.Action OnLoginStateChanged; // 이벤트 추가

    private void BroadcastLoginStateChanged()
    {
        OnLoginStateChanged?.Invoke();
    }

    // UserSession.cs 수정
    public void SetUserInfo(string id, string nickname)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("UserID가 비어있습니다.");
            return;
        }

        UserID = id;
        Nickname = nickname;
        IsLoggedIn = true;

        Debug.Log($"SetUserInfo 호출됨 - ID: {id}, Nickname: {nickname}, IsLoggedIn: {IsLoggedIn}"); // 추가
    }

    public void UpdateNickname(string newNickname)
    {
        if (string.IsNullOrEmpty(newNickname))
        {
            Debug.LogWarning("새 닉네임이 비어있습니다.");
            return;
        }
        Nickname = newNickname;
    }

    public void Logout()
    {
        IsLoggedIn = false;
        UserID = "";
        Nickname = "";
        Tags.Clear();

        if (APIManager.Instance != null)
        {
            APIManager.Instance.ClearToken();
        }
    }
}