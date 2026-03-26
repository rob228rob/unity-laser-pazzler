using UnityEngine;

namespace Project.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 12f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedStickForce = -2f;

        private CharacterController characterController;
        private Vector3 currentVelocity;
        private float verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 targetVelocity = (transform.right * input.x + transform.forward * input.y) * moveSpeed;
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickForce;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 movement = currentVelocity;
            movement.y = verticalVelocity;

            characterController.Move(movement * Time.deltaTime);
        }
    }
}
