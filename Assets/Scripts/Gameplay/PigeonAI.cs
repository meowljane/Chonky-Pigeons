using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public enum PigeonState
    {
        Normal,
        Cautious,
        BackOff,
        Flee
    }

    public class PigeonAI : MonoBehaviour
    {
        private PigeonInstanceStats stats;
        private float alert = 0f;
        private PigeonState currentState = PigeonState.Normal;
        private PigeonMovement movement;
        private float fleeStateStartTime = 0f;

        public float Alert => alert;
        public PigeonState CurrentState => currentState;
        public float FleeElapsedTime => currentState == PigeonState.Flee ? Time.time - fleeStateStartTime : 0f;

        private void Awake()
        {
            movement = GetComponent<PigeonMovement>();
        }

        public void Initialize(PigeonInstanceStats stats)
        {
            this.stats = stats;
            alert = 0f;
            UpdateState();
        }

        private void Update()
        {
            if (stats == null)
                return;

            // Flee 상태일 때는 alert 변경 안 함 (증가/감소 모두)
            if (currentState != PigeonState.Flee)
            {
                // Alert 감소 (모든 tier 통일: 10)
                const float alertDecayPerSec = 10f;
                alert = Mathf.Max(0, alert - alertDecayPerSec * Time.deltaTime);
            }
            UpdateState();
        }

        public void AddPlayerAlert(float deltaTime)
        {
            // Flee 상태일 때는 alert 증가 안 함
            if (currentState == PigeonState.Flee)
                return;

            if (movement == null)
                return;
            alert += stats.playerAlertPerSec * movement.AlertWeight * deltaTime;
        }

        public void AddCrowdAlert(int neighborCount, float deltaTime)
        {
            // Flee 상태일 때는 alert 증가 안 함
            if (currentState == PigeonState.Flee)
                return;

            if (movement == null)
                return;
            alert += stats.crowdAlertPerNeighborPerSec * movement.AlertWeight * neighborCount * deltaTime;
        }

        /// <summary>
        /// 비둘기를 강제로 Flee 상태로 만듦 (WorldPigeonManager에서 사용)
        /// </summary>
        public void ForceFlee()
        {
            PigeonState previousState = currentState;
            currentState = PigeonState.Flee;
            
            // Flee 상태가 되면 시간 기록
            if (previousState != PigeonState.Flee)
            {
                fleeStateStartTime = Time.time;
            }
        }

        private void UpdateState()
        {
            if (movement == null)
                return;

            // 이미 Flee 상태면 상태 변경 건너뛰기 (ForceFlee로 설정된 경우 유지)
            if (currentState == PigeonState.Flee)
                return;

            PigeonState previousState = currentState;

            // Alert 값에 따라 상태 결정
            if (alert >= movement.FleeThreshold)
            {
                currentState = PigeonState.Flee;
            }
            else if (alert >= movement.BackoffThreshold)
            {
                currentState = PigeonState.BackOff;
            }
            else if (alert >= movement.WarnThreshold)
            {
                currentState = PigeonState.Cautious;
            }
            else
            {
                currentState = PigeonState.Normal;
            }

            // Flee 상태가 되면 시간 기록
            if (currentState == PigeonState.Flee && previousState != PigeonState.Flee)
            {
                fleeStateStartTime = Time.time;
            }
        }

        public bool CanEat()
        {
            return currentState != PigeonState.BackOff && currentState != PigeonState.Flee;
        }

        public float GetEatChance()
        {
            if (stats == null || !CanEat())
                return 0f;

            float chance = stats.eatChance;
            if (currentState == PigeonState.Cautious)
            {
                var registry = GameDataRegistry.Instance;
                if (registry != null && registry.AIProfiles != null)
                {
                    var modifier = registry.AIProfiles.stressToEatModifier;
                    if (modifier.enabled)
                    {
                        chance *= modifier.warnEatChanceMultiplier;
                    }
                }
            }
            return chance;
        }

        public float GetEatInterval()
        {
            if (stats == null || !CanEat())
                return float.MaxValue;

            float interval = stats.eatInterval;
            if (currentState == PigeonState.Cautious)
            {
                var registry = GameDataRegistry.Instance;
                if (registry != null && registry.AIProfiles != null)
                {
                    var modifier = registry.AIProfiles.stressToEatModifier;
                    if (modifier.enabled)
                    {
                        interval *= modifier.warnEatIntervalMultiplier;
                    }
                }
            }
            return interval;
        }
    }
}
