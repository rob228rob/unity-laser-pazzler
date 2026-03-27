using System.IO;
using Project.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Core.Editor
{
    public static class MainMenuSceneBuilder
    {
        private const string MenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string GameplayScenePath = "Assets/_Project/Scenes/LaserPuzzle_Prototype.unity";

        [MenuItem("Tools/Laser Puzzle/Create Main Menu Scene")]
        public static void CreateMainMenuScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EnsureFolder("Assets/_Project");
            EnsureFolder("Assets/_Project/Scenes");

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject menuRoot = new GameObject("MainMenu");
            MainMenuController controller = menuRoot.AddComponent<MainMenuController>();
            AssignString(controller, "gameplaySceneName", Path.GetFileNameWithoutExtension(GameplayScenePath));

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), MenuScenePath);
            AddScenesToBuildSettings(MenuScenePath, GameplayScenePath);
            Selection.activeGameObject = menuRoot;

            Debug.Log($"Main menu scene created at {MenuScenePath}");
        }

        private static void AddScenesToBuildSettings(params string[] scenePaths)
        {
            var existing = EditorBuildSettings.scenes;
            var newScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(existing);

            foreach (string scenePath in scenePaths)
            {
                if (string.IsNullOrWhiteSpace(scenePath) || !File.Exists(scenePath))
                {
                    continue;
                }

                bool alreadyExists = false;
                foreach (var scene in newScenes)
                {
                    if (scene.path == scenePath)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    newScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }
            }

            EditorBuildSettings.scenes = newScenes.ToArray();
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
    }
}
