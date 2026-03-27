using UnityEngine;

namespace Project.Laser
{
    public class LaserReflector : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private string reflectorName = "Reflector";

        [Header("Rotation")]
        [SerializeField] private float rotationStep = 45f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        [Header("Debug")]
        [SerializeField] private bool drawNormalGizmo = true;
        [SerializeField] private float gizmoLength = 1f;

        public string ReflectorName => reflectorName;

        public void RotateForward()
        {
            transform.Rotate(rotationAxis, rotationStep, Space.Self);
        }

        public void RotateBackward()
        {
            transform.Rotate(rotationAxis, -rotationStep, Space.Self);
        }

        public string GetInteractionPrompt(KeyCode forwardKey, KeyCode backwardKey)
        {
            return $"{reflectorName}: [{forwardKey}] rotate right, [{backwardKey}] rotate left";
        }

        public Vector3 GetReflectedDirection(Vector3 incomingDirection, RaycastHit hit)
        {
            return Vector3.Reflect(incomingDirection.normalized, hit.normal).normalized;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawNormalGizmo)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmoLength);
        }
    }
}
