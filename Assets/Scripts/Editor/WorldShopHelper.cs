using UnityEngine;
using UnityEditor;
using PigeonGame.Gameplay;

namespace PigeonGame.Editor
{
    public static class WorldShopHelper
    {
        [MenuItem("Tools/Create World Shop/Pigeon Shop", false, 1)]
        public static void CreatePigeonShop()
        {
            CreateShop(WorldShop.ShopType.PigeonShop);
        }

        [MenuItem("Tools/Create World Shop/Trap Shop", false, 2)]
        public static void CreateTrapShop()
        {
            CreateShop(WorldShop.ShopType.TrapShop);
        }

        private static void CreateShop(WorldShop.ShopType shopType)
        {
            GameObject shopObj = new GameObject(shopType == WorldShop.ShopType.PigeonShop ? "PigeonShop" : "TrapShop");
            
            // WorldShop 컴포넌트 추가
            WorldShop shop = shopObj.AddComponent<WorldShop>();
            SerializedObject serializedShop = new SerializedObject(shop);
            serializedShop.FindProperty("shopType").enumValueIndex = (int)shopType;
            serializedShop.FindProperty("interactionRadius").floatValue = 2f;
            serializedShop.ApplyModifiedProperties();

            // SpriteRenderer 추가 (시각적 표시용)
            SpriteRenderer spriteRenderer = shopObj.AddComponent<SpriteRenderer>();
            spriteRenderer.color = shopType == WorldShop.ShopType.PigeonShop ? Color.green : Color.blue;
            // 기본 사각형 스프라이트는 Unity에서 직접 할당 필요

            // Collider 추가 (선택사항, 감지용)
            CircleCollider2D collider = shopObj.AddComponent<CircleCollider2D>();
            collider.radius = 2f;
            collider.isTrigger = true;

            // 선택
            Selection.activeGameObject = shopObj;
            
            Debug.Log($"{shopType} 상점이 생성되었습니다!");
        }

        [MenuItem("Tools/Create Shop Interaction System", false, 3)]
        public static void CreateShopInteractionSystem()
        {
            // 이미 있는지 확인
            ShopInteractionSystem existing = Object.FindObjectOfType<ShopInteractionSystem>();
            if (existing != null)
            {
                Debug.LogWarning("ShopInteractionSystem이 이미 씬에 존재합니다!");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            GameObject systemObj = new GameObject("ShopInteractionSystem");
            ShopInteractionSystem system = systemObj.AddComponent<ShopInteractionSystem>();
            
            SerializedObject serializedSystem = new SerializedObject(system);
            serializedSystem.FindProperty("detectionRadius").floatValue = 5f;
            serializedSystem.ApplyModifiedProperties();

            Selection.activeGameObject = systemObj;
            
            Debug.Log("ShopInteractionSystem이 생성되었습니다!");
        }
    }
}

