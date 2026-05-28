using UnityEngine;

public class SimpleLookCamera : MonoBehaviour
{
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;

    private float pitch = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        float yaw = transform.eulerAngles.y + mouseX;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
