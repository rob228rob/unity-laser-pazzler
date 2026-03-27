using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Project.Core
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = "LaserPuzzle_Prototype";

        private void Awake()
        {
            EnsureMenuCamera();
            EnsureEventSystem();
            BuildMenu();
        }

        private void PlayGame()
        {
            if (!Application.CanStreamedLevelBeLoaded(gameplaySceneName))
            {
                Debug.LogWarning($"Gameplay scene '{gameplaySceneName}' is not in Build Settings.");
                return;
            }

            Time.timeScale = 1f;
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void BuildMenu()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            CreatePanel("Backdrop", transform, new Vector2(1920f, 1080f), new Color(0.05f, 0.08f, 0.10f, 1f));
            CreateText("Title", transform, font, 52, "Laser Puzzle Prototype", new Vector2(0f, 180f), new Vector2(1000f, 80f), new Color(1f, 0.96f, 0.84f));
            CreateText("Subtitle", transform, font, 24, "Reflect beams. Open doors. Clear every room.", new Vector2(0f, 120f), new Vector2(800f, 40f), new Color(0.84f, 0.88f, 0.92f));
            CreateButton("PlayButton", transform, font, "Play", new Vector2(0f, 20f), PlayGame);
            CreateButton("QuitButton", transform, font, "Quit", new Vector2(0f, -50f), QuitGame);
            CreateText("Controls", transform, font, 20, "WASD move | Mouse look | E/Q rotate | Esc pause", new Vector2(0f, -170f), new Vector2(900f, 32f), new Color(0.82f, 0.85f, 0.88f));
        }

        private static void EnsureMenuCamera()
        {
            if (Object.FindFirstObjectByType<Camera>() != null)
            {
                return;
            }

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Camera cameraComponent = cameraObject.AddComponent<Camera>();
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = new Color(0.05f, 0.08f, 0.10f, 1f);
            cameraObject.AddComponent<AudioListener>();
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void CreatePanel(string name, Transform parent, Vector2 size, Color color)
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
        }

        private static void CreateText(string name, Transform parent, Font font, int fontSize, string content, Vector2 anchoredPosition, Vector2 size, Color color)
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
            text.color = color;
            text.text = content;
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
            rectTransform.sizeDelta = new Vector2(260f, 50f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.24f, 0.30f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(220f, 36f);

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = 22;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
        }
    }
}
