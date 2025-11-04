using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class MyPlanetUIController : MonoBehaviour
{
    public bool isMine = true;

    [Header("Grid Size Adjust")]
    public GridContentResizer gridResizer;

    [Header("Gallery Settings")]
    public GameObject photoCardPrefab;
    public Transform galleryContainer;

    [Header("Button Colors")]
    public Color activeColor = Color.gray;
    public Color normalColor = Color.white;

    [Header("Content Panels")]
    public GameObject panelGallery;
    public GameObject panelPhoto;
    public GameObject panelGuestbook;

    [Header("Modal Panels")]
    public GameObject panelProfile;
    public GameObject panelSettings;

    [Header("Sidebar")]
    public Button buttonProfileImage;
    public TMP_Text visitorCountText;
    public Button buttonGallery;
    public Button buttonGuestbook;

    [Header("TopRight Buttons")]
    public GameObject[] loginOnlyButtons;
    public Button buttonUserInfo;
    public Button buttonFriend;
    public Button buttonLogout;
    public Button buttonSettings;

    [Header("Fade")]
    public FadeController fadeController;

    private List<GalleryItem> galleryItems = new();
    private string currentPlanetUsername; // Store username

    void Start()
    {
        CloseContentPanels();
        CloseModalPanels();
        UpdateTopRightButtons();

        // Load planet data
        StartCoroutine(LoadPlanetData());
    }

    #region Load Planet Data
    private IEnumerator LoadPlanetData()
    {
        // Check current planet username
        string targetUsername = GetTargetUsername();

        Debug.Log($"[MyPlanet] targetUsername: {targetUsername}");

        if (string.IsNullOrEmpty(targetUsername))
        {
            Debug.LogError("[MyPlanet] Cannot find username");
            yield break;
        }

        // Fetch planet details
        yield return APIManager.Instance.Get(
            $"/planets/{targetUsername}",
            onSuccess: (response) =>
            {
                Debug.Log($"[MyPlanet] API response: {response}");

                PlanetDetailResponse wrapper = JsonUtility.FromJson<PlanetDetailResponse>(response);

                if (wrapper != null && wrapper.result != null)
                {
                    PlanetDetail planet = wrapper.result;

                    // FIXED: Store username (not ownerUsername)
                    currentPlanetUsername = planet.username;

                    Debug.Log($"[MyPlanet] Parsed - username: {planet.username}, title: {planet.title}");

                    // FIXED: Check if this is my planet using username
                    isMine = (UserSession.Instance.UserID == planet.username);

                    Debug.Log($"[MyPlanet] Planet loaded: {planet.title}, isMine: {isMine}");

                    // Update UI
                    UpdateVisitorCount(planet.visitCount);

                    // FIXED: Load gallery using username
                    StartCoroutine(LoadGallery(planet.username));

                    // Open default panel
                    OpenGallery();
                }
                else
                {
                    Debug.LogError("[MyPlanet] Failed to parse planet data");
                }
            },
            onError: (error) =>
            {
                Debug.LogError("[MyPlanet] Failed to load planet info: " + error);
            }
        );
    }

    private string GetTargetUsername()
    {
        // Get username from PlanetSession
        string username = PlanetSession.Instance?.CurrentPlanetOwnerID;

        Debug.Log($"[MyPlanet] PlanetSession username: {username}");

        // Use my username if PlanetSession has no info
        if (string.IsNullOrEmpty(username))
        {
            username = UserSession.Instance.UserID;
            Debug.Log($"[MyPlanet] UserSession username: {username}");
        }

        return username;
    }
    #endregion

    #region Load Gallery
    private IEnumerator LoadGallery(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("[MyPlanet] No username to load gallery");
            yield break;
        }

        yield return APIManager.Instance.Get(
            $"/planets/{username}/gallery",
            onSuccess: (response) =>
            {
                Debug.Log($"[MyPlanet] Gallery API response: {response}");

                GalleryListResponse galleryResponse = JsonUtility.FromJson<GalleryListResponse>(response);

                if (galleryResponse != null && galleryResponse.result != null && galleryResponse.result.galleries != null)
                {
                    galleryItems = new List<GalleryItem>(galleryResponse.result.galleries);
                    Debug.Log($"[MyPlanet] Gallery loaded: {galleryItems.Count} items");
                }
                else
                {
                    Debug.LogWarning("[MyPlanet] Gallery result is null");
                    galleryItems.Clear();
                }

                RefreshGalleryUI();
            },
            onError: (error) =>
            {
                Debug.LogWarning("[MyPlanet] Failed to load gallery: " + error);
                galleryItems.Clear();
                RefreshGalleryUI();
            }
        );
    }

    private void RefreshGalleryUI()
    {
        // Clear existing cards
        foreach (Transform child in galleryContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new cards
        foreach (var item in galleryItems)
        {
            var card = Instantiate(photoCardPrefab, galleryContainer);
            var photoCard = card.GetComponent<PhotoCard>();
            photoCard.Init(this, item);
        }

        // Adjust content size
        if (gridResizer != null)
            gridResizer.AdjustContentSize();
    }
    #endregion

    #region Panel Switching
    private void UpdateSidebarButtonStates(GameObject activePanel)
    {
        buttonGallery.image.color = normalColor;
        buttonGuestbook.image.color = normalColor;

        if (activePanel == panelGallery)
            buttonGallery.image.color = activeColor;
        else if (activePanel == panelGuestbook)
            buttonGuestbook.image.color = activeColor;
    }

    public void CloseContentPanels()
    {
        panelGallery.SetActive(false);
        panelPhoto.SetActive(false);
        panelGuestbook.SetActive(false);
    }

    public void OpenGallery()
    {
        CloseContentPanels();
        panelGallery.SetActive(true);
        UpdateSidebarButtonStates(panelGallery);
    }

    public void OpenPhoto(GalleryItem item)
    {
        CloseContentPanels();
        panelPhoto.SetActive(true);
        UpdateSidebarButtonStates(null);

        var photoController = panelPhoto.GetComponent<PhotoPanelController>();
        if (photoController != null)
        {
            photoController.SetPhotoData(item, isMine);
        }
    }

    public void OpenGuestbook()
    {
        CloseContentPanels();
        panelGuestbook.SetActive(true);
        UpdateSidebarButtonStates(panelGuestbook);

        var guestbook = panelGuestbook.GetComponent<GuestbookUIController>();
        if (guestbook != null)
        {
            guestbook.LoadGuestbook(currentPlanetUsername);
        }
    }

    public void CloseModalPanels()
    {
        panelProfile.SetActive(false);
        panelSettings.SetActive(false);
    }

    public void OpenProfile()
    {
        CloseModalPanels();
        panelProfile.SetActive(true);
    }

    public void OpenSettings()
    {
        CloseModalPanels();
        panelSettings.SetActive(true);
    }
    #endregion

    #region UI Button Handlers
    public void OnClick_Back()
    {
        fadeController.FadeToScene("000_MainMenu");
    }

    public void OnClick_UserInfo()
    {
        OpenProfile();
    }

    public void OnClick_Friend()
    {
        fadeController.FadeToScene("F001_Friend");
    }

    public void OnClick_Logout()
    {
        UserSession.Instance.Logout();
        fadeController.FadeToScene("000_MainMenu");
        Debug.Log("[MyPlanet] Logout complete");
    }

    private void UpdateTopRightButtons()
    {
        bool isLoggedIn = UserSession.Instance != null && UserSession.Instance.IsLoggedIn;

        foreach (var go in loginOnlyButtons)
            go.SetActive(isLoggedIn);

        buttonSettings.gameObject.SetActive(true);
    }

    private void UpdateVisitorCount(int count)
    {
        visitorCountText.text = $"Visitor : {count}";
    }
    #endregion
}