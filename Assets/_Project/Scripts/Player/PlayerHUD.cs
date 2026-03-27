using UnityEngine;
using UnityEngine.UI;

namespace Project.Player
{
    public class PlayerHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private PlayerObjectiveTracker objectiveTracker;
        [SerializeField] private PlayerGameFlow playerGameFlow;

        [Header("Fallback")]
        [SerializeField] private string defaultObjectiveText = "Solve the laser puzzle and open the next doorway.";

        private Text objectiveText;
        private Text promptText;
        private Text completionText;
        private GameObject promptPanel;
        private GameObject completionPanel;

        private void Awake()
        {
            if (playerInteraction == null)
            {
                playerInteraction = FindFirstObjectByType<PlayerInteraction>();
            }

            if (objectiveTracker == null)
            {
                objectiveTracker = FindFirstObjectByType<PlayerObjectiveTracker>();
            }

            if (playerGameFlow == null)
            {
                playerGameFlow = FindFirstObjectByType<PlayerGameFlow>();
            }

            BuildHud();
        }

        private void Update()
        {
            UpdateObjectiveText();
            UpdatePromptText();
            UpdateCompletionState();
        }

        private void BuildHud()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject objectivePanel = CreatePanel("ObjectivePanel", transform, new Vector2(20f, -20f), new Vector2(800f, 92f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.06f, 0.08f, 0.10f, 0.88f));
            objectiveText = CreateText("ObjectiveText", objectivePanel.transform, font, 24, TextAnchor.UpperLeft, Color.white, new Vector2(12f, -10f), new Vector2(776f, 72f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            promptPanel = CreatePanel("PromptPanel", transform, new Vector2(0f, 36f), new Vector2(860f, 56f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Color(0.06f, 0.08f, 0.10f, 0.88f));
            promptText = CreateText("PromptText", promptPanel.transform, font, 22, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.75f), new Vector2(0f, 0f), new Vector2(832f, 40f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            CreateCrosshair(transform);

            completionPanel = CreatePanel("CompletionPanel", transform, Vector2.zero, new Vector2(760f, 160f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.03f, 0.05f, 0.07f, 0.92f));
            completionText = CreateText("CompletionText", completionPanel.transform, font, 30, TextAnchor.MiddleCenter, new Color(1f, 0.97f, 0.86f), new Vector2(0f, 0f), new Vector2(720f, 120f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            completionText.text = "Prototype complete\nPress R to restart";

            UpdateObjectiveText();
            UpdatePromptText();
            UpdateCompletionState();
        }

        private void UpdateObjectiveText()
        {
            if (objectiveText == null)
            {
                return;
            }

            string objective = defaultObjectiveText;

            if (objectiveTracker != null && !string.IsNullOrWhiteSpace(objectiveTracker.CurrentObjective))
            {
                objective = objectiveTracker.CurrentObjective;
            }

            objectiveText.text = objective;
        }

        private void UpdatePromptText()
        {
            if (promptText == null || promptPanel == null)
            {
                return;
            }

            bool hasTarget = playerInteraction != null && playerInteraction.HasInteractableTarget;
            promptText.text = hasTarget ? playerInteraction.CurrentPrompt : string.Empty;
            promptPanel.SetActive(hasTarget);
        }

        private void UpdateCompletionState()
        {
            if (completionText == null || completionPanel == null)
            {
                return;
            }

            bool isCompleted = playerGameFlow != null && playerGameFlow.IsGameCompleted;
            completionPanel.SetActive(isCompleted);
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);

            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = anchorMax;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Image image = panelObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return panelObject;
        }

        private static Text CreateText(string name, Transform parent, Font font, int fontSize, TextAnchor alignment, Color color, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = anchorMax;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void CreateCrosshair(Transform parent)
        {
            CreateCrosshairLine(parent, "CrosshairTop", new Vector2(0f, 10f), new Vector2(2f, 10f));
            CreateCrosshairLine(parent, "CrosshairBottom", new Vector2(0f, -10f), new Vector2(2f, 10f));
            CreateCrosshairLine(parent, "CrosshairLeft", new Vector2(-10f, 0f), new Vector2(10f, 2f));
            CreateCrosshairLine(parent, "CrosshairRight", new Vector2(10f, 0f), new Vector2(10f, 2f));
        }

        private static void CreateCrosshairLine(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject lineObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            lineObject.transform.SetParent(parent, false);

            RectTransform rectTransform = lineObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Image image = lineObject.GetComponent<Image>();
            image.color = new Color(1f, 0.98f, 0.86f, 0.95f);
            image.raycastTarget = false;
        }
    }
}
