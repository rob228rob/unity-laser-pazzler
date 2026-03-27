using UnityEngine;

namespace Project.Core
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    public class PlayerRigGuard : MonoBehaviour
    {
        private const float SuspiciousCoordinateThreshold = 50f;
        private const float MaxFrameJumpDistance = 5f;
        private const float FallResetHeight = -10f;
        private const float RecoveryLogCooldown = 0.5f;

        [SerializeField] private Transform fallbackStartPoint;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera logicalCamera;
        [SerializeField] private Vector3 cameraPivotLocalPosition = new Vector3(0f, 0.8f, 0f);
        [SerializeField] private Vector3 logicalCameraLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 logicalCameraLocalEuler = Vector3.zero;

        private Vector3 lastSafePosition;
        private Quaternion lastSafeRotation;
        private bool hasSafePose;
        private float lastRecoveryTime = -999f;

        private void Awake()
        {
            ResolveReferences();
            CacheSafePose();
        }

        private void LateUpdate()
        {
            ResolveReferences();

            Vector3 currentPosition = transform.position;
            bool hasBadNumbers = IsSuspicious(currentPosition);
            bool fellOut = currentPosition.y < FallResetHeight;
            bool jumpedTooFar = hasSafePose && Vector3.Distance(currentPosition, lastSafePosition) > MaxFrameJumpDistance;

            if (hasBadNumbers || fellOut || jumpedTooFar)
            {
                RecoverPose(hasBadNumbers ? "suspicious coordinates" : fellOut ? "fell out of world" : "unexpected frame jump");
                return;
            }

            CacheSafePose();
        }

        public void MarkCurrentAsSafe(Transform newFallbackStartPoint = null)
        {
            if (newFallbackStartPoint != null)
            {
                fallbackStartPoint = newFallbackStartPoint;
            }

            CacheSafePose();
        }

        public void ForceResetToStart(string reason)
        {
            RecoverPose(reason);
        }

        private void ResolveReferences()
        {
            if (cameraPivot == null)
            {
                cameraPivot = transform.Find("CameraPivot");
            }

            if (logicalCamera == null && cameraPivot != null)
            {
                logicalCamera = cameraPivot.GetComponentInChildren<Camera>(true);
            }

            if (fallbackStartPoint == null)
            {
                GameObject startObject = GameObject.Find("PlayerStart");
                if (startObject != null)
                {
                    fallbackStartPoint = startObject.transform;
                }
            }
        }

        private void CacheSafePose()
        {
            lastSafePosition = transform.position;
            lastSafeRotation = transform.rotation;
            hasSafePose = true;
        }

        private void RecoverPose(string reason)
        {
            Vector3 targetPosition = hasSafePose ? lastSafePosition : transform.position;
            Quaternion targetRotation = hasSafePose ? lastSafeRotation : transform.rotation;

            if (fallbackStartPoint != null)
            {
                targetPosition = fallbackStartPoint.position;
                targetRotation = fallbackStartPoint.rotation;
            }

            transform.SetPositionAndRotation(targetPosition, targetRotation);

            if (cameraPivot != null)
            {
                cameraPivot.localPosition = cameraPivotLocalPosition;
            }

            if (logicalCamera != null)
            {
                logicalCamera.transform.localPosition = logicalCameraLocalPosition;
                logicalCamera.transform.localRotation = Quaternion.Euler(logicalCameraLocalEuler);
            }

            Physics.SyncTransforms();
            CacheSafePose();

            if (Time.unscaledTime - lastRecoveryTime >= RecoveryLogCooldown)
            {
                Debug.LogWarning($"[PlayerRigGuard] Recovered player rig after {reason}. position={targetPosition}");
                lastRecoveryTime = Time.unscaledTime;
            }
        }

        private static bool IsSuspicious(Vector3 position)
        {
            return
                float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z) ||
                float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z) ||
                Mathf.Abs(position.x) > SuspiciousCoordinateThreshold ||
                Mathf.Abs(position.y) > SuspiciousCoordinateThreshold ||
                Mathf.Abs(position.z) > SuspiciousCoordinateThreshold;
        }
    }
}
