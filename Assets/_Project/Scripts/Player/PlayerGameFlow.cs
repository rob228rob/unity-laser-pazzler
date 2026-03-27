using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Player
{
    public class PlayerGameFlow : MonoBehaviour
    {
        [SerializeField] private KeyCode restartKey = KeyCode.R;

        public bool IsGameCompleted { get; private set; }

        private void Update()
        {
            if (!IsGameCompleted)
            {
                return;
            }

            if (Input.GetKeyDown(restartKey))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        public void CompleteGame()
        {
            IsGameCompleted = true;
        }
    }
}
