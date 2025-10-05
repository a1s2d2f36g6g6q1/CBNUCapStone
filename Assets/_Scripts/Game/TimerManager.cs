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
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
        {
            Debug.LogWarning("TimerText가 할당되지 않았습니다.");
            return;
        }

        var minutes = (int)(elapsedTime / 60f);
        var seconds = (int)(elapsedTime % 60f);
        var milliseconds = (int)(elapsedTime * 1000f % 1000);

        timerText.text = string.Format("{0:00} : {1:00} : {2:000}", minutes, seconds, milliseconds);
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        isRunning = true;
        Debug.Log("타이머 시작");
    }

    public void StopTimer()
    {
        isRunning = false;
        Debug.Log($"타이머 정지 - 최종 시간: {GetFormattedTime()}");
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void ResumeTimer()
    {
        isRunning = true;
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    public string GetFormattedTime()
    {
        var minutes = (int)(elapsedTime / 60f);
        var seconds = (int)(elapsedTime % 60f);
        var milliseconds = (int)(elapsedTime * 1000f % 1000);

        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    public bool IsRunning()
    {
        return isRunning;
    }
}