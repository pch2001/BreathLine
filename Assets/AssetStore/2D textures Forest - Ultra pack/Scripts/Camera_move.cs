using UnityEngine;

namespace Forest2D
{
    public class CameraController : MonoBehaviour
    {
        public float moveSpeed = 5f; // Camera movement speed

        // Public variables to limit camera movement
        public Vector2 minBounds;
        public Vector2 maxBounds;

        void Update()
        {
            // Receiving input from the keyboard
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            // Calculate the motion vector
            Vector3 moveDirection = new Vector3(horizontalInput, verticalInput, 0f).normalized;

            // Calculate the new camera position
            Vector3 newPosition = Vector3.Lerp(transform.position, transform.position + moveDirection, moveSpeed * Time.deltaTime);

            // We limit the new camera position within the given coordinates
            newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
            newPosition.y = Mathf.Clamp(newPosition.y, minBounds.y, maxBounds.y);

            // Applying a new position to the camera
            transform.position = newPosition;
        }
    }
}