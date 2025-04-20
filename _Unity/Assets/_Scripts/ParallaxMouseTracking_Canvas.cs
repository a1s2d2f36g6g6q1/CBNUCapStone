using UnityEngine;

public class UIParallax_Canvas : MonoBehaviour
{
    public float moveAmount = 10f;
    private Vector2 originalPosition;
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        var normalizedMouse = new Vector2(
            (Input.mousePosition.x / Screen.width - 0.5f) * 2f,
            (Input.mousePosition.y / Screen.height - 0.5f) * 2f
        );

        var offset = -normalizedMouse * moveAmount;
        rectTransform.anchoredPosition = originalPosition + offset;
    }
}