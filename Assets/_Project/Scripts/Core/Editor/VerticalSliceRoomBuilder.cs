using System.Collections.Generic;
using System.IO;
using Project.Core;
using Project.Environment;
using Project.Laser;
using Project.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Project.Core.Editor
{
    public static class VerticalSliceRoomBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/LaserPuzzle_Prototype.unity";
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string MaterialsFolder = "Assets/_Project/Materials";

        private const float RoomWidth = 24f;
        private const float WallHeight = 6f;
        private const float WorldStartZ = -14f;
        private const float Door01Z = 14f;
        private const float Door02Z = 40f;
        private const float Door03Z = 66f;
        private const float Door04Z = 92f;
        private const float WorldEndZ = 108f;

        [MenuItem("Tools/Laser Puzzle/Create Production Prototype Scene")]
        public static void CreateProductionPrototypeScene()
        {
            BuildPrototypeScene();
        }

        [MenuItem("Tools/Laser Puzzle/Create Vertical Slice Scene")]
        public static void CreateVerticalSliceScene()
        {
            BuildPrototypeScene();
        }

        private static void BuildPrototypeScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureFolder("Assets/_Project");
            EnsureFolder("Assets/_Project/Scenes");
            EnsureFolder(MaterialsFolder);

            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Scene scene = SceneManager.GetActiveScene();

            SetupRenderSettings();
            MaterialPalette palette = CreateMaterialPalette();

            GameObject root = new GameObject("LaserPuzzle_Prototype");
            GameObject world = new GameObject("World");
            world.transform.SetParent(root.transform, false);

            ConfigureSceneDirectionalLight(root.transform);
            RemoveDefaultSceneCamera();
            CreateWorldShell(world.transform, palette);

            DoorController door01 = CreateDoorAssembly(world.transform, palette, "Room01_Doorway", Door01Z, new Color(1f, 0.67f, 0.34f));
            DoorController door02 = CreateDoorAssembly(world.transform, palette, "Room02_Doorway", Door02Z, new Color(0.38f, 0.84f, 0.96f));
            DoorController door03 = CreateDoorAssembly(world.transform, palette, "Room03_Doorway", Door03Z, new Color(0.50f, 0.96f, 0.76f));
            DoorController door04 = CreateDoorAssembly(world.transform, palette, "Room04_Doorway", Door04Z, new Color(1f, 0.78f, 0.42f));

            CreateRoom01(world.transform, palette, door01);
            CreateRoom02(world.transform, palette, door02);
            CreateRoom03(world.transform, palette, door03);
            CreateRoom04(world.transform, palette, door04);
            CreateFinishArea(world.transform, palette);

            Transform playerStart = CreatePlayerStart(world.transform);
            Transform playerLookTarget = CreatePlayerLookTarget(world.transform);
            Transform spectatorAnchor = CreateSpectatorAnchor(world.transform);
            Transform respawnAnchor = CreateRespawnAnchor(world.transform);
            CreateRespawnVolume(world.transform);

            PlayerBundle player = CreatePlayer(root.transform, playerStart, respawnAnchor);
            CreateSceneBootstrap(root.transform, playerStart, playerLookTarget, player);
            CreateCameraManager(root.transform, player, spectatorAnchor);
            CreateHud(root.transform, player);
            CreateObjectives(world.transform);
            CreateFinishVolume(world.transform);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddScenesToBuildSettings(ScenePath, MainMenuScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = root;

            Debug.Log($"Production prototype scene created at {ScenePath}");
        }

        private static void SetupRenderSettings()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.055f, 0.075f, 0.10f);
            RenderSettings.fogStartDistance = 24f;
            RenderSettings.fogEndDistance = 140f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.16f, 0.19f, 0.22f);
            RenderSettings.subtractiveShadowColor = new Color(0.08f, 0.10f, 0.12f);
        }

        private static MaterialPalette CreateMaterialPalette()
        {
            MaterialPalette palette = new MaterialPalette
            {
                Floor = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_ProtoFloor.mat", new Color(0.18f, 0.21f, 0.24f), 0.22f),
                Wall = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_ProtoWall.mat", new Color(0.23f, 0.27f, 0.31f), 0.12f),
                Trim = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_ProtoTrim.mat", new Color(0.55f, 0.47f, 0.34f), 0.45f),
                Door = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_ProtoDoor.mat", new Color(0.29f, 0.39f, 0.45f), 0.38f),
                Emitter = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_ProtoEmitter.mat", new Color(0.41f, 0.16f, 0.14f), 0.32f, new Color(0.95f, 0.24f, 0.18f) * 0.6f),
                Reflector = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_ProtoReflector.mat", new Color(0.86f, 0.84f, 0.76f), 0.75f),
                ReceiverBase = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_ProtoReceiverBase.mat", new Color(0.17f, 0.23f, 0.28f), 0.28f),
                Bridge = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_ProtoBridge.mat", new Color(0.17f, 0.23f, 0.28f), 0.18f),
                Pit = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_ProtoPit.mat", new Color(0.03f, 0.04f, 0.05f), 0f),
                GlowCyan = CreateOrUpdateUnlitMaterial($"{MaterialsFolder}/M_GlowCyan.mat", new Color(0.31f, 0.88f, 0.97f)),
                GlowAmber = CreateOrUpdateUnlitMaterial($"{MaterialsFolder}/M_GlowAmber.mat", new Color(1f, 0.72f, 0.38f)),
                GlowGreen = CreateOrUpdateUnlitMaterial($"{MaterialsFolder}/M_GlowGreen.mat", new Color(0.48f, 0.95f, 0.74f)),
                Laser = CreateOrUpdateUnlitMaterial($"{MaterialsFolder}/M_LaserRed.mat", new Color(1f, 0.18f, 0.14f))
            };

            return palette;
        }

        private static void ConfigureSceneDirectionalLight(Transform parent)
        {
            Light light = Object.FindFirstObjectByType<Light>();
            GameObject lightObject = light != null ? light.gameObject : new GameObject("Directional Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.rotation = Quaternion.Euler(46f, -28f, 0f);

            if (light == null)
            {
                light = lightObject.AddComponent<Light>();
            }

            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.9f);
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
        }

        private static void CreateWorldShell(Transform parent, MaterialPalette palette)
        {
            GameObject geometry = new GameObject("Geometry");
            geometry.transform.SetParent(parent, false);

            float worldLength = WorldEndZ - WorldStartZ;
            float worldCenterZ = (WorldStartZ + WorldEndZ) * 0.5f;
            float wallX = RoomWidth * 0.5f + 0.5f;

            CreateBox("GlobalFloor_A", geometry.transform, new Vector3(0f, -0.5f, 28f), new Vector3(RoomWidth, 1f, 84f), palette.Floor);
            CreateBox("GlobalFloor_B", geometry.transform, new Vector3(0f, -0.5f, 100f), new Vector3(RoomWidth, 1f, 16f), palette.Floor);
            CreateBox("GlobalCeiling", geometry.transform, new Vector3(0f, 5.5f, worldCenterZ), new Vector3(RoomWidth, 1f, worldLength), palette.Wall);
            CreateBox("GlobalWall_Left", geometry.transform, new Vector3(-wallX, 2.5f, worldCenterZ), new Vector3(1f, 5f, worldLength), palette.Wall);
            CreateBox("GlobalWall_Right", geometry.transform, new Vector3(wallX, 2.5f, worldCenterZ), new Vector3(1f, 5f, worldLength), palette.Wall);
            CreateBox("StartWall", geometry.transform, new Vector3(0f, 2.5f, WorldStartZ), new Vector3(RoomWidth, 5f, 1f), palette.Wall);
            CreateBox("EndWall", geometry.transform, new Vector3(0f, 2.5f, WorldEndZ), new Vector3(RoomWidth, 5f, 1f), palette.Wall);
            CreateBox("StartPlatformBackstop", geometry.transform, new Vector3(0f, 1.4f, -11f), new Vector3(8f, 2.8f, 1f), palette.Wall);

            GameObject pitVisual = CreateDecorativeBox("PitVoid", geometry.transform, new Vector3(0f, -4f, 80f), new Vector3(RoomWidth - 1f, 7f, 22f), palette.Pit);
            pitVisual.transform.SetAsFirstSibling();

            CreateFloorStrip(geometry.transform, palette.GlowAmber, new Vector3(0f, 0.03f, 0f), 18f);
            CreateFloorStrip(geometry.transform, palette.GlowCyan, new Vector3(0f, 0.03f, 26f), 18f);
            CreateFloorStrip(geometry.transform, palette.GlowGreen, new Vector3(0f, 0.03f, 52f), 18f);
            CreateFloorStrip(geometry.transform, palette.GlowAmber, new Vector3(0f, 0.03f, 98f), 10f);
            CreateDecorativeBox("StartGuideMarker", geometry.transform, new Vector3(0f, 0.08f, 6f), new Vector3(2.5f, 0.04f, 2.5f), palette.GlowAmber);

            CreateCeilingLamp(geometry.transform, palette.GlowAmber, new Vector3(0f, 4.8f, 0f), new Color(1f, 0.67f, 0.34f), 12f);
            CreateCeilingLamp(geometry.transform, palette.GlowCyan, new Vector3(0f, 4.8f, 26f), new Color(0.38f, 0.84f, 0.96f), 12f);
            CreateCeilingLamp(geometry.transform, palette.GlowGreen, new Vector3(0f, 4.8f, 52f), new Color(0.50f, 0.96f, 0.76f), 12f);
            CreateCeilingLamp(geometry.transform, palette.GlowAmber, new Vector3(0f, 4.8f, 78f), new Color(1f, 0.74f, 0.42f), 14f);
            CreateCeilingLamp(geometry.transform, palette.GlowGreen, new Vector3(0f, 4.8f, 100f), new Color(0.50f, 0.96f, 0.76f), 10f);
            CreateAccentLight(geometry.transform, "StartLight", new Vector3(0f, 2.8f, 1f), new Color(1f, 0.72f, 0.42f), 10f, 3.5f);
        }
        private static void CreateRoom01(Transform parent, MaterialPalette palette, DoorController door)
        {
            GameObject room = new GameObject("Room01");
            room.transform.SetParent(parent, false);

            CreateRoomFrame(room.transform, palette, 0f, new Color(1f, 0.67f, 0.34f));

            LaserEmitter emitter = CreateEmitter(room.transform, palette, "Room01_Emitter", new Vector3(-8.5f, 1.1f, -6f), Quaternion.Euler(0f, 90f, 0f));
            CreateReflector(room.transform, palette, "Room01_Reflector_A", new Vector3(0f, 1.1f, -6f), Quaternion.Euler(0f, 135f, 0f));
            LaserReceiver receiver = CreateReceiver(room.transform, palette, "Room01_Receiver", new Vector3(0f, 1.05f, 8f));

            AssignObjectArray(door, "requiredReceivers", new Object[] { receiver });
            CreateEmitterAccent(room.transform, palette.GlowAmber, emitter.transform.position + Vector3.up * 1.5f);
            CreateReceiverAccent(room.transform, palette.GlowAmber, receiver.transform.position + Vector3.up * 1.5f);
        }

        private static void CreateRoom02(Transform parent, MaterialPalette palette, DoorController door)
        {
            GameObject room = new GameObject("Room02");
            room.transform.SetParent(parent, false);

            CreateRoomFrame(room.transform, palette, 26f, new Color(0.38f, 0.84f, 0.96f));

            LaserEmitter emitter = CreateEmitter(room.transform, palette, "Room02_Emitter", new Vector3(-8.5f, 1.1f, 18f), Quaternion.Euler(0f, 90f, 0f));
            CreateReflector(room.transform, palette, "Room02_Reflector_A", new Vector3(-1.5f, 1.1f, 18f), Quaternion.Euler(0f, 135f, 0f));
            CreateReflector(room.transform, palette, "Room02_Reflector_B", new Vector3(-1.5f, 1.1f, 30f), Quaternion.Euler(0f, 45f, 0f));
            LaserReceiver receiver = CreateReceiver(room.transform, palette, "Room02_Receiver", new Vector3(7.5f, 1.05f, 30f));

            AssignObjectArray(door, "requiredReceivers", new Object[] { receiver });
            CreateEmitterAccent(room.transform, palette.GlowCyan, emitter.transform.position + Vector3.up * 1.5f);
            CreateReceiverAccent(room.transform, palette.GlowCyan, receiver.transform.position + Vector3.up * 1.5f);
        }

        private static void CreateRoom03(Transform parent, MaterialPalette palette, DoorController door)
        {
            GameObject room = new GameObject("Room03");
            room.transform.SetParent(parent, false);

            CreateRoomFrame(room.transform, palette, 52f, new Color(0.50f, 0.96f, 0.76f));

            LaserEmitter leftEmitter = CreateEmitter(room.transform, palette, "Room03_Emitter_Left", new Vector3(-8.5f, 1.1f, 44f), Quaternion.Euler(0f, 90f, 0f));
            CreateReflector(room.transform, palette, "Room03_Reflector_Left", new Vector3(-2.5f, 1.1f, 44f), Quaternion.Euler(0f, 135f, 0f));
            LaserReceiver leftReceiver = CreateReceiver(room.transform, palette, "Room03_Receiver_Left", new Vector3(-2.5f, 1.05f, 58f));

            LaserEmitter rightEmitter = CreateEmitter(room.transform, palette, "Room03_Emitter_Right", new Vector3(8.5f, 1.1f, 44f), Quaternion.Euler(0f, -90f, 0f));
            CreateReflector(room.transform, palette, "Room03_Reflector_Right", new Vector3(2.5f, 1.1f, 44f), Quaternion.Euler(0f, 135f, 0f));
            LaserReceiver rightReceiver = CreateReceiver(room.transform, palette, "Room03_Receiver_Right", new Vector3(2.5f, 1.05f, 58f));

            AssignObjectArray(door, "requiredReceivers", new Object[] { leftReceiver, rightReceiver });
            CreateEmitterAccent(room.transform, palette.GlowGreen, leftEmitter.transform.position + Vector3.up * 1.5f);
            CreateEmitterAccent(room.transform, palette.GlowGreen, rightEmitter.transform.position + Vector3.up * 1.5f);
            CreateReceiverAccent(room.transform, palette.GlowGreen, leftReceiver.transform.position + Vector3.up * 1.5f);
            CreateReceiverAccent(room.transform, palette.GlowGreen, rightReceiver.transform.position + Vector3.up * 1.5f);
        }

        private static void CreateRoom04(Transform parent, MaterialPalette palette, DoorController door)
        {
            GameObject room = new GameObject("Room04");
            room.transform.SetParent(parent, false);

            CreateRoomFrame(room.transform, palette, 78f, new Color(1f, 0.74f, 0.42f));

            LaserEmitter bridgeEmitter = CreateEmitter(room.transform, palette, "Room04_BridgeEmitter", new Vector3(-8.5f, 1.1f, 72f), Quaternion.Euler(0f, 90f, 0f));
            CreateReflector(room.transform, palette, "Room04_Reflector_A", new Vector3(-2.5f, 1.1f, 72f), Quaternion.Euler(0f, 135f, 0f));
            LaserReceiver bridgeReceiver = CreateReceiver(room.transform, palette, "Room04_BridgeReceiver", new Vector3(-2.5f, 1.05f, 86f));

            LaserBridgeController bridge = CreateBridge(room.transform, palette, new Vector3(0f, 0.2f, 80f), new Vector3(4f, 0.4f, 18f));
            AssignObjectArray(bridge, "requiredReceivers", new Object[] { bridgeReceiver });

            LaserEmitter doorEmitter = CreateEmitter(room.transform, palette, "Room04_DoorEmitter", new Vector3(8.5f, 1.1f, 84f), Quaternion.Euler(0f, -90f, 0f));
            CreateReflector(room.transform, palette, "Room04_Reflector_B", new Vector3(2.5f, 1.1f, 84f), Quaternion.Euler(0f, 135f, 0f));
            LaserReceiver doorReceiver = CreateReceiver(room.transform, palette, "Room04_DoorReceiver", new Vector3(2.5f, 1.05f, 90f));

            AssignObjectArray(door, "requiredReceivers", new Object[] { doorReceiver });
            CreateEmitterAccent(room.transform, palette.GlowAmber, bridgeEmitter.transform.position + Vector3.up * 1.5f);
            CreateEmitterAccent(room.transform, palette.GlowAmber, doorEmitter.transform.position + Vector3.up * 1.5f);
            CreateReceiverAccent(room.transform, palette.GlowAmber, bridgeReceiver.transform.position + Vector3.up * 1.5f);
            CreateReceiverAccent(room.transform, palette.GlowAmber, doorReceiver.transform.position + Vector3.up * 1.5f);
        }

        private static void CreateFinishArea(Transform parent, MaterialPalette palette)
        {
            GameObject finish = new GameObject("FinishArea");
            finish.transform.SetParent(parent, false);

            CreateBox("FinishPad", finish.transform, new Vector3(0f, -0.2f, 100f), new Vector3(8f, 0.6f, 8f), palette.Trim);
            CreateDecorativeBox("FinishStrip", finish.transform, new Vector3(0f, 0.05f, 100f), new Vector3(5f, 0.05f, 5f), palette.GlowGreen);
            CreateAccentLight(finish.transform, "FinishLight", new Vector3(0f, 2.6f, 100f), new Color(0.50f, 0.96f, 0.76f), 6f, 3.2f);
        }

        private static Transform CreatePlayerStart(Transform parent)
        {
            GameObject start = new GameObject("PlayerStart");
            start.transform.SetParent(parent, false);
            start.transform.position = new Vector3(0f, 1.10f, -2f);
            start.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            return start.transform;
        }

        private static Transform CreatePlayerLookTarget(Transform parent)
        {
            GameObject lookTarget = new GameObject("PlayerLookTarget");
            lookTarget.transform.SetParent(parent, false);
            lookTarget.transform.position = new Vector3(0f, 1.4f, 8f);
            return lookTarget.transform;
        }

        private static Transform CreateSpectatorAnchor(Transform parent)
        {
            GameObject spectatorAnchor = new GameObject("SpectatorAnchor");
            spectatorAnchor.transform.SetParent(parent, false);
            spectatorAnchor.transform.position = new Vector3(0f, 3.5f, -2f);
            spectatorAnchor.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            return spectatorAnchor.transform;
        }

        private static Transform CreateRespawnAnchor(Transform parent)
        {
            GameObject respawnAnchor = new GameObject("RespawnAnchor");
            respawnAnchor.transform.SetParent(parent, false);
            respawnAnchor.transform.position = new Vector3(0f, 1.10f, 69f);
            return respawnAnchor.transform;
        }

        private static void CreateRespawnVolume(Transform parent)
        {
            GameObject volumeObject = new GameObject("PitRespawnVolume");
            volumeObject.transform.SetParent(parent, false);
            volumeObject.transform.position = new Vector3(0f, -5f, 80f);

            BoxCollider boxCollider = volumeObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(RoomWidth - 1f, 6f, 24f);

            volumeObject.AddComponent<RespawnVolume>();
        }

        private static PlayerBundle CreatePlayer(Transform parent, Transform playerStart, Transform respawnPoint)
        {
            GameObject player = new GameObject("Player");
            player.transform.SetParent(parent, false);
            player.transform.position = playerStart.position;
            player.transform.rotation = playerStart.rotation;

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.center = new Vector3(0f, 1f, 0f);
            characterController.radius = 0.35f;
            characterController.height = 2f;
            characterController.stepOffset = 0.3f;

            player.AddComponent<FirstPersonPlayerController>();
            PlayerObjectiveTracker objectiveTracker = player.AddComponent<PlayerObjectiveTracker>();
            PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();
            PlayerGameFlow gameFlow = player.AddComponent<PlayerGameFlow>();
            PauseMenuController pauseMenu = player.AddComponent<PauseMenuController>();
            PlayerRespawnController respawnController = player.AddComponent<PlayerRespawnController>();

            GameObject cameraPivot = new GameObject("CameraPivot");
            cameraPivot.transform.SetParent(player.transform, false);
            cameraPivot.transform.localPosition = new Vector3(0f, 0.8f, 0f);

            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.transform.SetParent(cameraPivot.transform, false);
            camera.transform.localPosition = Vector3.zero;
            camera.transform.localRotation = Quaternion.identity;
            camera.name = "Main Camera";
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 150f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.08f, 0.10f, 1f);
            camera.cullingMask = ~0;
            camera.depth = 0f;
            camera.fieldOfView = 75f;
            camera.targetDisplay = 0;
            camera.enabled = true;
            camera.targetTexture = null;
            camera.rect = new Rect(0f, 0f, 1f, 1f);
            camera.orthographic = false;

            camera.gameObject.AddComponent<AudioListener>();

            PlayerLook look = cameraPivot.AddComponent<PlayerLook>();

            AssignObject(look, "playerBody", player.transform);
            AssignObject(interaction, "playerCamera", camera);
            AssignString(pauseMenu, "mainMenuSceneName", "MainMenu");
            AssignObject(respawnController, "respawnPoint", respawnPoint);

            return new PlayerBundle
            {
                Root = player,
                Camera = camera,
                Interaction = interaction,
                ObjectiveTracker = objectiveTracker,
                GameFlow = gameFlow,
                RespawnController = respawnController
            };
        }

        private static void CreateHud(Transform parent, PlayerBundle player)
        {
            GameObject hud = new GameObject("HUD");
            hud.transform.SetParent(parent, false);

            PlayerHUD playerHud = hud.AddComponent<PlayerHUD>();
            AssignObject(playerHud, "playerInteraction", player.Interaction);
            AssignObject(playerHud, "objectiveTracker", player.ObjectiveTracker);
            AssignObject(playerHud, "playerGameFlow", player.GameFlow);
            AssignString(playerHud, "defaultObjectiveText", "Room 1: Rotate the reflector to power the exit receiver.");
        }

        private static void CreateSceneBootstrap(Transform parent, Transform playerStart, Transform playerLookTarget, PlayerBundle player)
        {
            GameObject bootstrapObject = new GameObject("PrototypeSceneBootstrap");
            bootstrapObject.transform.SetParent(parent, false);

            PrototypeSceneBootstrap bootstrap = bootstrapObject.AddComponent<PrototypeSceneBootstrap>();
            AssignObject(bootstrap, "playerStart", playerStart);
            AssignObject(bootstrap, "playerLookTarget", playerLookTarget);
            AssignObject(bootstrap, "playerRoot", player.Root.transform);
            AssignObject(bootstrap, "cameraPivot", player.Camera.transform.parent);
            AssignObject(bootstrap, "gameplayCamera", player.Camera);
            AssignObject(bootstrap, "respawnController", player.RespawnController);
        }

        private static void CreateCameraManager(Transform parent, PlayerBundle player, Transform spectatorAnchor)
        {
            GameObject managerObject = new GameObject("PrototypeCameraManager");
            managerObject.transform.SetParent(parent, false);

            PrototypeCameraManager cameraManager = managerObject.AddComponent<PrototypeCameraManager>();
            AssignObject(cameraManager, "playerCamera", player.Camera);
            AssignObject(cameraManager, "playerCameraTransform", player.Camera.transform.parent);
            AssignObject(cameraManager, "spectatorAnchor", spectatorAnchor);
        }

        private static void CreateObjectives(Transform parent)
        {
            CreateObjectiveVolume(parent, "Room01_Objective", new Vector3(0f, 1.5f, 0f), new Vector3(22f, 3f, 24f), "Room 1: Rotate the reflector to power the exit receiver.");
            CreateObjectiveVolume(parent, "Room02_Objective", new Vector3(0f, 1.5f, 26f), new Vector3(22f, 3f, 24f), "Room 2: Chain two reflections to reach the side receiver.");
            CreateObjectiveVolume(parent, "Room03_Objective", new Vector3(0f, 1.5f, 52f), new Vector3(22f, 3f, 24f), "Room 3: Activate both receivers to unlock the bulkhead.");
            CreateObjectiveVolume(parent, "Room04_Objective", new Vector3(0f, 1.5f, 78f), new Vector3(22f, 3f, 24f), "Room 4: Raise the bridge, cross the pit, and open the final gate.");
        }

        private static void CreateFinishVolume(Transform parent)
        {
            GameObject finishVolume = new GameObject("FinishVolume");
            finishVolume.transform.SetParent(parent, false);
            finishVolume.transform.position = new Vector3(0f, 1.5f, 100f);

            BoxCollider boxCollider = finishVolume.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(8f, 3f, 8f);

            LevelFinishVolume finish = finishVolume.AddComponent<LevelFinishVolume>();
            AssignString(finish, "completionMessage", "Prototype complete. Press R to restart or Esc for menu.");
        }

        private static void CreateObjectiveVolume(Transform parent, string name, Vector3 position, Vector3 size, string text)
        {
            GameObject volumeObject = new GameObject(name);
            volumeObject.transform.SetParent(parent, false);
            volumeObject.transform.position = position;

            BoxCollider boxCollider = volumeObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = size;

            RoomObjectiveVolume volume = volumeObject.AddComponent<RoomObjectiveVolume>();
            AssignString(volume, "objectiveText", text);
        }
        private static DoorController CreateDoorAssembly(Transform parent, MaterialPalette palette, string name, float zPosition, Color glowColor)
        {
            GameObject assembly = new GameObject(name);
            assembly.transform.SetParent(parent, false);

            CreateBox("Frame_Left", assembly.transform, new Vector3(-7f, 2.5f, zPosition), new Vector3(10f, 5f, 1f), palette.Wall);
            CreateBox("Frame_Right", assembly.transform, new Vector3(7f, 2.5f, zPosition), new Vector3(10f, 5f, 1f), palette.Wall);
            CreateBox("Frame_Top", assembly.transform, new Vector3(0f, 4.5f, zPosition), new Vector3(4f, 1f, 1f), palette.Wall);
            CreateDecorativeBox("Trim_Left", assembly.transform, new Vector3(-2.25f, 2.1f, zPosition - 0.02f), new Vector3(0.3f, 4.2f, 0.1f), palette.Trim);
            CreateDecorativeBox("Trim_Right", assembly.transform, new Vector3(2.25f, 2.1f, zPosition - 0.02f), new Vector3(0.3f, 4.2f, 0.1f), palette.Trim);
            CreateDecorativeBox("Trim_Top", assembly.transform, new Vector3(0f, 4.25f, zPosition - 0.02f), new Vector3(4.4f, 0.25f, 0.1f), palette.Trim);

            Material doorwayGlow = glowColor.g > 0.8f && glowColor.r < 0.7f ? palette.GlowGreen : palette.GlowAmber;
            if (glowColor.b > 0.8f)
            {
                doorwayGlow = palette.GlowCyan;
            }

            CreateDecorativeBox("Glow_Left", assembly.transform, new Vector3(-2.02f, 2.1f, zPosition - 0.12f), new Vector3(0.08f, 3.8f, 0.08f), doorwayGlow);
            CreateDecorativeBox("Glow_Right", assembly.transform, new Vector3(2.02f, 2.1f, zPosition - 0.12f), new Vector3(0.08f, 3.8f, 0.08f), doorwayGlow);
            CreateDecorativeBox("Glow_Top", assembly.transform, new Vector3(0f, 4.02f, zPosition - 0.12f), new Vector3(3.9f, 0.08f, 0.08f), doorwayGlow);

            GameObject doorObject = CreateBox("Door", assembly.transform, new Vector3(0f, 2f, zPosition), new Vector3(4f, 4f, 0.8f), palette.Door);
            DoorController door = doorObject.AddComponent<DoorController>();
            AssignVector3(door, "openLocalOffset", new Vector3(0f, 4.2f, 0f));
            AssignFloat(door, "moveSpeed", 3f);

            CreateAccentLight(assembly.transform, "DoorLight", new Vector3(0f, 4.2f, zPosition - 1.3f), glowColor, 5f, 3.2f);
            return door;
        }

        private static void CreateRoomFrame(Transform parent, MaterialPalette palette, float centerZ, Color roomColor)
        {
            CreateDecorativeBox($"RoomFrame_Left_{centerZ:0}", parent, new Vector3(-11.3f, 2.4f, centerZ), new Vector3(0.25f, 4.4f, 20f), palette.Trim);
            CreateDecorativeBox($"RoomFrame_Right_{centerZ:0}", parent, new Vector3(11.3f, 2.4f, centerZ), new Vector3(0.25f, 4.4f, 20f), palette.Trim);
            CreateDecorativeBox($"RoomFrame_Top_{centerZ:0}", parent, new Vector3(0f, 4.7f, centerZ), new Vector3(22f, 0.18f, 20f), palette.Trim);

            Material glowMaterial = roomColor.b > 0.85f ? palette.GlowCyan : roomColor.g > 0.85f ? palette.GlowGreen : palette.GlowAmber;
            CreateDecorativeBox($"RoomGlow_Left_{centerZ:0}", parent, new Vector3(-11.05f, 2.2f, centerZ), new Vector3(0.08f, 3.6f, 18f), glowMaterial);
            CreateDecorativeBox($"RoomGlow_Right_{centerZ:0}", parent, new Vector3(11.05f, 2.2f, centerZ), new Vector3(0.08f, 3.6f, 18f), glowMaterial);
            CreateAccentLight(parent, $"RoomLight_{centerZ:0}", new Vector3(0f, 2.8f, centerZ), roomColor, 8f, 1.8f);
        }

        private static LaserEmitter CreateEmitter(Transform parent, MaterialPalette palette, string name, Vector3 position, Quaternion rotation)
        {
            GameObject emitterObject = new GameObject(name);
            emitterObject.transform.SetParent(parent, false);
            emitterObject.transform.position = position;
            emitterObject.transform.rotation = rotation;

            LineRenderer lineRenderer = emitterObject.AddComponent<LineRenderer>();
            lineRenderer.material = palette.Laser;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.08f;
            lineRenderer.numCapVertices = 2;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.alignment = LineAlignment.View;

            LaserEmitter emitter = emitterObject.AddComponent<LaserEmitter>();

            GameObject origin = new GameObject("LaserOrigin");
            origin.transform.SetParent(emitterObject.transform, false);
            origin.transform.localPosition = new Vector3(0f, 0f, 0.65f);

            GameObject body = CreateCylinder("EmitterBody", emitterObject.transform, Vector3.zero, new Vector3(0.6f, 0.35f, 0.6f), palette.Emitter);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreateDecorativeBox("EmitterGlow", emitterObject.transform, new Vector3(0f, 0f, 0.65f), new Vector3(0.28f, 0.28f, 0.08f), palette.GlowAmber);
            CreateCylinder("EmitterBase", emitterObject.transform, new Vector3(0f, -0.45f, 0f), new Vector3(0.8f, 0.16f, 0.8f), palette.Trim);

            AssignObject(emitter, "originPoint", origin.transform);
            AssignObject(emitter, "lineRenderer", lineRenderer);
            AssignFloat(emitter, "maxDistance", 48f);
            AssignInt(emitter, "maxBounces", 6);
            return emitter;
        }

        private static LaserReflector CreateReflector(Transform parent, MaterialPalette palette, string name, Vector3 position, Quaternion rotation)
        {
            GameObject reflectorRoot = new GameObject(name);
            reflectorRoot.transform.SetParent(parent, false);
            reflectorRoot.transform.position = position;
            reflectorRoot.transform.rotation = rotation;

            LaserReflector reflector = reflectorRoot.AddComponent<LaserReflector>();
            AssignString(reflector, "reflectorName", name.Replace('_', ' '));

            CreateCylinder("Base", reflectorRoot.transform, new Vector3(0f, -0.52f, 0f), new Vector3(1.1f, 0.14f, 1.1f), palette.Trim);
            CreateBox("Mesh", reflectorRoot.transform, Vector3.zero, new Vector3(2.2f, 2.2f, 0.18f), palette.Reflector);
            CreateDecorativeBox("Glow", reflectorRoot.transform, new Vector3(0f, 0f, -0.14f), new Vector3(1.8f, 0.06f, 0.04f), palette.GlowCyan);
            return reflector;
        }

        private static LaserReceiver CreateReceiver(Transform parent, MaterialPalette palette, string name, Vector3 position)
        {
            GameObject receiverRoot = new GameObject(name);
            receiverRoot.transform.SetParent(parent, false);
            receiverRoot.transform.position = position;

            CreateCylinder("Base", receiverRoot.transform, new Vector3(0f, -0.42f, 0f), new Vector3(1f, 0.16f, 1f), palette.ReceiverBase);
            GameObject core = CreateBox("Core", receiverRoot.transform, Vector3.zero, new Vector3(1f, 1f, 1f), palette.ReceiverBase);
            CreateDecorativeBox("Frame", receiverRoot.transform, new Vector3(0f, 0f, -0.44f), new Vector3(1.12f, 1.12f, 0.08f), palette.Trim);

            LaserReceiver receiver = receiverRoot.AddComponent<LaserReceiver>();
            AssignObject(receiver, "targetRenderer", core.GetComponent<Renderer>());
            return receiver;
        }

        private static LaserBridgeController CreateBridge(Transform parent, MaterialPalette palette, Vector3 position, Vector3 size)
        {
            GameObject bridgeRoot = new GameObject("Room04_LaserBridge");
            bridgeRoot.transform.SetParent(parent, false);
            bridgeRoot.transform.position = position;

            CreateBox("Deck", bridgeRoot.transform, Vector3.zero, size, palette.Bridge);
            CreateDecorativeBox("GlowStrip_Left", bridgeRoot.transform, new Vector3(-1.7f, 0.08f, 0f), new Vector3(0.12f, 0.05f, size.z - 0.8f), palette.GlowCyan);
            CreateDecorativeBox("GlowStrip_Right", bridgeRoot.transform, new Vector3(1.7f, 0.08f, 0f), new Vector3(0.12f, 0.05f, size.z - 0.8f), palette.GlowCyan);
            CreateBox("Rail_Left", bridgeRoot.transform, new Vector3(-1.9f, 0.52f, 0f), new Vector3(0.16f, 0.7f, size.z), palette.Trim);
            CreateBox("Rail_Right", bridgeRoot.transform, new Vector3(1.9f, 0.52f, 0f), new Vector3(0.16f, 0.7f, size.z), palette.Trim);

            LaserBridgeController bridge = bridgeRoot.AddComponent<LaserBridgeController>();
            AssignVector3(bridge, "hiddenLocalOffset", new Vector3(0f, -3.2f, 0f));
            AssignFloat(bridge, "moveSpeed", 3.4f);
            return bridge;
        }

        private static void CreateFloorStrip(Transform parent, Material material, Vector3 position, float length)
        {
            CreateDecorativeBox($"FloorStrip_{position.z:0}", parent, position, new Vector3(1.4f, 0.04f, length), material);
        }

        private static void CreateCeilingLamp(Transform parent, Material glowMaterial, Vector3 position, Color lightColor, float range)
        {
            CreateDecorativeBox($"CeilingLamp_{position.z:0}", parent, position, new Vector3(3.4f, 0.14f, 0.36f), glowMaterial);
            CreateAccentLight(parent, $"CeilingLampLight_{position.z:0}", position - new Vector3(0f, 0.7f, 0f), lightColor, range, 2.2f);
        }

        private static void CreateEmitterAccent(Transform parent, Material material, Vector3 position)
        {
            CreateDecorativeBox($"EmitterAccent_{position.z:0}_{position.x:0}", parent, position, new Vector3(0.24f, 0.24f, 0.24f), material);
        }

        private static void CreateReceiverAccent(Transform parent, Material material, Vector3 position)
        {
            CreateDecorativeBox($"ReceiverAccent_{position.z:0}_{position.x:0}", parent, position, new Vector3(0.18f, 0.18f, 0.18f), material);
        }

        private static void CreateAccentLight(Transform parent, string name, Vector3 position, Color color, float range, float intensity)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = position;

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
        }
        private static GameObject CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPosition;
            box.transform.localRotation = Quaternion.identity;
            box.transform.localScale = localScale;

            Renderer renderer = box.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            return box;
        }

        private static GameObject CreateDecorativeBox(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject box = CreateBox(name, parent, localPosition, localScale, material);
            Collider collider = box.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            return box;
        }

        private static GameObject CreateCylinder(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localPosition = localPosition;
            cylinder.transform.localRotation = Quaternion.identity;
            cylinder.transform.localScale = localScale;

            Renderer renderer = cylinder.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            return cylinder;
        }

        private static Material CreateOrUpdateLitMaterial(string assetPath, Color color, float smoothness, Color? emission = null)
        {
            return CreateOrUpdateLitMaterial(assetPath, color, smoothness, 0f, emission);
        }

        private static Material CreateOrUpdateLitMaterial(string assetPath, Color color, float smoothness, float metallic, Color? emission = null)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }

            material.shader = shader;
            SetMaterialColor(material, color);
            SetMaterialFloat(material, "_Smoothness", smoothness);
            SetMaterialFloat(material, "_Metallic", metallic);

            if (emission.HasValue && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
            }
            else if (material.HasProperty("_EmissionColor"))
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateOrUpdateUnlitMaterial(string assetPath, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }

            material.shader = shader;
            SetMaterialColor(material, color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
                return;
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private static void SetMaterialFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static void AddScenesToBuildSettings(params string[] scenePaths)
        {
           var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (var scenePath in scenePaths)
            {
                if (string.IsNullOrWhiteSpace(scenePath) || !File.Exists(scenePath))
                {
                    continue;
                }

                var alreadyExists = false;
                for (int i = 0; i < scenes.Count; i++)
                {
                    if (scenes[i].path == scenePath)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parentPath = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            var folderName = Path.GetFileName(folderPath);

            if (!string.IsNullOrEmpty(parentPath) && !AssetDatabase.IsValidFolder(parentPath))
            {
                EnsureFolder(parentPath);
            }

            AssetDatabase.CreateFolder(parentPath ?? "Assets", folderName);
        }

        private static void RemoveDefaultSceneCamera()
        {
            Camera existingCamera = Object.FindFirstObjectByType<Camera>();
            if (existingCamera != null)
            {
                Object.DestroyImmediate(existingCamera.gameObject);
            }
        }

        private static void AssignObject(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignObjectArray(Object target, string propertyName, Object[] values)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null || !property.isArray)
            {
                return;
            }

            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignString(Object target, string propertyName, string value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                return;
            }

            property.stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignVector3(Object target, string propertyName, Vector3 value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                return;
            }

            property.vector3Value = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void AssignFloat(Object target, string propertyName, float value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                return;
            }

            property.floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignInt(Object target, string propertyName, int value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                return;
            }

            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private sealed class MaterialPalette
        {
            public Material Floor;
            public Material Wall;
            public Material Trim;
            public Material Door;
            public Material Emitter;
            public Material Reflector;
            public Material ReceiverBase;
            public Material Bridge;
            public Material Pit;
            public Material GlowCyan;
            public Material GlowAmber;
            public Material GlowGreen;
            public Material Laser;
        }

        private sealed class PlayerBundle
        {
            public GameObject Root;
            public Camera Camera;
            public PlayerInteraction Interaction;
            public PlayerObjectiveTracker ObjectiveTracker;
            public PlayerGameFlow GameFlow;
            public PlayerRespawnController RespawnController;
        }
    }
}
