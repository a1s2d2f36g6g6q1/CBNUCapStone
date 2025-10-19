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

        nicknameText.text = data.ownerNickname;

        visitButton.onClick.RemoveAllListeners();
        visitButton.onClick.AddListener(OnClick_Visit);

        bookmarkButton.onClick.RemoveAllListeners();
        bookmarkButton.onClick.AddListener(OnClick_ToggleBookmark);

        UpdateBookmarkColor();
    }

    private void OnClick_Visit()
    {
        // planetId가 비어있으면 username 사용
        string actualPlanetId = string.IsNullOrEmpty(planetData.planetId) ?
            planetData.ownerUsername :
            planetData.planetId;

        Debug.Log($"{planetData.ownerNickname}의 행성 방문 (planetId: {actualPlanetId})");

        // PlanetSession에 저장
        if (PlanetSession.Instance != null)
        {
            PlanetSession.Instance.CurrentPlanetOwnerID = planetData.ownerUsername;
            PlanetSession.Instance.CurrentPlanetId = actualPlanetId;
        }

        // 행성 방문 기록
        StartCoroutine(RecordVisit(actualPlanetId));

        // P002_MyPlanet으로 이동
        fadeController.FadeToScene("P002_MyPlanet");
    }

    private System.Collections.IEnumerator RecordVisit(string planetId)
    {
        yield return APIManager.Instance.Post(
            $"/planets/{planetId}/visit",
            new { },
            onSuccess: (response) =>
            {
                Debug.Log($"{planetData.ownerNickname} 행성 방문 기록 완료");
            },
            onError: (error) =>
            {
                Debug.LogWarning("행성 방문 기록 실패: " + error);
            }
        );
    }

    private void OnClick_ToggleBookmark()
    {
        // planetId가 비어있으면 username 사용
        string actualPlanetId = string.IsNullOrEmpty(planetData.planetId) ?
            planetData.ownerUsername :
            planetData.planetId;

        if (planetManager != null)
        {
            planetManager.ToggleFavorite(actualPlanetId, isBookmarked);
        }
    }

    private void UpdateBookmarkColor()
    {
        if (bookmarkIcon != null)
            bookmarkIcon.color = isBookmarked ? bookmarkedColor : unbookmarkedColor;
    }

    public bool IsBookmarked => isBookmarked;
    public string PlanetOwner => planetData?.ownerNickname ?? "";
}