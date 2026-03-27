using Project.Player;
using UnityEngine;

namespace Project.Environment
{
    [RequireComponent(typeof(BoxCollider))]
    public class RoomObjectiveVolume : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string objectiveText;

        private void Reset()
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryApplyObjective(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryApplyObjective(other);
        }

        private void TryApplyObjective(Collider other)
        {
            PlayerObjectiveTracker tracker = other.GetComponent<PlayerObjectiveTracker>();

            if (tracker != null)
            {
                tracker.SetObjective(objectiveText);
            }
        }
    }
}
