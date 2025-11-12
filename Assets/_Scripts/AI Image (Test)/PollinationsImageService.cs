using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Web;
using System.Collections.Generic;

public class PollinationsImageService : MonoBehaviour
{
    // ===== Singleton =====
    public static PollinationsImageService Instance { get; private set; }

    // ===== Inspector Fields =====
    [Header("API Settings")]
    public string baseUrl = "https://image.pollinations.ai/prompt/";
    public int imageWidth = 512;
    public int imageHeight = 512;

    // ===== Unity Lifecycle =====
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

    // ===== Public Methods =====
    // Download image with tag list
    public void DownloadImage(List<string> tags, System.Action<Texture2D> callback)
    {
        string prompt = string.Join(", ", tags);
        StartCoroutine(GenerateImage(prompt, callback));
    }

    // Generate and download image
    public IEnumerator GenerateImage(string prompt, System.Action<Texture2D> callback)
    {
        Debug.Log($"[PollinationsImageService] Generating image: {prompt}");

        string encodedPrompt = HttpUtility.UrlEncode(prompt);
        string fullUrl = $"{baseUrl}{encodedPrompt}?width={imageWidth}&height={imageHeight}&seed={Random.Range(1, 10000)}";

        Debug.Log($"[PollinationsImageService] Request URL: {fullUrl}");

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[PollinationsImageService] HTTP Error: {request.responseCode}");
            Debug.LogError($"[PollinationsImageService] Error: {request.error}");
            callback?.Invoke(null);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            if (texture != null)
            {
                Debug.Log($"[PollinationsImageService] Image generated: {texture.width}x{texture.height}");
                callback?.Invoke(texture);
            }
            else
            {
                Debug.LogError("[PollinationsImageService] Texture creation failed");
                callback?.Invoke(null);
            }
        }
    }
}