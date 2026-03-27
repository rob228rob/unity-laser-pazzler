using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.Player
{
    public class PauseMenuController : MonoBehaviour
    {
        public static bool IsPauseMenuOpen { get; private set; }

        [Header("Config")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private GameObject panel;
        private bool isPaused;
        private CursorLockMode previousLockMode;
        private bool previousCursorVisible;

        private void Awake()
        {
            IsPauseMenuOpen = false;
            Time.timeScale = 1f;
            EnsureEventSystem();
            BuildMenu();
            SetMenuVisible(false);
        }

        private void OnDisable()
        {
            if (IsPauseMenuOpen)
            {
                Time.timeScale = 1f;
            }

            IsPauseMenuOpen = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePause();
            }
        }

        private void TogglePause()
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                previousLockMode = Cursor.lockState;
                previousCursorVisible = Cursor.visible;
                Time.timeScale = 0f;
                IsPauseMenuOpen = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1f;
                IsPauseMenuOpen = false;
                Cursor.lockState = previousLockMode == CursorLockMode.None ? CursorLockMode.Locked : previousLockMode;
                Cursor.visible = previousLockMode == CursorLockMode.None ? false : previousCursorVisible;
            }

            SetMenuVisible(isPaused);
        }

        private void ResumeGame()
        {
            if (!isPaused)
            {
                return;
            }

            TogglePause();
        }

        private void RestartScene()
        {
            IsPauseMenuOpen = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void LoadMainMenu()
        {
            if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            {
                Debug.LogWarning($"Main menu scene '{mainMenuSceneName}' is not in Build Settings.");
                return;
            }

            IsPauseMenuOpen = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void BuildMenu()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            panel = CreatePanel("PausePanel", transform, new Vector2(420f, 360f), new Color(0.03f, 0.05f, 0.07f, 0.95f));
            CreateText("PauseTitle", panel.transform, font, 36, "Paused", new Vector2(0f, 120f), new Vector2(340f, 60f));
            CreateButton("ResumeButton", panel.transform, font, "Resume", new Vector2(0f, 30f), ResumeGame);
            CreateButton("RestartButton", panel.transform, font, "Restart Room", new Vector2(0f, -40f), RestartScene);
            CreateButton("MenuButton", panel.transform, font, "Main Menu", new Vector2(0f, -110f), LoadMainMenu);
            CreateText("HintText", panel.transform, font, 18, "E/Q rotate reflectors", new Vector2(0f, -155f), new Vector2(320f, 30f));
        }

        private void SetMenuVisible(bool visible)
        {
            if (panel != null)
            {
                panel.SetActive(visible);
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Object.DontDestroyOnLoad(eventSystemObject);
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 size, Color color)
        {
            GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);

            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = size;

            Image image = panelObject.GetComponent<Image>();
            image.color = color;
            return panelObject;
        }

        private static Text CreateText(string name, Transform parent, Font font, int fontSize, string content, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = content;
            return text;
        }

        private static void CreateButton(string name, Transform parent, Font font, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(280f, 48f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.23f, 0.28f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.18f, 0.23f, 0.28f, 1f);
            colors.highlightedColor = new Color(0.25f, 0.32f, 0.38f, 1f);
            colors.pressedColor = new Color(0.12f, 0.16f, 0.20f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            button.onClick.AddListener(onClick);

            CreateText("Label", buttonObject.transform, font, 22, label, Vector2.zero, new Vector2(240f, 40f));
        }
    }
}
