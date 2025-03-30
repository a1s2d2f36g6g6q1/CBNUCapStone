using UnityEngine;
using UnityEngine.EventSystems;

public class RetroButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject hoverImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverImage.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverImage.SetActive(false);
    }
}