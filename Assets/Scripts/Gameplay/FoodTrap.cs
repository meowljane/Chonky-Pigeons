using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class FoodTrap : MonoBehaviour
    {
        [SerializeField] private string trapId;
        private TrapDefinition trapData;
        
        /// <summary>
        /// TrapId 설정 (외부에서 호출 가능)
        /// </summary>
        public void SetTrapId(string id)
        {
            trapId = id;
            // trapData 재로드
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
        private int currentFeedAmount;
        private List<PigeonAI> nearbyPigeons = new List<PigeonAI>();
        private Dictionary<PigeonAI, float> pigeonEatTimers = new Dictionary<PigeonAI, float>();
        private HashSet<PigeonAI> currentlyEatingPigeons = new HashSet<PigeonAI>(); // 실제로 먹고 있는 비둘기 목록
        private Dictionary<PigeonAI, float> eatingStateTimers = new Dictionary<PigeonAI, float>(); // 먹는 상태 유지 시간

        public string TrapId => trapId;
        public int CurrentFeedAmount => currentFeedAmount;
        public int MaxFeedAmount => trapData != null ? trapData.feedAmount : 20;
        public bool IsDepleted => currentFeedAmount <= 0;
        public event System.Action<PigeonAI> OnCaptured;

        /// <summary>
        /// 해당 비둘기가 현재 실제로 먹고 있는지 확인
        /// </summary>
        public bool IsPigeonEating(PigeonAI pigeon)
        {
            return currentlyEatingPigeons.Contains(pigeon);
        }

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

            // 먹는 상태 타이머 업데이트 (먹는 중 표시 유지 시간)
            UpdateEatingStateTimers();

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

        private void UpdateEatingStateTimers()
        {
            const float EATING_STATE_DURATION = 0.5f;
            
            foreach (var pigeon in currentlyEatingPigeons.ToArray())
            {
                if (pigeon == null)
                {
                    currentlyEatingPigeons.Remove(pigeon);
                    eatingStateTimers.Remove(pigeon);
                    continue;
                }

                if (!eatingStateTimers.ContainsKey(pigeon))
                {
                    eatingStateTimers[pigeon] = 0f;
                }

                eatingStateTimers[pigeon] += Time.deltaTime;
                
                if (eatingStateTimers[pigeon] >= EATING_STATE_DURATION)
                {
                    currentlyEatingPigeons.Remove(pigeon);
                    eatingStateTimers.Remove(pigeon);
                }
            }
        }

        private void UpdateNearbyPigeons()
        {
            nearbyPigeons.Clear();
            
            // 충분히 큰 범위로 비둘기 검색 (최대 eatingRadius를 고려하여 넉넉하게)
            float maxSearchRadius = 10f; // 비둘기들의 최대 eatingRadius를 고려한 검색 범위
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, maxSearchRadius);
            
            foreach (var col in colliders)
            {
                PigeonAI pigeon = col.GetComponent<PigeonAI>();
                if (pigeon == null)
                    continue;

                PigeonMovement movement = pigeon.GetComponent<PigeonMovement>();
                if (movement == null)
                    continue;

                float distance = Vector2.Distance(transform.position, pigeon.transform.position);
                float pigeonEatingRadius = movement.GetEatingRadius();
                
                // 비둘기의 eatingRadius 내에 있는지 확인
                if (distance <= pigeonEatingRadius)
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

            // 실제로 먹이를 먹음 - "먹는 중" 상태로 표시
            currentlyEatingPigeons.Add(pigeon);
            eatingStateTimers[pigeon] = 0f;

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

            return true; // 먹이를 먹었으므로 true 반환
        }

        private void OnDrawGizmosSelected()
        {
            // 주변 비둘기들의 eatingRadius 표시 (각 비둘기마다 다를 수 있음)
            Gizmos.color = Color.yellow;
            PigeonAI[] allPigeons = FindObjectsOfType<PigeonAI>();
            
            foreach (var pigeon in allPigeons)
            {
                if (pigeon == null)
                    continue;

                PigeonMovement movement = pigeon.GetComponent<PigeonMovement>();
                if (movement == null)
                    continue;

                float distance = Vector2.Distance(transform.position, pigeon.transform.position);
                float pigeonEatingRadius = movement.GetEatingRadius();
                
                // 이 덫이 비둘기의 eatingRadius 내에 있으면 표시
                if (distance <= pigeonEatingRadius)
                {
                    Gizmos.DrawWireSphere(pigeon.transform.position, pigeonEatingRadius);
                }
            }
        }
    }
}

