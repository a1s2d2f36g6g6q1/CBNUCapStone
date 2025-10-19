using UnityEngine;
using TMPro;

public class PhotoPanelController : MonoBehaviour
{
    [Header("UI 요소")]
    public TMP_InputField descriptionField;   // 내 행성용 (편집)
    public TMP_Text descriptionText;          // 남의 행성용 (읽기 전용)
    public TMP_Text tagText;

    private GalleryItem currentGalleryItem;
    private bool isMine = true;

    public void SetPhotoData(GalleryItem item, bool isMine = true)
    {
        currentGalleryItem = item;
        this.isMine = isMine;

        descriptionField.gameObject.SetActive(isMine);
        descriptionText.gameObject.SetActive(!isMine);

        if (isMine)
        {
            descriptionField.text = item.description;
        }
        else
        {
            descriptionText.text = item.description;
        }

        tagText.text = string.Join(" ", item.tags);
    }

    public void OnDescriptionChanged()
    {
        if (currentGalleryItem != null && isMine)
        {
            currentGalleryItem.description = descriptionField.text;
            // TODO: 설명 수정 API 호출
        }
    }
}