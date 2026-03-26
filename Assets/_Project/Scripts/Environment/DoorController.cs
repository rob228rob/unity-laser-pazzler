using UnityEngine;
using Project.Laser;

namespace Project.Environment
{
    public class DoorController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LaserReceiver[] requiredReceivers;

        [Header("Movement")]
        [SerializeField] private Vector3 openLocalOffset = new Vector3(0f, 3f, 0f);
        [SerializeField] private float moveSpeed = 2f;

        private Vector3 closedLocalPosition;
        private Vector3 openLocalPosition;

        private void Awake()
        {
            closedLocalPosition = transform.localPosition;
            openLocalPosition = closedLocalPosition + openLocalOffset;
        }

        private void Update()
        {
            Vector3 targetPosition = AreAllReceiversActive() ? openLocalPosition : closedLocalPosition;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);
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
    }
}
