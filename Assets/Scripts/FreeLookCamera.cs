using UnityEngine;

public class FreeMoveCamera : MonoBehaviour
{
    public float moveSpeed = 10f; // Speed of movement
    public float lookSpeed = 2f;  // Speed of looking around

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime; // A/D or Left/Right Arrow keys
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;   // W/S or Up/Down Arrow keys

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        transform.position += move;
    }

    private void HandleRotation()
    {
        float rotateX = Input.GetAxis("Mouse X") * lookSpeed;
        float rotateY = -Input.GetAxis("Mouse Y") * lookSpeed;

        transform.Rotate(0, rotateX, 0, Space.World);
        transform.Rotate(rotateY, 0, 0, Space.Self);
    }
}
