using UnityEngine;
using TMPro;

public class PhotoPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField descriptionField;   // For my planet (editable)
    public TMP_Text descriptionText;          // For other's planet (read-only)
    public TMP_Text tagText;

    private GalleryItem currentGalleryItem;
    private bool isMine = true;

    public void SetPhotoData(GalleryItem item, bool isMine = true)
    {
        currentGalleryItem = item;
        this.isMine = isMine;

        descriptionField.gameObject.SetActive(isMine);
        descriptionText.gameObject.SetActive(!isMine);

        // FIXED: GalleryItem has 'title' not 'description'
        if (isMine)
        {
            descriptionField.text = item.title ?? "";
        }
        else
        {
            descriptionText.text = item.title ?? "";
        }

        // FIXED: GalleryItem doesn't have 'tags' in list response
        // Tags are only in detail response. For now, hide or show empty
        tagText.text = "";
    }

    public void OnDescriptionChanged()
    {
        if (currentGalleryItem != null && isMine)
        {
            // FIXED: Update title field
            currentGalleryItem.title = descriptionField.text;
            // TODO: Call description update API
        }
    }
}