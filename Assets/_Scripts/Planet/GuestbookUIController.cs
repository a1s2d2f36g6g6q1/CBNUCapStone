using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GuestbookUIController : MonoBehaviour
{
    [Header("UI")]
    public GameObject guestbookCardPrefab;
    public Transform contentContainer;
    public TMP_InputField inputField;
    public Button submitButton;

    private string currentPlanetUsername;

    public void LoadGuestbook(string ownerUsername)
    {
        currentPlanetUsername = ownerUsername;

        if (string.IsNullOrEmpty(ownerUsername))
        {
            Debug.LogWarning("username이 없어 방명록을 로드할 수 없습니다.");
            return;
        }

        StartCoroutine(LoadGuestbookCoroutine(ownerUsername));
    }

    private IEnumerator LoadGuestbookCoroutine(string ownerUsername)
    {
        // 기존 카드 삭제
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        yield return APIManager.Instance.Get(
            $"/planets/{ownerUsername}/guestbook",
            onSuccess: (response) =>
            {
                Debug.Log($"[방명록 API 응답] {response}");

                GuestbookListResponse guestbookResponse = JsonUtility.FromJson<GuestbookListResponse>(response);

                if (guestbookResponse != null && guestbookResponse.result != null && guestbookResponse.result.guestbooks != null)
                {
                    Debug.Log($"방명록 로드 성공: {guestbookResponse.result.guestbooks.Length}개");

                    foreach (var entry in guestbookResponse.result.guestbooks)
                    {
                        var card = Instantiate(guestbookCardPrefab, contentContainer);
                        var cardUI = card.GetComponent<GuestbookCard>();
                        cardUI.SetData(entry);
                    }
                }
                else
                {
                    Debug.LogWarning("방명록 데이터가 null입니다.");
                }

                // 로그인 상태에 따라 작성 가능 여부 설정
                bool canWrite = UserSession.Instance != null && UserSession.Instance.IsLoggedIn;
                inputField.interactable = canWrite;
                submitButton.interactable = canWrite;
            },
            onError: (error) =>
            {
                Debug.LogError("방명록 로드 실패: " + error);
            }
        );
    }

    public void OnSubmit()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            Debug.LogWarning("방명록 내용이 비어있습니다.");
            return;
        }

        if (string.IsNullOrEmpty(currentPlanetUsername))
        {
            Debug.LogWarning("username이 없어 방명록을 작성할 수 없습니다.");
            return;
        }

        StartCoroutine(SubmitGuestbookCoroutine());
    }

    private IEnumerator SubmitGuestbookCoroutine()
    {
        GuestbookWriteRequest requestData = new GuestbookWriteRequest
        {
            content = inputField.text.Trim()
        };

        yield return APIManager.Instance.Post(
            $"/planets/{currentPlanetUsername}/guestbook",
            requestData,
            onSuccess: (response) =>
            {
                Debug.Log("방명록 작성 성공");
                inputField.text = "";

                // 방명록 새로고침
                LoadGuestbook(currentPlanetUsername);
            },
            onError: (error) =>
            {
                Debug.LogError("방명록 작성 실패: " + error);
            }
        );
    }
}