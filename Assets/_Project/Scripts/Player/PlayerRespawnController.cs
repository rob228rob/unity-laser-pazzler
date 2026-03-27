using UnityEngine;

namespace Project.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerRespawnController : MonoBehaviour
    {
        private static readonly Vector3 RespawnLiftOffset = new Vector3(0f, 0.18f, 0f);
        private const float GroundProbeHeight = 4f;
        private const float GroundProbeDistance = 12f;
        private const float GroundOffset = 0.18f;

        [SerializeField] private Transform respawnPoint;
        private CharacterController characterController;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            if (respawnPoint == null)
            {
                GameObject pointObject = new GameObject("RespawnPoint");
                pointObject.transform.SetParent(transform);
                pointObject.transform.localPosition = Vector3.zero;
                respawnPoint = pointObject.transform;
            }
        }

        public void SetRespawnPoint(Vector3 worldPosition)
        {
            if (respawnPoint == null)
            {
                return;
            }

            respawnPoint.position = worldPosition;
        }

        public void Respawn()
        {
            if (respawnPoint == null)
            {
                return;
            }

            transform.position = GetGroundedRespawnPosition(respawnPoint.position + RespawnLiftOffset);
        }

        private Vector3 GetGroundedRespawnPosition(Vector3 desiredPosition)
        {
            Vector3 rayOrigin = desiredPosition + Vector3.up * GroundProbeHeight;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, GroundProbeDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                return new Vector3(desiredPosition.x, hit.point.y + GroundOffset, desiredPosition.z);
            }

            return desiredPosition;
        }
    }
}
