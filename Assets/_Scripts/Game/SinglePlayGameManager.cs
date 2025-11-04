using System;
using System.Collections;
using UnityEngine;

public class SinglePlayGameManager : MonoBehaviour
{
    public static SinglePlayGameManager Instance { get; private set; }

    public string CurrentGameCode { get; private set; }
    public string CurrentImageUrl { get; private set; }
    public DateTime GameStartTime { get; private set; }

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

    /// <summary>
    /// Start single play game - works for both logged-in users and guests
    /// </summary>
    public void StartSingleGame(string[] tags, Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(StartSingleGameCoroutine(tags, onSuccess, onError));
    }

    private IEnumerator StartSingleGameCoroutine(string[] tags, Action<string> onSuccess, Action<string> onError)
    {
        if (tags == null || tags.Length < 4)
        {
            onError?.Invoke("Need 4 tags");
            yield break;
        }

        var request = new SingleGameStartRequest { tags = tags };

        yield return APIManager.Instance.Post(
            "/games/single/start",
            request,
            onSuccess: (response) =>
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<SingleGameStartResponse>(response);

                    if (wrapper.isSuccess && wrapper.result != null)
                    {
                        CurrentGameCode = wrapper.result.gameCode;
                        CurrentImageUrl = wrapper.result.imageUrl;
                        GameStartTime = DateTime.UtcNow;

                        Debug.Log($"[SinglePlay] Game start success - Code: {CurrentGameCode}");
                        Debug.Log($"[SinglePlay] Image URL: {CurrentImageUrl}");

                        onSuccess?.Invoke(CurrentImageUrl);
                    }
                    else
                    {
                        onError?.Invoke(wrapper.message ?? "Game start failed");
                    }
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Response parse error: {e.Message}");
                }
            },
            onError: onError
        );
    }

    /// <summary>
    /// Record single play game completion
    /// </summary>
    public void CompleteSingleGame(Action<SingleGameCompleteResult> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(CurrentGameCode))
        {
            Debug.LogWarning("[SinglePlay] No game code - cannot record completion");
            return;
        }

        StartCoroutine(CompleteSingleGameCoroutine(onSuccess, onError));
    }

    private IEnumerator CompleteSingleGameCoroutine(Action<SingleGameCompleteResult> onSuccess, Action<string> onError)
    {
        DateTime endTime = DateTime.UtcNow;

        var request = new SingleGameCompleteRequest
        {
            gameCode = CurrentGameCode,
            startTime = GameStartTime.ToString("o"),  // ISO 8601
            endTime = endTime.ToString("o")
        };

        Debug.Log($"[SinglePlay] Sending completion record - Code: {CurrentGameCode}");
        Debug.Log($"[SinglePlay] Start: {request.startTime}");
        Debug.Log($"[SinglePlay] End: {request.endTime}");

        yield return APIManager.Instance.Post(
            "/games/single/complete",
            request,
            onSuccess: (response) =>
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<SingleGameCompleteResponse>(response);

                    if (wrapper.isSuccess && wrapper.result != null)
                    {
                        Debug.Log($"[SinglePlay] Clear time: {wrapper.result.clearTimeMs}ms");
                        onSuccess?.Invoke(wrapper.result);
                    }
                    else
                    {
                        onError?.Invoke(wrapper.message ?? "Completion record failed");
                    }
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Response parse error: {e.Message}");
                }
            },
            onError: onError
        );
    }

    /// <summary>
    /// Save game image to planet (logged-in users only)
    /// </summary>
    public void SaveToPlanet(string title, Action<SaveToPlanetResult> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(CurrentGameCode))
        {
            Debug.LogWarning("[SinglePlay] No game code - cannot save to planet");
            onError?.Invoke("No game code available");
            return;
        }

        StartCoroutine(SaveToPlanetCoroutine(title, onSuccess, onError));
    }

    private IEnumerator SaveToPlanetCoroutine(string title, Action<SaveToPlanetResult> onSuccess, Action<string> onError)
    {
        var request = new SaveToPlanetRequest
        {
            gameCode = CurrentGameCode,
            title = title
        };

        Debug.Log($"[SinglePlay] Saving to planet - Code: {CurrentGameCode}");
        Debug.Log($"[SinglePlay] Title: {title}");

        yield return APIManager.Instance.Post(
            "/games/single/save-to-planet",
            request,
            onSuccess: (response) =>
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<SaveToPlanetResponse>(response);

                    if (wrapper.isSuccess && wrapper.result != null)
                    {
                        Debug.Log($"[SinglePlay] Saved to planet successfully");
                        Debug.Log($"[SinglePlay] Gallery title: {wrapper.result.galleryTitle}");
                        onSuccess?.Invoke(wrapper.result);
                    }
                    else
                    {
                        onError?.Invoke(wrapper.message ?? "Save to planet failed");
                    }
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Response parse error: {e.Message}");
                }
            },
            onError: onError
        );
    }

    /// <summary>
    /// Clear game data
    /// </summary>
    public void ClearGameData()
    {
        CurrentGameCode = null;
        CurrentImageUrl = null;
        GameStartTime = default;
        Debug.Log("[SinglePlay] Game data cleared");
    }
}