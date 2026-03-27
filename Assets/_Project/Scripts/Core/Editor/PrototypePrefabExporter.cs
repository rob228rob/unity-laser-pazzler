using System.IO;
using UnityEditor;
using UnityEngine;

namespace Project.Core.Editor
{
    public static class PrototypePrefabExporter
    {
        private const string PrefabsRoot = "Assets/_Project/Prefabs";

        [MenuItem("Tools/Laser Puzzle/Export Generated Scene Prefabs")]
        public static void ExportGeneratedScenePrefabs()
        {
            EnsureFolder(PrefabsRoot);
            EnsureFolder(PrefabsRoot + "/Rooms");
            EnsureFolder(PrefabsRoot + "/Systems");

            ExportIfFound("Room01", PrefabsRoot + "/Rooms/Room01.prefab");
            ExportIfFound("Room02", PrefabsRoot + "/Rooms/Room02.prefab");
            ExportIfFound("Room03", PrefabsRoot + "/Rooms/Room03.prefab");
            ExportIfFound("Room04", PrefabsRoot + "/Rooms/Room04.prefab");
            ExportIfFound("World", PrefabsRoot + "/Rooms/World.prefab");
            ExportIfFound("Player", PrefabsRoot + "/Systems/Player.prefab");
            ExportIfFound("HUD", PrefabsRoot + "/Systems/HUD.prefab");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated scene prefabs exported to Assets/_Project/Prefabs");
        }

        private static void ExportIfFound(string objectName, string prefabPath)
        {
            GameObject target = GameObject.Find(objectName);

            if (target == null)
            {
                Debug.LogWarning($"Could not find scene object '{objectName}' for prefab export.");
                return;
            }

            PrefabUtility.SaveAsPrefabAsset(target, prefabPath);
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
    }
}
