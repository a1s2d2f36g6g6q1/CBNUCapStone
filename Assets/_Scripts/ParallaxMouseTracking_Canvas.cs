using UnityEngine;

public class UIParallax_Canvas : MonoBehaviour
{
    public float moveAmount = 10f;
    private Vector2 originalPosition;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        Vector2 normalizedMouse = new Vector2(
            (Input.mousePosition.x / Screen.width - 0.5f) * 2f,
            (Input.mousePosition.y / Screen.height - 0.5f) * 2f
        );

        Vector2 offset = -normalizedMouse * moveAmount;
        rectTransform.anchoredPosition = originalPosition + offset;
    }
}