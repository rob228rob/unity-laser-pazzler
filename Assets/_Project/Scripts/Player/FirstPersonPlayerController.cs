using UnityEngine;

namespace Project.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -22f;
        [SerializeField] private float groundSnapOffset = 0.18f;
        [SerializeField] private float capsuleRadius = 0.35f;
        [SerializeField] private float capsuleHeight = 2f;
        [SerializeField] private float collisionSkin = 0.03f;
        [SerializeField] private LayerMask collisionMask = ~0;

        private CharacterController characterController;
        private float verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }
        }

        private void Update()
        {
            if (PauseMenuController.IsPauseMenuOpen)
            {
                return;
            }

            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.A))
            {
                horizontal -= 1f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                horizontal += 1f;
            }

            if (Input.GetKey(KeyCode.S))
            {
                vertical -= 1f;
            }

            if (Input.GetKey(KeyCode.W))
            {
                vertical += 1f;
            }

            Vector2 input = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
            Vector3 horizontalMove = (transform.right * input.x + transform.forward * input.y) * moveSpeed * Time.deltaTime;

            ApplyHorizontalMovement(horizontalMove);
            ApplyVerticalMovement();
        }

        private void ApplyHorizontalMovement(Vector3 desiredMove)
        {
            if (desiredMove.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector3 origin = transform.position;
            Vector3 direction = desiredMove.normalized;
            float distance = desiredMove.magnitude;

            GetCapsulePoints(origin, out Vector3 bottom, out Vector3 top);

            if (Physics.CapsuleCast(bottom, top, capsuleRadius, direction, out RaycastHit hit, distance + collisionSkin, collisionMask, QueryTriggerInteraction.Ignore))
            {
                distance = Mathf.Max(0f, hit.distance - collisionSkin);
            }

            transform.position = origin + direction * distance;
        }

        private void ApplyVerticalMovement()
        {
            Vector3 origin = transform.position + Vector3.up * 0.5f;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2.5f, collisionMask, QueryTriggerInteraction.Ignore))
            {
                float groundedY = hit.point.y + groundSnapOffset;
                if (transform.position.y <= groundedY + 0.2f && verticalVelocity <= 0f)
                {
                    verticalVelocity = 0f;
                    Vector3 groundedPosition = transform.position;
                    groundedPosition.y = groundedY;
                    transform.position = groundedPosition;
                    return;
                }
            }

            verticalVelocity += gravity * Time.deltaTime;
            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
        }

        private void GetCapsulePoints(Vector3 worldPosition, out Vector3 bottom, out Vector3 top)
        {
            float cylinderHeight = Mathf.Max(capsuleHeight - capsuleRadius * 2f, 0.01f);
            bottom = worldPosition + Vector3.up * capsuleRadius;
            top = bottom + Vector3.up * cylinderHeight;
        }
    }
}
