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
        public float playerAlertPerSec;
        public float crowdAlertPerNeighborPerSec;
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

                tiers = new Dictionary<int, RarityTierProfile>
                {
                    { 1, new RarityTierProfile { playerAlertPerSec = 38f, crowdAlertPerNeighborPerSec = 6f } },
                    { 2, new RarityTierProfile { playerAlertPerSec = 30f, crowdAlertPerNeighborPerSec = 10f } },
                    { 3, new RarityTierProfile { playerAlertPerSec = 22f, crowdAlertPerNeighborPerSec = 15f } },
                    { 4, new RarityTierProfile { playerAlertPerSec = 18f, crowdAlertPerNeighborPerSec = 22f } }, 
                    { 5, new RarityTierProfile { playerAlertPerSec = 14f, crowdAlertPerNeighborPerSec = 28f } }  
                };

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

            if (obesityRule != null)
            {
                obesityRule.OnBeforeSerialize();
            }
        }
    }
}

