using UnityEngine;
using UnityEngine.UI;

namespace Project.Room1
{
    [RequireComponent(typeof(BoxCollider))]
    public class Room1TestVictoryVolume : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string victoryMessage = "You crossed the bridge and cleared the prototype sector.";

        private bool triggered;

        private void Reset()
        {
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggered || other.GetComponent<Room1TestPlayerController>() == null)
            {
                return;
            }

            triggered = true;
            ShowOverlay();
        }

        private void ShowOverlay()
        {
            GameObject canvasObject = new GameObject("Room1TestVictoryCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject panelObject = new GameObject("VictoryPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvasObject.transform, false);
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(920f, 220f);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.04f, 0.07f, 0.09f, 0.92f);

            GameObject textObject = new GameObject("VictoryText", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(panelObject.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(820f, 140f);

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = 34;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.96f, 0.94f, 0.86f);
            text.text = victoryMessage + "\n\nPress Esc to stop play mode or keep exploring.";
        }
    }
}
