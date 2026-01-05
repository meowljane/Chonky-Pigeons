using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public static class PigeonInstanceFactory
    {
        /// <summary>
        /// speciesId, obesity, faceId로부터 최종 PigeonInstanceStats 생성
        /// </summary>
        public static PigeonInstanceStats CreateInstanceStats(string speciesId, int obesity, string faceId)
        {
            var registry = GameDataRegistry.Instance;
            if (registry == null)
            {
                Debug.LogError("GameDataRegistry not found!");
                return null;
            }

            var species = registry.SpeciesSet.GetSpeciesById(speciesId);
            if (species == null)
            {
                Debug.LogError($"Species not found: {speciesId}");
                return null;
            }

            var face = registry.Faces.GetFaceById(faceId);
            if (face == null)
            {
                Debug.LogError($"Face not found: {faceId}");
                return null;
            }

            var aiProfile = registry.AIProfiles;
            if (aiProfile == null)
            {
                Debug.LogError("AI Profile is null!");
                return null;
            }

            // Dictionary 초기화 확인
            if (aiProfile.tiers == null || aiProfile.rarityBasePrice == null)
            {
                aiProfile.OnAfterDeserialize();
            }

            if (!aiProfile.tiers.ContainsKey(species.rarityTier))
            {
                Debug.LogError($"AI Profile not found for tier: {species.rarityTier}");
                return null;
            }

            var tierProfile = aiProfile.tiers[species.rarityTier];
            var stats = new PigeonInstanceStats
            {
                speciesId = speciesId,
                obesity = obesity,
                faceId = faceId
            };

            // BitePower = Obesity
            stats.bitePower = obesity;

            // EatInterval 계산
            float obesityMultiplier = 1.0f;
            if (aiProfile.obesityRule != null && 
                aiProfile.obesityRule.eatIntervalMultiplierByObesity != null &&
                aiProfile.obesityRule.eatIntervalMultiplierByObesity.ContainsKey(obesity))
            {
                obesityMultiplier = aiProfile.obesityRule.eatIntervalMultiplierByObesity[obesity];
            }
            stats.eatInterval = tierProfile.eatInterval * obesityMultiplier;

            // AI 파라미터 복사
            stats.eatChance = tierProfile.eatChance;
            stats.personalSpaceRadius = tierProfile.personalSpaceRadius;
            stats.playerAlertPerSec = tierProfile.playerAlertPerSec;
            stats.crowdAlertPerNeighborPerSec = tierProfile.crowdAlertPerNeighborPerSec;
            stats.alertDecayPerSec = tierProfile.alertDecayPerSec;
            stats.warnThreshold = tierProfile.warnThreshold;
            stats.backoffThreshold = tierProfile.backoffThreshold;
            stats.fleeThreshold = tierProfile.fleeThreshold;
            stats.backoffDuration = tierProfile.backoffDuration;
            stats.backoffDistance = tierProfile.backoffDistance;
            stats.crowdWeight = tierProfile.crowdWeight;
            stats.playerWeight = tierProfile.playerWeight;

            // 가격 계산
            int basePrice = aiProfile.rarityBasePrice.ContainsKey(species.rarityTier) 
                ? aiProfile.rarityBasePrice[species.rarityTier] 
                : 10;

            float obesityDiscount = 1.0f;
            if (aiProfile.obesityRule != null && 
                aiProfile.obesityRule.obesityPriceDiscount != null &&
                aiProfile.obesityRule.obesityPriceDiscount.ContainsKey(obesity))
            {
                obesityDiscount = aiProfile.obesityRule.obesityPriceDiscount[obesity];
            }

            stats.price = Mathf.RoundToInt(basePrice * obesityDiscount * face.priceMultiplier);

            return stats;
        }
    }
}

