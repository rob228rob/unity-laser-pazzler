using UnityEngine;

namespace Project.Core
{
    public class PrototypeCameraManager : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform playerCameraTransform;
        [SerializeField] private Transform spectatorAnchor;
        [SerializeField] private KeyCode toggleCameraKey = KeyCode.F2;
        [SerializeField] private bool startWithPlayerCamera = true;
        [SerializeField] private float spectatorMoveSpeed = 12f;
        [SerializeField] private float spectatorLookSpeed = 2f;

        private Camera runtimeViewCamera;
        private AudioListener runtimeViewListener;
        private AudioListener playerListener;
        private bool usingPlayerView;
        private float spectatorPitch;

        private void Awake()
        {
            ResolveReferences();
            CreateRuntimeViewCamera();
            SetActiveCamera(startWithPlayerCamera);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleCameraKey))
            {
                SetActiveCamera(!usingPlayerView);
            }

            if (!usingPlayerView)
            {
                UpdateSpectatorControls();
            }
        }

        private void ResolveReferences()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (playerCamera != null)
            {
                if (playerCameraTransform == null)
                {
                    playerCameraTransform = playerCamera.transform.parent != null ? playerCamera.transform.parent : playerCamera.transform;
                }

                playerListener = playerCamera.GetComponent<AudioListener>();
                if (playerListener == null)
                {
                    playerListener = playerCamera.gameObject.AddComponent<AudioListener>();
                }
            }
        }

        private void CreateRuntimeViewCamera()
        {
            GameObject cameraObject = new GameObject("Runtime View Camera");
            cameraObject.transform.SetParent(transform, false);

            if (spectatorAnchor != null)
            {
                cameraObject.transform.SetPositionAndRotation(spectatorAnchor.position, spectatorAnchor.rotation);
            }
            else
            {
                cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 4f, -6f), Quaternion.Euler(12f, 0f, 0f));
            }

            runtimeViewCamera = cameraObject.AddComponent<Camera>();
            runtimeViewCamera.clearFlags = CameraClearFlags.SolidColor;
            runtimeViewCamera.backgroundColor = new Color(0.05f, 0.08f, 0.10f, 1f);
            runtimeViewCamera.nearClipPlane = 0.03f;
            runtimeViewCamera.farClipPlane = 200f;
            runtimeViewCamera.fieldOfView = 75f;
            runtimeViewCamera.cullingMask = ~0;
            runtimeViewCamera.depth = 1f;
            runtimeViewCamera.targetTexture = null;
            runtimeViewCamera.rect = new Rect(0f, 0f, 1f, 1f);
            runtimeViewCamera.targetDisplay = 0;
            runtimeViewCamera.orthographic = false;
            runtimeViewCamera.enabled = false;
            runtimeViewCamera.tag = "Untagged";

            runtimeViewListener = cameraObject.AddComponent<AudioListener>();
            runtimeViewListener.enabled = false;
            spectatorPitch = cameraObject.transform.eulerAngles.x;
        }

        private void SetActiveCamera(bool usePlayerView)
        {
            if (playerCamera == null || playerCameraTransform == null || runtimeViewCamera == null)
            {
                ResolveReferences();
                if (playerCamera == null || playerCameraTransform == null || runtimeViewCamera == null)
                {
                    return;
                }
            }

            usingPlayerView = usePlayerView;

            if (usePlayerView)
            {
                EnablePlayerView();
            }
            else
            {
                EnableSpectatorView();
            }
        }

        private void EnablePlayerView()
        {
            runtimeViewCamera.enabled = false;
            runtimeViewCamera.tag = "Untagged";
            runtimeViewCamera.transform.SetParent(transform, false);

            if (runtimeViewListener != null)
            {
                runtimeViewListener.enabled = false;
            }

            playerCamera.enabled = true;
            playerCamera.tag = "MainCamera";

            if (playerListener != null)
            {
                playerListener.enabled = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void EnableSpectatorView()
        {
            runtimeViewCamera.transform.SetParent(transform, false);

            if (playerCamera != null)
            {
                runtimeViewCamera.transform.SetPositionAndRotation(playerCamera.transform.position, playerCamera.transform.rotation);
                spectatorPitch = runtimeViewCamera.transform.eulerAngles.x;
            }
            else if (spectatorAnchor != null)
            {
                runtimeViewCamera.transform.SetPositionAndRotation(spectatorAnchor.position, spectatorAnchor.rotation);
                spectatorPitch = runtimeViewCamera.transform.eulerAngles.x;
            }

            runtimeViewCamera.fieldOfView = playerCamera != null ? playerCamera.fieldOfView : runtimeViewCamera.fieldOfView;
            runtimeViewCamera.nearClipPlane = playerCamera != null ? playerCamera.nearClipPlane : runtimeViewCamera.nearClipPlane;
            runtimeViewCamera.farClipPlane = playerCamera != null ? playerCamera.farClipPlane : runtimeViewCamera.farClipPlane;
            runtimeViewCamera.cullingMask = playerCamera != null ? playerCamera.cullingMask : runtimeViewCamera.cullingMask;
            runtimeViewCamera.clearFlags = playerCamera != null ? playerCamera.clearFlags : runtimeViewCamera.clearFlags;
            runtimeViewCamera.backgroundColor = playerCamera != null ? playerCamera.backgroundColor : runtimeViewCamera.backgroundColor;
            runtimeViewCamera.targetTexture = null;
            runtimeViewCamera.rect = new Rect(0f, 0f, 1f, 1f);
            runtimeViewCamera.targetDisplay = 0;
            runtimeViewCamera.enabled = true;
            runtimeViewCamera.tag = "MainCamera";

            if (runtimeViewListener != null)
            {
                runtimeViewListener.enabled = true;
            }

            playerCamera.enabled = false;
            playerCamera.tag = "Untagged";

            if (playerListener != null)
            {
                playerListener.enabled = false;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void UpdateSpectatorControls()
        {
            float mouseX = Input.GetAxis("Mouse X") * spectatorLookSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * spectatorLookSpeed;

            spectatorPitch -= mouseY;
            spectatorPitch = Mathf.Clamp(spectatorPitch, -80f, 80f);

            Vector3 currentEuler = runtimeViewCamera.transform.eulerAngles;
            runtimeViewCamera.transform.rotation = Quaternion.Euler(spectatorPitch, currentEuler.y + mouseX, 0f);

            Vector3 moveInput = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) moveInput += runtimeViewCamera.transform.forward;
            if (Input.GetKey(KeyCode.S)) moveInput -= runtimeViewCamera.transform.forward;
            if (Input.GetKey(KeyCode.D)) moveInput += runtimeViewCamera.transform.right;
            if (Input.GetKey(KeyCode.A)) moveInput -= runtimeViewCamera.transform.right;
            if (Input.GetKey(KeyCode.E)) moveInput += Vector3.up;
            if (Input.GetKey(KeyCode.Q)) moveInput -= Vector3.up;

            float speed = spectatorMoveSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed *= 2f;
            }

            if (moveInput.sqrMagnitude > 0f)
            {
                runtimeViewCamera.transform.position += moveInput.normalized * speed * Time.unscaledDeltaTime;
            }
        }
    }
}
