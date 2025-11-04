using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class F001_Friend : MonoBehaviour
{
    [Header("Friend List")]
    public Transform friendListContent;
    public GameObject friendCardPrefab;

    [Header("UI")]
    public TMP_Text friendCountText;

    private List<FriendItem> friendList = new List<FriendItem>();
    private bool useAPI = true; // Set to false for dummy data

    private void Start()
    {
        LoadFriendList();
    }

    private void LoadFriendList()
    {
        if (!useAPI)
        {
            LoadDummyFriendList();
        }
        else
        {
            StartCoroutine(LoadFriendListFromAPI());
        }
    }

    private void LoadDummyFriendList()
    {
        // FIXED: Use correct field names from API
        friendList = new List<FriendItem>
        {
            new FriendItem {
                id = "friend-001",
                friend_id = "user-001",
                friend_username = "user001",
                friend_nickname = "Alice",
                friend_profile_image_url = ""
            },
            new FriendItem {
                id = "friend-002",
                friend_id = "user-002",
                friend_username = "user002",
                friend_nickname = "Bob",
                friend_profile_image_url = ""
            },
            new FriendItem {
                id = "friend-003",
                friend_id = "user-003",
                friend_username = "user003",
                friend_nickname = "Charlie",
                friend_profile_image_url = ""
            },
            new FriendItem {
                id = "friend-004",
                friend_id = "user-004",
                friend_username = "user004",
                friend_nickname = "David",
                friend_profile_image_url = ""
            },
            new FriendItem {
                id = "friend-005",
                friend_id = "user-005",
                friend_username = "user005",
                friend_nickname = "Eve",
                friend_profile_image_url = ""
            },
            new FriendItem {
                id = "friend-006",
                friend_id = "user-006",
                friend_username = "user006",
                friend_nickname = "Frank",
                friend_profile_image_url = ""
            },
            new FriendItem {
                id = "friend-007",
                friend_id = "user-007",
                friend_username = "user007",
                friend_nickname = "Grace",
                friend_profile_image_url = ""
            },
            new FriendItem {
                id = "friend-008",
                friend_id = "user-008",
                friend_username = "user008",
                friend_nickname = "Henry",
                friend_profile_image_url = ""
            }
        };

        Debug.Log($"Loaded dummy friend list: {friendList.Count} friends");
        RefreshFriendListUI();
    }

    private IEnumerator LoadFriendListFromAPI()
    {
        yield return APIManager.Instance.Get(
            "/friends",
            onSuccess: (response) =>
            {
                // FIXED: FriendListResponse has 'friends' array, not 'result'
                FriendListResponse friendResponse = JsonUtility.FromJson<FriendListResponse>(response);
                friendList = new List<FriendItem>(friendResponse.friends);

                Debug.Log($"Friend list loaded: {friendList.Count} friends");
                RefreshFriendListUI();
            },
            onError: (error) =>
            {
                Debug.LogError("Friend list load failed: " + error);
                // Load dummy data on error
                LoadDummyFriendList();
            }
        );
    }

    private void RefreshFriendListUI()
    {
        // Clear existing cards
        foreach (Transform child in friendListContent)
        {
            Destroy(child.gameObject);
        }

        // Create friend cards
        foreach (var friend in friendList)
        {
            GameObject card = Instantiate(friendCardPrefab, friendListContent);
            FriendCard friendCard = card.GetComponent<FriendCard>();
            if (friendCard != null)
            {
                friendCard.Init(friend);
            }
        }

        // Update count
        if (friendCountText != null)
        {
            friendCountText.text = $"Friends: {friendList.Count}";
        }
    }

    public void OnClick_Refresh()
    {
        LoadFriendList();
    }
}