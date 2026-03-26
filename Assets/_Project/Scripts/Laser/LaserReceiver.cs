using UnityEngine;

namespace Project.Laser
{
    public class LaserReceiver : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color inactiveColor = Color.gray;
        [SerializeField] private Color activeColor = Color.green;

        [Header("Behaviour")]
        [SerializeField] private float activeHoldDuration = 0.1f;

        public bool IsActive { get; private set; }

        private float lastHitTime = -999f;

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

            if (IsActive == shouldBeActive)
            {
                return;
            }

            IsActive = shouldBeActive;
            UpdateVisual();
        }

        public void ReceiveLaserHit()
        {
            lastHitTime = Time.time;
        }

        private void UpdateVisual()
        {
            if (targetRenderer == null)
            {
                return;
            }

            Color targetColor = IsActive ? activeColor : inactiveColor;
            Material materialInstance = targetRenderer.material;

            if (materialInstance.HasProperty("_BaseColor"))
            {
                materialInstance.SetColor("_BaseColor", targetColor);
                return;
            }

            if (materialInstance.HasProperty("_Color"))
            {
                materialInstance.color = targetColor;
            }
        }
    }
}
