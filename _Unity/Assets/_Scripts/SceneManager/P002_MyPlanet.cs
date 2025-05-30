using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class MyPlanetUIController : MonoBehaviour
{
    [Header("Gallery 설정")]
    public GameObject photoCardPrefab;
    public Transform galleryContainer;

    [Header("버튼 색상 설정")]
    public Color activeColor = Color.gray;
    public Color normalColor = Color.white;

    [Header("본문 패널 (Content Panels)")]
    public GameObject panelGallery;
    public GameObject panelPhoto;
    public GameObject panelGuestbook;

    [Header("모달 패널 (Modal Panels)")]
    public GameObject panelProfile;
    public GameObject panelSettings;

    [Header("사이드바")]
    public Button buttonProfileImage;
    public TMP_Text visitorCountText;
    public Button buttonGallery;
    public Button buttonGuestbook;

    [Header("TopRight 버튼")]
    public GameObject[] loginOnlyButtons;
    public Button buttonUserInfo;
    public Button buttonFriend;
    public Button buttonLogout;
    public Button buttonSettings;

    [Header("Fade")]
    public FadeController fadeController;

    private void Start()
    {
        CloseContentPanels();
        CloseModalPanels();
        OpenGallery();

        UpdateTopRightButtons();
        UpdateVisitorCount(300);

        // 테스트용: 카드 20장 생성
        for (int i = 1; i <= 20; i++)
        {
            string description = $"테스트 설명 {i}";
            string[] tags = new string[] { $"#태그{i}", "#샘플" };
            PlanetDataManager.Instance?.AddPhoto(description, tags);
        }

        LoadGallery();
    }
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

    public void OpenPhoto(PlanetDataManager.PlanetPhotoData data)
    {
        CloseContentPanels();
        panelPhoto.SetActive(true);
        UpdateSidebarButtonStates(null);

        var photoController = panelPhoto.GetComponent<PhotoPanelController>();
        if (photoController != null)
        {
            photoController.SetPhotoData(data);
        }
    }

    public void OpenGuestbook()
    {
        CloseContentPanels();
        panelGuestbook.SetActive(true);
        UpdateSidebarButtonStates(panelGuestbook);
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
        Debug.Log("로그아웃 완료");
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

    public void LoadGallery()
    {
        var photos = PlanetDataManager.Instance.GetAllPhotos();

        foreach (Transform child in galleryContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var photoData in photos)
        {
            var card = Instantiate(photoCardPrefab, galleryContainer);
            var photoCard = card.GetComponent<PhotoCard>();
            photoCard.Init(this, photoData);
        }
    }

    private Sprite LoadSpriteFromPath(string path)
    {
        if (!File.Exists(path)) return null;
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
}