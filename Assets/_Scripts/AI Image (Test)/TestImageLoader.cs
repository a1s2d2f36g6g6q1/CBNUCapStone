using UnityEngine;
using UnityEngine.UI;

public class TestImageLoader : MonoBehaviour
{
    // ===== Inspector Fields =====
    [Header("Service Reference")]
    public PollinationsImageService imageService;

    [Header("UI Reference")]
    public Image displayImage;

    // ===== Private Fields =====
    private readonly string[] randomTags = { "space", "flower", "castle", "cat", "robot", "mountain", "ocean" };

    // ===== Unity Lifecycle =====
    void Start()
    {
        if (imageService == null)
        {
            Debug.LogError("[TestImageLoader] PollinationsImageService is not assigned! Please assign in Inspector.");
            return;
        }

        if (displayImage == null)
        {
            Debug.LogError("[TestImageLoader] Display Image is not assigned! Please assign in Inspector.");
            return;
        }

        string prompt = randomTags[Random.Range(0, randomTags.Length)];
        Debug.Log($"[TestImageLoader] Selected tag: {prompt}");

        StartCoroutine(imageService.GenerateImage(prompt, OnImageGenerated));
    }

    // ===== Private Methods =====
    private void OnImageGenerated(Texture2D texture)
    {
        if (texture != null)
        {
            Debug.Log("[TestImageLoader] Image generated successfully");
            displayImage.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                Vector2.one * 0.5f
            );
        }
        else
        {
            Debug.LogError("[TestImageLoader] Image generation failed");
        }
    }
}