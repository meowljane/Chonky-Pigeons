using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class PigeonController : MonoBehaviour
    {
        private PigeonInstanceStats stats;
        [SerializeField] private PigeonAI ai;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public PigeonInstanceStats Stats => stats;

        public void Initialize(PigeonInstanceStats stats)
        {
            this.stats = stats;
            if (ai == null)
                ai = GetComponent<PigeonAI>();
            if (ai != null)
                ai.Initialize(stats);

            // SpriteRenderer 찾기 (없으면 자동으로 찾음)
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // SpriteRenderer 활성화
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                
            // 스프라이트 설정
            var species = GameDataRegistry.Instance.SpeciesSet.GetSpeciesById(stats.speciesId);
                if (species != null && species.icon != null)
            {
                spriteRenderer.sprite = species.icon;
                }
                else if (spriteRenderer.sprite == null)
                {
                    Debug.LogWarning($"PigeonController: Species icon이 없거나 설정되지 않았습니다. Species ID: {stats.speciesId}");
                }
            }
            else
            {
                Debug.LogError("PigeonController: SpriteRenderer를 찾을 수 없습니다!");
            }
        }
    }
}

