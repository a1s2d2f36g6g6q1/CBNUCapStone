using UnityEngine;
using UnityEngine.UI;

public class PhotoCard : MonoBehaviour
{
    [Header("UI")]
    public Image thumbnailImage;

    private MyPlanetUIController uiController;
    private GalleryItem galleryItem;

    public void Init(MyPlanetUIController controller, GalleryItem item)
    {
        uiController = controller;
        galleryItem = item;

        // TODO: imageUrl로 썸네일 로드
        // 현재는 기본 이미지 사용
    }

    public void OnClick()
    {
        if (uiController != null && galleryItem != null)
        {
            uiController.OpenPhoto(galleryItem);
        }
        else
        {
            Debug.LogWarning("PhotoCard 클릭 시 데이터가 누락되었습니다.");
        }
    }
}