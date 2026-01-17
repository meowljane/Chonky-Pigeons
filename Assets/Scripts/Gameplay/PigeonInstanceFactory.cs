using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public static class PigeonInstanceFactory
    {
        /// <summary>
        /// speciesId, obesity, weight, faceId로부터 최종 PigeonInstanceStats 생성
        /// </summary>
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

            // Dictionary 초기화 확인
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

            // BitePower = Obesity
            stats.bitePower = obesity;

            // EatInterval 계산 (비만도 기반, tier 1 base 값 사용)
            float baseEatInterval = 0.65f; // tier 1 값으로 통일
            float obesityIntervalMultiplier = 1.0f;
            
            // EatChance 계산 (비만도 기반)
            float baseEatChance = 0.95f; // tier 1 값으로 통일
            float obesityChanceMultiplier = 1.0f;
            
            // 비만도별 설정 가져오기 (obesity는 1~5 범위)
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
            // detectionRadius, warnThreshold, backoffThreshold, fleeThreshold, alertWeight, backoffDistance, alertDecayPerSec는 PigeonMovement에서 관리 (모든 tier 통일)

            // 가격 계산 (종별 basePrice 사용)
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

