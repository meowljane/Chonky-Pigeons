using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public static class PigeonInstanceFactory
    {
        public static PigeonInstanceStats CreateInstanceStats(PigeonSpecies speciesType, int obesity, float weight, FaceType faceType)
        {
            var registry = GameDataRegistry.Instance;
            if (registry == null)
                return null;

            var species = registry.SpeciesSet.GetSpeciesById(speciesType);
            if (species == null)
                return null;

            var face = registry.Faces.GetFaceById(faceType);
            if (face == null)
                return null;

            var aiProfile = registry.AIProfiles;
            if (aiProfile == null)
                return null;

            if (aiProfile.tiers == null)
            {
                aiProfile.OnAfterDeserialize();
            }

            if (!aiProfile.tiers.ContainsKey(species.rarityTier))
                return null;

            var tierProfile = aiProfile.tiers[species.rarityTier];
            var stats = new PigeonInstanceStats
            {
                speciesId = speciesType,
                obesity = obesity,
                weight = weight,
                faceId = faceType
            };

            stats.bitePower = obesity;

            float baseEatInterval = 1.8f; 
            float obesityIntervalMultiplier = 1.0f;

            float baseEatChance = 0.75f; 
            float obesityChanceMultiplier = 1.0f;

            int obesityTier = obesity;

            if (aiProfile.obesityRule != null && aiProfile.obesityRule.obesityProfiles != null)
            {
                if (aiProfile.obesityRule.obesityProfiles.ContainsKey(obesityTier))
                {
                    var obesityProfile = aiProfile.obesityRule.obesityProfiles[obesityTier];
                    obesityIntervalMultiplier = obesityProfile.eatIntervalMultiplier;
                    obesityChanceMultiplier = obesityProfile.eatChanceMultiplier;
                }
            }

            stats.eatInterval = baseEatInterval * obesityIntervalMultiplier;
            stats.eatChance = baseEatChance * obesityChanceMultiplier;
            stats.playerAlertPerSec = tierProfile.playerAlertPerSec;
            stats.crowdAlertPerNeighborPerSec = tierProfile.crowdAlertPerNeighborPerSec;

            int basePrice = species.basePrice;

            float obesityDiscount = 1.0f;

            if (aiProfile.obesityRule != null && aiProfile.obesityRule.obesityProfiles != null)
            {
                if (aiProfile.obesityRule.obesityProfiles.ContainsKey(obesityTier))
                {
                    obesityDiscount = aiProfile.obesityRule.obesityProfiles[obesityTier].priceDiscount;
                }
            }

            stats.price = Mathf.RoundToInt(basePrice * obesityDiscount * face.priceMultiplier);

            return stats;
        }
    }
}

