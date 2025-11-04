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
        // FIXED: Use friend_nickname from API
        nicknameText.text = friend.friend_nickname;

        visitButton.onClick.RemoveAllListeners();
        visitButton.onClick.AddListener(OnClick_Visit);

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(OnClick_Delete);
    }

    public void OnClick_Visit()
    {
        // FIXED: Use friend_nickname and friend_username
        Debug.Log($"Visiting {friendData.friend_nickname}'s planet (username: {friendData.friend_username})");

        // Save friend planet info to PlanetSession
        if (PlanetSession.Instance != null)
        {
            PlanetSession.Instance.CurrentPlanetOwnerID = friendData.friend_username;
            PlanetSession.Instance.CurrentPlanetId = friendData.friend_username; // username = planetId
        }

        // Navigate to P002_MyPlanet (automatically recognized as other's planet)
        fadeController.FadeToScene("P002_MyPlanet");
    }

    public void OnClick_Delete()
    {
        Debug.Log($"Delete friend request: {friendData.friend_nickname}");

        // TODO: Call friend delete API
        // For now, just remove from UI
        Destroy(gameObject);
    }
}