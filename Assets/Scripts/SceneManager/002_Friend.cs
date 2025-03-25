using UnityEngine;
using TMPro;

public class FriendListManager : MonoBehaviour
{
    public GameObject friendCardPrefab;
    public Transform contentParent;

    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject card = Instantiate(friendCardPrefab, contentParent);
            card.transform.localScale = Vector3.one;

            card.transform.Find("NameText").GetComponent<TextMeshProUGUI>().text = "가나다";
            card.transform.Find("CountText").GetComponent<TextMeshProUGUI>().text = $"No.{i + 1}";
        }
    }
}
