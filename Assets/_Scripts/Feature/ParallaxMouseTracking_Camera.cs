using UnityEngine;

public class UIParallax_Camera : MonoBehaviour
{
    public float rotationAmount = 5f;

    private Quaternion originalRotation;

    private void Start()
    {
        originalRotation = transform.rotation;
    }

    private void Update()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;

        var worldMouse = Camera.main.ScreenToWorldPoint(mousePos);
        var dir = (worldMouse - transform.position).normalized;

        var rotX = -dir.y * rotationAmount;
        var rotY = dir.x * rotationAmount;

        var targetRotation = Quaternion.Euler(rotX, rotY, 0f);
        transform.rotation =
            Quaternion.Slerp(transform.rotation, originalRotation * targetRotation, Time.deltaTime * 5f);
    }
}