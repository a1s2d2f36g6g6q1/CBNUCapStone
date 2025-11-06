using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhotoCard : MonoBehaviour
{
    [Header("UI")]
    public Image thumbnailImage;

    private MyPlanetUIController uiController;
    private GalleryItem galleryItem;
    private string ownerUsername;

    public void Init(MyPlanetUIController controller, GalleryItem item, string username)
    {
        uiController = controller;
        galleryItem = item;
        ownerUsername = username;

        // 썸네일 이미지 로드
        if (!string.IsNullOrEmpty(item.image_url))
        {
            StartCoroutine(LoadThumbnail(item.image_url));
        }
    }

    private IEnumerator LoadThumbnail(string url)
    {
        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
            if (thumbnailImage != null)
            {
                thumbnailImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
        else
        {
            Debug.LogError("썸네일 이미지 로드 실패: " + request.error);
        }
    }

    public void OnClick()
    {
        if (uiController != null && galleryItem != null)
        {
            // imageId를 사용하여 상세 조회
            uiController.OpenPhoto(ownerUsername, galleryItem.imageId);
        }
        else
        {
            Debug.LogWarning("PhotoCard 클릭 시 데이터가 누락되었습니다.");
        }
    }
}