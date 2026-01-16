using UnityEngine;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class PigeonController : MonoBehaviour
    {
        private PigeonInstanceStats stats;
        [SerializeField] private PigeonAI ai;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private bool isExhibitionPigeon = false; // 전시관 비둘기 여부
        private Collider2D exhibitionArea = null; // 전시관 영역

        public PigeonInstanceStats Stats => stats;
        public bool IsExhibitionPigeon => isExhibitionPigeon;
        public Collider2D ExhibitionArea => exhibitionArea;

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

            // SpriteRenderer 활성화 및 스프라이트 설정
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                
                // 스프라이트 설정
                var species = GameDataRegistry.Instance?.SpeciesSet?.GetSpeciesById(stats.speciesId);
                if (species?.icon != null)
                {
                    spriteRenderer.sprite = species.icon;
                }
            }
        }

        /// <summary>
        /// 전시관 비둘기로 설정
        /// </summary>
        public void SetAsExhibitionPigeon(Collider2D area)
        {
            isExhibitionPigeon = true;
            exhibitionArea = area;
        }
    }
}

