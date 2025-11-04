using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIImageService : MonoBehaviour
{
    public static AIImageService Instance { get; private set; }

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

    public void GenerateImage(List<string> tags, Action<Texture2D> onSuccess, Action<string> onError = null)
    {
        if (tags == null || tags.Count < 4)
        {
            onError?.Invoke("Invalid tags: Need 4 tags");
            return;
        }

        StartCoroutine(GenerateImageCoroutine(tags, onSuccess, onError));
    }

    private IEnumerator GenerateImageCoroutine(List<string> tags, Action<Texture2D> onSuccess, Action<string> onError)
    {
        string taskId = null;
        string imageUrl = null;
        bool requestComplete = false;
        string requestError = null;

        var requestData = new { tags = tags.ToArray() };

        yield return APIManager.Instance.Post(
            "/games/image/generate",
            requestData,
            onSuccess: (response) =>
            {
                try
                {
                    var json = JsonUtility.FromJson<GenerateImageResponse>(response);
                    
                    if (json.isSuccess && json.result != null)
                    {
                        taskId = json.result.taskId;
                        imageUrl = json.result.imageUrl;
                        
                        // MultiplaySession에 URL 저장 (멀티플레이용)
                        if (MultiplaySession.Instance != null)
                        {
                            MultiplaySession.Instance.SharedImageUrl = imageUrl;
                        }
                        
                        Debug.Log($"[AIImageService] TaskId: {taskId}");
                        Debug.Log($"[AIImageService] Image URL: {imageUrl}");
                    }
                    else
                    {
                        requestError = json.message ?? "Unknown error";
                    }
                    
                    requestComplete = true;
                }
                catch (Exception e)
                {
                    requestError = $"Parse error: {e.Message}";
                    requestComplete = true;
                }
            },
            onError: (error) =>
            {
                requestError = error;
                requestComplete = true;
            }
        );

        yield return new WaitUntil(() => requestComplete);

        if (!string.IsNullOrEmpty(requestError))
        {
            onError?.Invoke(requestError);
            yield break;
        }

        if (!string.IsNullOrEmpty(imageUrl))
        {
            yield return StartCoroutine(DownloadImage(imageUrl, onSuccess, onError));
        }
        else
        {
            onError?.Invoke("No image URL returned");
        }
    }

    private IEnumerator DownloadImage(string imageUrl, Action<Texture2D> onSuccess, Action<string> onError)
    {
        Debug.Log($"[AIImageService] Downloading: {imageUrl}");

        yield return APIManager.Instance.DownloadImage(
            imageUrl,
            onSuccess: (texture) =>
            {
                Debug.Log($"[AIImageService] Downloaded: {texture.width}x{texture.height}");
                onSuccess?.Invoke(texture);
            },
            onError: (error) =>
            {
                Debug.LogError($"[AIImageService] Download failed: {error}");
                onError?.Invoke($"Download failed: {error}");
            }
        );
    }

    [Serializable]
    private class GenerateImageResponse
    {
        public bool isSuccess;
        public int code;
        public string message;
        public GenerateImageResult result;
    }

    [Serializable]
    private class GenerateImageResult
    {
        public string taskId;
        public string imageUrl;
        public string status;
    }
}