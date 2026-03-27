using System.Collections.Generic;
using UnityEngine;

namespace Project.Laser
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserRelayEmitter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform originPoint;
        [SerializeField] private LineRenderer lineRenderer;

        [Header("Laser")]
        [SerializeField] private float maxDistance = 24f;
        [SerializeField] private int maxBounces = 4;
        [SerializeField] private float hitOffset = 0.01f;
        [SerializeField] private float activeHoldDuration = 0.12f;
        [SerializeField] private LayerMask collisionMask = ~0;

        private readonly List<Vector3> points = new List<Vector3>();
        private float lastHitTime = -999f;

        private void Awake()
        {
            if (originPoint == null)
            {
                originPoint = transform;
            }

            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
        }

        private void Update()
        {
            if (Time.time - lastHitTime > activeHoldDuration)
            {
                ClearLine();
                return;
            }

            TraceLaser();
        }

        public void ReceiveLaserHit()
        {
            lastHitTime = Time.time;
        }

        private void TraceLaser()
        {
            points.Clear();

            Vector3 currentOrigin = originPoint.position;
            Vector3 currentDirection = originPoint.forward.normalized;
            float remainingDistance = maxDistance;

            points.Add(currentOrigin);

            for (int bounceIndex = 0; bounceIndex <= maxBounces && remainingDistance > 0f; bounceIndex++)
            {
                if (Physics.Raycast(currentOrigin, currentDirection, out RaycastHit hit, remainingDistance, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    points.Add(hit.point);

                    LaserReceiver receiver = hit.collider.GetComponentInParent<LaserReceiver>();
                    if (receiver != null)
                    {
                        receiver.ReceiveLaserHit();
                    }

                    LaserBeamSplitter splitter = hit.collider.GetComponentInParent<LaserBeamSplitter>();
                    if (splitter != null)
                    {
                        splitter.ReceiveLaserHit();
                        UpdateLineRenderer();
                        return;
                    }

                    LaserRelayEmitter relayEmitter = hit.collider.GetComponentInParent<LaserRelayEmitter>();
                    if (relayEmitter != null && relayEmitter.gameObject != gameObject)
                    {
                        relayEmitter.ReceiveLaserHit();
                        UpdateLineRenderer();
                        return;
                    }

                    LaserColorFilter colorFilter = hit.collider.GetComponentInParent<LaserColorFilter>();
                    if (colorFilter != null)
                    {
                        colorFilter.ReceiveLaserHit();
                        UpdateLineRenderer();
                        return;
                    }

                    LaserReflector reflector = hit.collider.GetComponentInParent<LaserReflector>();
                    if (reflector != null && bounceIndex < maxBounces)
                    {
                        remainingDistance -= hit.distance;
                        currentDirection = reflector.GetReflectedDirection(currentDirection, hit);
                        currentOrigin = hit.point + currentDirection * hitOffset;
                        continue;
                    }

                    UpdateLineRenderer();
                    return;
                }

                points.Add(currentOrigin + currentDirection * remainingDistance);
                UpdateLineRenderer();
                return;
            }

            UpdateLineRenderer();
        }

        private void ClearLine()
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.positionCount = 0;
        }

        private void UpdateLineRenderer()
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.positionCount = points.Count;
            if (points.Count > 0)
            {
                lineRenderer.SetPositions(points.ToArray());
            }
        }
    }
}
