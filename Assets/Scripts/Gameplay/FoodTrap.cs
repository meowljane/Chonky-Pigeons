using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class FoodTrap : MonoBehaviour
    {
        [SerializeField] private string trapId;
        [SerializeField] private float attractionRadius = 5f; // 비둘기를 이끄는 반경
        [SerializeField] private float eatingRadius = 2f; // 먹을 수 있는 범위 반경
        private TrapDefinition trapData;
        private int currentFeedAmount;
        private List<PigeonAI> nearbyPigeons = new List<PigeonAI>();
        private Dictionary<PigeonAI, float> pigeonEatTimers = new Dictionary<PigeonAI, float>();
        private HashSet<PigeonAI> currentlyEatingPigeons = new HashSet<PigeonAI>(); // 실제로 먹고 있는 비둘기 목록
        private Dictionary<PigeonAI, float> eatingStateTimers = new Dictionary<PigeonAI, float>(); // 먹는 상태 유지 시간

        public string TrapId => trapId;
        public float AttractionRadius => attractionRadius; // 비둘기를 이끄는 반경
        public float EatingRadius => eatingRadius; // 먹을 수 있는 범위 반경
        [System.Obsolete("Use EatingRadius instead")]
        public float DetectionRadius => eatingRadius; // 하위 호환성을 위한 프로퍼티
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
                    // 데이터에서 반경 값 가져오기 (설정되어 있으면 사용)
                    if (trapData.attractionRadius > 0)
                        attractionRadius = trapData.attractionRadius;
                    if (trapData.eatingRadius > 0)
                        eatingRadius = trapData.eatingRadius;
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
            foreach (var pigeon in currentlyEatingPigeons.ToArray())
            {
                if (pigeon == null)
                {
                    currentlyEatingPigeons.Remove(pigeon);
                    if (eatingStateTimers.ContainsKey(pigeon))
                        eatingStateTimers.Remove(pigeon);
                    continue;
                }

                if (!eatingStateTimers.ContainsKey(pigeon))
                {
                    eatingStateTimers[pigeon] = 0f;
                }

                eatingStateTimers[pigeon] += Time.deltaTime;
                
                // 0.5초 동안 "먹는 중" 상태 유지
                if (eatingStateTimers[pigeon] >= 0.5f)
                {
                    currentlyEatingPigeons.Remove(pigeon);
                    eatingStateTimers.Remove(pigeon);
                }
            }

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
            // 먹을 수 있는 범위 반경으로 비둘기 감지
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, eatingRadius);
            
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

            // 실제로 먹이를 먹음 - "먹는 중" 상태로 표시
            currentlyEatingPigeons.Add(pigeon);
            if (!eatingStateTimers.ContainsKey(pigeon))
            {
                eatingStateTimers[pigeon] = 0f;
            }

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
            // 비둘기를 이끄는 반경 (녹색)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, attractionRadius);
            
            // 먹을 수 있는 범위 반경 (노란색)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, eatingRadius);
        }
    }
}

