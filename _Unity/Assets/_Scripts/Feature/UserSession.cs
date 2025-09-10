using UnityEngine;
using System.Collections.Generic;

public class UserSession : MonoBehaviour
{
    public static UserSession Instance { get; private set; }

    public string UserID { get; private set; } = "";
    public string Nickname { get; private set; } = "";
    public bool IsLoggedIn { get; private set; } = false;
    public List<string> Tags { get; set; } = new List<string>(); // 태그 데이터 저장

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

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
    }
}