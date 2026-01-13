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

        public float Alert => alert;
        public PigeonState CurrentState => currentState;

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

            // Alert 감소
            alert = Mathf.Max(0, alert - stats.alertDecayPerSec * Time.deltaTime);
            UpdateState();
        }

        public void AddPlayerAlert(float deltaTime)
        {
            alert += stats.playerAlertPerSec * stats.alertWeight * PigeonMovement.GlobalAlertWeightMultiplier * deltaTime;
        }

        public void AddCrowdAlert(int neighborCount, float deltaTime)
        {
            alert += stats.crowdAlertPerNeighborPerSec * stats.alertWeight * PigeonMovement.GlobalAlertWeightMultiplier * neighborCount * deltaTime;
        }

        private void UpdateState()
        {
            if (alert >= stats.fleeThreshold)
            {
                currentState = PigeonState.Flee;
            }
            else if (alert >= stats.backoffThreshold)
            {
                currentState = PigeonState.BackOff;
            }
            else if (alert >= stats.warnThreshold)
            {
                currentState = PigeonState.Cautious;
            }
            else
            {
                currentState = PigeonState.Normal;
            }
        }

        public bool CanEat()
        {
            // BackOff 이상이면 먹이 경쟁 참여 안 함
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

