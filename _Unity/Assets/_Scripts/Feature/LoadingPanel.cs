using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanelController : MonoBehaviour
{
    public Image loadingImage; // 로딩 패널의 Image
    public float fadeDuration = 0.4f;

    private void Awake()
    {
        if (loadingImage == null)
        {
            Debug.LogError("loadingImage가 연결되지 않았습니다!");
        }

        loadingImage.gameObject.SetActive(false);
    }

    public void ShowLoading()
    {
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    public void HideLoading()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        loadingImage.gameObject.SetActive(true);
        var t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            var a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            loadingImage.color = new Color(loadingImage.color.r, loadingImage.color.g, loadingImage.color.b, a);
            yield return null;
        }

        loadingImage.color = new Color(loadingImage.color.r, loadingImage.color.g, loadingImage.color.b, 1f);
    }

    private IEnumerator FadeOut()
    {
        var t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            var a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            loadingImage.color = new Color(loadingImage.color.r, loadingImage.color.g, loadingImage.color.b, a);
            yield return null;
        }

        loadingImage.color = new Color(loadingImage.color.r, loadingImage.color.g, loadingImage.color.b, 0f);
        loadingImage.gameObject.SetActive(false);
    }
}