using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 0.01f;

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    IEnumerator FadeIn()
    {
        float time = 0f;
        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            float easedT = t * t * t;
            color.a = 1f - easedT;
            fadeImage.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
    }

    IEnumerator FadeOut(string sceneName)
    {
        float time = 0f;
        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            float easedT = t * t; // 💡 점점 빨라지게
            color.a = easedT;
            fadeImage.color = color;
            time += Time.deltaTime;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        SceneManager.LoadScene(sceneName);
    }

}