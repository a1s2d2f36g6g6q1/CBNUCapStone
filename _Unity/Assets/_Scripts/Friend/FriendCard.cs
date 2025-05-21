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

    public void Init(string nickname)
    {
        friendName = nickname;
        nicknameText.text = nickname;
    }

    public void OnClick_Visit()
    {
        Debug.Log($"{friendName}의 행성 방문 요청");
        // TODO: 씬 전환 또는 상세 보기 구현
    }

    public void OnClick_Delete()
    {
        Debug.Log($"{friendName} 삭제 요청");
        // TODO: 삭제 로직 연결 예정
    }
}