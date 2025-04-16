using UnityEngine;

public class UIParallax_Camera : MonoBehaviour
{
    public float rotationAmount = 5f;

    private Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.rotation;
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;

        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3 dir = (worldMouse - transform.position).normalized;

        float rotX = -dir.y * rotationAmount;
        float rotY = dir.x * rotationAmount;

        Quaternion targetRotation = Quaternion.Euler(rotX, rotY, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation * targetRotation, Time.deltaTime * 5f);
    }
}