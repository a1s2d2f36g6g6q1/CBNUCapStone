using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class PhotoPanelController : MonoBehaviour
{
    [Header("UI 요소")]
    public Image photoImage;                 // 이미지 표시용
    public TMP_Text tagText;                 // 태그 표시
    public TMP_Text createdAtText;           // 생성 날짜
    public GameObject loadingIndicator;      // 로딩 표시

    [Header("버튼")]
    public Button backButton;                // 갤러리 목록으로 돌아가기
    public Button downloadButton;            // 이미지 다운로드
    public Button favoriteButton;            // 즐겨찾기
    public Button playButton;                // 퍼즐 플레이

    [Header("즐겨찾기 아이콘")]
    public Image favoriteIcon;               // 즐겨찾기 아이콘 이미지
    private static readonly Color favoriteColor = new Color32(0xFF, 0x69, 0xB4, 0xFF); // 핫핑크
    private static readonly Color normalColor = Color.white;

    [Header("다운로드 버튼")]
    public TMP_Text downloadButtonText;      // 다운로드 버튼 텍스트

    private GalleryDetailItem currentDetailItem;
    private Texture2D currentTexture;        // 다운로드용 텍스처 저장
    private string currentUsername;
    private string currentImageId;
    private bool isFavorited = false;        // 즐겨찾기 상태
    private bool isDownloaded = false;       // 다운로드 완료 상태
    private MyPlanetUIController planetUIController;

    private void Awake()
    {
        // 버튼 이벤트 연결
        if (backButton != null)
            backButton.onClick.AddListener(OnClick_Back);

        if (downloadButton != null)
            downloadButton.onClick.AddListener(OnClick_Download);

        if (favoriteButton != null)
            favoriteButton.onClick.AddListener(OnClick_ToggleFavorite);

        if (playButton != null)
            playButton.onClick.AddListener(OnClick_Play);
    }

    /// <summary>
    /// 갤러리 아이템 클릭 시 호출 - 상세 정보를 API에서 가져옴
    /// </summary>
    public void LoadPhotoDetail(string username, string imageId, MyPlanetUIController controller)
    {
        this.currentUsername = username;
        this.currentImageId = imageId;
        this.planetUIController = controller;

        // Initialize states
        InitializeStates();

        StartCoroutine(LoadPhotoDetailCoroutine(username, imageId));
    }

    /// <summary>
    /// 사진 상세 진입 시 상태 초기화
    /// </summary>
    private void InitializeStates()
    {
        // 즐겨찾기 상태 초기화
        isFavorited = false;
        UpdateFavoriteIcon();

        // 다운로드 상태 초기화
        isDownloaded = false;
        UpdateDownloadButtonText();

        Debug.Log("[PhotoPanel] States initialized - Favorite: false, Downloaded: false");
    }

    private IEnumerator LoadPhotoDetailCoroutine(string username, string imageId)
    {
        SetLoadingState(true);

        yield return APIManager.Instance.Get(
            $"/planets/{username}/gallery/{imageId}",
            onSuccess: (response) =>
            {
                Debug.Log($"[갤러리 상세 API 응답] {response}");

                GalleryDetailResponse detailResponse = JsonUtility.FromJson<GalleryDetailResponse>(response);

                if (detailResponse != null && detailResponse.result != null)
                {
                    currentDetailItem = detailResponse.result;
                    UpdatePhotoUI();

                    // Check if this image is favorited
                    CheckFavoriteStatus();
                }
                else
                {
                    Debug.LogError("갤러리 상세 정보 파싱 실패");
                }

                SetLoadingState(false);
            },
            onError: (error) =>
            {
                Debug.LogError("갤러리 상세 정보 로드 실패: " + error);
                SetLoadingState(false);
            }
        );
    }

    /// <summary>
    /// Check if this planet/image is in favorites
    /// </summary>
    private void CheckFavoriteStatus()
    {
        // Currently, API doesn't support checking individual gallery favorites
        // So we check if the planet owner is favorited instead
        StartCoroutine(CheckPlanetFavoriteCoroutine());
    }

    private IEnumerator CheckPlanetFavoriteCoroutine()
    {
        if (UserSession.Instance == null || !UserSession.Instance.IsLoggedIn)
        {
            // Not logged in - cannot favorite
            isFavorited = false;
            UpdateFavoriteIcon();
            yield break;
        }

        yield return APIManager.Instance.Get(
            "/planets/favorites/me",
            onSuccess: (response) =>
            {
                FavoriteListResponse favoriteResponse = JsonUtility.FromJson<FavoriteListResponse>(response);

                if (favoriteResponse != null && favoriteResponse.result != null)
                {
                    // Check if current planet owner is in favorites
                    bool found = false;
                    foreach (var planet in favoriteResponse.result)
                    {
                        if (planet.ownerUsername == currentUsername)
                        {
                            found = true;
                            break;
                        }
                    }

                    isFavorited = found;
                    UpdateFavoriteIcon();

                    Debug.Log($"[PhotoPanel] Favorite status checked - isFavorited: {isFavorited}");
                }
            },
            onError: (error) =>
            {
                Debug.LogError("즐겨찾기 상태 확인 실패: " + error);
                isFavorited = false;
                UpdateFavoriteIcon();
            }
        );
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdatePhotoUI()
    {
        if (currentDetailItem == null)
            return;

        // 태그 설정
        if (tagText != null && currentDetailItem.metadata != null && currentDetailItem.metadata.tags != null)
        {
            tagText.text = string.Join(" / ", currentDetailItem.metadata.tags);
        }

        // 생성 날짜 설정
        if (createdAtText != null && currentDetailItem.metadata != null)
        {
            createdAtText.text = FormatDateTime(currentDetailItem.metadata.generatedAt);
        }

        // 이미지 로드
        if (photoImage != null && !string.IsNullOrEmpty(currentDetailItem.image_url))
        {
            StartCoroutine(LoadImageFromURL(currentDetailItem.image_url));
        }
    }

    /// <summary>
    /// URL에서 이미지 로드
    /// </summary>
    private IEnumerator LoadImageFromURL(string url)
    {
        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            currentTexture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
            photoImage.sprite = Sprite.Create(currentTexture, new Rect(0, 0, currentTexture.width, currentTexture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogError("이미지 로드 실패: " + request.error);
        }
    }

    #region 버튼 이벤트

    /// <summary>
    /// 1. Back 버튼 - 갤러리 목록으로 돌아가기
    /// </summary>
    private void OnClick_Back()
    {
        if (planetUIController != null)
        {
            planetUIController.OpenGallery();
        }
        else
        {
            Debug.LogWarning("MyPlanetUIController 참조가 없습니다.");
        }
    }

    /// <summary>
    /// 2. 다운로드 버튼 - 이미지 다운로드
    /// </summary>
    private void OnClick_Download()
    {
        if (currentTexture == null)
        {
            Debug.LogWarning("다운로드할 이미지가 없습니다.");
            return;
        }

        // Prevent multiple downloads
        if (isDownloaded)
        {
            Debug.Log("[PhotoPanel] Image already downloaded");
            return;
        }

        StartCoroutine(DownloadImage());
    }

    private IEnumerator DownloadImage()
    {
        // PNG로 인코딩
        byte[] bytes = currentTexture.EncodeToPNG();

        // 파일명 생성 (타임스탬프 사용)
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"Puzzle13_{timestamp}.png";

#if UNITY_EDITOR
        // 에디터에서는 프로젝트 폴더에 저장
        string path = Path.Combine(Application.dataPath, "..", "Downloads", filename);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
#elif UNITY_ANDROID
        // 안드로이드에서는 Pictures 폴더에 저장
        string path = Path.Combine(Application.persistentDataPath, filename);
#elif UNITY_IOS
        // iOS에서는 Documents 폴더에 저장
        string path = Path.Combine(Application.persistentDataPath, filename);
#else
        // 기타 플랫폼
        string path = Path.Combine(Application.persistentDataPath, filename);
#endif

        // 파일 저장
        File.WriteAllBytes(path, bytes);
        Debug.Log($"이미지 저장 완료: {path}");

        // Update download state
        isDownloaded = true;
        UpdateDownloadButtonText();

        yield return null;
    }

    /// <summary>
    /// Update download button text based on state
    /// </summary>
    private void UpdateDownloadButtonText()
    {
        if (downloadButtonText != null)
        {
            downloadButtonText.text = isDownloaded ? "Downloaded !" : "Download";
        }
        else if (downloadButton != null)
        {
            // Fallback: try to get text from button
            var textComponent = downloadButton.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = isDownloaded ? "Downloaded !" : "Download";
            }
        }
    }

    /// <summary>
    /// 3. 즐겨찾기 버튼 - 즐겨찾기 토글
    /// </summary>
    private void OnClick_ToggleFavorite()
    {
        // Check login status
        if (UserSession.Instance == null || !UserSession.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[PhotoPanel] Cannot favorite - not logged in");
            return;
        }

        StartCoroutine(ToggleFavoriteCoroutine());
    }

    private IEnumerator ToggleFavoriteCoroutine()
    {
        bool targetState = !isFavorited;

        if (targetState)
        {
            // Add to favorites
            yield return APIManager.Instance.Post(
                $"/planets/{currentUsername}/favorite",
                new { },
                onSuccess: (response) =>
                {
                    Debug.Log("[PhotoPanel] 즐겨찾기 추가 성공");
                    isFavorited = true;
                    UpdateFavoriteIcon();
                },
                onError: (error) =>
                {
                    Debug.LogError("[PhotoPanel] 즐겨찾기 추가 실패: " + error);
                }
            );
        }
        else
        {
            // Remove from favorites
            yield return APIManager.Instance.Delete(
                $"/planets/{currentUsername}/favorite",
                onSuccess: (response) =>
                {
                    Debug.Log("[PhotoPanel] 즐겨찾기 제거 성공");
                    isFavorited = false;
                    UpdateFavoriteIcon();
                },
                onError: (error) =>
                {
                    Debug.LogError("[PhotoPanel] 즐겨찾기 제거 실패: " + error);
                }
            );
        }
    }

    private void UpdateFavoriteIcon()
    {
        if (favoriteIcon != null)
        {
            favoriteIcon.color = isFavorited ? favoriteColor : normalColor;
        }
    }

    /// <summary>
    /// 4. Play 버튼 - 해당 이미지로 퍼즐 플레이
    /// </summary>
    private void OnClick_Play()
    {
        if (currentDetailItem == null || string.IsNullOrEmpty(currentDetailItem.image_url))
        {
            Debug.LogWarning("플레이할 이미지가 없습니다.");
            return;
        }

        // PuzzleSession이 없으면 생성
        if (PuzzleSession.Instance == null)
        {
            Debug.Log("[PhotoPanel] Creating PuzzleSession instance...");
            GameObject sessionObj = new GameObject("PuzzleSession");
            sessionObj.AddComponent<PuzzleSession>();
        }

        // 퍼즐 세션에 이미지 정보 저장
        if (PuzzleSession.Instance != null)
        {
            PuzzleSession.Instance.SetImageForReplay(
                currentDetailItem.image_url,
                currentDetailItem.metadata?.tags,
                3, // 3x3 고정
                false // 업로드 불가
            );

            Debug.Log($"[PhotoPanel] Starting replay with image: {currentDetailItem.image_url}");
            Debug.Log($"[PhotoPanel] ReplayMode: {PuzzleSession.Instance.IsReplayMode()}");
            Debug.Log($"[PhotoPanel] ReplayImageUrl: {PuzzleSession.Instance.ReplayImageUrl}");
        }
        else
        {
            Debug.LogError("[PhotoPanel] Failed to create PuzzleSession!");
            return;
        }

        // 퍼즐 게임 씬으로 이동
        var fadeController = FindObjectOfType<FadeController>();
        if (fadeController != null)
        {
            fadeController.FadeToScene("G002_Game");
        }
        else
        {
            Debug.LogError("FadeController를 찾을 수 없습니다.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("G002_Game");
        }
    }

    #endregion

    /// <summary>
    /// 날짜 포맷 변환
    /// </summary>
    private string FormatDateTime(string isoDate)
    {
        if (string.IsNullOrEmpty(isoDate))
            return "";

        try
        {
            System.DateTime dateTime = System.DateTime.Parse(isoDate);
            return dateTime.ToString("yyyy-MM-dd HH:mm");
        }
        catch
        {
            return isoDate;
        }
    }

    /// <summary>
    /// 로딩 상태 설정
    /// </summary>
    private void SetLoadingState(bool isLoading)
    {
        if (loadingIndicator != null)
            loadingIndicator.SetActive(isLoading);
    }
}