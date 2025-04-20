using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public TMP_Text timerText;
    private float elapsedTime;
    private bool isRunning;

    private void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;

        var minutes = (int)(elapsedTime / 60f);
        var seconds = (int)(elapsedTime % 60f);
        var milliseconds = (int)(elapsedTime * 1000f % 1000);

        timerText.text = string.Format("{0:00} : {1:00} : {2:000}", minutes, seconds, milliseconds);
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }
}