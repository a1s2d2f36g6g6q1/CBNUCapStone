using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;              // FadePanel의 Image
    public float fadeDuration = 1f;      // 페이드 시간

    public static class SceneHistory
    {
        public static Stack<string> history = new Stack<string>();
    }

    
    private void Start()
    {
        if (fadeImage == null)
        {
            Debug.LogError("fadeImage가 연결되지 않았습니다!");
            return;
        }

        // 검정 상태로 시작 후 → 서서히 사라지기
        fadeImage.color = new Color(0, 0, 0, 1);
        StartCoroutine(FadeIn());
    }
    
    public void GoBack()
    {
        if (SceneHistory.history.Count > 0)
        {
            string previousScene = SceneHistory.history.Pop();
            StartCoroutine(FadeOut(previousScene));
        }
        else
        {
            Debug.Log("뒤로 갈 씬이 없습니다!");
        }
    }

    

    public void FadeToScene(string sceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneHistory.history.Push(currentScene); // 현재 씬 저장
        StartCoroutine(FadeOut(sceneName));
    }


    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }

        // 페이드 끝났으니 안 가리게 처리
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.gameObject.SetActive(false); // 또는 알파만 0으로 남겨도 OK
    }

    IEnumerator FadeOut(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float linear = Mathf.Clamp01(t / fadeDuration);
            float eased = 1f - Mathf.Pow(1f - linear, 2f); // Ease Out

            fadeImage.color = new Color(0, 0, 0, eased);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1);
        SceneManager.LoadScene(sceneName);
    }

}