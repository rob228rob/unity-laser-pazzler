using System.Collections.Generic;
using UnityEngine;

namespace Project.Laser
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserEmitter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform originPoint;
        [SerializeField] private LineRenderer lineRenderer;

        [Header("Laser")]
        [SerializeField] private float maxDistance = 30f;
        [SerializeField] private int maxBounces = 5;
        [SerializeField] private float hitOffset = 0.01f;
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private bool drawDebugLines = true;

        private readonly List<Vector3> points = new List<Vector3>();

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
            TraceLaser();
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
                        DrawDebugSegments();
                        UpdateLineRenderer();
                        return;
                    }

                    LaserRelayEmitter relayEmitter = hit.collider.GetComponentInParent<LaserRelayEmitter>();
                    if (relayEmitter != null && relayEmitter.gameObject != gameObject)
                    {
                        relayEmitter.ReceiveLaserHit();
                        DrawDebugSegments();
                        UpdateLineRenderer();
                        return;
                    }

                    LaserColorFilter colorFilter = hit.collider.GetComponentInParent<LaserColorFilter>();
                    if (colorFilter != null)
                    {
                        colorFilter.ReceiveLaserHit();
                        DrawDebugSegments();
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

                    DrawDebugSegments();
                    UpdateLineRenderer();
                    return;
                }

                points.Add(currentOrigin + currentDirection * remainingDistance);
                DrawDebugSegments();
                UpdateLineRenderer();
                return;
            }

            DrawDebugSegments();
            UpdateLineRenderer();
        }

        private void UpdateLineRenderer()
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
        }

        private void DrawDebugSegments()
        {
            if (!drawDebugLines)
            {
                return;
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                Debug.DrawLine(points[i], points[i + 1], Color.red);
            }
        }
    }
}
