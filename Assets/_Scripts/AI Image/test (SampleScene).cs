using UnityEngine;
using UnityEngine.UI;

public class TestImageLoader : MonoBehaviour
{
    [Header("Service Reference")]
    public PollinationsImageService imageService; // PollinationsImageService로 변경
    
    [Header("UI Reference")]
    public Image displayImage;
    
    private readonly string[] randomTags = { "space", "flower", "castle", "cat", "robot", "mountain", "ocean" };
    
    void Start()
    {
        // Null 체크 추가
        if (imageService == null)
        {
            Debug.LogError("PollinationsImageService가 할당되지 않았습니다! Inspector에서 할당해주세요.");
            return;
        }
        
        if (displayImage == null)
        {
            Debug.LogError("Display Image가 할당되지 않았습니다! Inspector에서 할당해주세요.");
            return;
        }
        
        string prompt = randomTags[Random.Range(0, randomTags.Length)];
        Debug.Log($"선택된 태그: {prompt}");
        
        StartCoroutine(imageService.GenerateImage(prompt, OnImageGenerated));
    }
    
    private void OnImageGenerated(Texture2D texture)
    {
        if (texture != null)
        {
            Debug.Log("이미지 생성 성공!");
            displayImage.sprite = Sprite.Create(
                texture, 
                new Rect(0, 0, texture.width, texture.height), 
                Vector2.one * 0.5f
            );
        }
        else
        {
            Debug.LogError("이미지 생성 실패");
        }
    }
}