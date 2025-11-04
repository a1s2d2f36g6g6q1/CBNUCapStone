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

        // Auto login check on app start
        CheckAutoLogin();
    }

    private void CheckAutoLogin()
    {
        if (APIManager.Instance != null && APIManager.Instance.HasToken())
        {
            // Load profile if token exists
            StartCoroutine(LoadUserProfile());
        }
        else
        {
            // Trigger UI update if no token
            BroadcastLoginStateChanged();
        }
    }

    // FIXED: Parse profile response with proper wrapper structure
    private System.Collections.IEnumerator LoadUserProfile()
    {
        yield return APIManager.Instance.Get("/users/profile",
            onSuccess: (response) =>
            {
                Debug.Log($"[UserSession] Profile response: {response}");

                ProfileResponse profileResponse = JsonUtility.FromJson<ProfileResponse>(response);

                if (profileResponse.isSuccess && profileResponse.result != null)
                {
                    // Use nickname as ID since API doesn't return username in profile
                    SetUserInfo(
                        profileResponse.result.nickname,  // Using nickname as ID
                        profileResponse.result.nickname,
                        false
                    );
                    Debug.Log("[UserSession] Auto login success");

                    BroadcastLoginStateChanged();
                }
                else
                {
                    Debug.LogWarning($"[UserSession] Profile fetch failed: {profileResponse.message}");
                    Logout();
                    BroadcastLoginStateChanged();
                }
            },
            onError: (error) =>
            {
                Debug.LogWarning("[UserSession] Auto login failed: " + error);
                Logout();
                BroadcastLoginStateChanged();
            }
        );
    }

    public System.Action OnLoginStateChanged;

    private void BroadcastLoginStateChanged()
    {
        OnLoginStateChanged?.Invoke();
    }

    /// <summary>
    /// Set user info (supports both regular users and guests)
    /// </summary>
    public void SetUserInfo(string id, string nickname, bool isGuest = false)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[UserSession] UserID is empty");
            return;
        }

        UserID = id;
        Nickname = nickname;
        IsLoggedIn = true;
        IsGuest = isGuest;

        Debug.Log($"[UserSession] SetUserInfo - ID: {id}, Nickname: {nickname}, Guest: {isGuest}");
    }

    /// <summary>
    /// Create guest session with auto-generated nickname
    /// </summary>
    public void SetGuestMode()
    {
        string guestId = System.Guid.NewGuid().ToString("N").Substring(0, 8);
        string guestNickname = $"Guest_{UnityEngine.Random.Range(1000, 9999)}";

        UserID = guestId;
        Nickname = guestNickname;
        IsLoggedIn = false;  // Guest is NOT logged in
        IsGuest = true;

        Debug.Log($"[UserSession] Guest mode - Nickname: {guestNickname}");
    }

    public void UpdateNickname(string newNickname)
    {
        if (string.IsNullOrEmpty(newNickname))
        {
            Debug.LogWarning("[UserSession] New nickname is empty");
            return;
        }
        Nickname = newNickname;
    }

    public void Logout()
    {
        IsLoggedIn = false;
        IsGuest = false;
        UserID = "";
        Nickname = "";
        Tags.Clear();

        if (APIManager.Instance != null)
        {
            APIManager.Instance.ClearToken();
        }

        Debug.Log("[UserSession] Logged out");
    }

    /// <summary>
    /// Check if user can save to planet (logged-in users only)
    /// </summary>
    public bool CanSaveToPlanet()
    {
        return IsLoggedIn && !IsGuest;
    }
}