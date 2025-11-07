using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

        // username 표시
        if (nicknameText != null)
        {
            nicknameText.text = $"{data.username}'s Planet";
            Debug.Log($"[PlanetCard] Init: {data.username} - Text: {nicknameText.text}");
        }
        else
        {
            Debug.LogError("[PlanetCard] nicknameText가 null입니다!");
        }

        visitButton.onClick.RemoveAllListeners();
        visitButton.onClick.AddListener(OnClick_Visit);

        bookmarkButton.onClick.RemoveAllListeners();
        bookmarkButton.onClick.AddListener(OnClick_ToggleBookmark);

        UpdateBookmarkColor();
    }

    private void OnClick_Visit()
    {
        Debug.Log($"[PlanetCard] ========== 행성 방문 시작 ==========");
        Debug.Log($"[PlanetCard] Username: {planetData.username}");
        Debug.Log($"[PlanetCard] Planet ID: {planetData.id}");

        // 로딩 패널 표시
        if (planetManager != null)
        {
            Debug.Log($"[PlanetCard] 로딩 패널 표시 요청");
            planetManager.ShowLoadingPanel(true);
        }
        else
        {
            Debug.LogError("[PlanetCard] planetManager가 null입니다!");
        }

        // PlanetSession에 저장
        if (PlanetSession.Instance != null)
        {
            PlanetSession.Instance.CurrentPlanetOwnerID = planetData.username;
            PlanetSession.Instance.CurrentPlanetId = planetData.id;
            Debug.Log($"[PlanetCard] PlanetSession 저장 완료 - OwnerID: {planetData.username}, PlanetID: {planetData.id}");
        }
        else
        {
            Debug.LogError("[PlanetCard] PlanetSession.Instance가 null입니다!");
        }

        // 행성 방문 기록 후 씬 전환
        StartCoroutine(RecordVisitAndTransition());
    }

    private IEnumerator RecordVisitAndTransition()
    {
        bool visitRecorded = false;

        Debug.Log($"[PlanetCard] API 호출 시작: /planets/{planetData.username}/visit");

        // 행성 방문 기록 API 호출
        yield return APIManager.Instance.Post(
            $"/planets/{planetData.username}/visit",
            new { },
            onSuccess: (response) =>
            {
                Debug.Log($"[PlanetCard] 방문 기록 성공: {response}");
                visitRecorded = true;
            },
            onError: (error) =>
            {
                Debug.LogWarning($"[PlanetCard] 방문 기록 실패 (계속 진행): {error}");
                visitRecorded = true; // 실패해도 진행
            }
        );

        // API 완료 대기
        Debug.Log($"[PlanetCard] API 완료 대기 중...");
        yield return new WaitUntil(() => visitRecorded);
        Debug.Log($"[PlanetCard] API 완료");

        // 추가 딜레이 (안정성)
        Debug.Log($"[PlanetCard] 0.3초 대기 중...");
        yield return new WaitForSeconds(0.3f);

        // 로딩 패널 숨김
        if (planetManager != null)
        {
            Debug.Log($"[PlanetCard] 로딩 패널 숨김 요청");
            planetManager.ShowLoadingPanel(false);
        }

        // P002_MyPlanet으로 이동
        Debug.Log($"[PlanetCard] 씬 전환 시작: P002_MyPlanet");
        if (fadeController != null)
        {
            fadeController.FadeToScene("P002_MyPlanet");
        }
        else
        {
            Debug.LogError("[PlanetCard] FadeController를 찾을 수 없습니다!");
        }
    }

    private void OnClick_ToggleBookmark()
    {
        if (planetManager != null)
        {
            planetManager.ToggleFavorite(planetData.username, isBookmarked);
        }
    }

    private void UpdateBookmarkColor()
    {
        if (bookmarkIcon != null)
            bookmarkIcon.color = isBookmarked ? bookmarkedColor : unbookmarkedColor;
    }

    public bool IsBookmarked => isBookmarked;
    public string PlanetOwner => planetData?.username ?? "";
}