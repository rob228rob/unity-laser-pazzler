using UnityEngine;

namespace Project.Room1
{
    [RequireComponent(typeof(CharacterController))]
    public class Room1TestHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 7;
        [SerializeField] private float damageCooldown = 0.35f;
        [SerializeField] private Transform respawnPoint;

        private CharacterController characterController;
        private int currentHealth;
        private float lastDamageTime = -999f;
        private float damageFlashAlpha;

        private Texture2D whiteTexture;
        private GUIStyle titleStyle;
        private GUIStyle valueStyle;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            currentHealth = maxHealth;
        }

        private void Update()
        {
            damageFlashAlpha = Mathf.MoveTowards(damageFlashAlpha, 0f, Time.deltaTime * 2.5f);
        }

        public void ApplyLaserDamage(int amount)
        {
            if (amount <= 0 || Time.time - lastDamageTime < damageCooldown)
            {
                return;
            }

            lastDamageTime = Time.time;
            currentHealth = Mathf.Max(0, currentHealth - amount);
            damageFlashAlpha = 0.16f;

            if (currentHealth <= 0)
            {
                RespawnAndRestore();
            }
        }

        private void RespawnAndRestore()
        {
            if (respawnPoint != null)
            {
                bool wasEnabled = characterController != null && characterController.enabled;
                if (characterController != null)
                {
                    characterController.enabled = false;
                }

                transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);

                if (characterController != null)
                {
                    characterController.enabled = wasEnabled;
                }
            }

            currentHealth = maxHealth;
        }

        private void OnGUI()
        {
            EnsureGuiResources();

            Rect panelRect = new Rect(22f, 22f, 340f, 96f);
            DrawRect(panelRect, new Color(0.05f, 0.08f, 0.10f, 0.88f));

            GUI.Label(new Rect(38f, 30f, 220f, 22f), "PLAYER INTEGRITY", titleStyle);
            GUI.Label(new Rect(38f, 52f, 220f, 26f), currentHealth + " / " + maxHealth, valueStyle);

            for (int i = 0; i < maxHealth; i++)
            {
                Color pipColor = i < currentHealth
                    ? new Color(0.96f, 0.42f, 0.22f, 0.98f)
                    : new Color(0.22f, 0.10f, 0.10f, 0.85f);

                DrawRect(new Rect(38f + i * 42f, 90f, 30f, 14f), pipColor);
            }

            if (damageFlashAlpha > 0f)
            {
                Color previousColor = GUI.color;
                GUI.color = new Color(1f, 0.12f, 0.08f, damageFlashAlpha);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), whiteTexture);
                GUI.color = previousColor;
            }
        }

        private void EnsureGuiResources()
        {
            if (whiteTexture == null)
            {
                whiteTexture = Texture2D.whiteTexture;
            }

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.fontSize = 16;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.normal.textColor = new Color(0.96f, 0.78f, 0.48f);
            }

            if (valueStyle == null)
            {
                valueStyle = new GUIStyle(GUI.skin.label);
                valueStyle.fontSize = 22;
                valueStyle.fontStyle = FontStyle.Bold;
                valueStyle.normal.textColor = new Color(0.96f, 0.94f, 0.86f);
            }
        }

        private void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, whiteTexture);
            GUI.color = previousColor;
        }
    }
}
