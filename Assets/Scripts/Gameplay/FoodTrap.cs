using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class FoodTrap : MonoBehaviour
    {
        [SerializeField] private string trapId;
        [SerializeField] private float detectionRadius = 2f;
        private TrapDefinition trapData;
        private int currentFeedAmount;
        private List<PigeonAI> nearbyPigeons = new List<PigeonAI>();
        private Dictionary<PigeonAI, float> pigeonEatTimers = new Dictionary<PigeonAI, float>();

        public string TrapId => trapId;
        public float DetectionRadius => detectionRadius;
        public int CurrentFeedAmount => currentFeedAmount;
        public int MaxFeedAmount => trapData != null ? trapData.feedAmount : 20;
        public bool IsDepleted => currentFeedAmount <= 0;
        public event System.Action<PigeonAI> OnCaptured;

        private void Start()
        {
            var registry = GameDataRegistry.Instance;
            if (registry != null)
            {
                trapData = registry.Traps.GetTrapById(trapId);
                if (trapData != null)
                {
                    currentFeedAmount = trapData.feedAmount;
                }
            }
        }

        private void Update()
        {
            if (IsDepleted)
                return;

            // 주변 비둘기 감지
            UpdateNearbyPigeons();

            // 각 비둘기의 EatTick 처리
            foreach (var pigeon in nearbyPigeons.ToArray())
            {
                if (pigeon == null || !pigeon.CanEat())
                {
                    if (pigeonEatTimers.ContainsKey(pigeon))
                        pigeonEatTimers.Remove(pigeon);
                    continue;
                }

                if (!pigeonEatTimers.ContainsKey(pigeon))
                {
                    pigeonEatTimers[pigeon] = 0f;
                }

                float eatInterval = pigeon.GetEatInterval();
                pigeonEatTimers[pigeon] += Time.deltaTime;

                if (pigeonEatTimers[pigeon] >= eatInterval)
                {
                    pigeonEatTimers[pigeon] = 0f;
                    TryEat(pigeon);
                }
            }
        }

        private void UpdateNearbyPigeons()
        {
            nearbyPigeons.Clear();
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
            
            foreach (var col in colliders)
            {
                PigeonAI pigeon = col.GetComponent<PigeonAI>();
                if (pigeon != null)
                {
                    nearbyPigeons.Add(pigeon);
                }
            }
        }

        private bool TryEat(PigeonAI pigeon)
        {
            if (IsDepleted || !pigeon.CanEat())
                return false;

            var controller = pigeon.GetComponent<PigeonController>();
            if (controller == null || controller.Stats == null)
                return false;

            var stats = controller.Stats;

            // EatChance 체크
            if (Random.value > pigeon.GetEatChance())
                return false;

            int bitePower = stats.bitePower;
            currentFeedAmount -= bitePower;

            Debug.Log($"{stats.speciesId}가 {bitePower}만큼 먹음! 남은 먹이: {currentFeedAmount}");

            if (currentFeedAmount <= 0)
            {
                // 포획!
                Debug.Log($"포획! {stats.speciesId} (비만도: {stats.obesity}, 얼굴: {stats.faceId})");
                OnCaptured?.Invoke(pigeon);
                return true;
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}

