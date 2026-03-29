using System.Collections.Generic;
using Project.Room1;
using UnityEngine;

namespace Project.Laser
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserEmitter : MonoBehaviour
    {
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static Texture2D sharedBeamTexture;

        [Header("References")]
        [SerializeField] private Transform originPoint;
        [SerializeField] private LineRenderer lineRenderer;

        [Header("Laser")]
        [SerializeField] private float maxDistance = 30f;
        [SerializeField] private int maxBounces = 5;
        [SerializeField] private float hitOffset = 0.01f;
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private bool drawDebugLines = true;
        [SerializeField] private float beamFlowSpeed = 3.8f;
        [SerializeField] private float beamTiling = 3.2f;
        [SerializeField] private float beamPulseSpeed = 7.5f;
        [SerializeField] private float beamPulseAmplitude = 0.10f;
        [SerializeField] private bool allowWorldSurfaceBounce = true;

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

            ConfigureBeamVisuals();
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
            bool usedWorldSurfaceBounce = false;
            bool wasReflected = false;

            points.Add(currentOrigin);

            for (int bounceIndex = 0; bounceIndex <= maxBounces && remainingDistance > 0f; bounceIndex++)
            {
                if (Physics.Raycast(currentOrigin, currentDirection, out RaycastHit hit, remainingDistance, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    points.Add(hit.point);

                    Room1TestHealth health = hit.collider.GetComponentInParent<Room1TestHealth>();
                    if (health != null)
                    {
                        health.ApplyLaserDamage(1);
                        DrawDebugSegments();
                        UpdateLineRenderer();
                        return;
                    }

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
                        wasReflected = true;
                        currentDirection = reflector.GetReflectedDirection(currentDirection, hit);
                        currentOrigin = hit.point + currentDirection * hitOffset;
                        continue;
                    }

                    LaserBounceSurface bounceSurface = hit.collider.GetComponentInParent<LaserBounceSurface>();
                    if (bounceSurface != null && allowWorldSurfaceBounce && !wasReflected && !usedWorldSurfaceBounce && bounceIndex < maxBounces)
                    {
                        remainingDistance -= hit.distance;
                        usedWorldSurfaceBounce = true;
                        wasReflected = true;
                        currentDirection = bounceSurface.GetReflectedDirection(currentDirection, hit);
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
            AnimateBeamMaterial();
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

        private void ConfigureBeamVisuals()
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.textureMode = LineTextureMode.Tile;

            Material beamMaterial = lineRenderer.material;
            if (beamMaterial == null)
            {
                return;
            }

            Texture2D beamTexture = GetOrCreateBeamTexture();
            if (beamMaterial.HasProperty(BaseMapId))
            {
                beamMaterial.SetTexture(BaseMapId, beamTexture);
            }

            if (beamMaterial.HasProperty(MainTexId))
            {
                beamMaterial.SetTexture(MainTexId, beamTexture);
            }
        }

        private void AnimateBeamMaterial()
        {
            if (lineRenderer == null || points.Count < 2)
            {
                return;
            }

            Material beamMaterial = lineRenderer.material;
            if (beamMaterial == null)
            {
                return;
            }

            float totalLength = 0f;
            for (int i = 0; i < points.Count - 1; i++)
            {
                totalLength += Vector3.Distance(points[i], points[i + 1]);
            }

            Vector2 scale = new Vector2(Mathf.Max(1f, totalLength * beamTiling), 1f);
            Vector2 offset = new Vector2(-Time.time * beamFlowSpeed, 0f);
            float widthPulse = 1f + Mathf.Sin(Time.time * beamPulseSpeed) * beamPulseAmplitude;
            lineRenderer.widthMultiplier = widthPulse;

            if (beamMaterial.HasProperty(BaseMapId))
            {
                beamMaterial.SetTextureScale(BaseMapId, scale);
                beamMaterial.SetTextureOffset(BaseMapId, offset);
            }

            if (beamMaterial.HasProperty(MainTexId))
            {
                beamMaterial.SetTextureScale(MainTexId, scale);
                beamMaterial.SetTextureOffset(MainTexId, offset);
            }
        }

        private static Texture2D GetOrCreateBeamTexture()
        {
            if (sharedBeamTexture != null)
            {
                return sharedBeamTexture;
            }

            sharedBeamTexture = new Texture2D(64, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                name = "Runtime_LaserBeamTex"
            };

            for (int x = 0; x < sharedBeamTexture.width; x++)
            {
                float t = x / (float)(sharedBeamTexture.width - 1);
                float stripe = Mathf.PingPong(t * 14f, 1f);
                float alpha = Mathf.Lerp(0.08f, 1f, Mathf.SmoothStep(0f, 1f, stripe));
                sharedBeamTexture.SetPixel(x, 0, new Color(1f, 1f, 1f, alpha));
            }

            sharedBeamTexture.Apply();
            return sharedBeamTexture;
        }
    }
}
