using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleLoader : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject loadingPanel;
    public PuzzleManager puzzleManager;
    public LoadingPanelFade loadingPanelFade;
    public GameObject timerManagerObj;
    public GameObject backButtonObj;

    [Header("Fallback Settings")]
    public float serverTimeout = 10f;
    public string fallbackImagePath = "test";

    private void Start()
    {
        InitializeUI();
        StartCoroutine(LoadAndInitPuzzleCoroutine());
    }

    private void InitializeUI()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        if (timerManagerObj != null)
            timerManagerObj.SetActive(false);
        if (backButtonObj != null)
            backButtonObj.SetActive(false);
    }

    IEnumerator LoadAndInitPuzzleCoroutine()
    {
        Texture2D puzzleTexture = null;

        // Check if multiplay
        bool isMultiplay = MultiplaySession.Instance != null &&
                          MultiplaySession.Instance.CurrentRoom != null;

        GameData.isMultiplay = isMultiplay;

        if (isMultiplay)
        {
            // Multiplay: Use image from MultiplaySession
            yield return StartCoroutine(LoadMultiplayImage((result) => puzzleTexture = result));
        }
        else
        {
            // Singleplay: Request image from server
            yield return StartCoroutine(LoadSingleplayImage((result) => puzzleTexture = result));
        }

        // Fallback to dummy image if failed
        if (puzzleTexture == null)
        {
            Debug.LogWarning("[PuzzleLoader] All image sources failed. Using fallback image.");
            puzzleTexture = LoadFallbackImage();
        }

        // Initialize puzzle
        if (puzzleTexture != null)
        {
            InitializePuzzle(puzzleTexture);
        }
        else
        {
            Debug.LogError("[PuzzleLoader] Complete failure loading image!");
            HandleLoadingFailure();
        }

        FinalizePuzzleLoading();
    }

    private IEnumerator LoadSingleplayImage(System.Action<Texture2D> callback)
    {
        Debug.Log("[PuzzleLoader] Requesting image from server for singleplay...");

        // Get tags from UserSession
        var tags = GetTags();

        SingleGameStartRequest request = new SingleGameStartRequest
        {
            tags = tags.ToArray()
        };

        bool requestCompleted = false;
        Texture2D resultTexture = null;

        yield return APIManager.Instance.Post(
            "/games/single/start",
            request,
            onSuccess: (response) =>
            {
                Debug.Log($"[PuzzleLoader] Server response: {response}");

                SingleGameStartResponse apiResponse = JsonUtility.FromJson<SingleGameStartResponse>(response);

                if (apiResponse != null && apiResponse.result != null)
                {
                    // Save game data
                    GameData.gameCode = apiResponse.result.gameCode;
                    GameData.imageUrl = apiResponse.result.imageUrl;

                    Debug.Log($"[PuzzleLoader] GameCode: {GameData.gameCode}");
                    Debug.Log($"[PuzzleLoader] ImageURL: {GameData.imageUrl}");

                    // Download image from URL
                    StartCoroutine(DownloadImageFromURL(GameData.imageUrl, (texture) =>
                    {
                        resultTexture = texture;
                        requestCompleted = true;
                    }));
                }
                else
                {
                    Debug.LogError("[PuzzleLoader] Failed to parse server response");
                    requestCompleted = true;
                }
            },
            onError: (error) =>
            {
                Debug.LogError("[PuzzleLoader] Server request failed: " + error);
                requestCompleted = true;
            }
        );

        // Wait for completion
        float elapsed = 0f;
        while (!requestCompleted && elapsed < serverTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!requestCompleted)
        {
            Debug.LogWarning("[PuzzleLoader] Server request timeout");
        }

        callback?.Invoke(resultTexture);
    }

    private IEnumerator LoadMultiplayImage(System.Action<Texture2D> callback)
    {
        Debug.Log("[PuzzleLoader] Loading image for multiplay...");

        if (MultiplaySession.Instance == null || MultiplaySession.Instance.CurrentRoom == null)
        {
            Debug.LogError("[PuzzleLoader] MultiplaySession or CurrentRoom is null!");
            callback?.Invoke(null);
            yield break;
        }

        // Get image URL from room data
        string imageUrl = MultiplaySession.Instance.CurrentRoom.imageUrl;
        string gameCode = MultiplaySession.Instance.CurrentRoom.sessionCode;

        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogError("[PuzzleLoader] No image URL in multiplay room data!");
            callback?.Invoke(null);
            yield break;
        }

        // Save game data
        GameData.gameCode = gameCode;
        GameData.imageUrl = imageUrl;

        Debug.Log($"[PuzzleLoader] Multiplay GameCode: {GameData.gameCode}");
        Debug.Log($"[PuzzleLoader] Multiplay ImageURL: {GameData.imageUrl}");

        // Download image
        Texture2D resultTexture = null;
        yield return StartCoroutine(DownloadImageFromURL(imageUrl, (texture) => resultTexture = texture));

        callback?.Invoke(resultTexture);
    }

    private IEnumerator DownloadImageFromURL(string url, System.Action<Texture2D> callback)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("[PuzzleLoader] Image URL is empty!");
            callback?.Invoke(null);
            yield break;
        }

        Debug.Log($"[PuzzleLoader] Downloading image from: {url}");

        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
                Debug.Log("[PuzzleLoader] Image download successful");
                callback?.Invoke(texture);
            }
            else
            {
                Debug.LogError($"[PuzzleLoader] Image download failed: {request.error}");
                callback?.Invoke(null);
            }
        }
    }

    private List<string> GetTags()
    {
        var tags = (UserSession.Instance != null && UserSession.Instance.Tags != null)
            ? UserSession.Instance.Tags
            : null;

        if (tags == null || tags.Count < 1)
        {
            Debug.LogWarning("[PuzzleLoader] No tags found. Using default tags.");
            tags = new List<string> { "default", "puzzle", "game", "art" };
        }

        return tags;
    }

    private Texture2D LoadFallbackImage()
    {
        // Check PuzzleManager's default image first
        if (puzzleManager != null && puzzleManager.puzzleImage != null)
        {
            Debug.Log("[PuzzleLoader] Using PuzzleManager default image");
            return puzzleManager.puzzleImage;
        }

        // Load from Resources
        var fallbackTexture = Resources.Load<Texture2D>(fallbackImagePath);
        if (fallbackTexture == null)
        {
            Debug.LogError($"[PuzzleLoader] Fallback image load failed: Resources/{fallbackImagePath}");
        }
        else
        {
            Debug.Log("[PuzzleLoader] Resources fallback image loaded");
        }
        return fallbackTexture;
    }

    private void InitializePuzzle(Texture2D texture)
    {
        if (puzzleManager == null)
        {
            Debug.LogError("[PuzzleLoader] PuzzleManager not assigned!");
            return;
        }

        int puzzleSize = GetPuzzleSize();
        puzzleManager.InitializePuzzle(texture, puzzleSize, puzzleSize);
        Debug.Log($"[PuzzleLoader] Puzzle initialized: {puzzleSize}x{puzzleSize}");
    }

    private int GetPuzzleSize()
    {
        int size = (GameData.difficulty < 2 || GameData.difficulty > 5) ? 3 : GameData.difficulty;
        Debug.Log($"[PuzzleLoader] Puzzle size: {size}x{size}");
        return size;
    }

    private void HandleLoadingFailure()
    {
        Debug.LogError("[PuzzleLoader] Complete loading failure!");
        // TODO: Show error popup or return to main menu
    }

    private void FinalizePuzzleLoading()
    {
        if (timerManagerObj != null)
            timerManagerObj.SetActive(true);
        if (backButtonObj != null)
            backButtonObj.SetActive(true);

        if (loadingPanelFade != null)
            loadingPanelFade.Hide();
        else if (loadingPanel != null)
            loadingPanel.SetActive(false);

        Debug.Log("[PuzzleLoader] Puzzle loading complete");
    }
}