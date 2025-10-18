using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Web;
using System.Collections.Generic;

public class PollinationsImageService : MonoBehaviour
{
    public static PollinationsImageService Instance { get; private set; }

    [Header("API Settings")]
    public string baseUrl = "https://image.pollinations.ai/prompt/";
    public int imageWidth = 512;
    public int imageHeight = 512;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 태그 리스트로 다운로드 요청
    public void DownloadImage(List<string> tags, System.Action<Texture2D> callback)
    {
        string prompt = string.Join(", ", tags);
        StartCoroutine(GenerateImage(prompt, callback));
    }

    // 실제 이미지 생성 및 다운로드
    public IEnumerator GenerateImage(string prompt, System.Action<Texture2D> callback)
    {
        Debug.Log($"이미지 생성 요청: {prompt}");

        string encodedPrompt = HttpUtility.UrlEncode(prompt);
        string fullUrl = $"{baseUrl}{encodedPrompt}?width={imageWidth}&height={imageHeight}&seed={Random.Range(1, 10000)}";

        Debug.Log($"요청 URL: {fullUrl}");

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"HTTP Error: {request.responseCode}");
            Debug.LogError($"Error: {request.error}");
            callback?.Invoke(null);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            if (texture != null)
            {
                Debug.Log($"이미지 생성 완료: {texture.width}x{texture.height}");
                callback?.Invoke(texture);
            }
            else
            {
                Debug.LogError("텍스처 생성 실패");
                callback?.Invoke(null);
            }
        }
    }
}