using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 0.4f;

    private void Awake()
    {
        fadeDuration = 0.4f;
    }

    private void Start()
    {
        if (fadeImage == null)
        {
            Debug.LogError("fadeImage가 연결되지 않았습니다!");
            return;
        }

        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0, 0, 0, 1);
        StartCoroutine(FadeIn());
    }

    public void GoBack()
    {
        if (SceneHistory.history.Count > 0)
        {
            var previousScene = SceneHistory.history.Pop();
            StartCoroutine(FadeOut(previousScene));
        }
        else
        {
            Debug.Log("뒤로 갈 씬이 없습니다!");
        }
    }

    public void FadeToScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("씬 이름이 비어있습니다.");
            return;
        }

        var currentScene = SceneManager.GetActiveScene().name;
        SceneHistory.history.Push(currentScene);
        StartCoroutine(FadeOut(sceneName));
    }

    private IEnumerator FadeIn()
    {
        fadeImage.gameObject.SetActive(true);
        var t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            var a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);
        var t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            var linear = Mathf.Clamp01(t / fadeDuration);
            var eased = 1f - Mathf.Pow(1f - linear, 2f);
            fadeImage.color = new Color(0, 0, 0, eased);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 1);
        SceneManager.LoadScene(sceneName);
    }

    public static class SceneHistory
    {
        public static Stack<string> history = new Stack<string>();
    }
}