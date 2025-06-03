using UnityEngine;
using System.Collections.Generic;

public class UserSession : MonoBehaviour
{
    public static UserSession Instance { get; private set; }

    public string UserID { get; private set; } = "";
    public string Nickname { get; private set; } = "";
    public bool IsLoggedIn { get; private set; } = false;

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
        UserID = id;
        Nickname = nickname;
        IsLoggedIn = true;
    }

    public void UpdateNickname(string newNickname)
    {
        Nickname = newNickname;
    }

    public void Logout()
    {
        IsLoggedIn = false;
        UserID = "";
        Nickname = "";
    }

        public List<string> Tags { get; set; } // 태그 데이터 저장
}
