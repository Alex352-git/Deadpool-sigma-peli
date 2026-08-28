using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 2f;
    private float xRotation = 0f;

    void Update()
    {
        if (Mouse.current == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity * 0.02f;

        // Rotate the camera target up/down and left/right based on mouse movement
        xRotation -= mouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -35f, 60f); // Prevents camera from flipping over

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseDelta.x);
        }
    }
}