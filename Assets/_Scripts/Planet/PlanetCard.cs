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

        // title 필드 사용 (예: "tester3의 행성")
        // title이 없으면 username을 표시
        nicknameText.text = string.IsNullOrEmpty(data.title)
            ? data.ownerUsername
            : data.title;

        visitButton.onClick.RemoveAllListeners();
        visitButton.onClick.AddListener(OnClick_Visit);

        bookmarkButton.onClick.RemoveAllListeners();
        bookmarkButton.onClick.AddListener(OnClick_ToggleBookmark);

        UpdateBookmarkColor();
    }

    private void OnClick_Visit()
    {
        string displayName = string.IsNullOrEmpty(planetData.title)
            ? planetData.ownerUsername
            : planetData.title;

        Debug.Log($"{displayName} 행성 방문 (username: {planetData.ownerUsername})");

        // PlanetSession에 저장
        if (PlanetSession.Instance != null)
        {
            PlanetSession.Instance.CurrentPlanetOwnerID = planetData.ownerUsername;
            PlanetSession.Instance.CurrentPlanetId = planetData.ownerUsername;
        }

        // 행성 방문 기록 후 씬 전환
        StartCoroutine(RecordVisitAndMove(planetData.ownerUsername));
    }

    private System.Collections.IEnumerator RecordVisitAndMove(string ownerUsername)
    {
        // 로그인 상태가 아니면 방문 기록 없이 바로 이동
        if (UserSession.Instance == null || !UserSession.Instance.IsLoggedIn)
        {
            Debug.Log("비로그인 상태 - 방문 기록 없이 행성 이동");
            fadeController.FadeToScene("P002_MyPlanet");
            yield break;
        }

        // 자신의 행성이면 방문 기록 없이 바로 이동
        if (UserSession.Instance.UserID == ownerUsername)
        {
            Debug.Log("내 행성 - 방문 기록 없이 행성 이동");
            fadeController.FadeToScene("P002_MyPlanet");
            yield break;
        }

        // 타인의 행성이면 방문 기록 후 이동
        bool visitRecorded = false;

        yield return APIManager.Instance.Post(
            $"/planets/{ownerUsername}/visit",
            new { },
            onSuccess: (response) =>
            {
                string displayName = string.IsNullOrEmpty(planetData.title)
                    ? planetData.ownerUsername
                    : planetData.title;
                Debug.Log($"{displayName} 행성 방문 기록 완료");
                visitRecorded = true;
            },
            onError: (error) =>
            {
                Debug.LogWarning($"행성 방문 기록 실패: {error}");
                // 방문 기록 실패해도 이동은 가능
                visitRecorded = true;
            }
        );

        // API 응답 대기
        yield return new WaitUntil(() => visitRecorded);

        // 씬 전환
        fadeController.FadeToScene("P002_MyPlanet");
    }

    private void OnClick_ToggleBookmark()
    {
        if (planetManager != null)
        {
            planetManager.ToggleFavorite(planetData.ownerUsername, isBookmarked);
        }
    }

    private void UpdateBookmarkColor()
    {
        if (bookmarkIcon != null)
            bookmarkIcon.color = isBookmarked ? bookmarkedColor : unbookmarkedColor;
    }

    public bool IsBookmarked => isBookmarked;

    public string PlanetOwner => string.IsNullOrEmpty(planetData?.title)
        ? planetData?.ownerUsername ?? ""
        : planetData.title;
}