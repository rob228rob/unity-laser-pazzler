using UnityEngine;
using Project.Laser;

namespace Project.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera playerCamera;

        [Header("Interaction")]
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private KeyCode rotateForwardKey = KeyCode.E;
        [SerializeField] private KeyCode rotateBackwardKey = KeyCode.Q;
        [SerializeField] private bool drawDebugRay = true;

        private LaserReflector currentReflector;

        public bool HasInteractableTarget => currentReflector != null;
        public string CurrentPrompt => currentReflector == null ? string.Empty : currentReflector.GetInteractionPrompt(rotateForwardKey, rotateBackwardKey);

        private void Awake()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }
        }

        private void Update()
        {
            if (playerCamera == null)
            {
                currentReflector = null;
                return;
            }

            if (PauseMenuController.IsPauseMenuOpen)
            {
                currentReflector = null;
                return;
            }

            UpdateCurrentReflector();
            HandleRotationInput();
        }

        private void UpdateCurrentReflector()
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (drawDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.yellow);
            }

            if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                currentReflector = null;
                return;
            }

            currentReflector = hit.collider.GetComponentInParent<LaserReflector>();
        }

        private void HandleRotationInput()
        {
            if (currentReflector == null)
            {
                return;
            }

            if (Input.GetKeyDown(rotateForwardKey))
            {
                currentReflector.RotateForward();
            }

            if (Input.GetKeyDown(rotateBackwardKey))
            {
                currentReflector.RotateBackward();
            }
        }
    }
}
