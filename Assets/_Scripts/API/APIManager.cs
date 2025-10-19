using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    public static APIManager Instance { get; private set; }

    private const string BASE_URL = "http://13.209.33.42:3000";
    private string jwtToken = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 저장된 토큰 로드
        LoadToken();
    }

    #region Token Management
    public void SetToken(string token)
    {
        jwtToken = token;
        PlayerPrefs.SetString("JWT_TOKEN", token);
        PlayerPrefs.Save();
    }

    private void LoadToken()
    {
        jwtToken = PlayerPrefs.GetString("JWT_TOKEN", "");
    }

    public void ClearToken()
    {
        jwtToken = "";
        PlayerPrefs.DeleteKey("JWT_TOKEN");
        PlayerPrefs.Save();
    }

    public bool HasToken()
    {
        return !string.IsNullOrEmpty(jwtToken);
    }
    #endregion

    #region HTTP Methods
    public IEnumerator Get(string endpoint, Action<string> onSuccess, Action<string> onError)
    {
        string url = BASE_URL + endpoint;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.certificateHandler = new BypassCertificate();
            request.disposeCertificateHandlerOnDispose = true;

            if (HasToken())
            {
                request.SetRequestHeader("Authorization", "Bearer " + jwtToken);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                HandleError(request, onError);
            }
        }
    }

    public IEnumerator Post(string endpoint, object data, Action<string> onSuccess, Action<string> onError)
    {
        string url = BASE_URL + endpoint;
        string json = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.certificateHandler = new BypassCertificate();
            request.disposeCertificateHandlerOnDispose = true;

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (HasToken())
            {
                request.SetRequestHeader("Authorization", "Bearer " + jwtToken);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                HandleError(request, onError);
            }
        }
    }

    public IEnumerator Put(string endpoint, object data, Action<string> onSuccess, Action<string> onError)
    {
        string url = BASE_URL + endpoint;
        string json = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            request.certificateHandler = new BypassCertificate();
            request.disposeCertificateHandlerOnDispose = true;

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (HasToken())
            {
                request.SetRequestHeader("Authorization", "Bearer " + jwtToken);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                HandleError(request, onError);
            }
        }
    }

    public IEnumerator Delete(string endpoint, Action<string> onSuccess, Action<string> onError)
    {
        string url = BASE_URL + endpoint;

        using (UnityWebRequest request = UnityWebRequest.Delete(url))
        {
            request.certificateHandler = new BypassCertificate();
            request.disposeCertificateHandlerOnDispose = true;

            if (HasToken())
            {
                request.SetRequestHeader("Authorization", "Bearer " + jwtToken);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                HandleError(request, onError);
            }
        }
    }
    #endregion

    #region Error Handling
    private void HandleError(UnityWebRequest request, Action<string> onError)
    {
        string errorMsg = $"Error {request.responseCode}: {request.error}";

        if (!string.IsNullOrEmpty(request.downloadHandler.text))
        {
            errorMsg += $"\nServer Response: {request.downloadHandler.text}";
        }

        Debug.LogError($"[API Error] {errorMsg}");

        // 401 Unauthorized - 토큰 만료
        if (request.responseCode == 401)
        {
            Debug.LogWarning("Token expired. Logging out...");
            ClearToken();
            UserSession.Instance?.Logout();
        }

        onError?.Invoke(errorMsg);
    }
    #endregion
}

// HTTP 인증서 우회 클래스
public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}