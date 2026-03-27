using Project.Player;
using UnityEngine;

namespace Project.Core
{
    [DefaultExecutionOrder(-1000)]
    public class PrototypeSceneBootstrap : MonoBehaviour
    {
        private static readonly Vector3 SpawnLiftOffset = new Vector3(0f, 0.18f, 0f);
        private const float GroundProbeHeight = 4f;
        private const float GroundProbeDistance = 12f;
        private const float GroundOffset = 0.18f;
        private const float FallResetHeight = -10f;

        [SerializeField] private Vector3 cameraPivotLocalPosition = new Vector3(0f, 0.8f, 0f);
        [SerializeField] private Vector3 gameplayCameraLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 gameplayCameraLocalEuler = Vector3.zero;

        [SerializeField] private Transform playerStart;
        [SerializeField] private Transform playerLookTarget;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private PlayerRespawnController respawnController;
        [SerializeField] private PlayerRigGuard rigGuard;

        private bool hasAppliedStartup;

        private void Awake()
        {
            Time.timeScale = 1f;
            ResolveReferences();
            ApplyStartupState();
        }

        private void LateUpdate()
        {
            ResolveReferences();
            MaintainRigLocalState();
            RecoverIfBroken();
        }

        private void ResolveReferences()
        {
            if (playerRoot == null)
            {
                GameObject playerObject = GameObject.Find("Player");
                if (playerObject != null)
                {
                    playerRoot = playerObject.transform;
                }
            }

            if (cameraPivot == null && playerRoot != null)
            {
                cameraPivot = playerRoot.Find("CameraPivot");
            }

            if (gameplayCamera == null && cameraPivot != null)
            {
                gameplayCamera = cameraPivot.GetComponentInChildren<Camera>(true);
            }

            if (respawnController == null && playerRoot != null)
            {
                respawnController = playerRoot.GetComponent<PlayerRespawnController>();
            }

            if (rigGuard == null && playerRoot != null)
            {
                rigGuard = playerRoot.GetComponent<PlayerRigGuard>();
                if (rigGuard == null)
                {
                    rigGuard = playerRoot.gameObject.AddComponent<PlayerRigGuard>();
                }
            }
        }

        private void ApplyStartupState()
        {
            if (hasAppliedStartup || playerRoot == null || playerStart == null)
            {
                return;
            }

            DisableCharacterController();

            playerRoot.SetPositionAndRotation(GetGroundedSpawnPosition(playerStart.position + SpawnLiftOffset), playerStart.rotation);

            if (cameraPivot != null)
            {
                cameraPivot.localPosition = cameraPivotLocalPosition;
                cameraPivot.localRotation = Quaternion.identity;
            }

            if (gameplayCamera != null)
            {
                gameplayCamera.transform.localPosition = gameplayCameraLocalPosition;
                gameplayCamera.transform.localRotation = Quaternion.Euler(gameplayCameraLocalEuler);
                gameplayCamera.enabled = true;
                gameplayCamera.clearFlags = CameraClearFlags.SolidColor;
                gameplayCamera.backgroundColor = new Color(0.05f, 0.08f, 0.10f, 1f);
                gameplayCamera.cullingMask = ~0;
                gameplayCamera.nearClipPlane = 0.03f;
                gameplayCamera.farClipPlane = 150f;
                gameplayCamera.fieldOfView = 75f;
                gameplayCamera.depth = 1f;
                gameplayCamera.targetDisplay = 0;
                gameplayCamera.targetTexture = null;
                gameplayCamera.rect = new Rect(0f, 0f, 1f, 1f);
                gameplayCamera.orthographic = false;
                gameplayCamera.tag = "MainCamera";
            }

            if (playerRoot != null && playerLookTarget != null)
            {
                Vector3 lookDirection = playerLookTarget.position - playerRoot.position;
                lookDirection.y = 0f;
                if (lookDirection.sqrMagnitude > 0.001f)
                {
                    playerRoot.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                }
            }

            if (respawnController != null)
            {
                respawnController.SetRespawnPoint(playerStart.position);
            }

            if (rigGuard != null)
            {
                rigGuard.MarkCurrentAsSafe(playerStart);
            }

            Physics.SyncTransforms();
            hasAppliedStartup = true;
        }

        private void MaintainRigLocalState()
        {
            if (cameraPivot != null)
            {
                cameraPivot.localPosition = cameraPivotLocalPosition;
            }

            if (gameplayCamera != null)
            {
                gameplayCamera.transform.localPosition = gameplayCameraLocalPosition;
                gameplayCamera.transform.localRotation = Quaternion.Euler(gameplayCameraLocalEuler);
            }
        }

        private void RecoverIfBroken()
        {
            if (playerRoot == null || playerStart == null)
            {
                return;
            }

            Vector3 position = playerRoot.position;
            if (position.y >= FallResetHeight)
            {
                return;
            }

            DisableCharacterController();

            playerRoot.SetPositionAndRotation(GetGroundedSpawnPosition(playerStart.position + SpawnLiftOffset), playerStart.rotation);

            if (cameraPivot != null)
            {
                cameraPivot.localPosition = cameraPivotLocalPosition;
                cameraPivot.localRotation = Quaternion.identity;
            }

            if (gameplayCamera != null)
            {
                gameplayCamera.transform.localPosition = gameplayCameraLocalPosition;
                gameplayCamera.transform.localRotation = Quaternion.Euler(gameplayCameraLocalEuler);
            }

            if (respawnController != null)
            {
                respawnController.SetRespawnPoint(playerStart.position);
            }

            if (rigGuard != null)
            {
                rigGuard.MarkCurrentAsSafe(playerStart);
            }

            Physics.SyncTransforms();
        }

        private void DisableCharacterController()
        {
            if (playerRoot == null)
            {
                return;
            }

            CharacterController controller = playerRoot.GetComponent<CharacterController>();
            if (controller != null && controller.enabled)
            {
                controller.enabled = false;
            }
        }
        private Vector3 GetGroundedSpawnPosition(Vector3 desiredPosition)
        {
            Vector3 rayOrigin = desiredPosition + Vector3.up * GroundProbeHeight;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, GroundProbeDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                return new Vector3(desiredPosition.x, hit.point.y + GroundOffset, desiredPosition.z);
            }

            return desiredPosition;
        }
    }
}
