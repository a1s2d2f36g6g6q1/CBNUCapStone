using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public RectTransform fadePanel; // 검정 패널 (UI Image)
    public float transitionTime = 1.0f;

    // 🎢 EaseOut 커브 (인스펙터에서 연결 가능)
    public AnimationCurve easeOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private void Start()
    {
        // 시작할 때 화면을 아래에서 위로 쓱 올라오게
        fadePanel.anchoredPosition = new Vector2(0, -Screen.height);
        StartCoroutine(SlideIn());
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(SlideOut(sceneName));
    }

    IEnumerator SlideIn()
    {
        float elapsed = 0f;
        Vector2 startPos = new Vector2(0, -Screen.height);
        Vector2 endPos = Vector2.zero;

        while (elapsed < transitionTime)
        {
            float t = elapsed / transitionTime;
            float curvedT = easeOutCurve.Evaluate(t);
            fadePanel.anchoredPosition = Vector2.Lerp(startPos, endPos, curvedT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        fadePanel.anchoredPosition = endPos;
    }

    IEnumerator SlideOut(string sceneName)
    {
        float elapsed = 0f;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = new Vector2(0, Screen.height);

        while (elapsed < transitionTime)
        {
            float t = elapsed / transitionTime;
            float curvedT = easeOutCurve.Evaluate(t);
            fadePanel.anchoredPosition = Vector2.Lerp(startPos, endPos, curvedT);
            elapsed += Time.deltaTime;
            yield return null;
        }

        fadePanel.anchoredPosition = endPos;

        // 다음 씬 로딩
        SceneManager.LoadScene(sceneName);
    }
}
