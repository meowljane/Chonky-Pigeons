using UnityEngine;
using UnityEditor;
using PigeonGame.Data;
using System.Reflection;

namespace PigeonGame.Editor
{
    public class GameDataUpdater
    {
        [MenuItem("Tools/Game Data/Update All ScriptableObjects")]
        public static void UpdateAllScriptableObjects()
        {
            UpdateTrapTypeSet();
            UpdateUpgradeDefinitionSet();
            UpdatePigeonSpeciesSet();
            UpdateDoorSet();
            UpdatePigeonAIProfile();
            UpdatePigeonFaceSet();
            UpdateTerrainTypeSet();
            UpdateMapTypeSet();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("All ScriptableObjects updated successfully!");
        }

        [MenuItem("Tools/Game Data/Update TrapTypeSet")]
        public static void UpdateTrapTypeSet()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TrapTypeSet>("Assets/GameData/Generated/Traps.asset");
            if (asset != null)
            {
                // 리플렉션으로 InitializeData 호출
                var method = typeof(TrapTypeSet).GetMethod("InitializeData", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    asset.traps = null; // 배열을 null로 설정하여 InitializeData가 실행되도록 함
                    method.Invoke(asset, null);
                    EditorUtility.SetDirty(asset);
                    Debug.Log("TrapTypeSet updated - InitializeData called via reflection");
                }
                else
                {
                    asset.traps = null;
                    EditorUtility.SetDirty(asset);
                    Debug.Log("TrapTypeSet updated - traps array reset (InitializeData will be called on reload)");
                }
            }
            else
            {
                Debug.LogWarning("Traps.asset not found at Assets/GameData/Generated/Traps.asset");
            }
        }

        [MenuItem("Tools/Game Data/Update UpgradeDefinitionSet")]
        public static void UpdateUpgradeDefinitionSet()
        {
            var asset = AssetDatabase.LoadAssetAtPath<UpgradeDefinitionSet>("Assets/GameData/Generated/UpgradeDefinitions.asset");
            if (asset != null)
            {
                var method = typeof(UpgradeDefinitionSet).GetMethod("InitializeData", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    asset.upgrades = null;
                    method.Invoke(asset, null);
                    EditorUtility.SetDirty(asset);
                    Debug.Log("UpgradeDefinitionSet updated - InitializeData called via reflection");
                }
                else
                {
                    asset.upgrades = null;
                    EditorUtility.SetDirty(asset);
                    Debug.Log("UpgradeDefinitionSet updated - upgrades array reset");
                }
            }
            else
            {
                Debug.LogWarning("UpgradeDefinitions.asset not found at Assets/GameData/Generated/UpgradeDefinitions.asset");
            }
        }

        [MenuItem("Tools/Game Data/Update PigeonSpeciesSet")]
        public static void UpdatePigeonSpeciesSet()
        {
            var asset = AssetDatabase.LoadAssetAtPath<PigeonSpeciesSet>("Assets/GameData/Generated/SpeciesSet.asset");
            if (asset != null)
            {
                var method = typeof(PigeonSpeciesSet).GetMethod("InitializeData", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    asset.species = null;
                    method.Invoke(asset, null);
                    EditorUtility.SetDirty(asset);
                    Debug.Log("PigeonSpeciesSet updated - InitializeData called via reflection");
                }
                else
                {
                    asset.species = null;
                    EditorUtility.SetDirty(asset);
                    Debug.Log("PigeonSpeciesSet updated - species array reset");
                }
            }
            else
            {
                Debug.LogWarning("SpeciesSet.asset not found at Assets/GameData/Generated/SpeciesSet.asset");
            }
        }

        [MenuItem("Tools/Game Data/Update DoorSet")]
        public static void UpdateDoorSet()
        {
            var asset = AssetDatabase.LoadAssetAtPath<DoorSet>("Assets/GameData/Generated/Doors.asset");
            if (asset != null)
            {
                var method = typeof(DoorSet).GetMethod("InitializeData", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    asset.doors = null;
                    method.Invoke(asset, null);
                    EditorUtility.SetDirty(asset);
                    Debug.Log("DoorSet updated - InitializeData called via reflection");
                }
                else
                {
                    asset.doors = null;
                    EditorUtility.SetDirty(asset);
                    Debug.Log("DoorSet updated - doors array reset");
                }
            }
            else
            {
                Debug.LogWarning("Doors.asset not found at Assets/GameData/Generated/Doors.asset");
            }
        }

        [MenuItem("Tools/Game Data/Update PigeonAIProfile")]
        public static void UpdatePigeonAIProfile()
        {
            var asset = AssetDatabase.LoadAssetAtPath<PigeonAIProfile>("Assets/GameData/Generated/AIProfiles.asset");
            if (asset != null)
            {
                // PigeonAIProfile은 Dictionary를 사용하므로 리플렉션으로 InitializeData 호출
                var method = typeof(PigeonAIProfile).GetMethod("InitializeData", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(asset, null);
                    EditorUtility.SetDirty(asset);
                    Debug.Log("PigeonAIProfile updated - InitializeData called via reflection");
                }
                else
                {
                    Debug.LogWarning("Could not find InitializeData method in PigeonAIProfile");
                }
            }
            else
            {
                Debug.LogWarning("AIProfiles.asset not found at Assets/GameData/Generated/AIProfiles.asset");
            }
        }

        [MenuItem("Tools/Game Data/Update PigeonFaceSet")]
        public static void UpdatePigeonFaceSet()
        {
            var asset = AssetDatabase.LoadAssetAtPath<PigeonFaceSet>("Assets/GameData/Generated/FaceSet.asset");
            if (asset != null)
            {
                var method = typeof(PigeonFaceSet).GetMethod("InitializeData", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    asset.faces = null;
                    method.Invoke(asset, null);
                    EditorUtility.SetDirty(asset);
                    Debug.Log("PigeonFaceSet updated - InitializeData called via reflection");
                }
                else
                {
                    asset.faces = null;
                    EditorUtility.SetDirty(asset);
                    Debug.Log("PigeonFaceSet updated - faces array reset");
                }
            }
            else
            {
                Debug.LogWarning("FaceSet.asset not found at Assets/GameData/Generated/FaceSet.asset");
            }
        }

        [MenuItem("Tools/Game Data/Update TerrainTypeSet")]
        public static void UpdateTerrainTypeSet()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TerrainTypeSet>("Assets/GameData/Generated/TerrainTypes.asset");
            if (asset != null)
            {
                var method = typeof(TerrainTypeSet).GetMethod("InitializeData", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    asset.terrains = null;
                    method.Invoke(asset, null);
                    EditorUtility.SetDirty(asset);
                    Debug.Log("TerrainTypeSet updated - InitializeData called via reflection");
                }
                else
                {
                    asset.terrains = null;
                    EditorUtility.SetDirty(asset);
                    Debug.Log("TerrainTypeSet updated - terrains array reset");
                }
            }
            else
            {
                Debug.LogWarning("TerrainTypes.asset not found at Assets/GameData/Generated/TerrainTypes.asset");
            }
        }

        [MenuItem("Tools/Game Data/Update MapTypeSet")]
        public static void UpdateMapTypeSet()
        {
            var asset = AssetDatabase.LoadAssetAtPath<MapTypeSet>("Assets/GameData/Generated/MapTypes.asset");
            if (asset != null)
            {
                var method = typeof(MapTypeSet).GetMethod("InitializeData", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    asset.maps = null;
                    method.Invoke(asset, null);
                    EditorUtility.SetDirty(asset);
                    Debug.Log("MapTypeSet updated - InitializeData called via reflection");
                }
                else
                {
                    asset.maps = null;
                    EditorUtility.SetDirty(asset);
                    Debug.Log("MapTypeSet updated - maps array reset");
                }
            }
            else
            {
                Debug.LogWarning("MapTypes.asset not found at Assets/GameData/Generated/MapTypes.asset");
            }
        }
    }
}
