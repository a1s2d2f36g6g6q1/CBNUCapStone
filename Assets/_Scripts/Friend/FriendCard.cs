using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendCard : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nicknameText;
    public Button visitButton;
    public Button deleteButton;

    private string friendName;
    private FadeController fadeController;

    private void Awake()
    {
        // FadeController는 메인 씬에서 찾거나 주입
        fadeController = FindObjectOfType<FadeController>();
    }

    public void Init(string nickname)
    {
        friendName = nickname;
        nicknameText.text = nickname;

        visitButton.onClick.AddListener(OnClick_Visit);
        deleteButton.onClick.AddListener(OnClick_Delete);
    }

    public void OnClick_Visit()
    {
        Debug.Log($"{friendName}의 행성 방문 요청");
        // TODO: friendName을 기반으로 방문할 행성 데이터 전달

        // 지금은 그냥 메인 메뉴로 이동
        fadeController.FadeToScene("000_MainMenu");
    }

    public void OnClick_Delete()
    {
        Debug.Log($"{friendName} 삭제 요청");
        Destroy(gameObject);
        // TODO: friendName을 기반으로 DB 삭제 요청 전송 필요
    }
}