using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class MyPlanetUIController : MonoBehaviour
{
    public bool isMine = true;

    [Header("그리드 크기 조절")]
    public GridContentResizer gridResizer;

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

    private PlanetDetailResponse currentPlanetData;
    private List<GalleryItem> galleryItems = new();

    void Start()
    {
        CloseContentPanels();
        CloseModalPanels();
        UpdateTopRightButtons();

        // 행성 정보 로드
        StartCoroutine(LoadPlanetData());
    }

    #region 행성 데이터 로드
    private IEnumerator LoadPlanetData()
    {
        // 현재 보고 있는 행성 ID 확인
        string targetPlanetId = GetTargetPlanetId();

        if (string.IsNullOrEmpty(targetPlanetId))
        {
            Debug.LogError("행성 ID를 찾을 수 없습니다.");
            yield break;
        }

        // 행성 상세 정보 조회
        yield return APIManager.Instance.Get(
            $"/planets/{targetPlanetId}",
            onSuccess: (response) =>
            {
                currentPlanetData = JsonUtility.FromJson<PlanetDetailResponse>(response);

                // 내 행성인지 확인
                isMine = (UserSession.Instance.UserID == currentPlanetData.ownerUsername);

                Debug.Log($"행성 로드 완료: {currentPlanetData.ownerNickname}, 내 행성: {isMine}");

                // UI 업데이트
                UpdateVisitorCount(currentPlanetData.visitCount);

                // 갤러리 로드
                StartCoroutine(LoadGallery());

                // 기본 패널 열기
                OpenGallery();
            },
            onError: (error) =>
            {
                Debug.LogError("행성 정보 로드 실패: " + error);
            }
        );
    }

    private string GetTargetPlanetId()
    {
        // PlanetSession에서 planetId 가져오기
        string planetId = PlanetSession.Instance?.CurrentPlanetId;

        // PlanetSession에 정보가 없으면 내 username 사용
        if (string.IsNullOrEmpty(planetId))
        {
            planetId = UserSession.Instance.UserID;  // username = planetId
        }

        return planetId;
    }
    #endregion

    #region 갤러리 로드
    private IEnumerator LoadGallery()
    {
        string planetId = currentPlanetData?.planetId;

        if (string.IsNullOrEmpty(planetId))
        {
            Debug.LogWarning("갤러리를 로드할 행성 ID가 없습니다.");
            yield break;
        }

        yield return APIManager.Instance.Get(
            $"/planets/{planetId}/gallery",
            onSuccess: (response) =>
            {
                GalleryListResponse galleryResponse = JsonUtility.FromJson<GalleryListResponse>(response);
                galleryItems = new List<GalleryItem>(galleryResponse.result);

                Debug.Log($"갤러리 로드 성공: {galleryItems.Count}개");

                RefreshGalleryUI();
            },
            onError: (error) =>
            {
                Debug.LogWarning("갤러리 로드 실패: " + error);
                galleryItems.Clear();
                RefreshGalleryUI();
            }
        );
    }

    private void RefreshGalleryUI()
    {
        // 기존 카드 삭제
        foreach (Transform child in galleryContainer)
        {
            Destroy(child.gameObject);
        }

        // 새 카드 생성
        foreach (var item in galleryItems)
        {
            var card = Instantiate(photoCardPrefab, galleryContainer);
            var photoCard = card.GetComponent<PhotoCard>();
            photoCard.Init(this, item);
        }

        // Content 사이즈 조정
        if (gridResizer != null)
            gridResizer.AdjustContentSize();
    }
    #endregion

    #region 패널 전환
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
            string planetId = currentPlanetData?.planetId;
            guestbook.LoadGuestbook(planetId);
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

    #region UI 버튼 핸들러
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
    #endregion
}