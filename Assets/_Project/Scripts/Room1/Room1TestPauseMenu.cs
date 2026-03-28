using Project.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Room1
{
    public sealed class Room1TestPauseMenu : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private Room1TestPlayerController playerController;
        private Room1TestInteraction interaction;
        private Room1TestHealth health;
        private Room1TestLook look;
        private bool isOpen;
        private Rect windowRect;

        private void Awake()
        {
            playerController = GetComponent<Room1TestPlayerController>();
            interaction = GetComponent<Room1TestInteraction>();
            health = GetComponent<Room1TestHealth>();
            look = GetComponentInChildren<Room1TestLook>();
            windowRect = new Rect(0f, 0f, 320f, 240f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Toggle();
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        private void OnGUI()
        {
            if (!isOpen)
            {
                return;
            }

            GUI.depth = -1000;
            Color previousColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;

            windowRect.x = (Screen.width - windowRect.width) * 0.5f;
            windowRect.y = (Screen.height - windowRect.height) * 0.5f;
            windowRect = GUI.ModalWindow(GetInstanceID(), windowRect, DrawWindowContents, "Paused");
        }

        private void DrawWindowContents(int windowId)
        {
            GUILayout.Space(18f);
            GUILayout.Label("Room1_Test paused");
            GUILayout.Space(16f);

            if (GUILayout.Button("Resume", GUILayout.Height(40f)))
            {
                Close();
            }

            GUILayout.Space(10f);

            if (GUILayout.Button("Restart Room", GUILayout.Height(40f)))
            {
                ResumeRuntime();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            GUILayout.Space(10f);

            if (GUILayout.Button("Main Menu", GUILayout.Height(40f)))
            {
                ResumeRuntime();
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }

        private void Toggle()
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        private void Open()
        {
            isOpen = true;
            Time.timeScale = 0f;
            SetGameplayEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Close()
        {
            ResumeRuntime();
        }

        private void ResumeRuntime()
        {
            isOpen = false;
            Time.timeScale = 1f;
            SetGameplayEnabled(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void SetGameplayEnabled(bool enabled)
        {
            if (playerController != null)
            {
                playerController.enabled = enabled;
            }

            if (interaction != null)
            {
                interaction.enabled = enabled;
            }

            if (health != null)
            {
                health.enabled = enabled;
            }

            if (look != null)
            {
                look.enabled = enabled;
            }
        }
    }
}
