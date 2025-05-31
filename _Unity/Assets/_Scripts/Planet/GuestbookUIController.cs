using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GuestbookUIController : MonoBehaviour
{
    [Header("UI")]
    public GameObject guestbookCardPrefab;
    public Transform contentContainer;
    public TMP_InputField inputField;
    public Button submitButton;

    public void LoadGuestbook()
    {
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        var entries = PlanetDataManager.Instance.guestbookEntries;

        foreach (var entry in entries)
        {
            var card = Instantiate(guestbookCardPrefab, contentContainer);
            var cardUI = card.GetComponent<GuestbookCard>();
            cardUI.SetData(entry.author, entry.content, entry.timestamp);
        }

        // 항상 작성 가능
        inputField.interactable = true;
        submitButton.interactable = true;
    }

    public void OnSubmit()
    {
        if (string.IsNullOrEmpty(inputField.text)) return;

        PlanetDataManager.Instance.AddGuestbookEntry("Visitor", inputField.text);
        inputField.text = "";
        LoadGuestbook();
    }
}