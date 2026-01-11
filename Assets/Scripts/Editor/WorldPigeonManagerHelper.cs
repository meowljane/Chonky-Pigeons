using UnityEngine;
using UnityEditor;
using PigeonGame.Gameplay;

namespace PigeonGame.Editor
{
    public static class WorldPigeonManagerHelper
    {
        [MenuItem("Tools/Create World Pigeon Manager", false, 1)]
        public static void CreateWorldPigeonManager()
        {
            // 이미 있는지 확인
            WorldPigeonManager existing = Object.FindObjectOfType<WorldPigeonManager>();
            if (existing != null)
            {
                Debug.LogWarning("WorldPigeonManager가 이미 씬에 존재합니다!");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            GameObject managerObj = new GameObject("WorldPigeonManager");
            WorldPigeonManager manager = managerObj.AddComponent<WorldPigeonManager>();
            
            SerializedObject serializedManager = new SerializedObject(manager);
            serializedManager.FindProperty("initialSpawnCount").intValue = 5;
            serializedManager.FindProperty("spawnInterval").floatValue = 10f;
            serializedManager.FindProperty("despawnChance").floatValue = 0.1f;
            serializedManager.FindProperty("screenMargin").floatValue = 2f;
            serializedManager.ApplyModifiedProperties();

            Selection.activeGameObject = managerObj;
            
            Debug.Log("WorldPigeonManager가 생성되었습니다! Pigeon Prefab을 할당해주세요.");
        }
    }
}

