using UnityEngine;

namespace Project.Laser
{
    public class LaserBeamSplitter : MonoBehaviour
    {
        [SerializeField] private LaserRelayEmitter[] outputEmitters;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color inactiveColor = new Color(0.22f, 0.28f, 0.30f);
        [SerializeField] private Color activeColor = new Color(0.35f, 0.90f, 0.97f);
        [SerializeField] private float activeHoldDuration = 0.12f;

        private float lastHitTime = -999f;
        private bool isActive;

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }

            UpdateVisual();
        }

        private void Update()
        {
            bool shouldBeActive = Time.time - lastHitTime <= activeHoldDuration;
            if (isActive == shouldBeActive)
            {
                return;
            }

            isActive = shouldBeActive;
            UpdateVisual();
        }

        public void ReceiveLaserHit()
        {
            lastHitTime = Time.time;

            if (outputEmitters == null)
            {
                return;
            }

            for (int i = 0; i < outputEmitters.Length; i++)
            {
                if (outputEmitters[i] != null)
                {
                    outputEmitters[i].ReceiveLaserHit();
                }
            }
        }

        private void UpdateVisual()
        {
            if (targetRenderer == null)
            {
                return;
            }

            Material materialInstance = targetRenderer.material;
            Color color = isActive ? activeColor : inactiveColor;

            if (materialInstance.HasProperty("_BaseColor"))
            {
                materialInstance.SetColor("_BaseColor", color);
            }
            else if (materialInstance.HasProperty("_Color"))
            {
                materialInstance.color = color;
            }
        }
    }
}
