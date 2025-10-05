using UnityEngine;
using TMPro;

public class PhotoPanelController : MonoBehaviour
{
    [Header("UI 요소")]
    public TMP_InputField descriptionField;   // 내 행성용 (편집)
    public TMP_Text descriptionText;          // 남의 행성용 (읽기 전용)
    public TMP_Text tagText;

    private PlanetDataManager.PlanetPhotoData currentPhotoData;
    private bool isMine = true;  // 기본값: 내 행성

    public void SetPhotoData(PlanetDataManager.PlanetPhotoData data, bool isMine = true)
    {
        currentPhotoData = data;
        this.isMine = isMine;

        descriptionField.gameObject.SetActive(isMine);
        descriptionText.gameObject.SetActive(!isMine);

        if (isMine)
        {
            descriptionField.text = data.description;
        }
        else
        {
            descriptionText.text = data.description;
        }

        tagText.text = string.Join(" ", data.tags);
    }

    public void OnDescriptionChanged()
    {
        if (currentPhotoData != null && isMine)
        {
            currentPhotoData.description = descriptionField.text;
        }
    }
}