using UnityEngine;

namespace Project.Laser
{
    public sealed class LaserBounceSurface : MonoBehaviour
    {
        [SerializeField] private bool drawGizmo = true;
        [SerializeField] private float gizmoLength = 0.8f;

        public Vector3 GetReflectedDirection(Vector3 incomingDirection, RaycastHit hit)
        {
            return Vector3.Reflect(incomingDirection.normalized, hit.normal).normalized;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmo)
            {
                return;
            }

            Gizmos.color = new Color(0.35f, 0.90f, 0.97f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmoLength);
        }
    }
}
