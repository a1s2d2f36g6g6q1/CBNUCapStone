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

    private GalleryDetailItem currentDetailItem;
    private Texture2D currentTexture;        // 다운로드용 텍스처 저장
    private string currentUsername;
    private string currentImageId;
    private bool isFavorited = false;        // 즐겨찾기 상태
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

        StartCoroutine(LoadPhotoDetailCoroutine(username, imageId));
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

        // TODO: 즐겨찾기 상태 확인 (현재 API에 갤러리 즐겨찾기 기능 없음)
        UpdateFavoriteIcon();
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

        // TODO: 사용자에게 저장 완료 메시지 표시
        yield return null;
    }

    /// <summary>
    /// 3. 즐겨찾기 버튼 - 즐겨찾기 토글
    /// </summary>
    private void OnClick_ToggleFavorite()
    {
        // TODO: 현재 API 명세서에 갤러리 이미지 즐겨찾기 기능이 없음
        // 로컬에서만 상태 토글
        isFavorited = !isFavorited;
        UpdateFavoriteIcon();

        Debug.Log($"즐겨찾기 {(isFavorited ? "추가" : "제거")}");

        // TODO: 백엔드에 갤러리 즐겨찾기 API 추가되면 여기서 호출
        // StartCoroutine(ToggleFavoriteCoroutine());
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

        // 퍼즐 세션에 이미지 정보 저장
        if (PuzzleSession.Instance != null)
        {
            PuzzleSession.Instance.SetImageForReplay(
                currentDetailItem.image_url,
                currentDetailItem.metadata?.tags,
                3, // 3x3 고정
                false // 업로드 불가
            );
        }

        // 퍼즐 게임 씬으로 이동
        var fadeController = FindObjectOfType<FadeController>();
        if (fadeController != null)
        {
            fadeController.FadeToScene("G002_PuzzleGame"); // 실제 씬 이름으로 변경 필요
        }
        else
        {
            Debug.LogError("FadeController를 찾을 수 없습니다.");
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