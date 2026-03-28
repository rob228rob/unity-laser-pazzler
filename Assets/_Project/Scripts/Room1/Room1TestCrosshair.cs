using UnityEngine;

namespace Project.Room1
{
    public sealed class Room1TestCrosshair : MonoBehaviour
    {
        [SerializeField] private float size = 7f;
        [SerializeField] private float thickness = 2f;
        [SerializeField] private float gap = 4f;
        [SerializeField] private Color color = new Color(0.92f, 0.95f, 0.98f, 0.95f);

        private static Texture2D pixel;

        private void OnGUI()
        {
            EnsurePixel();

            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.5f;

            DrawRect(new Rect(centerX - thickness * 0.5f, centerY - gap - size, thickness, size));
            DrawRect(new Rect(centerX - thickness * 0.5f, centerY + gap, thickness, size));
            DrawRect(new Rect(centerX - gap - size, centerY - thickness * 0.5f, size, thickness));
            DrawRect(new Rect(centerX + gap, centerY - thickness * 0.5f, size, thickness));
        }

        private void DrawRect(Rect rect)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, pixel);
            GUI.color = previousColor;
        }

        private static void EnsurePixel()
        {
            if (pixel != null)
            {
                return;
            }

            pixel = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "Runtime_CrosshairPixel"
            };
            pixel.SetPixel(0, 0, Color.white);
            pixel.Apply();
        }
    }
}
