using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayRankPopup : MonoBehaviour
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

    [Header("Player Rank Display")]
    public TMP_Text[] playerNames; // 4개 (Player1~4)
    public TMP_Text[] playerTimes; // 4개 (Player1~4)

    [Header("Rank Result Display")]
    public TMP_Text rankResultText; // "You finished 1st!" / "You finished 2nd!" etc.

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;

    private string currentClearTime;
    private bool isUploading = false;
    private Vector3 originalScale;
    private bool isWinner = false;
    private int myRank = 0;

    private void Start()
    {
        gameObject.SetActive(false);
        originalScale = transform.localScale;
        SetUploadStatus("_");
    }

    public void ShowRankPopup(Texture2D completedPuzzleImage, string myClearTime)
    {
        currentClearTime = myClearTime;

        gameObject.SetActive(true);

        StartCoroutine(ShowPopupWithDelay(completedPuzzleImage, myClearTime));
    }

    private IEnumerator ShowPopupWithDelay(Texture2D completedPuzzleImage, string myClearTime)
    {
        var canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        yield return new WaitForSeconds(0.1f);

        PreparePopupContent(completedPuzzleImage, myClearTime);

        yield return StartCoroutine(ShowPopupFromBackAnimation());

        // Subscribe to multiplay events
        SubscribeMultiplayEvents();

        // Initial rank update
        UpdateRankDisplay();
    }

    private void PreparePopupContent(Texture2D completedPuzzleImage, string myClearTime)
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

        // Initially disable upload button
        if (uploadButton != null)
        {
            uploadButton.interactable = false;
            var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = "Waiting...";
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

    #region Rank Display Update
    private void SubscribeMultiplayEvents()
    {
        if (MultiplaySession.Instance != null)
        {
            MultiplaySession.Instance.OnRoomDataUpdated += OnRoomDataUpdated;
        }
    }

    private void UnsubscribeMultiplayEvents()
    {
        if (MultiplaySession.Instance != null)
        {
            MultiplaySession.Instance.OnRoomDataUpdated -= OnRoomDataUpdated;
        }
    }

    private void OnRoomDataUpdated(RoomData roomData)
    {
        UpdateRankDisplay();
    }

    private void UpdateRankDisplay()
    {
        if (MultiplaySession.Instance == null || MultiplaySession.Instance.CurrentRoom == null)
        {
            Debug.LogWarning("[MultiplayRankPopup] No room data");
            return;
        }

        var players = MultiplaySession.Instance.CurrentRoom.players;
        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("[MultiplayRankPopup] No players in room");
            return;
        }

        Debug.Log($"[MultiplayRankPopup] Updating rank display with {players.Count} players");

        // Sort players by clear time (cleared players first, then by time)
        var sortedPlayers = new List<PlayerData>(players);
        sortedPlayers.Sort((a, b) =>
        {
            // Not cleared players go to end
            if (a.clearTime <= 0 && b.clearTime <= 0) return 0;
            if (a.clearTime <= 0) return 1;
            if (b.clearTime <= 0) return -1;

            // Both cleared: sort by time
            return a.clearTime.CompareTo(b.clearTime);
        });

        // Update UI
        for (int i = 0; i < 4; i++)
        {
            if (i < sortedPlayers.Count)
            {
                var player = sortedPlayers[i];

                Debug.Log($"[MultiplayRankPopup] Slot {i}: {player.nickname}, Time: {player.clearTime}");

                // Update name
                if (playerNames != null && playerNames.Length > i && playerNames[i] != null)
                {
                    string displayName = player.nickname;
                    if (player.isHost)
                    {
                        displayName += " [Host]";
                    }
                    playerNames[i].text = displayName;
                }

                // Update time
                if (playerTimes != null && playerTimes.Length > i && playerTimes[i] != null)
                {
                    if (player.clearTime > 0)
                    {
                        playerTimes[i].text = FormatTime(player.clearTime);
                    }
                    else
                    {
                        playerTimes[i].text = "Playing...";
                    }
                }

                // Check if this is me
                if (player.userId == MultiplaySession.Instance.MyUserId)
                {
                    myRank = i + 1;
                    isWinner = (i == 0);
                    Debug.Log($"[MultiplayRankPopup] My rank: {myRank}, Winner: {isWinner}");
                }
            }
            else
            {
                // Empty slot
                if (playerNames != null && playerNames.Length > i && playerNames[i] != null)
                {
                    playerNames[i].text = "Empty";
                }

                if (playerTimes != null && playerTimes.Length > i && playerTimes[i] != null)
                {
                    playerTimes[i].text = "-";
                }
            }
        }

        // Update rank result text
        UpdateRankResultText();

        // Check if all players finished
        CheckAllPlayersFinished(sortedPlayers);
    }
    private void UpdateRankResultText()
    {
        if (rankResultText == null) return;

        if (myRank == 0)
        {
            rankResultText.text = "Waiting for results...";
            return;
        }

        string suffix = "th";
        if (myRank == 1) suffix = "st";
        else if (myRank == 2) suffix = "nd";
        else if (myRank == 3) suffix = "rd";

        rankResultText.text = $"You finished {myRank}{suffix}!";
    }

    private void CheckAllPlayersFinished(List<PlayerData> players)
    {
        Debug.Log("[MultiplayRankPopup] Checking if all players finished...");

        bool allFinished = true;
        int finishedCount = 0;

        foreach (var player in players)
        {
            Debug.Log($"[MultiplayRankPopup] Player {player.nickname}: clearTime={player.clearTime}");

            if (player.clearTime > 0)
            {
                finishedCount++;
            }
            else
            {
                allFinished = false;
            }
        }

        Debug.Log($"[MultiplayRankPopup] {finishedCount}/{players.Count} players finished");

        if (allFinished && finishedCount > 0)
        {
            Debug.Log("[MultiplayRankPopup] All players finished!");

            // Enable upload for winner
            if (isWinner && myRank == 1)
            {
                if (uploadButton != null)
                {
                    uploadButton.interactable = true;
                    var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                        buttonText.text = "Upload to Planet";
                }
                Debug.Log("[MultiplayRankPopup] Upload button enabled for winner");
            }
            else
            {
                if (uploadButton != null)
                {
                    uploadButton.interactable = false;
                    var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                        buttonText.text = "Only Winner Can Upload";
                }
                Debug.Log("[MultiplayRankPopup] Upload button disabled (not winner)");
            }
        }
        else
        {
            Debug.Log("[MultiplayRankPopup] Waiting for other players...");

            if (uploadButton != null)
            {
                uploadButton.interactable = false;
                var buttonText = uploadButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = $"Waiting... ({finishedCount}/{players.Count})";
            }
        }
    }
    private string FormatTime(float timeInSeconds)
    {
        int minutes = (int)(timeInSeconds / 60f);
        int seconds = (int)(timeInSeconds % 60f);
        int milliseconds = (int)(timeInSeconds * 1000f % 1000);

        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }
    #endregion

    #region Upload
    public void OnUploadClick()
    {
        if (isUploading) return;

        if (!isWinner)
        {
            Debug.LogWarning("[MultiplayRankPopup] Only winner can upload!");
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

        return new List<string> { "multiplay", "puzzle", "victory", "winner" };
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

        Debug.Log("[MultiplayRankPopup] Starting upload to planet...");

        SaveToPlanetRequest request = new SaveToPlanetRequest
        {
            gameCode = GameData.gameCode,
            title = titleInputField.text.Trim()
        };

        bool uploadCompleted = false;
        bool uploadSuccess = false;

        yield return APIManager.Instance.Post(
            "/games/multiplay/save-to-planet",
            request,
            onSuccess: (response) =>
            {
                Debug.Log($"[MultiplayRankPopup] Upload response: {response}");

                SaveToPlanetResponse apiResponse = JsonUtility.FromJson<SaveToPlanetResponse>(response);

                if (apiResponse != null && apiResponse.result != null)
                {
                    Debug.Log($"[MultiplayRankPopup] Upload successful! PlanetId: {apiResponse.result.planetId}");
                    uploadSuccess = true;
                }
                else
                {
                    Debug.LogError("[MultiplayRankPopup] Failed to parse upload response");
                    uploadSuccess = false;
                }

                uploadCompleted = true;
            },
            onError: (error) =>
            {
                Debug.LogError($"[MultiplayRankPopup] Upload failed: {error}");
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

            Debug.Log($"[MultiplayRankPopup] Upload completed - Title: {titleInputField.text}");
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

            Debug.LogWarning("[MultiplayRankPopup] Upload failed");
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
    #endregion

    public void OnExitClick()
    {
        // Unsubscribe events
        UnsubscribeMultiplayEvents();

        // Clear multiplay session
        if (MultiplaySession.Instance != null)
        {
            MultiplaySession.Instance.ClearRoomData();
        }

        // Disconnect websocket
        if (SocketIOManager.Instance != null && SocketIOManager.Instance.IsConnected)
        {
            SocketIOManager.Instance.UnregisterMultiplayEvents();
            SocketIOManager.Instance.Disconnect();
        }

        // Return to main menu
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
        UnsubscribeMultiplayEvents();

        if (uploadButton != null)
            uploadButton.onClick.RemoveAllListeners();

        if (exitButton != null)
            exitButton.onClick.RemoveAllListeners();
    }
}