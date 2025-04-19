using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public TMP_Text timerText;
    private float elapsedTime = 0f;
    private bool isRunning = false;

    public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;

        int minutes = (int)(elapsedTime / 60f);
        int seconds = (int)(elapsedTime % 60f);
        int milliseconds = (int)((elapsedTime * 1000f) % 1000);

        timerText.text = string.Format("{0:00} : {1:00} : {2:000}", minutes, seconds, milliseconds);
    }
}