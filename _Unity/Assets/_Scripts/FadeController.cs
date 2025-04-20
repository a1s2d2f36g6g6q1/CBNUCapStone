using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public Image fadeImage; // FadePanel의 Image
    public float fadeDuration = 0.4f; // 페이드 시간

    private void Awake()
    {
        fadeDuration = 0.4f; // Inspector 값 무시하고 코드로 덮어쓰기
    }


    private void Start()
    {
        if (fadeImage == null)
        {
            Debug.LogError("fadeImage가 연결되지 않았습니다!");
            return;
        }

        // 꺼져있으면 먼저 켜주기 (혹시 비활성화된 상태로 저장되어있을 수도 있어서)
        fadeImage.gameObject.SetActive(true);

        // 처음에 검정색 → 점점 사라지기
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
        var currentScene = SceneManager.GetActiveScene().name;
        SceneHistory.history.Push(currentScene); // 현재 씬 저장
        StartCoroutine(FadeOut(sceneName));
    }


    private IEnumerator FadeIn()
    {
        fadeImage.gameObject.SetActive(true);

        var t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // <- unscaledDeltaTime 추천
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
            var eased = 1f - Mathf.Pow(1f - linear, 2f); // Ease Out

            fadeImage.color = new Color(0, 0, 0, eased);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1);
        SceneManager.LoadScene(sceneName);
    }

    public static class SceneHistory
    {
        public static Stack<string> history = new();
    }
}