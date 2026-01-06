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

            // 스프라이트 설정
            var species = GameDataRegistry.Instance.SpeciesSet.GetSpeciesById(stats.speciesId);
            if (species != null && species.icon != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = species.icon;
            }
        }
    }
}

