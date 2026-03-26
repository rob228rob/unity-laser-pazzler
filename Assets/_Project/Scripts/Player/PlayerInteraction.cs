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
                return;
            }

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (drawDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.yellow);
            }

            if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            if (!hit.collider.TryGetComponent(out LaserReflector reflector))
            {
                return;
            }

            if (Input.GetKeyDown(rotateForwardKey))
            {
                reflector.RotateForward();
            }

            if (Input.GetKeyDown(rotateBackwardKey))
            {
                reflector.RotateBackward();
            }
        }
    }
}
