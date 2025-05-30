using UnityEngine;
using UnityEngine.UI;

public class PhotoCard : MonoBehaviour
{
    [Header("UI")]
    public Image thumbnailImage;

    private MyPlanetUIController uiController;
    private PlanetDataManager.PlanetPhotoData photoData;

    public void Init(MyPlanetUIController controller, PlanetDataManager.PlanetPhotoData data)
    {
        uiController = controller;
        photoData = data;
    }

    public void OnClick()
    {
        if (uiController != null && photoData != null)
        {
            uiController.OpenPhoto(photoData);
        }
        else
        {
            Debug.LogWarning("PhotoCard 클릭 시 데이터가 누락되었습니다.");
        }
    }
}