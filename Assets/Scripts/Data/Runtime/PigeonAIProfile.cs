using UnityEngine;
using System.Collections.Generic;

namespace PigeonGame.Data
{
    [System.Serializable]
    public class ObesityRule
    {
        public bool bitePowerEqualsObesity;
        public Dictionary<int, float> eatIntervalMultiplierByObesity;
        public Dictionary<int, float> obesityPriceDiscount;
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
        public float eatInterval;
        public float eatChance;
        public float personalSpaceRadius;
        public float playerAlertPerSec;
        public float crowdAlertPerNeighborPerSec;
        public float alertDecayPerSec;
        public float warnThreshold;
        public float backoffThreshold;
        public float fleeThreshold;
        public float backoffDuration;
        public float backoffDistance;
        public float crowdWeight;
        public float playerWeight;
    }

    [CreateAssetMenu(fileName = "AIProfiles", menuName = "PigeonGame/AI Profiles")]
    public class PigeonAIProfile : ScriptableObject
    {
        public int version;
        public ObesityRule obesityRule;
        public Dictionary<int, int> rarityBasePrice;
        public Dictionary<int, RarityTierProfile> tiers;
        public StressToEatModifier stressToEatModifier;

        [SerializeField] private List<int> rarityBasePriceKeys;
        [SerializeField] private List<int> rarityBasePriceValues;
        [SerializeField] private List<int> tierKeys;
        [SerializeField] private List<RarityTierProfile> tierValues;

        public void OnAfterDeserialize()
        {
            rarityBasePrice = new Dictionary<int, int>();
            tiers = new Dictionary<int, RarityTierProfile>();

            if (rarityBasePriceKeys != null && rarityBasePriceValues != null)
            {
                for (int i = 0; i < rarityBasePriceKeys.Count && i < rarityBasePriceValues.Count; i++)
                {
                    rarityBasePrice[rarityBasePriceKeys[i]] = rarityBasePriceValues[i];
                }
            }

            if (tierKeys != null && tierValues != null)
            {
                for (int i = 0; i < tierKeys.Count && i < tierValues.Count; i++)
                {
                    tiers[tierKeys[i]] = tierValues[i];
                }
            }
        }

        public void OnBeforeSerialize()
        {
            rarityBasePriceKeys = new List<int>();
            rarityBasePriceValues = new List<int>();
            tierKeys = new List<int>();
            tierValues = new List<RarityTierProfile>();

            if (rarityBasePrice != null)
            {
                foreach (var kvp in rarityBasePrice)
                {
                    rarityBasePriceKeys.Add(kvp.Key);
                    rarityBasePriceValues.Add(kvp.Value);
                }
            }

            if (tiers != null)
            {
                foreach (var kvp in tiers)
                {
                    tierKeys.Add(kvp.Key);
                    tierValues.Add(kvp.Value);
                }
            }
        }
    }
}


