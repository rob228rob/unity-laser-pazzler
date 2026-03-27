using UnityEngine;

namespace Project.Player
{
    public class PlayerObjectiveTracker : MonoBehaviour
    {
        public string CurrentObjective { get; private set; } = string.Empty;

        public void SetObjective(string objectiveText)
        {
            CurrentObjective = objectiveText ?? string.Empty;
        }
    }
}
