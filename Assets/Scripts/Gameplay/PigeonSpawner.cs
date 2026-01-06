using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class PigeonSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject pigeonPrefab;
        [SerializeField] private int spawnCount = 5;
        [SerializeField] private float spawnRadius = 3f;

        private List<PigeonController> spawnedPigeons = new List<PigeonController>();

        public void SpawnPigeonsAtPosition(Vector3 position, FoodTrap trap)
        {
            if (pigeonPrefab == null)
            {
                Debug.LogError("Pigeon Prefab이 설정되지 않았습니다!");
                return;
            }

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
            {
                Debug.LogError("GameDataRegistry를 찾을 수 없습니다!");
                return;
            }

            // 간단한 테스트용: 첫 번째 종만 스폰
            var species = registry.SpeciesSet.species[0];
            int obesity = Random.Range(species.defaultObesityRange.x, species.defaultObesityRange.y + 1);
            string faceId = registry.Faces.faces[0].id;

            for (int i = 0; i < spawnCount; i++)
            {
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = position + (Vector3)randomOffset;

                GameObject pigeonObj = Instantiate(pigeonPrefab, spawnPos, Quaternion.identity);
                PigeonController controller = pigeonObj.GetComponent<PigeonController>();
                
                if (controller != null)
                {
                    var stats = PigeonInstanceFactory.CreateInstanceStats(
                        species.speciesId, 
                        obesity, 
                        faceId
                    );
                    
                    controller.Initialize(stats);
                    spawnedPigeons.Add(controller);
                }
            }
        }

        public void ClearPigeons()
        {
            foreach (var pigeon in spawnedPigeons)
            {
                if (pigeon != null)
                    Destroy(pigeon.gameObject);
            }
            spawnedPigeons.Clear();
        }
    }
}

