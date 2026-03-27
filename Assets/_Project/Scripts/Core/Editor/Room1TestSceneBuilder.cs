using System.Collections.Generic;
using System.IO;
using Project.Environment;
using Project.Laser;
using Project.Room1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Project.Core.Editor
{
    public static class Room1TestSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Room1_Test.unity";
        private const string MaterialsFolder = "Assets/_Project/Materials";

        private const float RoomWidth = 24f;
        private const float RoomHeight = 6f;
        private const float WorldStartZ = -14f;
        private const float Door01Z = 14f;
        private const float Door02Z = 42f;
        private const float Door03Z = 70f;
        private const float Door04Z = 98f;
        private const float WorldEndZ = 114f;

        [MenuItem("Tools/Laser Puzzle/Create Room1 Test Scene")]
        public static void CreateRoom1TestScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureFolder("Assets/_Project");
            EnsureFolder("Assets/_Project/Scenes");
            EnsureFolder(MaterialsFolder);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Scene scene = SceneManager.GetActiveScene();

            SetupRenderSettings();
            MaterialPalette palette = CreateMaterialPalette();

            GameObject root = new GameObject("Room1_Test");
            GameObject world = new GameObject("World");
            world.transform.SetParent(root.transform, false);

            CreateDirectionalLight(root.transform);
            CreateWorldShell(world.transform, palette);

            DoorController door01 = CreateDoorAssembly(world.transform, palette, "Door01", Door01Z, new Color(1f, 0.74f, 0.40f));
            DoorController door02 = CreateDoorAssembly(world.transform, palette, "Door02", Door02Z, new Color(0.36f, 0.88f, 0.97f));
            DoorController door03 = CreateDoorAssembly(world.transform, palette, "Door03", Door03Z, new Color(0.54f, 0.93f, 0.74f));
            DoorController door04 = CreateDoorAssembly(world.transform, palette, "Door04", Door04Z, new Color(1f, 0.82f, 0.46f));

            CreateRoom01(world.transform, palette, door01);
            CreateRoom02(world.transform, palette, door02);
            CreateRoom03(world.transform, palette, door03);
            CreateRoom04(world.transform, palette, door04);
            CreateFinishArea(world.transform, palette);

            Transform startPoint = CreatePlayerStart(world.transform);
            CreatePlayer(root.transform, startPoint);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = root;

            Debug.Log($"Room1 multi-room scene created at {ScenePath}");
        }

        private static void SetupRenderSettings()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.05f, 0.07f, 0.09f);
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 135f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.17f, 0.20f, 0.23f);
        }

        private static MaterialPalette CreateMaterialPalette()
        {
            return new MaterialPalette
            {
                Floor = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_Room1_Floor.mat", new Color(0.16f, 0.19f, 0.22f), 0.44f, 0.10f),
                FloorInset = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_Room1_FloorInset.mat", new Color(0.08f, 0.11f, 0.13f), 0.70f, 0.26f),
                Wall = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_Room1_Wall.mat", new Color(0.28f, 0.31f, 0.34f), 0.18f, 0.02f),
                Ceiling = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_Room1_Ceiling.mat", new Color(0.14f, 0.17f, 0.20f), 0.20f, 0.02f),
                Trim = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_Room1_Trim.mat", new Color(0.67f, 0.56f, 0.36f), 0.78f, 0.22f),
                Door = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_Room1_Door.mat", new Color(0.15f, 0.24f, 0.30f), 0.58f, 0.18f, new Color(0.08f, 0.24f, 0.30f) * 0.35f),
                Emitter = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_Room1_Emitter.mat", new Color(0.40f, 0.14f, 0.10f), 0.48f, 0.20f, new Color(0.95f, 0.22f, 0.14f) * 0.65f),
                Reflector = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_Room1_Reflector.mat", new Color(0.88f, 0.85f, 0.78f), 0.92f, 0.10f),
                Receiver = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_Room1_Receiver.mat", new Color(0.18f, 0.22f, 0.26f), 0.42f, 0.12f),
                Bridge = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_Room1_Bridge.mat", new Color(0.14f, 0.21f, 0.26f), 0.48f, 0.12f),
                Pit = CreateOrUpdateLitMaterial($"{MaterialsFolder}/M_Room1_Pit.mat", new Color(0.03f, 0.04f, 0.05f), 0.0f, 0.0f),
                GlowCyan = CreateOrUpdateUnlitMaterial($"{MaterialsFolder}/M_Room1_GlowCyan.mat", new Color(0.35f, 0.90f, 0.97f)),
                GlowAmber = CreateOrUpdateUnlitMaterial($"{MaterialsFolder}/M_Room1_GlowAmber.mat", new Color(1.0f, 0.73f, 0.40f)),
                GlowGreen = CreateOrUpdateUnlitMaterial($"{MaterialsFolder}/M_Room1_GlowGreen.mat", new Color(0.54f, 0.93f, 0.74f)),
                Laser = CreateOrUpdateUnlitMaterial($"{MaterialsFolder}/M_Room1_Laser.mat", new Color(1.0f, 0.18f, 0.14f)),
                LaserCyan = CreateOrUpdateUnlitMaterial($"{MaterialsFolder}/M_Room1_LaserCyan.mat", new Color(0.35f, 0.90f, 0.97f)),
                LaserGreen = CreateOrUpdateUnlitMaterial($"{MaterialsFolder}/M_Room1_LaserGreen.mat", new Color(0.54f, 0.93f, 0.74f))
            };
        }

        private static void CreateDirectionalLight(Transform parent)
        {
            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1.0f, 0.96f, 0.91f);
            light.shadows = LightShadows.Soft;
        }

        private static void CreateWorldShell(Transform parent, MaterialPalette palette)
        {
            float worldLength = WorldEndZ - WorldStartZ;
            float centerZ = (WorldStartZ + WorldEndZ) * 0.5f;
            float wallX = RoomWidth * 0.5f + 0.5f;

            CreateBox("Ceiling", parent, new Vector3(0f, 5.5f, centerZ), new Vector3(RoomWidth, 1f, worldLength), palette.Ceiling);
            CreateBox("Wall_Left", parent, new Vector3(-wallX, 2.5f, centerZ), new Vector3(1f, 5f, worldLength), palette.Wall);
            CreateBox("Wall_Right", parent, new Vector3(wallX, 2.5f, centerZ), new Vector3(1f, 5f, worldLength), palette.Wall);
            CreateBox("Wall_Start", parent, new Vector3(0f, 2.5f, WorldStartZ), new Vector3(RoomWidth, 5f, 1f), palette.Wall);
            CreateBox("Wall_End", parent, new Vector3(0f, 2.5f, WorldEndZ), new Vector3(RoomWidth, 5f, 1f), palette.Wall);

            CreateBox("Floor_Room01", parent, new Vector3(0f, -0.5f, 0f), new Vector3(RoomWidth, 1f, 24f), palette.Floor);
            CreateBox("Floor_Connector_01", parent, new Vector3(0f, -0.5f, Door01Z), new Vector3(RoomWidth, 1f, 4f), palette.Floor);
            CreateBox("Floor_Room02", parent, new Vector3(0f, -0.5f, 28f), new Vector3(RoomWidth, 1f, 24f), palette.Floor);
            CreateBox("Floor_Connector_02", parent, new Vector3(0f, -0.5f, Door02Z), new Vector3(RoomWidth, 1f, 4f), palette.Floor);
            CreateBox("Floor_Room03", parent, new Vector3(0f, -0.5f, 56f), new Vector3(RoomWidth, 1f, 24f), palette.Floor);
            CreateBox("Floor_Connector_03", parent, new Vector3(0f, -0.5f, Door03Z), new Vector3(RoomWidth, 1f, 4f), palette.Floor);
            CreateBox("Floor_Room04_Back", parent, new Vector3(0f, -0.5f, 76f), new Vector3(RoomWidth, 1f, 8f), palette.Floor);
            CreateBox("Floor_Connector_04", parent, new Vector3(0f, -0.5f, Door04Z), new Vector3(RoomWidth, 1f, 4f), palette.Floor);
            CreateBox("Floor_Room04_Front", parent, new Vector3(0f, -0.5f, 92f), new Vector3(RoomWidth, 1f, 8f), palette.Floor);
            CreateBox("Floor_Finish", parent, new Vector3(0f, -0.5f, 106f), new Vector3(RoomWidth, 1f, 14f), palette.Floor);

            CreateDecorativeBox("Inset_01", parent, new Vector3(0f, -0.46f, 0f), new Vector3(9f, 0.05f, 18f), palette.FloorInset);
            CreateDecorativeBox("Inset_Connector_01", parent, new Vector3(0f, -0.46f, Door01Z), new Vector3(8f, 0.05f, 2.8f), palette.FloorInset);
            CreateDecorativeBox("Inset_02", parent, new Vector3(0f, -0.46f, 28f), new Vector3(11f, 0.05f, 18f), palette.FloorInset);
            CreateDecorativeBox("Inset_Connector_02", parent, new Vector3(0f, -0.46f, Door02Z), new Vector3(8f, 0.05f, 2.8f), palette.FloorInset);
            CreateDecorativeBox("Inset_03", parent, new Vector3(0f, -0.46f, 56f), new Vector3(13f, 0.05f, 18f), palette.FloorInset);
            CreateDecorativeBox("Inset_Connector_03", parent, new Vector3(0f, -0.46f, Door03Z), new Vector3(8f, 0.05f, 2.8f), palette.FloorInset);
            CreateDecorativeBox("Inset_04", parent, new Vector3(0f, -0.46f, 84f), new Vector3(10f, 0.05f, 10f), palette.FloorInset);
            CreateDecorativeBox("Inset_Connector_04", parent, new Vector3(0f, -0.46f, Door04Z), new Vector3(8f, 0.05f, 2.8f), palette.FloorInset);
            CreateDecorativeBox("Inset_Finish", parent, new Vector3(0f, -0.46f, 106f), new Vector3(10f, 0.05f, 8f), palette.FloorInset);

            CreateDecorativeBox("PitVoid", parent, new Vector3(0f, -3.2f, 84f), new Vector3(7.2f, 5f, 8f), palette.Pit);

            CreateRoomPanels(parent, palette, 0f, palette.GlowAmber);
            CreateRoomPanels(parent, palette, 28f, palette.GlowCyan);
            CreateRoomPanels(parent, palette, 56f, palette.GlowGreen);
            CreateRoomPanels(parent, palette, 84f, palette.GlowAmber);

            CreateCeilingLamp(parent, palette, new Vector3(0f, 4.8f, -8f), new Color(1f, 0.74f, 0.42f), 10f, 2.2f, palette.GlowAmber);
            CreateCeilingLamp(parent, palette, new Vector3(0f, 4.8f, 8f), new Color(1f, 0.74f, 0.42f), 10f, 2.2f, palette.GlowAmber);
            CreateCeilingLamp(parent, palette, new Vector3(0f, 4.8f, 20f), new Color(0.35f, 0.90f, 0.97f), 10f, 2.2f, palette.GlowCyan);
            CreateCeilingLamp(parent, palette, new Vector3(0f, 4.8f, 36f), new Color(0.35f, 0.90f, 0.97f), 10f, 2.2f, palette.GlowCyan);
            CreateCeilingLamp(parent, palette, new Vector3(0f, 4.8f, 48f), new Color(0.54f, 0.93f, 0.74f), 10f, 2.2f, palette.GlowGreen);
            CreateCeilingLamp(parent, palette, new Vector3(0f, 4.8f, 64f), new Color(0.54f, 0.93f, 0.74f), 10f, 2.2f, palette.GlowGreen);
            CreateCeilingLamp(parent, palette, new Vector3(0f, 4.8f, 76f), new Color(1f, 0.82f, 0.46f), 9f, 2f, palette.GlowAmber);
            CreateCeilingLamp(parent, palette, new Vector3(0f, 4.8f, 92f), new Color(1f, 0.82f, 0.46f), 9f, 2f, palette.GlowAmber);
            CreateCeilingLamp(parent, palette, new Vector3(0f, 4.8f, 106f), new Color(0.56f, 0.98f, 0.82f), 8f, 2f, palette.GlowGreen);
        }

        private static DoorController CreateDoorAssembly(Transform parent, MaterialPalette palette, string name, float zPosition, Color glowColor)
        {
            GameObject assembly = new GameObject(name);
            assembly.transform.SetParent(parent, false);

            CreateBox("Frame_Left", assembly.transform, new Vector3(-7f, 2.5f, zPosition), new Vector3(10f, 5f, 1f), palette.Wall);
            CreateBox("Frame_Right", assembly.transform, new Vector3(7f, 2.5f, zPosition), new Vector3(10f, 5f, 1f), palette.Wall);
            CreateBox("Frame_Top", assembly.transform, new Vector3(0f, 5.25f, zPosition), new Vector3(4f, 1.5f, 1f), palette.Wall);
            CreateDecorativeBox("Trim_Left", assembly.transform, new Vector3(-2.15f, 2.3f, zPosition - 0.04f), new Vector3(0.18f, 4.4f, 0.1f), palette.Trim);
            CreateDecorativeBox("Trim_Right", assembly.transform, new Vector3(2.15f, 2.3f, zPosition - 0.04f), new Vector3(0.18f, 4.4f, 0.1f), palette.Trim);
            GameObject door = CreateBox("Door", assembly.transform, new Vector3(0f, 1.5f, zPosition), new Vector3(4f, 3f, 1f), palette.Door);
            DoorController controller = door.AddComponent<DoorController>();
            AssignVector3(controller, "openLocalOffset", new Vector3(0f, 3f, 0f));
            AssignFloat(controller, "moveSpeed", 2.4f);
            CreateAccentLight(assembly.transform, "DoorLight", new Vector3(0f, 4.1f, zPosition - 1.6f), glowColor, 5.5f, 2.5f);
            return controller;
        }

        private static void CreateRoom01(Transform parent, MaterialPalette palette, DoorController door)
        {
            LaserEmitter emitter = CreateEmitter(parent, palette, "Emitter_01", new Vector3(-8.5f, 1.1f, -6f), Quaternion.Euler(0f, 90f, 0f));
            CreateReflector(parent, palette, "Reflector_01", new Vector3(0f, 1.1f, -6f), Quaternion.Euler(0f, 90f, 0f));
            LaserReceiver receiver = CreateReceiver(parent, palette, "Receiver_01", new Vector3(0f, 1.05f, 8f));
            CreateAccentBeacon(parent, palette.GlowAmber, emitter.transform.position + Vector3.up * 1.2f);
            CreateAccentBeacon(parent, palette.GlowAmber, receiver.transform.position + Vector3.up * 1.1f);
            AssignObjectArray(door, "requiredReceivers", new Object[] { receiver });
        }

        private static void CreateRoom02(Transform parent, MaterialPalette palette, DoorController door)
        {
            LaserEmitter emitter = CreateEmitter(parent, palette, "Emitter_02", new Vector3(-8.5f, 1.1f, 20f), Quaternion.Euler(0f, 90f, 0f));
            CreateReflector(parent, palette, "Reflector_02A", new Vector3(-2.5f, 1.1f, 20f), Quaternion.Euler(0f, 90f, 0f));
            LaserRelayEmitter cyanRelay = CreateRelayEmitter(parent, palette, "Relay_02_Filtered", new Vector3(-2.5f, 1.1f, 28f), Quaternion.Euler(0f, 90f, 0f), palette.LaserCyan);
            CreateColorFilter(parent, palette, "ColorFilter_02", new Vector3(-2.5f, 1.1f, 28f), cyanRelay, palette.GlowCyan);
            CreateReflector(parent, palette, "Reflector_02B", new Vector3(5.5f, 1.1f, 28f), Quaternion.Euler(0f, 90f, 0f));
            LaserReceiver receiver = CreateReceiver(parent, palette, "Receiver_02", new Vector3(5.5f, 1.05f, 36f));
            CreateAccentBeacon(parent, palette.GlowCyan, emitter.transform.position + Vector3.up * 1.2f);
            CreateAccentBeacon(parent, palette.GlowCyan, receiver.transform.position + Vector3.up * 1.1f);
            AssignObjectArray(door, "requiredReceivers", new Object[] { receiver });
        }

        private static void CreateRoom03(Transform parent, MaterialPalette palette, DoorController door)
        {
            LaserEmitter sourceEmitter = CreateEmitter(parent, palette, "Emitter_03_Source", new Vector3(0f, 1.1f, 48f), Quaternion.Euler(0f, 0f, 0f));
            LaserBeamSplitter splitter = CreateBeamSplitter(parent, palette, "Splitter_03", new Vector3(0f, 1.1f, 56f));
            LaserRelayEmitter leftRelay = CreateRelayEmitter(parent, palette, "Relay_03L", new Vector3(-4f, 1.1f, 56f), Quaternion.Euler(0f, 0f, 0f), palette.LaserGreen);
            LaserRelayEmitter rightRelay = CreateRelayEmitter(parent, palette, "Relay_03R", new Vector3(4f, 1.1f, 56f), Quaternion.Euler(0f, 0f, 0f), palette.Laser);
            CreateReflector(parent, palette, "Reflector_03L_A", new Vector3(-4f, 1.1f, 62f), Quaternion.Euler(0f, 0f, 0f));
            CreateReflector(parent, palette, "Reflector_03L_B", new Vector3(-9f, 1.1f, 62f), Quaternion.Euler(0f, 45f, 0f));
            CreateReflector(parent, palette, "Reflector_03R_A", new Vector3(4f, 1.1f, 62f), Quaternion.Euler(0f, 90f, 0f));
            CreateReflector(parent, palette, "Reflector_03R_B", new Vector3(9f, 1.1f, 62f), Quaternion.Euler(0f, -45f, 0f));
            LaserReceiver leftReceiver = CreateTimedReceiver(parent, palette, "Receiver_03L_Timed", new Vector3(-9f, 1.05f, 54f), 2.75f, palette.GlowGreen);
            LaserReceiver rightReceiver = CreateReceiver(parent, palette, "Receiver_03R", new Vector3(9f, 1.05f, 54f));

            AssignObjectArray(splitter, "outputEmitters", new Object[] { leftRelay, rightRelay });
            CreateAccentBeacon(parent, palette.GlowGreen, sourceEmitter.transform.position + Vector3.up * 1.2f);
            AssignObjectArray(door, "requiredReceivers", new Object[] { leftReceiver, rightReceiver });
        }

        private static void CreateRoom04(Transform parent, MaterialPalette palette, DoorController door)
        {
            LaserEmitter bridgeEmitter = CreateEmitter(parent, palette, "Emitter_04A", new Vector3(-8.5f, 1.1f, 76f), Quaternion.Euler(0f, 90f, 0f));
            CreateReflector(parent, palette, "Reflector_04A", new Vector3(-2.5f, 1.1f, 76f), Quaternion.Euler(0f, 90f, 0f));
            LaserReceiver bridgeReceiver = CreateTimedReceiver(parent, palette, "Receiver_04A_Timed", new Vector3(-2.5f, 1.05f, 84f), 3.5f, palette.GlowAmber);

            LaserBridgeController bridge = CreateBridge(parent, palette, new Vector3(0f, 0.2f, 84f), new Vector3(4f, 0.4f, 12f));
            AssignObjectArray(bridge, "requiredReceivers", new Object[] { bridgeReceiver });

            LaserEmitter exitEmitter = CreateEmitter(parent, palette, "Emitter_04B", new Vector3(0f, 1.1f, 92f), Quaternion.Euler(0f, 0f, 0f));
            LaserBeamSplitter exitSplitter = CreateBeamSplitter(parent, palette, "Splitter_04", new Vector3(0f, 1.1f, 98f));
            LaserRelayEmitter leftRelay = CreateRelayEmitter(parent, palette, "Relay_04L", new Vector3(-4f, 1.1f, 98f), Quaternion.Euler(0f, 0f, 0f), palette.LaserCyan);
            LaserRelayEmitter rightRelay = CreateRelayEmitter(parent, palette, "Relay_04R", new Vector3(4f, 1.1f, 98f), Quaternion.Euler(0f, 0f, 0f), palette.LaserGreen);
            CreateReflector(parent, palette, "Reflector_04B_L", new Vector3(-4f, 1.1f, 104f), Quaternion.Euler(0f, 0f, 0f));
            CreateReflector(parent, palette, "Reflector_04B_R", new Vector3(4f, 1.1f, 104f), Quaternion.Euler(0f, 90f, 0f));
            LaserReceiver leftExitReceiver = CreateReceiver(parent, palette, "Receiver_04B_L", new Vector3(-8f, 1.05f, 104f));
            LaserReceiver rightExitReceiver = CreateReceiver(parent, palette, "Receiver_04B_R", new Vector3(8f, 1.05f, 104f));

            AssignObjectArray(exitSplitter, "outputEmitters", new Object[] { leftRelay, rightRelay });
            AssignObjectArray(door, "requiredReceivers", new Object[] { bridgeReceiver, leftExitReceiver, rightExitReceiver });
            CreateAccentBeacon(parent, palette.GlowAmber, bridgeEmitter.transform.position + Vector3.up * 1.2f);
            CreateAccentBeacon(parent, palette.GlowAmber, exitEmitter.transform.position + Vector3.up * 1.2f);
        }

        private static void CreateFinishArea(Transform parent, MaterialPalette palette)
        {
            CreateDecorativeBox("FinishPad", parent, new Vector3(0f, -0.42f, 106f), new Vector3(8f, 0.08f, 8f), palette.GlowGreen);
            CreateDecorativeBox("FinishMonolith", parent, new Vector3(0f, 2f, 110f), new Vector3(3.2f, 4f, 0.8f), palette.Trim);
            CreateDecorativeBox("FinishCore", parent, new Vector3(0f, 2f, 109.45f), new Vector3(1.4f, 2.6f, 0.08f), palette.GlowGreen);
            CreateAccentLight(parent, "FinishLight_Main", new Vector3(0f, 3.4f, 106f), new Color(0.56f, 0.98f, 0.82f), 10f, 3.2f);
            CreateAccentLight(parent, "FinishLight_Left", new Vector3(-4f, 2.4f, 106f), new Color(0.35f, 0.90f, 0.97f), 7f, 1.6f);
            CreateAccentLight(parent, "FinishLight_Right", new Vector3(4f, 2.4f, 106f), new Color(1f, 0.73f, 0.40f), 7f, 1.6f);

            GameObject finishVolume = new GameObject("VictoryVolume");
            finishVolume.transform.SetParent(parent, false);
            finishVolume.transform.localPosition = new Vector3(0f, 1.5f, 106f);

            BoxCollider boxCollider = finishVolume.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(8f, 3f, 8f);

            Room1TestVictoryVolume victoryVolume = finishVolume.AddComponent<Room1TestVictoryVolume>();
            AssignString(victoryVolume, "victoryMessage", "Bridge crossed. Prototype wing secured.");
        }

        private static Transform CreatePlayerStart(Transform parent)
        {
            GameObject start = new GameObject("PlayerStart");
            start.transform.SetParent(parent, false);
            start.transform.localPosition = new Vector3(0f, 1.1f, -10f);
            return start.transform;
        }

        private static void CreatePlayer(Transform parent, Transform startPoint)
        {
            GameObject player = new GameObject("Player");
            player.transform.SetParent(parent, false);
            player.transform.position = startPoint.position;

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.3f;
            controller.slopeLimit = 45f;

            player.AddComponent<Room1TestPlayerController>();
            Room1TestInteraction interaction = player.AddComponent<Room1TestInteraction>();
            Room1TestFallReset fallReset = player.AddComponent<Room1TestFallReset>();
            AssignObject(fallReset, "respawnPoint", startPoint);

            GameObject pivot = new GameObject("CameraPivot");
            pivot.transform.SetParent(player.transform, false);
            pivot.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            Room1TestLook look = pivot.AddComponent<Room1TestLook>();
            AssignObject(look, "playerBody", player.transform);

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(pivot.transform, false);
            cameraObject.transform.localPosition = Vector3.zero;
            cameraObject.transform.localRotation = Quaternion.identity;
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.08f, 0.10f, 1f);
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 160f;
            camera.fieldOfView = 75f;

            cameraObject.AddComponent<AudioListener>();
            AssignObject(interaction, "playerCamera", camera);
        }

        private static LaserEmitter CreateEmitter(Transform parent, MaterialPalette palette, string name, Vector3 position, Quaternion rotation)
        {
            GameObject emitterObject = new GameObject(name);
            emitterObject.transform.SetParent(parent, false);
            emitterObject.transform.localPosition = position;
            emitterObject.transform.localRotation = rotation;

            LineRenderer lineRenderer = emitterObject.AddComponent<LineRenderer>();
            lineRenderer.material = palette.Laser;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.08f;
            lineRenderer.numCapVertices = 2;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.textureMode = LineTextureMode.Stretch;

            LaserEmitter emitter = emitterObject.AddComponent<LaserEmitter>();

            GameObject origin = new GameObject("LaserOrigin");
            origin.transform.SetParent(emitterObject.transform, false);
            origin.transform.localPosition = new Vector3(0f, 0f, 0.65f);

            GameObject body = CreateCylinder("EmitterBody", emitterObject.transform, Vector3.zero, new Vector3(0.55f, 0.32f, 0.55f), palette.Emitter);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreateCylinder("EmitterBase", emitterObject.transform, new Vector3(0f, -0.40f, 0f), new Vector3(0.75f, 0.12f, 0.75f), palette.Trim);
            CreateDecorativeBox("EmitterCore", emitterObject.transform, new Vector3(0f, 0f, 0.62f), new Vector3(0.22f, 0.22f, 0.06f), palette.GlowAmber);

            AssignObject(emitter, "originPoint", origin.transform);
            AssignObject(emitter, "lineRenderer", lineRenderer);
            AssignFloat(emitter, "maxDistance", 30f);
            AssignInt(emitter, "maxBounces", 5);
            return emitter;
        }

        private static LaserReflector CreateReflector(Transform parent, MaterialPalette palette, string name, Vector3 position, Quaternion rotation)
        {
            GameObject reflector = CreateBox(name, parent, position, new Vector3(2f, 2f, 0.2f), palette.Reflector);
            reflector.transform.localRotation = rotation;
            reflector.AddComponent<LaserReflector>();
            CreateCylinder("ReflectorBase", parent, position + new Vector3(0f, -0.55f, 0f), new Vector3(0.8f, 0.12f, 0.8f), palette.Trim);
            CreateDecorativeBox("ReflectorCore", parent, position + new Vector3(0f, 0f, -0.14f), new Vector3(0.34f, 0.34f, 0.04f), palette.GlowAmber);
            return reflector.GetComponent<LaserReflector>();
        }

        private static LaserReceiver CreateReceiver(Transform parent, MaterialPalette palette, string name, Vector3 position)
        {
            GameObject receiver = CreateBox(name, parent, position, Vector3.one, palette.Receiver);
            receiver.AddComponent<LaserReceiver>();
            CreateCylinder("ReceiverBase", parent, position + new Vector3(0f, -0.42f, 0f), new Vector3(0.7f, 0.10f, 0.7f), palette.Trim);
            return receiver.GetComponent<LaserReceiver>();
        }

        private static LaserRelayEmitter CreateRelayEmitter(Transform parent, MaterialPalette palette, string name, Vector3 position, Quaternion rotation, Material laserMaterial)
        {
            GameObject relayObject = new GameObject(name);
            relayObject.transform.SetParent(parent, false);
            relayObject.transform.localPosition = position;
            relayObject.transform.localRotation = rotation;

            LineRenderer lineRenderer = relayObject.AddComponent<LineRenderer>();
            lineRenderer.material = laserMaterial;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = 0.07f;
            lineRenderer.endWidth = 0.07f;
            lineRenderer.numCapVertices = 2;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.textureMode = LineTextureMode.Stretch;

            LaserRelayEmitter relayEmitter = relayObject.AddComponent<LaserRelayEmitter>();

            GameObject origin = new GameObject("LaserOrigin");
            origin.transform.SetParent(relayObject.transform, false);
            origin.transform.localPosition = new Vector3(0f, 0f, 0.58f);

            GameObject body = CreateCylinder("RelayBody", relayObject.transform, Vector3.zero, new Vector3(0.42f, 0.26f, 0.42f), palette.Trim);
            body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            CreateDecorativeBox("RelayCore", relayObject.transform, new Vector3(0f, 0f, 0.54f), new Vector3(0.18f, 0.18f, 0.05f), palette.GlowGreen);

            AssignObject(relayEmitter, "originPoint", origin.transform);
            AssignObject(relayEmitter, "lineRenderer", lineRenderer);
            return relayEmitter;
        }

        private static LaserBeamSplitter CreateBeamSplitter(Transform parent, MaterialPalette palette, string name, Vector3 position)
        {
            GameObject splitterObject = CreateBox(name, parent, position, new Vector3(1.4f, 1.4f, 1.4f), palette.Receiver);
            CreateDecorativeBox($"{name}_Core", splitterObject.transform, new Vector3(0f, 0f, 0.72f), new Vector3(0.36f, 0.36f, 0.05f), palette.GlowCyan);
            LaserBeamSplitter splitter = splitterObject.AddComponent<LaserBeamSplitter>();
            AssignObject(splitter, "targetRenderer", splitterObject.GetComponent<Renderer>());
            return splitter;
        }

        private static LaserColorFilter CreateColorFilter(Transform parent, MaterialPalette palette, string name, Vector3 position, LaserRelayEmitter outputEmitter, Material glowMaterial)
        {
            GameObject filterObject = CreateBox(name, parent, position, new Vector3(1.2f, 1.2f, 1.2f), palette.Receiver);
            CreateDecorativeBox($"{name}_Core", filterObject.transform, new Vector3(0f, 0f, 0.62f), new Vector3(0.48f, 0.48f, 0.05f), glowMaterial);
            LaserColorFilter filter = filterObject.AddComponent<LaserColorFilter>();
            AssignObject(filter, "outputEmitter", outputEmitter);
            AssignObject(filter, "targetRenderer", filterObject.GetComponent<Renderer>());
            return filter;
        }

        private static LaserReceiver CreateTimedReceiver(Transform parent, MaterialPalette palette, string name, Vector3 position, float holdDuration, Material glowMaterial)
        {
            LaserReceiver receiver = CreateReceiver(parent, palette, name, position);
            AssignFloat(receiver, "activeHoldDuration", holdDuration);
            CreateDecorativeBox($"{name}_TimerGlow", parent, position + new Vector3(0f, 0.52f, 0f), new Vector3(0.30f, 0.30f, 0.30f), glowMaterial);
            return receiver;
        }

        private static LaserBridgeController CreateBridge(Transform parent, MaterialPalette palette, Vector3 position, Vector3 size)
        {
            GameObject bridgeRoot = new GameObject("LaserBridge");
            bridgeRoot.transform.SetParent(parent, false);
            bridgeRoot.transform.localPosition = position;

            CreateBox("Deck", bridgeRoot.transform, Vector3.zero, size, palette.Bridge);
            CreateDecorativeBox("DeckPlate_Left", bridgeRoot.transform, new Vector3(-1.2f, 0.08f, 0f), new Vector3(0.55f, 0.05f, size.z - 1.2f), palette.FloorInset);
            CreateDecorativeBox("DeckPlate_Right", bridgeRoot.transform, new Vector3(1.2f, 0.08f, 0f), new Vector3(0.55f, 0.05f, size.z - 1.2f), palette.FloorInset);
            CreateBox("Rail_Left", bridgeRoot.transform, new Vector3(-1.9f, 0.52f, 0f), new Vector3(0.16f, 0.7f, size.z), palette.Trim);
            CreateBox("Rail_Right", bridgeRoot.transform, new Vector3(1.9f, 0.52f, 0f), new Vector3(0.16f, 0.7f, size.z), palette.Trim);
            CreateAccentBeacon(bridgeRoot.transform, palette.GlowAmber, new Vector3(0f, 0.28f, -size.z * 0.5f + 0.8f));
            CreateAccentBeacon(bridgeRoot.transform, palette.GlowAmber, new Vector3(0f, 0.28f, size.z * 0.5f - 0.8f));

            LaserBridgeController bridge = bridgeRoot.AddComponent<LaserBridgeController>();
            AssignVector3(bridge, "hiddenLocalOffset", new Vector3(0f, -3.2f, 0f));
            AssignFloat(bridge, "moveSpeed", 3.4f);
            return bridge;
        }

        private static void CreateAccentBeacon(Transform parent, Material material, Vector3 position)
        {
            CreateDecorativeBox($"Beacon_{position.x:0}_{position.z:0}", parent, position, new Vector3(0.18f, 0.18f, 0.18f), material);
        }

        private static void CreateRoomPanels(Transform parent, MaterialPalette palette, float centerZ, Material glowMaterial)
        {
            CreateDecorativeBox($"Panel_Left_{centerZ:0}", parent, new Vector3(-10.8f, 2.2f, centerZ), new Vector3(0.18f, 3.5f, 16f), palette.Trim);
            CreateDecorativeBox($"Panel_Right_{centerZ:0}", parent, new Vector3(10.8f, 2.2f, centerZ), new Vector3(0.18f, 3.5f, 16f), palette.Trim);
            CreateDecorativeBox($"GlowPanel_Left_{centerZ:0}", parent, new Vector3(-10.55f, 2f, centerZ), new Vector3(0.45f, 2.8f, 12f), glowMaterial);
            CreateDecorativeBox($"GlowPanel_Right_{centerZ:0}", parent, new Vector3(10.55f, 2f, centerZ), new Vector3(0.45f, 2.8f, 12f), glowMaterial);
        }

        private static void CreateCeilingLamp(Transform parent, MaterialPalette palette, Vector3 position, Color lightColor, float range, float intensity, Material glowMaterial)
        {
            CreateDecorativeBox($"Lamp_{position.z:0}", parent, position, new Vector3(3.0f, 0.12f, 0.32f), glowMaterial);
            CreateAccentLight(parent, $"LampLight_{position.z:0}", position + Vector3.down * 0.65f, lightColor, range, intensity);
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

            if (material.HasProperty("_EmissionColor"))
            {
                if (emission.HasValue)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", emission.Value);
                }
                else
                {
                    material.DisableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", Color.black);
                }
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
            }
            else if (material.HasProperty("_Color"))
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

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.path == scenePath)
                {
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parentPath = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(folderPath);

            if (!string.IsNullOrEmpty(parentPath) && !AssetDatabase.IsValidFolder(parentPath))
            {
                EnsureFolder(parentPath);
            }

            AssetDatabase.CreateFolder(parentPath ?? "Assets", folderName);
        }

        private static void AssignObject(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);

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

        private sealed class MaterialPalette
        {
            public Material Floor;
            public Material FloorInset;
            public Material Wall;
            public Material Ceiling;
            public Material Trim;
            public Material Door;
            public Material Emitter;
            public Material Reflector;
            public Material Receiver;
            public Material Bridge;
            public Material Pit;
            public Material GlowCyan;
            public Material GlowAmber;
            public Material GlowGreen;
            public Material Laser;
            public Material LaserCyan;
            public Material LaserGreen;
        }
    }
}
