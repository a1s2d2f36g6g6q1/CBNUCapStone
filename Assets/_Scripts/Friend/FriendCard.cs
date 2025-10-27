using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendCard : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nicknameText;
    public Button visitButton;
    public Button deleteButton;

    private FriendItem friendData;
    private FadeController fadeController;

    private void Awake()
    {
        fadeController = FindObjectOfType<FadeController>();
    }

    public void Init(FriendItem friend)
    {
        friendData = friend;
        nicknameText.text = friend.nickname;

        visitButton.onClick.RemoveAllListeners();
        visitButton.onClick.AddListener(OnClick_Visit);

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(OnClick_Delete);
    }

    public void OnClick_Visit()
    {
        Debug.Log($"{friendData.nickname}의 행성 방문 (username: {friendData.username})");

        // PlanetSession에 친구 행성 정보 저장
        if (PlanetSession.Instance != null)
        {
            PlanetSession.Instance.CurrentPlanetOwnerID = friendData.username;
            PlanetSession.Instance.CurrentPlanetId = friendData.username; // username = planetId
        }

        // P002_MyPlanet으로 이동 (자동으로 타인 행성으로 인식됨)
        fadeController.FadeToScene("P002_MyPlanet");
    }

    public void OnClick_Delete()
    {
        Debug.Log($"{friendData.nickname} 삭제 요청");

        // TODO: 친구 삭제 API 호출
        // 지금은 UI에서만 제거
        Destroy(gameObject);
    }
}