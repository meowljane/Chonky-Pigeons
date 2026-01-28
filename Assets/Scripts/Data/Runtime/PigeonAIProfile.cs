using UnityEngine;
using System.Collections.Generic;

namespace PigeonGame.Data
{
    [System.Serializable]
    public class ObesityProfile
    {
        public float eatIntervalMultiplier;
        public float eatChanceMultiplier;
        public float priceDiscount;
    }

    [System.Serializable]
    public class ObesityRule
    {
        public bool bitePowerEqualsObesity;
        public Dictionary<int, ObesityProfile> obesityProfiles;
        
        [SerializeField] private List<int> obesityKeys;
        [SerializeField] private List<ObesityProfile> obesityValues;

        public void OnAfterDeserialize()
        {
            obesityProfiles = new Dictionary<int, ObesityProfile>();

            if (obesityKeys != null && obesityValues != null)
            {
                for (int i = 0; i < obesityKeys.Count && i < obesityValues.Count; i++)
                {
                    obesityProfiles[obesityKeys[i]] = obesityValues[i];
                }
            }
        }

        public void OnBeforeSerialize()
        {
            obesityKeys = new List<int>();
            obesityValues = new List<ObesityProfile>();

            if (obesityProfiles != null)
            {
                foreach (var kvp in obesityProfiles)
                {
                    obesityKeys.Add(kvp.Key);
                    obesityValues.Add(kvp.Value);
                }
            }
        }
    }

    [System.Serializable]
    public class StressToEatModifier
    {
        public bool enabled;
        public float warnEatChanceMultiplier;
        public float warnEatIntervalMultiplier;
        public bool backoffStopsEating;
    }

    [System.Serializable]
    public class RarityTierProfile
    {
        // eatInterval, eatChance, alertDecayPerSec는 비만도 기반으로 계산하거나 통일된 값 사용 (base 값은 Tier 1 값으로 통일)
        public float playerAlertPerSec;
        public float crowdAlertPerNeighborPerSec;
        // detectionRadius, warnThreshold, backoffThreshold, fleeThreshold, alertWeight, backoffDistance, alertDecayPerSec는 PigeonMovement에서 관리 (모든 tier 통일)
        // 가격은 종별로 SpeciesDefinition.basePrice에서 관리
    }

    [CreateAssetMenu(fileName = "AIProfiles", menuName = "PigeonGame/AI Profiles")]
    public class PigeonAIProfile : ScriptableObject
    {
        public int version;
        public ObesityRule obesityRule;
        public Dictionary<int, RarityTierProfile> tiers;
        public StressToEatModifier stressToEatModifier;

        [SerializeField] private List<int> tierKeys;
        [SerializeField] private List<RarityTierProfile> tierValues;

        private void OnEnable()
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (tiers == null || tiers.Count == 0)
            {
                version = 1;

                // ObesityRule 초기화
                obesityRule = new ObesityRule
                {
                    bitePowerEqualsObesity = true,
                    obesityProfiles = new Dictionary<int, ObesityProfile>
                    {
                        { 1, new ObesityProfile { eatIntervalMultiplier = 1.08f, eatChanceMultiplier = 0.85f, priceDiscount = 1.0f } },
                        { 2, new ObesityProfile { eatIntervalMultiplier = 1.0f, eatChanceMultiplier = 0.9f, priceDiscount = 0.9f } },
                        { 3, new ObesityProfile { eatIntervalMultiplier = 0.92f, eatChanceMultiplier = 0.95f, priceDiscount = 0.8f } },
                        { 4, new ObesityProfile { eatIntervalMultiplier = 0.85f, eatChanceMultiplier = 1.0f, priceDiscount = 0.7f } },
                        { 5, new ObesityProfile { eatIntervalMultiplier = 0.8f, eatChanceMultiplier = 1.05f, priceDiscount = 0.6f } }
                    }
                };

                // Tiers 초기화
                tiers = new Dictionary<int, RarityTierProfile>
                {
                    { 1, new RarityTierProfile { playerAlertPerSec = 38f, crowdAlertPerNeighborPerSec = 6f } },
                    { 2, new RarityTierProfile { playerAlertPerSec = 30f, crowdAlertPerNeighborPerSec = 10f } },
                    { 3, new RarityTierProfile { playerAlertPerSec = 22f, crowdAlertPerNeighborPerSec = 15f } },
                    { 4, new RarityTierProfile { playerAlertPerSec = 18f, crowdAlertPerNeighborPerSec = 22f } }, // 희귀 비둘기: 경쟁에 민감하게
                    { 5, new RarityTierProfile { playerAlertPerSec = 14f, crowdAlertPerNeighborPerSec = 28f } }  // 최고 희귀: 매우 민감하게
                };

                // StressToEatModifier 초기화
                stressToEatModifier = new StressToEatModifier
                {
                    enabled = true,
                    warnEatChanceMultiplier = 0.65f,
                    warnEatIntervalMultiplier = 1.25f,
                    backoffStopsEating = true
                };

                OnBeforeSerialize();
                }
            }

        public void OnAfterDeserialize()
        {
            tiers = new Dictionary<int, RarityTierProfile>();

            if (tierKeys != null && tierValues != null)
            {
                for (int i = 0; i < tierKeys.Count && i < tierValues.Count; i++)
                {
                    tiers[tierKeys[i]] = tierValues[i];
                }
            }

            // ObesityRule도 초기화
            if (obesityRule != null)
            {
                obesityRule.OnAfterDeserialize();
            }
        }

        public void OnBeforeSerialize()
        {
            tierKeys = new List<int>();
            tierValues = new List<RarityTierProfile>();

            if (tiers != null)
            {
                foreach (var kvp in tiers)
                {
                    tierKeys.Add(kvp.Key);
                    tierValues.Add(kvp.Value);
                }
            }

            // ObesityRule도 직렬화
            if (obesityRule != null)
            {
                obesityRule.OnBeforeSerialize();
            }
        }
    }
}


