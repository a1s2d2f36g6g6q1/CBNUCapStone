using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClearPopup : MonoBehaviour
{
    [Header("UI References")]
    public Button exitButton;
    public Image clearImage;
    public TMP_InputField titleInputField;
    public TMP_InputField descriptionInputField;
    public Button uploadButton;
    public GameObject uploadStatusDefault;
    public GameObject uploadStatusChecked;
    public GameObject uploadStatusError;
    public FadeController fadeController;

    [Header("Upload Settings")]
    public float uploadSimulationTime = 2f;

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;

    [Header("Login Check UI")]
    public TMP_Text loginRequiredText; // "Login required to upload"

    private string currentClearTime;
    private bool isUploading = false;
    private Vector3 originalScale;
    private bool canUpload = false;

    private void Start()
    {
        gameObject.SetActive(false);
        originalScale = transform.localScale;
        SetUploadStatus("_");
    }

    public void ShowClearPopup(Texture2D completedPuzzleImage, string clearTime)
    {
        currentClearTime = clearTime;

        gameObject.SetActive(true);

        StartCoroutine(ShowPopupWithDelay(completedPuzzleImage, clearTime));
    }

    private IEnumerator ShowPopupWithDelay(Texture2D completedPuzzleImage, string clearTime)
    {
        var canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        yield return new WaitForSeconds(0.1f);

        PreparePopupContent(completedPuzzleImage, clearTime);

        yield return StartCoroutine(ShowPopupFromBackAnimation());
    }

    private void PreparePopupContent(Texture2D completedPuzzleImage, string clearTime)
    {
        // Set clear image
        if (clearImage != null && completedPuzzleImage != null)
        {
            var sprite = Sprite.Create(completedPuzzleImage,
                new Rect(0, 0, completedPuzzleImage.width, completedPuzzleImage.height),
                new Vector2(0.5f, 0.5f));
            clearImage.sprite = sprite;
        }

        // Initialize input fields
        if (titleInputField != null)
            titleInputField.text = "";

        if (descriptionInputField != null)
            descriptionInputField.text = "";

        // Check if replay mode (gallery replay cannot upload)
        bool isReplayMode = PuzzleSession.Instance != null && PuzzleSession.Instance.IsReplayMode();

        // Check login status (guest cannot upload)
        bool isLoggedIn = UserSession.Instance != null && UserSession.Instance.IsLoggedIn;
        bool isGuest = UserSession.Instance != null && UserSession.Instance.IsGuest;

        // Can upload only if: logged in, not guest, and not replay mode
        canUpload = isLoggedIn && !isGuest && !isReplayMode;

        // Update upload button state
        if (uploadButton != null)
        {
            uploadButton.interactable = canUpload;
            var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                if (isReplayMode)
                    buttonText.text = "Replay Mode - Upload Disabled";
                else if (isGuest)
                    buttonText.text = "Login Required";
                else if (!isLoggedIn)
                    buttonText.text = "Login Required";
                else
                    buttonText.text = "Upload";
            }
        }

        // Show/hide login required message
        if (loginRequiredText != null)
        {
            loginRequiredText.gameObject.SetActive(!canUpload);
            if (!canUpload)
            {
                if (isReplayMode)
                    loginRequiredText.text = "Replay mode - Upload not available";
                else if (isGuest)
                    loginRequiredText.text = "Can't upload - Please login with real account";
                else
                    loginRequiredText.text = "Can't upload - Login required";
            }
        }

        SetUploadStatus("_");

        // Button events
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitClick);
        }

        if (uploadButton != null)
        {
            uploadButton.onClick.RemoveAllListeners();
            uploadButton.onClick.AddListener(OnUploadClick);
        }
    }

    private IEnumerator ShowPopupFromBackAnimation()
    {
        transform.localScale = originalScale;
        transform.localRotation = Quaternion.identity;

        var canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        Vector3 originalPos = transform.localPosition;
        Vector3 startPos = new Vector3(originalPos.x, -500f, originalPos.z);
        Vector3 endPos = new Vector3(originalPos.x, 0f, originalPos.z);

        transform.localPosition = startPos;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            t = 1f - (1f - t) * (1f - t) * (1f - t) * (1f - t);

            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.localPosition = endPos;
    }

    public void OnUploadClick()
    {
        if (isUploading) return;

        if (!canUpload)
        {
            Debug.LogWarning("[ClearPopup] Cannot upload - not logged in or replay mode");
            SetUploadStatus("Error");
            if (loginRequiredText != null)
            {
                bool isReplayMode = PuzzleSession.Instance != null && PuzzleSession.Instance.IsReplayMode();
                if (isReplayMode)
                    loginRequiredText.text = "Replay mode - Upload not available";
                else
                    loginRequiredText.text = "Can't upload - Login required";
                loginRequiredText.gameObject.SetActive(true);
            }
            return;
        }

        SetDefaultValuesIfEmpty();

        StartCoroutine(UploadCoroutine());
    }

    private void SetDefaultValuesIfEmpty()
    {
        // Set default title if empty
        if (titleInputField != null && string.IsNullOrEmpty(titleInputField.text.Trim()))
        {
            var now = System.DateTime.Now;
            titleInputField.text = $"{now.Year:D4}. {now.Month:D2}. {now.Day:D2}. {now.Hour:D2}:{now.Minute:D2}";
        }

        // Set default description if empty
        if (descriptionInputField != null && string.IsNullOrEmpty(descriptionInputField.text.Trim()))
        {
            var tags = GetUsedTags();
            if (tags.Count > 0)
            {
                var tagText = "";
                for (int i = 0; i < Mathf.Min(4, tags.Count); i++)
                {
                    tagText += $"[{tags[i]}]";
                    if (i < Mathf.Min(3, tags.Count - 1))
                        tagText += ", ";
                }
                descriptionInputField.text = tagText;
            }
        }
    }

    private List<string> GetUsedTags()
    {
        if (UserSession.Instance != null && UserSession.Instance.Tags != null)
        {
            return UserSession.Instance.Tags;
        }

        return new List<string> { "puzzle", "game", "clear", "achievement" };
    }

    private IEnumerator UploadCoroutine()
    {
        isUploading = true;

        SetUploadStatus("_");

        if (uploadButton != null)
        {
            uploadButton.interactable = false;
            var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = "Uploading...";
        }

        Debug.Log("[ClearPopup] Starting upload to planet...");

        SaveToPlanetRequest request = new SaveToPlanetRequest
        {
            gameCode = GameData.gameCode,
            title = titleInputField.text.Trim()
        };

        bool uploadCompleted = false;
        bool uploadSuccess = false;

        yield return APIManager.Instance.Post(
            "/games/single/save-to-planet",
            request,
            onSuccess: (response) =>
            {
                Debug.Log($"[ClearPopup] Upload response: {response}");

                SaveToPlanetResponse apiResponse = JsonUtility.FromJson<SaveToPlanetResponse>(response);

                if (apiResponse != null && apiResponse.result != null)
                {
                    Debug.Log($"[ClearPopup] Upload successful! PlanetId: {apiResponse.result.planetId}");
                    uploadSuccess = true;
                }
                else
                {
                    Debug.LogError("[ClearPopup] Failed to parse upload response");
                    uploadSuccess = false;
                }

                uploadCompleted = true;
            },
            onError: (error) =>
            {
                Debug.LogError($"[ClearPopup] Upload failed: {error}");
                uploadSuccess = false;
                uploadCompleted = true;
            }
        );

        // Wait for completion
        while (!uploadCompleted)
        {
            yield return null;
        }

        if (uploadSuccess)
        {
            SetUploadStatus("Checked");

            if (uploadButton != null)
            {
                var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = "Upload Complete";
            }

            Debug.Log($"[ClearPopup] Upload completed - Title: {titleInputField.text}");
        }
        else
        {
            SetUploadStatus("Error");

            if (uploadButton != null)
            {
                uploadButton.interactable = true;
                var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = "Retry Upload";
            }

            Debug.LogWarning("[ClearPopup] Upload failed");
        }

        isUploading = false;
    }

    private void SetUploadStatus(string status)
    {
        if (uploadStatusDefault != null)
            uploadStatusDefault.SetActive(false);
        if (uploadStatusChecked != null)
            uploadStatusChecked.SetActive(false);
        if (uploadStatusError != null)
            uploadStatusError.SetActive(false);

        switch (status)
        {
            case "_":
                if (uploadStatusDefault != null)
                    uploadStatusDefault.SetActive(true);
                break;
            case "Checked":
                if (uploadStatusChecked != null)
                    uploadStatusChecked.SetActive(true);
                break;
            case "Error":
                if (uploadStatusError != null)
                    uploadStatusError.SetActive(true);
                break;
        }
    }

    public void OnExitClick()
    {
        // Clear replay data if in replay mode
        if (PuzzleSession.Instance != null && PuzzleSession.Instance.IsReplayMode())
        {
            PuzzleSession.Instance.ClearReplayData();
        }

        if (fadeController != null)
        {
            fadeController.FadeToScene("000_MainMenu");
        }
        else
        {
            Debug.LogWarning("FadeController is not assigned.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("000_MainMenu");
        }
    }

    private void OnDestroy()
    {
        if (uploadButton != null)
            uploadButton.onClick.RemoveAllListeners();

        if (exitButton != null)
            exitButton.onClick.RemoveAllListeners();
    }
}