using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class FoodTrap : InteractableBase
    {
        [SerializeField] private TrapType trapId;
        private TrapDefinition trapData;
        
        /// <summary>
        /// TrapId 설정 (외부에서 호출 가능)
        /// </summary>
        public void SetTrapId(TrapType trapType)
        {
            trapId = trapType;
            LoadTrapData();
            if (trapData != null)
            {
                currentFeedAmount = trapData.feedAmount;
            }
        }

        /// <summary>
        /// TrapId와 커스텀 feedAmount 설정
        /// </summary>
        public void SetTrapIdAndFeedAmount(TrapType trapType, int feedAmount)
        {
            trapId = trapType;
            LoadTrapData();
            if (trapData != null)
            {
                currentFeedAmount = Mathf.Max(1, feedAmount);
                initialFeedAmount = currentFeedAmount;
            }
        }

        private void LoadTrapData()
        {
            trapData = GameDataRegistry.Instance?.Traps?.GetTrapById(trapId);
        }
        private int currentFeedAmount;
        private int initialFeedAmount; // 설치 시 초기 모이 수량 (MaxFeedAmount로 사용)
        private List<PigeonAI> nearbyPigeons = new List<PigeonAI>();
        private Dictionary<PigeonAI, float> pigeonEatTimers = new Dictionary<PigeonAI, float>();
        private HashSet<PigeonAI> currentlyEatingPigeons = new HashSet<PigeonAI>(); // 실제로 먹고 있는 비둘기 목록
        private Dictionary<PigeonAI, float> eatingStateTimers = new Dictionary<PigeonAI, float>(); // 먹는 상태 유지 시간

        // 포획 관련
        private PigeonInstanceStats capturedPigeonStats; // 포획된 비둘기 정보
        private bool isCaptured = false; // 포획 상태
        private SpriteRenderer spriteRenderer;
        
        [Header("Captured Pigeon Overlay")]
        [SerializeField] private SpriteRenderer pigeonIconSpriteRenderer; // 포획된 비둘기 종 아이콘 (Inspector에서 설정)
        [SerializeField] private SpriteRenderer pigeonFaceIconSpriteRenderer; // 포획된 비둘기 표정 아이콘 (Inspector에서 설정)

        public TrapType TrapId => trapId;
        public int CurrentFeedAmount => currentFeedAmount;
        public int MaxFeedAmount => initialFeedAmount > 0 ? initialFeedAmount : trapData?.feedAmount ?? 20;
        public bool HasCapturedPigeon => isCaptured && capturedPigeonStats != null;
        public PigeonInstanceStats CapturedPigeonStats => capturedPigeonStats;
        public event System.Action<PigeonAI> OnCaptured;

        /// <summary>
        /// 해당 비둘기가 현재 실제로 먹고 있는지 확인
        /// </summary>
        public bool IsPigeonEating(PigeonAI pigeon)
        {
            return currentlyEatingPigeons.Contains(pigeon);
        }

        protected override void Start()
        {
            base.Start();

            spriteRenderer = GetComponent<SpriteRenderer>();
            
            LoadTrapData();
            if (trapData != null)
            {
                if (currentFeedAmount <= 0)
                {
                    currentFeedAmount = trapData.feedAmount;
                }
                if (initialFeedAmount <= 0)
                {
                    initialFeedAmount = currentFeedAmount;
                }
                
                // 설치되었을 때의 이미지 설정 (trapData.icon 사용)
                if (trapData.icon != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = trapData.icon;
                }
            }
        }

        private void Update()
        {
            // 포획된 상태면 더 이상 업데이트하지 않음
            if (isCaptured)
                return;

            // 주변 비둘기 감지
            UpdateNearbyPigeons();

            // 먹이 경쟁 참여 비둘기들끼리 스트레스 증가
            UpdateCompetitionAlert();

            // 먹는 상태 타이머 업데이트 (먹는 중 표시 유지 시간)
            UpdateEatingStateTimers();

            // 각 비둘기의 EatTick 처리 (복사본으로 순회하여 수정 안전)
            PigeonAI[] pigeonsArray = new PigeonAI[nearbyPigeons.Count];
            nearbyPigeons.CopyTo(pigeonsArray);
            foreach (var pigeon in pigeonsArray)
            {
                if (pigeon == null || !pigeon.CanEat())
                {
                    if (pigeonEatTimers.ContainsKey(pigeon))
                        pigeonEatTimers.Remove(pigeon);
                    continue;
                }

                if (!pigeonEatTimers.ContainsKey(pigeon))
                {
                    pigeonEatTimers[pigeon] = 0f;
                }

                float eatInterval = pigeon.GetEatInterval();
                pigeonEatTimers[pigeon] += Time.deltaTime;

                if (pigeonEatTimers[pigeon] >= eatInterval)
                {
                    pigeonEatTimers[pigeon] = 0f;
                    TryEat(pigeon);
                }
            }
        }

        /// <summary>
        /// 먹이 경쟁 참여 비둘기들끼리 스트레스 증가
        /// </summary>
        private void UpdateCompetitionAlert()
        {
            if (nearbyPigeons.Count <= 1)
                return;

            float deltaTime = Time.deltaTime;

            // 먹이 경쟁에 참여할 수 있는 비둘기들만 필터링
            List<PigeonAI> competingPigeons = new List<PigeonAI>();
            foreach (var pigeon in nearbyPigeons)
            {
                if (pigeon != null && pigeon.CanEat())
                {
                    competingPigeons.Add(pigeon);
                }
            }

            if (competingPigeons.Count <= 1)
                return;

            // 각 비둘기마다 다른 경쟁자 수만큼 스트레스 증가
            int competitorCount = competingPigeons.Count - 1;
            foreach (var pigeon in competingPigeons)
            {
                if (pigeon == null || pigeon.CurrentState == PigeonState.Flee)
                    continue;

                pigeon.AddCrowdAlert(competitorCount, deltaTime);
            }
        }

        private void UpdateEatingStateTimers()
        {
            const float EATING_STATE_DURATION = 0.5f;
            
            // 복사본으로 순회하여 수정 안전
            PigeonAI[] pigeonsArray = new PigeonAI[currentlyEatingPigeons.Count];
            currentlyEatingPigeons.CopyTo(pigeonsArray);
            foreach (var pigeon in pigeonsArray)
            {
                if (pigeon == null)
                {
                    currentlyEatingPigeons.Remove(pigeon);
                    eatingStateTimers.Remove(pigeon);
                    continue;
                }

                if (!eatingStateTimers.ContainsKey(pigeon))
                {
                    eatingStateTimers[pigeon] = 0f;
                }

                eatingStateTimers[pigeon] += Time.deltaTime;
                
                if (eatingStateTimers[pigeon] >= EATING_STATE_DURATION)
                {
                    currentlyEatingPigeons.Remove(pigeon);
                    eatingStateTimers.Remove(pigeon);
                }
            }
        }

        private void UpdateNearbyPigeons()
        {
            nearbyPigeons.Clear();
            
            // interactionRadius + 여유분으로 비둘기 검색
            float searchRadius = interactionRadius + 0.5f;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);
            
            foreach (var col in colliders)
            {
                PigeonAI pigeon = col.GetComponent<PigeonAI>();
                if (pigeon == null)
                    continue;

                PigeonMovement movement = pigeon.GetComponent<PigeonMovement>();
                if (movement == null)
                    continue;

                Vector2 toPigeon = (Vector2)(pigeon.transform.position - transform.position);
                float sqrDistance = toPigeon.sqrMagnitude;
                float pigeonEatingRadius = movement.GetEatingRadius();
                float sqrRadius = pigeonEatingRadius * pigeonEatingRadius;
                
                // 비둘기의 eatingRadius 내에 있는지 확인
                if (sqrDistance <= sqrRadius)
                {
                    nearbyPigeons.Add(pigeon);
                }
            }
        }

        private bool TryEat(PigeonAI pigeon)
        {
            if (isCaptured || !pigeon.CanEat())
                return false;

            var controller = pigeon.GetComponent<PigeonController>();
            if (controller == null || controller.Stats == null)
                return false;

            var stats = controller.Stats;

            // EatChance 체크
            if (Random.value > pigeon.GetEatChance())
                return false;

            // 실제로 먹이를 먹음 - "먹는 중" 상태로 표시
            currentlyEatingPigeons.Add(pigeon);
            eatingStateTimers[pigeon] = 0f;

            int bitePower = stats.bitePower;
            currentFeedAmount -= bitePower;

            if (currentFeedAmount <= 0)
            {
                capturedPigeonStats = stats.Clone();
                isCaptured = true;
                ChangeToCapturedState();
                OnCaptured?.Invoke(pigeon);
                Destroy(pigeon.gameObject);
                return true;
            }

            return true;
        }

        /// <summary>
        /// 포획된 덫 상태로 시각적 변경
        /// </summary>
        private void ChangeToCapturedState()
        {
            // 덫 스프라이트 변경
            if (trapData != null && trapData.capturedSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = trapData.capturedSprite;
            }
            
            // 포획된 비둘기 이미지 오버레이 표시
            ShowCapturedPigeonOverlay();
        }
        
        /// <summary>
        /// 포획된 비둘기 이미지 오버레이 표시 (opacity 0.7)
        /// </summary>
        private void ShowCapturedPigeonOverlay()
        {
            if (capturedPigeonStats == null)
                return;
            
            var registry = GameDataRegistry.Instance;
            if (registry == null)
                return;
            
            // Species icon 설정
            if (pigeonIconSpriteRenderer != null)
            {
                var species = registry.SpeciesSet?.GetSpeciesById(capturedPigeonStats.speciesId);
                var defaultSpecies = registry.SpeciesSet?.GetSpeciesById(PigeonSpecies.SP01);
                var iconToUse = species?.icon ?? defaultSpecies?.icon;
                
                if (iconToUse != null)
                {
                    pigeonIconSpriteRenderer.sprite = iconToUse;
                    pigeonIconSpriteRenderer.enabled = true;
                    
                    // Opacity 0.7 설정
                    Color color = pigeonIconSpriteRenderer.color;
                    color.a = 0.7f;
                    pigeonIconSpriteRenderer.color = color;
                }
                else
                {
                    pigeonIconSpriteRenderer.enabled = false;
                }
            }
            
            // Face icon 설정
            if (pigeonFaceIconSpriteRenderer != null)
            {
                var face = registry.Faces?.GetFaceById(capturedPigeonStats.faceId);
                var defaultFace = registry.Faces?.GetFaceById(FaceType.F00);
                var faceIconToUse = face?.icon ?? defaultFace?.icon;
                
                if (faceIconToUse != null)
                {
                    pigeonFaceIconSpriteRenderer.sprite = faceIconToUse;
                    pigeonFaceIconSpriteRenderer.enabled = true;
                    
                    // Opacity 0.7 설정
                    Color color = pigeonFaceIconSpriteRenderer.color;
                    color.a = 0.7f;
                    pigeonFaceIconSpriteRenderer.color = color;
                }
                else
                {
                    pigeonFaceIconSpriteRenderer.enabled = false;
                }
            }
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (!isCaptured)
                return;
            base.OnTriggerEnter2D(other);
        }

        protected override void OnTriggerExit2D(Collider2D other)
        {
            if (!isCaptured)
                return;
            base.OnTriggerExit2D(other);
        }

        public override bool CanInteract()
        {
            return HasCapturedPigeon && isPlayerInRange;
        }

        public override void OnInteract()
        {
            if (!CanInteract())
                return;

            var pigeonStats = CapturedPigeonStats;
            if (pigeonStats == null || GameManager.Instance == null)
                return;

            // 인벤토리가 가득 찼는지 먼저 확인
            if (GameManager.Instance.InventoryCount >= GameManager.Instance.MaxInventorySlots)
            {
                UI.ToastNotificationManager.ShowWarning("인벤토리가 가득 찼습니다!");
                return;
            }

            GameManager.Instance.AddPigeonToInventory(pigeonStats);

            var detailPanelUI = UnityEngine.Object.FindFirstObjectByType<UI.PigeonDetailPanelUI>();
            detailPanelUI?.ShowDetail(pigeonStats);

            Destroy(gameObject);
        }
    }
}

