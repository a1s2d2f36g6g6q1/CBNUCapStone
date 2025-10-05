using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlashEffect : MonoBehaviour
{
    public Image flashImage;
    public float flashInDuration = 0.05f;
    public float fadeOutDuration = 0.5f;

    public void PlayFlash()
    {
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        flashImage.color = new Color(1f, 1f, 1f, 1f); // 초기 알파 = 1 (보이게)
    
        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        flashImage.color = new Color(1f, 1f, 1f, 0f); // 마지막 알파 = 0
    }

}