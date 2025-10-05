using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingPanelFade : MonoBehaviour
{
    public Image loadingImage; // 반드시 "검은색" Image를 연결
    public float fadeDuration = 0.6f;

    private void Awake()
    {
        // 완전히 투명하게 시작 (혹은 필요하면 알파=1로 시작)
        if (loadingImage != null)
            loadingImage.color = new Color(0, 0, 0, 1f);

        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (loadingImage != null)
        {
            loadingImage.color = new Color(0, 0, 0, 1f);
        }
    }

    public void Hide()
    {
        if (loadingImage != null)
            StartCoroutine(FadeOutAndDeactivate());
        else
            gameObject.SetActive(false);
    }

    private IEnumerator FadeOutAndDeactivate()
    {
        float t = 0f;
        Color c = loadingImage.color;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            loadingImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
        loadingImage.color = new Color(c.r, c.g, c.b, 0f);
        gameObject.SetActive(false);
    }
}