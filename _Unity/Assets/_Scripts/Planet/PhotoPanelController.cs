using UnityEngine;
using TMPro;

public class PhotoPanelController : MonoBehaviour
{
    [Header("UI 요소")]
    public TMP_InputField descriptionField;
    public TMP_Text tagText;

    private PlanetDataManager.PlanetPhotoData currentPhotoData;

    public void SetPhotoData(PlanetDataManager.PlanetPhotoData data)
    {
        currentPhotoData = data;
        descriptionField.text = data.description;
        tagText.text = string.Join(" ", data.tags);
    }

    public void OnDescriptionChanged()
    {
        if (currentPhotoData != null)
        {
            currentPhotoData.description = descriptionField.text;
        }
    }
}