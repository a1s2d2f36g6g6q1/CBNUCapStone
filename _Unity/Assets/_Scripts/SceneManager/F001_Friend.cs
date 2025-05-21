using System.Collections.Generic;
using UnityEngine;

public class FriendManager : MonoBehaviour
{
    public FadeController fadeController;

    [Header("UI 연결")]
    public Transform friendListContainer; // Scroll View > Content
    public GameObject friendCardPrefab;

    private void Start()
    {
        LoadFriendList();
    }

    private void LoadFriendList()
    {
        List<string> dummyNames = new();

        for (int i = 1; i <= 30; i++) // 충분한 수로 확장
        {
            dummyNames.Add($"Test Friend {i}");
        }

        foreach (var name in dummyNames)
        {
            var card = Instantiate(friendCardPrefab, friendListContainer);
            var friendCard = card.GetComponent<FriendCard>();
            friendCard.Init(name);
        }
    }

    public void Back()
    {
        fadeController.FadeToScene("000_MainMenu");
    }
}