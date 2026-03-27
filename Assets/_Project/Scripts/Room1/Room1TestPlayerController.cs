using UnityEngine;

namespace Project.Room1
{
    [RequireComponent(typeof(CharacterController))]
    public class Room1TestPlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpHeight = 1.1f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedStickForce = -2f;

        private CharacterController characterController;
        private float verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            characterController.enabled = true;
        }

        private void Update()
        {
            Vector3 input = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
            {
                input += transform.forward;
            }

            if (Input.GetKey(KeyCode.S))
            {
                input -= transform.forward;
            }

            if (Input.GetKey(KeyCode.D))
            {
                input += transform.right;
            }

            if (Input.GetKey(KeyCode.A))
            {
                input -= transform.right;
            }

            input.y = 0f;
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickForce;
            }

            if (characterController.isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 movement = input * moveSpeed;
            movement.y = verticalVelocity;
            characterController.Move(movement * Time.deltaTime);
        }
    }
}
