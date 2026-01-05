using UnityEngine;
using UnityEngine.InputSystem;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class TrapPlacer : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject trapPrefab;
        [SerializeField] private PigeonSpawner spawner;
        [SerializeField] private string defaultTrapId = "BREAD";

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            
            if (spawner == null)
                spawner = FindObjectOfType<PigeonSpawner>();
        }

        private void Update()
        {
            // 마우스 클릭으로 덫 설치 (새 Input System)
            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                PlaceTrap();
            }
        }

        private void PlaceTrap()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            Vector3 mousePos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
            mousePos.z = 0;

            // 덫 프리팹 생성
            GameObject trapObj = Instantiate(trapPrefab, mousePos, Quaternion.identity);
            FoodTrap trap = trapObj.GetComponent<FoodTrap>();
            
            if (trap != null)
            {
                // TrapId 설정
                var trapType = typeof(FoodTrap).GetField("trapId", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (trapType != null)
                {
                    trapType.SetValue(trap, defaultTrapId);
                }

                // 포획 이벤트 연결
                trap.OnCaptured += OnPigeonCaptured;

                // 비둘기 스폰
                if (spawner != null)
                {
                    spawner.SpawnPigeonsAtPosition(mousePos, trap);
                }
            }
        }

        private void OnPigeonCaptured(PigeonAI pigeon)
        {
            Debug.Log("비둘기 포획 성공!");
            // TODO: 인벤토리에 추가, UI 업데이트 등
        }
    }
}

