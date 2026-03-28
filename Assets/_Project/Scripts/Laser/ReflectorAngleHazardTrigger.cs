using UnityEngine;

namespace Project.Laser
{
    public sealed class ReflectorAngleHazardTrigger : MonoBehaviour
    {
        [SerializeField] private LaserRelayEmitter hazardEmitter;
        [SerializeField] private float dangerousYaw = 180f;
        [SerializeField] private float tolerance = 10f;

        private void Update()
        {
            if (hazardEmitter == null)
            {
                return;
            }

            float currentYaw = NormalizeAngle(transform.localEulerAngles.y);
            float targetYaw = NormalizeAngle(dangerousYaw);
            float delta = Mathf.Abs(Mathf.DeltaAngle(currentYaw, targetYaw));

            if (delta <= tolerance)
            {
                hazardEmitter.ReceiveLaserHit();
            }
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle < 0f)
            {
                angle += 360f;
            }

            while (angle >= 360f)
            {
                angle -= 360f;
            }

            return angle;
        }
    }
}
