using System.Collections.Generic;
using Project.Room1;
using UnityEngine;

namespace Project.Laser
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserRelayEmitter : MonoBehaviour
    {
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");
        private static Texture2D sharedBeamTexture;

        [Header("References")]
        [SerializeField] private Transform originPoint;
        [SerializeField] private LineRenderer lineRenderer;

        [Header("Laser")]
        [SerializeField] private float maxDistance = 24f;
        [SerializeField] private int maxBounces = 4;
        [SerializeField] private float hitOffset = 0.01f;
        [SerializeField] private float activeHoldDuration = 0.12f;
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private float beamFlowSpeed = 4.8f;
        [SerializeField] private float beamTiling = 3.8f;
        [SerializeField] private float beamPulseSpeed = 9.0f;
        [SerializeField] private float beamPulseAmplitude = 0.12f;

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

            ConfigureBeamVisuals();
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

                    Room1TestHealth health = hit.collider.GetComponentInParent<Room1TestHealth>();
                    if (health != null)
                    {
                        health.ApplyLaserDamage(1);
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
                AnimateBeamMaterial();
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
                name = "Runtime_RelayBeamTex"
            };

            for (int x = 0; x < sharedBeamTexture.width; x++)
            {
                float t = x / (float)(sharedBeamTexture.width - 1);
                float stripe = Mathf.PingPong(t * 16f, 1f);
                float alpha = Mathf.Lerp(0.06f, 1f, Mathf.SmoothStep(0f, 1f, stripe));
                sharedBeamTexture.SetPixel(x, 0, new Color(1f, 1f, 1f, alpha));
            }

            sharedBeamTexture.Apply();
            return sharedBeamTexture;
        }
    }
}
