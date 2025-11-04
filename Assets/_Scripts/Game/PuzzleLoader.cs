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
    public float aiImageTimeout = 60f;
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
        var tags = GetTags();
        Texture2D puzzleTexture = null;

        bool isMultiplay = MultiplaySession.Instance != null &&
                           MultiplaySession.Instance.CurrentRoom != null;

        if (isMultiplay)
        {
            Debug.Log("[PuzzleLoader] Multiplay mode");

            string sharedUrl = MultiplaySession.Instance.SharedImageUrl;

            if (!string.IsNullOrEmpty(sharedUrl))
            {
                Debug.Log($"[PuzzleLoader] Using shared URL: {sharedUrl}");
                yield return StartCoroutine(DownloadImageFromUrl(sharedUrl, (result) => puzzleTexture = result));
            }
            else
            {
                Debug.LogWarning("[PuzzleLoader] No shared URL. Generating...");
                yield return StartCoroutine(TryGenerateAIImage(tags, (result) => puzzleTexture = result));
            }
        }
        else
        {
            Debug.Log("[PuzzleLoader] Single play mode");
            yield return StartCoroutine(TryGenerateAIImage(tags, (result) => puzzleTexture = result));
        }

        if (puzzleTexture == null)
        {
            Debug.LogWarning("[PuzzleLoader] Using fallback image");
            puzzleTexture = LoadFallbackImage();
        }

        if (puzzleTexture != null)
        {
            InitializePuzzle(puzzleTexture);
        }
        else
        {
            Debug.LogError("[PuzzleLoader] All image loading failed!");
            HandleLoadingFailure();
        }

        FinalizePuzzleLoading();
    }

    private IEnumerator DownloadImageFromUrl(string url, System.Action<Texture2D> callback)
    {
        Debug.Log($"[PuzzleLoader] Downloading: {url}");

        yield return APIManager.Instance.DownloadImage(
            url,
            onSuccess: (texture) =>
            {
                Debug.Log($"[PuzzleLoader] Downloaded: {texture.width}x{texture.height}");
                callback?.Invoke(texture);
            },
            onError: (error) =>
            {
                Debug.LogError($"[PuzzleLoader] Download failed: {error}");
                callback?.Invoke(null);
            }
        );
    }

    private List<string> GetTags()
    {
        var tags = (UserSession.Instance != null && UserSession.Instance.Tags != null)
            ? UserSession.Instance.Tags
            : null;

        if (tags == null || tags.Count < 4)
        {
            Debug.LogWarning("[PuzzleLoader] Invalid tags. Using default tags.");
            tags = new List<string> { "default", "test", "puzzle", "image" };
        }

        return tags;
    }

    private IEnumerator TryGenerateAIImage(List<string> tags, System.Action<Texture2D> callback)
    {
        if (SinglePlayGameManager.Instance == null)
        {
            Debug.LogWarning("[PuzzleLoader] SinglePlayGameManager not found!");
            callback?.Invoke(null);
            yield break;
        }

        bool isDone = false;
        string imageUrl = null;

        SinglePlayGameManager.Instance.StartSingleGame(
            tags.ToArray(),
            onSuccess: (url) =>
            {
                imageUrl = url;
                isDone = true;
            },
            onError: (error) =>
            {
                Debug.LogError($"[PuzzleLoader] Single game start error: {error}");
                isDone = true;
            }
        );

        // Wait with timeout
        float elapsed = 0f;
        while (!isDone && elapsed < aiImageTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!isDone)
        {
            Debug.LogWarning("[PuzzleLoader] Single game start timeout");
            callback?.Invoke(null);
            yield break;
        }

        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogWarning("[PuzzleLoader] No image URL returned");
            callback?.Invoke(null);
            yield break;
        }

        // Download image
        Texture2D texture = null;
        yield return DownloadImageFromUrl(imageUrl, (result) => texture = result);
        callback?.Invoke(texture);
    }

    private Texture2D LoadFallbackImage()
    {
        if (puzzleManager != null && puzzleManager.puzzleImage != null)
        {
            Debug.Log("[PuzzleLoader] Using PuzzleManager default image");
            return puzzleManager.puzzleImage;
        }

        var fallbackTexture = Resources.Load<Texture2D>(fallbackImagePath);
        if (fallbackTexture == null)
        {
            Debug.LogError($"[PuzzleLoader] Failed to load fallback image: Resources/{fallbackImagePath}");
        }
        else
        {
            Debug.Log("[PuzzleLoader] Fallback image loaded from Resources");
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
        Debug.LogError("[PuzzleLoader] Puzzle loading completely failed!");
        // TODO: Show error popup
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