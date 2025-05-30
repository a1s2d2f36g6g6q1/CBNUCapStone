using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetCard : MonoBehaviour
{
    public TMP_Text nicknameText;
    public Button visitButton;
    public Button bookmarkButton;
    public Image bookmarkIcon;

    private string planetOwner;
    private bool isBookmarked = false;
    private FadeController fadeController;

    private static readonly Color bookmarkedColor = new Color32(0xFA, 0xFF, 0x55, 0xFF);
    private static readonly Color unbookmarkedColor = Color.white;

    private void Awake()
    {
        fadeController = FindObjectOfType<FadeController>();
    }

    public void Init(string nickname)
    {
        planetOwner = nickname;
        nicknameText.text = nickname;

        visitButton.onClick.AddListener(OnClick_Visit);
        bookmarkButton.onClick.AddListener(OnClick_ToggleBookmark);

        UpdateBookmarkColor();
    }

    private void OnClick_Visit()
    {
        Debug.Log($"{planetOwner}의 행성 방문 요청");
        fadeController.FadeToScene("P003_OtherPlanet");
    }

    private void OnClick_ToggleBookmark()
    {
        isBookmarked = !isBookmarked;
        UpdateBookmarkColor();

        Debug.Log($"{planetOwner} 즐겨찾기 상태: {isBookmarked}");
    }

    private void UpdateBookmarkColor()
    {
        if (bookmarkIcon != null)
            bookmarkIcon.color = isBookmarked ? bookmarkedColor : unbookmarkedColor;
    }

    public bool IsBookmarked => isBookmarked;
    public string PlanetOwner => planetOwner;
}