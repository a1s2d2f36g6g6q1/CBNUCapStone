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

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;

    private string currentClearTime;
    private Texture2D currentPuzzleImage;
    private bool isUploading = false;
    private Vector3 originalScale;
    private bool gameCompleteRecorded = false;

    private void Start()
    {
        gameObject.SetActive(false);
        originalScale = transform.localScale;
        SetUploadStatus("_");
    }

    public void ShowClearPopup(Texture2D completedPuzzleImage, string clearTime)
    {
        currentClearTime = clearTime;
        currentPuzzleImage = completedPuzzleImage;
        gameObject.SetActive(true);
        StartCoroutine(ShowPopupWithDelay(completedPuzzleImage, clearTime));

        // Record single play completion
        RecordSingleGameComplete();
    }

    /// <summary>
    /// Record single play game completion
    /// </summary>
    private void RecordSingleGameComplete()
    {
        if (gameCompleteRecorded) return;

        // Skip for multiplayer
        bool isMultiplay = MultiplaySession.Instance != null &&
                           MultiplaySession.Instance.CurrentRoom != null;

        if (isMultiplay)
        {
            Debug.Log("[ClearPopup] Multiplay mode - skip completion record");
            return;
        }

        if (SinglePlayGameManager.Instance == null)
        {
            Debug.LogWarning("[ClearPopup] SinglePlayGameManager not found");
            return;
        }

        gameCompleteRecorded = true;

        SinglePlayGameManager.Instance.CompleteSingleGame(
            onSuccess: (result) =>
            {
                Debug.Log($"[ClearPopup] Game completion recorded - Clear time: {result.clearTimeMs}ms");
            },
            onError: (error) =>
            {
                Debug.LogWarning($"[ClearPopup] Game completion record failed: {error}");
            }
        );
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
        if (clearImage != null && completedPuzzleImage != null)
        {
            var sprite = Sprite.Create(completedPuzzleImage,
                new Rect(0, 0, completedPuzzleImage.width, completedPuzzleImage.height),
                new Vector2(0.5f, 0.5f));
            clearImage.sprite = sprite;
        }

        if (titleInputField != null)
            titleInputField.text = "";

        if (descriptionInputField != null)
            descriptionInputField.text = "";

        SetUploadStatus("_");

        if (uploadButton != null)
        {
            uploadButton.interactable = true;
            var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = "Upload";
        }

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

        SetDefaultValuesIfEmpty();
        StartCoroutine(UploadCoroutine());
    }

    private void SetDefaultValuesIfEmpty()
    {
        if (titleInputField != null && string.IsNullOrEmpty(titleInputField.text.Trim()))
        {
            var now = System.DateTime.Now;
            titleInputField.text = $"{now.Year:D4}. {now.Month:D2}. {now.Day:D2}. {now.Hour:D2}:{now.Minute:D2}";
        }

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

        // ✅ Check if user can save to planet (logged-in non-guest users only)
        if (UserSession.Instance == null || !UserSession.Instance.CanSaveToPlanet())
        {
            string message = "";

            if (UserSession.Instance != null && UserSession.Instance.IsGuest)
            {
                message = "[ClearPopup] Guest cannot upload to planet - Login required";
            }
            else
            {
                message = "[ClearPopup] Not logged in - cannot upload to planet";
            }

            Debug.LogError(message);
            SetUploadStatus("Error");

            if (uploadButton != null)
            {
                uploadButton.interactable = true;
                var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = "Login Required";
            }

            isUploading = false;
            yield break;
        }

        // Save to planet using /games/single/save-to-planet API
        yield return SaveToPlanet();

        isUploading = false;
    }

    /// <summary>
    /// Save game image to planet using /games/single/save-to-planet API
    /// </summary>
    private IEnumerator SaveToPlanet()
    {
        if (SinglePlayGameManager.Instance == null)
        {
            Debug.LogError("[ClearPopup] SinglePlayGameManager not found");
            SetUploadStatus("Error");
            yield break;
        }

        // Get title from input field
        string title = titleInputField != null ? titleInputField.text.Trim() : "";
        if (string.IsNullOrEmpty(title))
        {
            var now = System.DateTime.Now;
            title = $"Puzzle Clear {now.Year:D4}-{now.Month:D2}-{now.Day:D2}";
        }

        Debug.Log($"[ClearPopup] Saving to planet with title: {title}");

        bool saveComplete = false;
        bool saveSuccess = false;

        SinglePlayGameManager.Instance.SaveToPlanet(
            title,
            onSuccess: (result) =>
            {
                Debug.Log($"[ClearPopup] Save to planet success");
                Debug.Log($"[ClearPopup] Gallery title: {result.galleryTitle}");
                saveSuccess = true;
                saveComplete = true;
            },
            onError: (error) =>
            {
                Debug.LogError($"[ClearPopup] Save to planet failed: {error}");
                saveSuccess = false;
                saveComplete = true;
            }
        );

        yield return new WaitUntil(() => saveComplete);

        if (saveSuccess)
        {
            SetUploadStatus("Checked");

            if (uploadButton != null)
            {
                var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = "Upload Complete";
            }

            Debug.Log("[ClearPopup] Upload completed successfully");
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
        // Clear game data before scene transition
        if (SinglePlayGameManager.Instance != null)
        {
            SinglePlayGameManager.Instance.ClearGameData();
        }

        if (fadeController != null)
        {
            fadeController.FadeToScene("000_MainMenu");
        }
        else
        {
            Debug.LogWarning("[ClearPopup] FadeController not assigned");
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