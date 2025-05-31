using UnityEngine;
using TMPro;
using System;

public class GuestbookCard : MonoBehaviour
{
    public TMP_Text authorText;
    public TMP_Text contentText;
    public TMP_Text timestampText;

    public void SetData(string author, string content, DateTime timestamp)
    {
        authorText.text = author;
        contentText.text = content;
        timestampText.text = timestamp.ToString("yyyy-MM-dd HH:mm");
    }
}