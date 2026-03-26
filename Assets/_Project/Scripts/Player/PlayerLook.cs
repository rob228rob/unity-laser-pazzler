using UnityEngine;

namespace Project.Player
{
    public class PlayerLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerBody;

        [Header("Look")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private bool lockCursorOnStart = true;

        private float pitch;

        private void Start()
        {
            if (lockCursorOnStart)
            {
                LockCursor(true);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LockCursor(false);
            }

            if (!Cursor.lockState.Equals(CursorLockMode.Locked) && Input.GetMouseButtonDown(0))
            {
                LockCursor(true);
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }

        private void LockCursor(bool shouldLock)
        {
            Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !shouldLock;
        }
    }
}
