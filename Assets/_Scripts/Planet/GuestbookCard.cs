using UnityEngine;
using TMPro;

public class GuestbookCard : MonoBehaviour
{
    public TMP_Text authorText;
    public TMP_Text contentText;
    public TMP_Text timestampText;

    public void SetData(GuestbookEntry entry)
    {
        authorText.text = entry.author;
        contentText.text = entry.content;
        timestampText.text = entry.createdAt;  // 백엔드에서 포맷된 문자열로 옴
    }
}