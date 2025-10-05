using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System.Text;


public class GuestbookUIController : MonoBehaviour
{
    [Header("UI")]
    public GameObject guestbookCardPrefab;
    public Transform contentContainer;
    public TMP_InputField inputField;
    public Button submitButton;

    public void LoadGuestbook()
    {
        StartCoroutine(FetchGuestbookFromServer());
    }

    private IEnumerator FetchGuestbookFromServer()
    {
        string planetId = PlanetSession.Instance.CurrentPlanetOwnerID;

        UnityWebRequest request = UnityWebRequest.Get(
            $"http://13.209.33.42/api/planet/{planetId}/guestbook"
        );
        request.SetRequestHeader("Authorization", $"Bearer {UserSession.Instance.AuthToken}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // 기존 로컬 로직 대신 서버 데이터 사용
            var response = JsonUtility.FromJson<GuestbookListResponse>(request.downloadHandler.text);

            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var entry in response.entries)
            {
                var card = Instantiate(guestbookCardPrefab, contentContainer);
                var cardUI = card.GetComponent<GuestbookCard>();
                cardUI.SetData(entry.author, entry.content, entry.timestamp);
            }
        }
    }


    private IEnumerator PostGuestbookToServer(string content)
    {
        string planetId = PlanetSession.Instance.CurrentPlanetOwnerID;

        var requestData = new { content = content };
        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(
            $"http://13.209.33.42/api/planet/{planetId}/guestbook",
            "POST"
        );
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {UserSession.Instance.AuthToken}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            inputField.text = "";
            LoadGuestbook(); // 다시 로드
        }
    }

    public void OnSubmit()
    {
        if (string.IsNullOrEmpty(inputField.text)) return;
        StartCoroutine(PostGuestbookToServer(inputField.text));
    }
}

[System.Serializable]
public class GuestbookListResponse
{
    public GuestbookEntry[] entries;
}