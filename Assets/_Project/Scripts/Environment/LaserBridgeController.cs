using UnityEngine;
using Project.Laser;

namespace Project.Environment
{
    public class LaserBridgeController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LaserReceiver[] requiredReceivers;
        [SerializeField] private Renderer[] bridgeRenderers;
        [SerializeField] private Collider[] bridgeColliders;

        [Header("Movement")]
        [SerializeField] private Vector3 hiddenLocalOffset = new Vector3(0f, -2.5f, 0f);
        [SerializeField] private float moveSpeed = 3f;

        [Header("Visuals")]
        [SerializeField] private Color inactiveColor = new Color(0.18f, 0.22f, 0.26f);
        [SerializeField] private Color activeColor = new Color(0.32f, 0.88f, 0.92f);

        private Vector3 activeLocalPosition;
        private Vector3 hiddenLocalPosition;

        private void Awake()
        {
            activeLocalPosition = transform.localPosition;
            hiddenLocalPosition = activeLocalPosition + hiddenLocalOffset;

            if (bridgeRenderers == null || bridgeRenderers.Length == 0)
            {
                bridgeRenderers = GetComponentsInChildren<Renderer>();
            }

            if (bridgeColliders == null || bridgeColliders.Length == 0)
            {
                bridgeColliders = GetComponentsInChildren<Collider>();
            }

            transform.localPosition = hiddenLocalPosition;
            ApplyVisualState(false);
        }

        private void Update()
        {
            bool isActive = AreAllReceiversActive();
            Vector3 targetPosition = isActive ? activeLocalPosition : hiddenLocalPosition;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);

            bool shouldEnableColliders = Vector3.Distance(transform.localPosition, activeLocalPosition) < 0.05f;
            SetColliderState(shouldEnableColliders);
            ApplyVisualState(isActive);
        }

        private bool AreAllReceiversActive()
        {
            if (requiredReceivers == null || requiredReceivers.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < requiredReceivers.Length; i++)
            {
                if (requiredReceivers[i] == null || !requiredReceivers[i].IsActive)
                {
                    return false;
                }
            }

            return true;
        }

        private void SetColliderState(bool shouldEnable)
        {
            if (bridgeColliders == null)
            {
                return;
            }

            for (int i = 0; i < bridgeColliders.Length; i++)
            {
                if (bridgeColliders[i] != null)
                {
                    bridgeColliders[i].enabled = shouldEnable;
                }
            }
        }

        private void ApplyVisualState(bool isActive)
        {
            if (bridgeRenderers == null)
            {
                return;
            }

            Color targetColor = isActive ? activeColor : inactiveColor;

            for (int i = 0; i < bridgeRenderers.Length; i++)
            {
                if (bridgeRenderers[i] == null)
                {
                    continue;
                }

                Material material = bridgeRenderers[i].material;
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", targetColor);
                }
                else if (material.HasProperty("_Color"))
                {
                    material.color = targetColor;
                }
            }
        }
    }
}
