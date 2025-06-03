using System.Collections;
using UnityEngine;

public class PuzzleLoader : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject loadingPanel;         // Inspector에서 할당
    public PuzzleManager puzzleManager;     // Inspector에서 할당
    public LoadingPanelFade loadingPanelFade;
    public GameObject timerManagerObj; // Inspector에 TimerManager 오브젝트 할당
    public GameObject backButtonObj;   // Inspector에 BackButton 오브젝트 할당


    private void Start()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        if (timerManagerObj != null)
            timerManagerObj.SetActive(false);
        if (backButtonObj != null)
            backButtonObj.SetActive(false);

        StartCoroutine(LoadAndInitPuzzleCoroutine());
    }

    IEnumerator LoadAndInitPuzzleCoroutine()
    {
        // 1. 태그 받아오기 (없으면 예외처리)
        var tags = (UserSession.Instance != null && UserSession.Instance.Tags != null)
            ? UserSession.Instance.Tags
            : null;

        if (tags == null || tags.Count < 1)
        {
            Debug.LogWarning("[PuzzleLoader] 태그 정보가 없습니다. 기본 이미지로 진행.");
            tags = new System.Collections.Generic.List<string> { "default", "test", "puzzle", "image" };
        }

        Texture2D puzzleTexture = null;
        bool isDone = false;

        // 2. PollinationsImageService 통해 이미지 요청
        if (PollinationsImageService.Instance != null)
        {
            PollinationsImageService.Instance.DownloadImage(tags, (result) =>
            {
                puzzleTexture = result;
                isDone = true;
            });
        }
        else
        {
            Debug.LogWarning("[PuzzleLoader] PollinationsImageService Instance 없음. 기본 이미지로 진행.");
            isDone = true;
        }

        // 3. 25초 타임아웃 대기
        float timeout = 25f;
        float elapsed = 0f;
        while (!isDone && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4. 실패 시 fallback (Resources/test.png)
        if (puzzleTexture == null)
        {
            Debug.LogWarning("[PuzzleLoader] AI 이미지 로딩 실패, test.png로 대체");
            puzzleTexture = Resources.Load<Texture2D>("test");
            if (puzzleTexture == null)
            {
                Debug.LogError("Resources/test.png 로드 실패 - 퍼즐 생성 불가!");
                // 플레이 진행을 막거나, 완전히 fallback 처리를 할 것
                yield break;
            }
        }
        // 5. 퍼즐 초기화 함수 직접 호출 (퍼즐매니저의 Start는 사용 X)
        if (puzzleManager != null)
        {
            int puzzleSize = (GameData.difficulty < 2 || GameData.difficulty > 5) ? 3 : GameData.difficulty;
            puzzleManager.InitializePuzzle(puzzleTexture, puzzleSize, puzzleSize);
        }
        else
        {
            Debug.LogError("[PuzzleLoader] puzzleManager 할당 안됨.");
        }

        // 6. 로딩 패널 종료
        if (timerManagerObj != null)
            timerManagerObj.SetActive(true);
        if (backButtonObj != null)
            backButtonObj.SetActive(true);

        if (loadingPanelFade != null)
            loadingPanelFade.Hide();
        else if (loadingPanel != null)
            loadingPanel.SetActive(false);


    }
}