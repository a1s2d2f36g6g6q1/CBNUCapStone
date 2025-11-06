using UnityEngine;
using System.Collections.Generic;

public class UserSession : MonoBehaviour
{
    public static UserSession Instance { get; private set; }

    public string UserID { get; private set; } = "";
    public string Nickname { get; private set; } = "";
    public bool IsLoggedIn { get; private set; } = false;
    public bool IsGuest { get; private set; } = false;
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

        // App start - auto login check
        CheckAutoLogin();
    }

    private void CheckAutoLogin()
    {
        if (APIManager.Instance != null && APIManager.Instance.HasToken())
        {
            // If token exists, load profile
            StartCoroutine(LoadUserProfile());
        }
        else
        {
            // No token - create guest account
            StartCoroutine(GuestLogin());
        }
    }

    private System.Collections.IEnumerator LoadUserProfile()
    {
        yield return APIManager.Instance.Get("/users/profile",
            onSuccess: (response) =>
            {
                UserData userData = JsonUtility.FromJson<UserData>(response);
                SetUserInfo(userData.username, userData.nickname, false);
                Debug.Log("Auto login successful");

                BroadcastLoginStateChanged();
            },
            onError: (error) =>
            {
                Debug.LogWarning("Auto login failed: " + error);

                // Auto login failed - clear token and create guest
                APIManager.Instance.ClearToken();
                StartCoroutine(GuestLogin());
            }
        );
    }

    private System.Collections.IEnumerator GuestLogin()
    {
        Debug.Log("[UserSession] Creating guest account...");

        // Generate random guest credentials
        string guestId = "guest_" + System.Guid.NewGuid().ToString().Substring(0, 8);
        string guestPassword = System.Guid.NewGuid().ToString();
        string guestNickname = "Guest_" + Random.Range(1000, 9999);

        // Create guest account
        SignupRequest signupRequest = new SignupRequest
        {
            username = guestId,
            password = guestPassword,
            nickname = guestNickname
        };

        bool signupCompleted = false;
        bool signupSuccess = false;

        yield return APIManager.Instance.Post(
            "/users/signup",
            signupRequest,
            onSuccess: (response) =>
            {
                Debug.Log("[UserSession] Guest account created successfully");
                signupSuccess = true;
                signupCompleted = true;
            },
            onError: (error) =>
            {
                Debug.LogError("[UserSession] Guest account creation failed: " + error);
                signupCompleted = true;
            }
        );

        while (!signupCompleted)
            yield return null;

        if (!signupSuccess)
        {
            Debug.LogError("[UserSession] Cannot continue without guest account");
            BroadcastLoginStateChanged();
            yield break;
        }

        // Auto login with guest account
        LoginRequest loginRequest = new LoginRequest
        {
            username = guestId,
            password = guestPassword
        };

        bool loginCompleted = false;

        yield return APIManager.Instance.Post(
            "/users/login",
            loginRequest,
            onSuccess: (response) =>
            {
                LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(response);
                APIManager.Instance.SetToken(loginResponse.token);

                // Save guest account info
                SetUserInfo(guestId, guestNickname, true);

                Debug.Log($"[UserSession] Guest login successful - {guestNickname}");
                loginCompleted = true;
            },
            onError: (error) =>
            {
                Debug.LogError("[UserSession] Guest login failed: " + error);
                loginCompleted = true;
            }
        );

        while (!loginCompleted)
            yield return null;

        BroadcastLoginStateChanged();
    }

    public System.Action OnLoginStateChanged;

    private void BroadcastLoginStateChanged()
    {
        OnLoginStateChanged?.Invoke();
    }

    public void SetUserInfo(string id, string nickname, bool isGuest = false)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("UserID is empty.");
            return;
        }

        UserID = id;
        Nickname = nickname;
        IsLoggedIn = true;
        IsGuest = isGuest;

        Debug.Log($"SetUserInfo called - ID: {id}, Nickname: {nickname}, IsGuest: {isGuest}, IsLoggedIn: {IsLoggedIn}");
    }

    public void UpdateNickname(string newNickname)
    {
        if (string.IsNullOrEmpty(newNickname))
        {
            Debug.LogWarning("New nickname is empty.");
            return;
        }
        Nickname = newNickname;
    }

    public void Logout()
    {
        // If guest, just create new guest account
        if (IsGuest)
        {
            Debug.Log("[UserSession] Guest logout - creating new guest account");

            IsLoggedIn = false;
            IsGuest = false;
            UserID = "";
            Nickname = "";
            Tags.Clear();

            if (APIManager.Instance != null)
            {
                APIManager.Instance.ClearToken();
            }

            // Create new guest account
            StartCoroutine(GuestLogin());
        }
        else
        {
            // Real user logout
            IsLoggedIn = false;
            IsGuest = false;
            UserID = "";
            Nickname = "";
            Tags.Clear();

            if (APIManager.Instance != null)
            {
                APIManager.Instance.ClearToken();
            }

            // Create guest account after logout
            StartCoroutine(GuestLogin());
        }

        BroadcastLoginStateChanged();
    }

    /// <summary>
    /// Switch from guest to real account
    /// </summary>
    public void UpgradeFromGuest(string newUserId, string newNickname)
    {
        if (!IsGuest)
        {
            Debug.LogWarning("[UserSession] Not a guest account - cannot upgrade");
            return;
        }

        SetUserInfo(newUserId, newNickname, false);
        Debug.Log("[UserSession] Guest account upgraded to real account");
        BroadcastLoginStateChanged();
    }
}