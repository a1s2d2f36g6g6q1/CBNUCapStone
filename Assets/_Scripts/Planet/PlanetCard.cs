using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetCard : MonoBehaviour
{
    public TMP_Text nicknameText;
    public Button visitButton;
    public Button bookmarkButton;
    public Image bookmarkIcon;

    private PlanetListItem planetData;
    private bool isBookmarked = false;
    private P001_Planet planetManager;
    private FadeController fadeController;

    private static readonly Color bookmarkedColor = new Color32(0xFA, 0xFF, 0x55, 0xFF);
    private static readonly Color unbookmarkedColor = Color.white;

    private void Awake()
    {
        fadeController = FindObjectOfType<FadeController>();
    }

    public void Init(PlanetListItem data, bool isFavorite, P001_Planet manager)
    {
        planetData = data;
        isBookmarked = isFavorite;
        planetManager = manager;

        // FIXED: API doesn't return ownerNickname, only username and title
        // Use title or username as display name
        nicknameText.text = string.IsNullOrEmpty(data.title) ? data.username : data.title;

        visitButton.onClick.RemoveAllListeners();
        visitButton.onClick.AddListener(OnClick_Visit);

        bookmarkButton.onClick.RemoveAllListeners();
        bookmarkButton.onClick.AddListener(OnClick_ToggleBookmark);

        UpdateBookmarkColor();
    }

    private void OnClick_Visit()
    {
        // FIXED: Use username field from API
        Debug.Log($"Visiting planet (username: {planetData.username})");

        // Save to PlanetSession
        if (PlanetSession.Instance != null)
        {
            PlanetSession.Instance.CurrentPlanetOwnerID = planetData.username;
            PlanetSession.Instance.CurrentPlanetId = planetData.username; // username = planetId
        }

        // Record visit
        StartCoroutine(RecordVisit(planetData.username));

        // Navigate to P002_MyPlanet
        fadeController.FadeToScene("P002_MyPlanet");
    }

    private System.Collections.IEnumerator RecordVisit(string ownerUsername)
    {
        yield return APIManager.Instance.Post(
            $"/planets/{ownerUsername}/visit",
            new { },
            onSuccess: (response) =>
            {
                Debug.Log($"Planet visit recorded for {planetData.username}");
            },
            onError: (error) =>
            {
                Debug.LogWarning("Planet visit record failed: " + error);
            }
        );
    }

    private void OnClick_ToggleBookmark()
    {
        if (planetManager != null)
        {
            // FIXED: Use username
            planetManager.ToggleFavorite(planetData.username, isBookmarked);
        }
    }

    private void UpdateBookmarkColor()
    {
        if (bookmarkIcon != null)
            bookmarkIcon.color = isBookmarked ? bookmarkedColor : unbookmarkedColor;
    }

    public bool IsBookmarked => isBookmarked;
    // FIXED: Use username or title
    public string PlanetOwner => planetData?.username ?? "";
}