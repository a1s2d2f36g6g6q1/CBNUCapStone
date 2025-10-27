using UnityEngine;
using TMPro;

public class GuestbookCard : MonoBehaviour
{
    public TMP_Text authorText;
    public TMP_Text contentText;
    public TMP_Text timestampText;

    public void SetData(GuestbookEntry entry)
    {
        authorText.text = entry.authorUsername;
        contentText.text = entry.content;
        timestampText.text = FormatTimestamp(entry.written_at);
    }

    private string FormatTimestamp(string isoDate)
    {
        if (string.IsNullOrEmpty(isoDate))
            return "";

        try
        {
            System.DateTime dateTime = System.DateTime.Parse(isoDate);
            return dateTime.ToString("yyyy-MM-dd HH:mm");
        }
        catch
        {
            return isoDate; // 파싱 실패하면 원본 그대로
        }
    }
}