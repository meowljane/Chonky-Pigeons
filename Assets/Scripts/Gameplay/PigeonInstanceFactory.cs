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
            if (aiProfile.tiers == null)
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

            // EatInterval 계산 (비만도 기반, tier 1 base 값 사용)
            float baseEatInterval = 0.65f; // tier 1 값으로 통일
            float obesityIntervalMultiplier = 1.0f;
            
            // EatChance 계산 (비만도 기반)
            float baseEatChance = 0.95f; // tier 1 값으로 통일
            float obesityChanceMultiplier = 1.0f;
            
            // 비만도별 설정 가져오기
            if (aiProfile.obesityRule != null && aiProfile.obesityRule.obesityProfiles != null)
            {
                if (aiProfile.obesityRule.obesityProfiles.ContainsKey(obesity))
            {
                    var obesityProfile = aiProfile.obesityRule.obesityProfiles[obesity];
                    obesityIntervalMultiplier = obesityProfile.eatIntervalMultiplier;
                    obesityChanceMultiplier = obesityProfile.eatChanceMultiplier;
            }
            }
            
            stats.eatInterval = baseEatInterval * obesityIntervalMultiplier;
            stats.eatChance = baseEatChance * obesityChanceMultiplier;
            stats.playerAlertPerSec = tierProfile.playerAlertPerSec;
            stats.crowdAlertPerNeighborPerSec = tierProfile.crowdAlertPerNeighborPerSec;
            // detectionRadius, warnThreshold, backoffThreshold, fleeThreshold, alertWeight, backoffDistance, alertDecayPerSec는 PigeonMovement에서 관리 (모든 tier 통일)

            // 가격 계산
            int basePrice = tierProfile.basePrice;
            float obesityDiscount = 1.0f;
            
            if (aiProfile.obesityRule != null && aiProfile.obesityRule.obesityProfiles != null)
            {
                if (aiProfile.obesityRule.obesityProfiles.ContainsKey(obesity))
                {
                    obesityDiscount = aiProfile.obesityRule.obesityProfiles[obesity].priceDiscount;
                }
            }

            stats.price = Mathf.RoundToInt(basePrice * obesityDiscount * face.priceMultiplier);

            return stats;
        }
    }
}

