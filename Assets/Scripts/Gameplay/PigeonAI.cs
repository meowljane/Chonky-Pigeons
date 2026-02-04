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

            if (currentState != PigeonState.Flee)
            {
                const float alertDecayPerSec = 10f;
                alert = Mathf.Max(0, alert - alertDecayPerSec * Time.deltaTime);
            }
            UpdateState();
        }

        public void AddPlayerAlert(float deltaTime)
        {
            if (!CanAddAlert())
                return;
            alert += stats.playerAlertPerSec * movement.AlertWeight * deltaTime;
        }

        public void AddCrowdAlert(int neighborCount, float deltaTime)
        {
            if (!CanAddAlert())
                return;
            alert += stats.crowdAlertPerNeighborPerSec * movement.AlertWeight * neighborCount * deltaTime;
        }

        private bool CanAddAlert()
        {
            return currentState != PigeonState.Flee && movement != null;
        }

        public void ForceFlee()
        {
            PigeonState previousState = currentState;
            currentState = PigeonState.Flee;

            if (previousState != PigeonState.Flee)
            {
                fleeStateStartTime = Time.time;
            }
        }

        private void UpdateState()
        {
            if (movement == null)
                return;

            if (currentState == PigeonState.Flee)
                return;

            PigeonState previousState = currentState;

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
                var modifier = GetStressModifier();
                if (modifier?.enabled == true)
                {
                    chance *= modifier.warnEatChanceMultiplier;
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
                var modifier = GetStressModifier();
                if (modifier?.enabled == true)
                {
                    interval *= modifier.warnEatIntervalMultiplier;
                }
            }
            return interval;
        }

        private PigeonGame.Data.StressToEatModifier GetStressModifier()
        {
            return GameDataRegistry.Instance?.AIProfiles?.stressToEatModifier;
        }
    }
}
