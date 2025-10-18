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
    public float serverTimeout = 10f;
    public float pollinationsTimeout = 15f;
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
        // 태그 정보 가져오기
        var tags = GetTags();
        Texture2D puzzleTexture = null;

        // 1단계: 서버에서 이미지 받기 시도
        yield return StartCoroutine(TryGetServerImage(tags, (result) => puzzleTexture = result));

        // 2단계: 서버 실패 시 Pollinations API 시도
        if (puzzleTexture == null)
        {
            Debug.Log("[PuzzleLoader] 서버에서 이미지 로딩 실패. Pollinations API 시도 중...");
            yield return StartCoroutine(TryGetPollinationsImage(tags, (result) => puzzleTexture = result));
        }

        // 3단계: 모든 것 실패 시 더미 이미지 사용
        if (puzzleTexture == null)
        {
            Debug.LogWarning("[PuzzleLoader] 모든 이미지 소스 실패. 더미 이미지 사용.");
            puzzleTexture = LoadFallbackImage();
        }

        // 최종 검증 및 퍼즐 초기화
        if (puzzleTexture != null)
        {
            InitializePuzzle(puzzleTexture);
        }
        else
        {
            Debug.LogError("[PuzzleLoader] 모든 이미지 로딩 실패!");
            HandleLoadingFailure();
        }

        FinalizePuzzleLoading();
    }

    private List<string> GetTags()
    {
        var tags = (UserSession.Instance != null && UserSession.Instance.Tags != null)
            ? UserSession.Instance.Tags
            : null;

        if (tags == null || tags.Count < 1)
        {
            Debug.LogWarning("[PuzzleLoader] 태그 정보가 없습니다. 기본 태그 사용.");
            tags = new List<string> { "default", "test", "puzzle", "image" };
        }

        return tags;
    }

    private IEnumerator TryGetServerImage(List<string> tags, System.Action<Texture2D> callback)
    {
        // TODO: 서버 이미지 요청 로직 구현 예정
        // 현재는 서버 연결이 없으므로 바로 실패 처리
        Debug.Log("[PuzzleLoader] 서버 이미지 요청 시도...");

        yield return new WaitForSeconds(0.1f); // 서버 요청 시뮬레이션

        // 서버 구현 전까지는 null 반환
        callback?.Invoke(null);
    }

    private IEnumerator TryGetPollinationsImage(List<string> tags, System.Action<Texture2D> callback)
    {
        bool isDone = false;
        Texture2D result = null;

        if (PollinationsImageService.Instance != null)
        {
            PollinationsImageService.Instance.DownloadImage(tags, (texture) =>
            {
                result = texture;
                isDone = true;
            });

            // 타임아웃 대기
            float elapsed = 0f;
            while (!isDone && elapsed < pollinationsTimeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!isDone)
            {
                Debug.LogWarning("[PuzzleLoader] Pollinations API 타임아웃");
            }
        }
        else
        {
            Debug.LogWarning("[PuzzleLoader] PollinationsImageService Instance 없음");
        }

        callback?.Invoke(result);
    }

    private Texture2D LoadFallbackImage()
    {
        // 먼저 PuzzleManager에 설정된 기본 이미지 확인
        if (puzzleManager != null && puzzleManager.puzzleImage != null)
        {
            Debug.Log("[PuzzleLoader] PuzzleManager 기본 이미지 사용");
            return puzzleManager.puzzleImage;
        }

        // PuzzleManager에 기본 이미지가 없으면 Resources에서 로드
        var fallbackTexture = Resources.Load<Texture2D>(fallbackImagePath);
        if (fallbackTexture == null)
        {
            Debug.LogError($"[PuzzleLoader] 더미 이미지 로드 실패: Resources/{fallbackImagePath}");
        }
        else
        {
            Debug.Log("[PuzzleLoader] Resources 더미 이미지 로드 성공");
        }
        return fallbackTexture;
    }

    private void InitializePuzzle(Texture2D texture)
    {
        if (puzzleManager == null)
        {
            Debug.LogError("[PuzzleLoader] puzzleManager가 할당되지 않았습니다.");
            return;
        }

        int puzzleSize = GetPuzzleSize();
        puzzleManager.InitializePuzzle(texture, puzzleSize, puzzleSize);
        Debug.Log($"[PuzzleLoader] 퍼즐 초기화 완료: {puzzleSize}x{puzzleSize}");
    }

    private int GetPuzzleSize()
    {
        int size = (GameData.difficulty < 2 || GameData.difficulty > 5) ? 3 : GameData.difficulty;
        Debug.Log($"[PuzzleLoader] 퍼즐 크기: {size}x{size}");
        return size;
    }

    private void HandleLoadingFailure()
    {
        // 로딩 실패 시 처리 (예: 에러 팝업, 메인 메뉴로 돌아가기 등)
        Debug.LogError("[PuzzleLoader] 퍼즐 로딩에 완전히 실패했습니다.");
        // TODO: 에러 처리 UI 표시
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

        Debug.Log("[PuzzleLoader] 퍼즐 로딩 완료");
    }
}