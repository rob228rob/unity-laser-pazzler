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

        private void Awake()
        {
            if (playerBody == null && transform.parent != null)
            {
                playerBody = transform.parent;
            }
        }

        private void Start()
        {
            if (lockCursorOnStart)
            {
                LockCursor(true);
            }
        }

        private void Update()
        {
            if (playerBody == null)
            {
                return;
            }

            if (PauseMenuController.IsPauseMenuOpen)
            {
                return;
            }

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor(true);
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
