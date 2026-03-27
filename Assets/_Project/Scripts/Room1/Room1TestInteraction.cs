using Project.Laser;
using UnityEngine;

namespace Project.Room1
{
    public class Room1TestInteraction : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private KeyCode rotateForwardKey = KeyCode.E;
        [SerializeField] private KeyCode rotateBackwardKey = KeyCode.Q;

        private LaserReflector currentReflector;

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

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.yellow);

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                currentReflector = hit.collider.GetComponentInParent<LaserReflector>();
            }
            else
            {
                currentReflector = null;
            }

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
