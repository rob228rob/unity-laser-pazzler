using Project.Player;
using UnityEngine;

namespace Project.Environment
{
    [RequireComponent(typeof(BoxCollider))]
    public class LevelFinishVolume : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string completionMessage = "Prototype complete. Press R to restart.";

        private void Reset()
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerGameFlow gameFlow = other.GetComponent<PlayerGameFlow>();
            PlayerObjectiveTracker tracker = other.GetComponent<PlayerObjectiveTracker>();

            if (gameFlow != null)
            {
                gameFlow.CompleteGame();
            }

            if (tracker != null)
            {
                tracker.SetObjective(completionMessage);
            }
        }
    }
}
