using UnityEngine;

namespace Project.Room1
{
    [RequireComponent(typeof(CharacterController))]
    public class Room1TestFallReset : MonoBehaviour
    {
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private float fallThreshold = -6f;

        private CharacterController characterController;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (transform.position.y >= fallThreshold || respawnPoint == null)
            {
                return;
            }

            bool wasEnabled = characterController != null && characterController.enabled;
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);

            if (characterController != null)
            {
                characterController.enabled = wasEnabled;
            }
        }
    }
}
