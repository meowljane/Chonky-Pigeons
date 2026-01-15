using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class FoodTrap : MonoBehaviour, IInteractable
    {
        [SerializeField] private string trapId;
        [SerializeField] private Sprite capturedTrapSprite; // 포획된 덫의 스프라이트 (Inspector에서 설정)
        private TrapDefinition trapData;
        
        /// <summary>
        /// TrapId 설정 (외부에서 호출 가능)
        /// </summary>
        public void SetTrapId(string id)
        {
            trapId = id;
            // trapData 재로드
            var registry = GameDataRegistry.Instance;
            if (registry != null)
            {
                trapData = registry.Traps.GetTrapById(trapId);
                if (trapData != null)
                {
                    currentFeedAmount = trapData.feedAmount;
                }
            }
        }
        private int currentFeedAmount;
        private List<PigeonAI> nearbyPigeons = new List<PigeonAI>();
        private Dictionary<PigeonAI, float> pigeonEatTimers = new Dictionary<PigeonAI, float>();
        private HashSet<PigeonAI> currentlyEatingPigeons = new HashSet<PigeonAI>(); // 실제로 먹고 있는 비둘기 목록
        private Dictionary<PigeonAI, float> eatingStateTimers = new Dictionary<PigeonAI, float>(); // 먹는 상태 유지 시간

        // 포획 관련
        private PigeonInstanceStats capturedPigeonStats; // 포획된 비둘기 정보
        private bool isCaptured = false; // 포획 상태
        private SpriteRenderer spriteRenderer;
        private Image imageComponent;
        private Sprite originalSprite; // 원래 스프라이트 저장
        private Collider2D detectionCollider; // 비둘기 감지용 콜라이더 (항상 활성화)
        private Collider2D interactionTrigger; // 상호작용 트리거 영역 (포획 후에만 활성화)
        private bool isPlayerInRange = false; // 플레이어가 범위 안에 있는지

        public string TrapId => trapId;
        public int CurrentFeedAmount => currentFeedAmount;
        public int MaxFeedAmount => trapData != null ? trapData.feedAmount : 20;
        public bool IsDepleted => currentFeedAmount <= 0;
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

        private void Start()
        {
            var registry = GameDataRegistry.Instance;
            if (registry != null)
            {
                trapData = registry.Traps.GetTrapById(trapId);
                if (trapData != null)
                {
                    currentFeedAmount = trapData.feedAmount;
                }
            }

            // 시각적 컴포넌트 찾기 및 원래 스프라이트 저장
            spriteRenderer = GetComponent<SpriteRenderer>();
            imageComponent = GetComponent<Image>();
            
            // 원래 스프라이트 저장
            if (spriteRenderer != null)
            {
                originalSprite = spriteRenderer.sprite;
            }
            else if (imageComponent != null)
            {
                originalSprite = imageComponent.sprite;
            }

            // 상호작용 트리거 설정
            SetupInteractionTrigger();
        }

        private void Update()
        {
            // 포획된 상태면 더 이상 업데이트하지 않음
            if (isCaptured)
                return;

            if (IsDepleted)
                return;

            // 주변 비둘기 감지
            UpdateNearbyPigeons();

            // 먹이 경쟁 참여 비둘기들끼리 스트레스 증가
            UpdateCompetitionAlert();

            // 먹는 상태 타이머 업데이트 (먹는 중 표시 유지 시간)
            UpdateEatingStateTimers();

            // 각 비둘기의 EatTick 처리
            foreach (var pigeon in nearbyPigeons.ToArray())
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
            
            foreach (var pigeon in currentlyEatingPigeons.ToArray())
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
            
            // 충분히 큰 범위로 비둘기 검색 (최대 eatingRadius를 고려하여 넉넉하게)
            float maxSearchRadius = 10f; // 비둘기들의 최대 eatingRadius를 고려한 검색 범위
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, maxSearchRadius);
            
            foreach (var col in colliders)
            {
                PigeonAI pigeon = col.GetComponent<PigeonAI>();
                if (pigeon == null)
                    continue;

                PigeonMovement movement = pigeon.GetComponent<PigeonMovement>();
                if (movement == null)
                    continue;

                float distance = Vector2.Distance(transform.position, pigeon.transform.position);
                float pigeonEatingRadius = movement.GetEatingRadius();
                
                // 비둘기의 eatingRadius 내에 있는지 확인
                if (distance <= pigeonEatingRadius)
                {
                    nearbyPigeons.Add(pigeon);
                }
            }
        }

        private bool TryEat(PigeonAI pigeon)
        {
            if (IsDepleted || !pigeon.CanEat())
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
                // 포획! (즉시 등록하지 않고 저장만 함)
                // 포획된 비둘기 정보 저장
                capturedPigeonStats = stats.Clone();
                isCaptured = true;
                
                // 덫 시각적 상태 변경
                ChangeToCapturedState();
                
                // 이벤트 발생 (알림용, 실제 등록은 상호작용 시)
                OnCaptured?.Invoke(pigeon);
                
                // 포획된 비둘기 오브젝트 삭제
                if (pigeon != null)
                {
                    Destroy(pigeon.gameObject);
                }
                
                return true;
            }

            return true; // 먹이를 먹었으므로 true 반환
        }

        /// <summary>
        /// 상호작용 트리거 설정
        /// </summary>
        private void SetupInteractionTrigger()
        {
            // 비둘기 감지용 콜라이더 설정 (항상 활성화되어야 함)
            Collider2D[] allColliders = GetComponents<Collider2D>();
            detectionCollider = null;
            interactionTrigger = null;

            // 기존 콜라이더 찾기
            foreach (var col in allColliders)
            {
                if (col.isTrigger)
                {
                    // 트리거 콜라이더는 상호작용용으로 사용
                    interactionTrigger = col;
                }
                else
                {
                    // 일반 콜라이더는 감지용으로 사용
                    detectionCollider = col;
                }
            }

            // 비둘기 감지용 콜라이더가 없으면 생성 (비둘기들이 덫을 찾을 수 있도록)
            if (detectionCollider == null)
            {
                CircleCollider2D detectionCol = gameObject.AddComponent<CircleCollider2D>();
                detectionCol.radius = 1f; // 감지 범위
                detectionCol.isTrigger = false; // 트리거가 아니어야 OverlapCircleAll에서 감지됨
                detectionCollider = detectionCol;
            }

            // 상호작용 트리거가 없으면 생성 (포획 후 플레이어 상호작용용)
            if (interactionTrigger == null)
            {
                CircleCollider2D interactionCol = gameObject.AddComponent<CircleCollider2D>();
                interactionCol.radius = 2f; // 상호작용 범위
                interactionCol.isTrigger = true;
                interactionTrigger = interactionCol;
            }
            else
            {
                // 기존 트리거를 상호작용용으로 설정
                interactionTrigger.isTrigger = true;
            }
            
            // 비둘기 감지용 콜라이더는 항상 활성화
            if (detectionCollider != null)
            {
                detectionCollider.enabled = true;
            }

            // 포획되지 않았을 때는 상호작용 트리거 비활성화
            if (interactionTrigger != null)
            {
                interactionTrigger.enabled = false;
            }
        }

        /// <summary>
        /// 포획된 덫 상태로 시각적 변경
        /// </summary>
        private void ChangeToCapturedState()
        {
            if (capturedTrapSprite != null)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = capturedTrapSprite;
                }
                else if (imageComponent != null)
                {
                    imageComponent.sprite = capturedTrapSprite;
                }
            }
            
            // 포획되면 트리거 활성화
            if (interactionTrigger != null)
            {
                interactionTrigger.enabled = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 포획된 상태가 아니면 무시
            if (!isCaptured)
                return;

            // 플레이어인지 확인 (컴포넌트로만 체크)
            if (other.GetComponent<PlayerController>() != null)
            {
                isPlayerInRange = true;
                // InteractionSystem에 알림
                if (InteractionSystem.Instance != null)
                {
                    InteractionSystem.Instance.RegisterInteractable(this);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // 포획된 상태가 아니면 무시
            if (!isCaptured)
                return;

            // 플레이어인지 확인 (컴포넌트로만 체크)
            if (other.GetComponent<PlayerController>() != null)
            {
                isPlayerInRange = false;
                // InteractionSystem에서 제거
                if (InteractionSystem.Instance != null)
                {
                    InteractionSystem.Instance.UnregisterInteractable(this);
                }
            }
        }

        // IInteractable 구현
        public bool CanInteract()
        {
            return HasCapturedPigeon && isPlayerInRange;
        }

        public void OnInteract()
        {
            if (!CanInteract())
                return;

            var pigeonStats = CapturedPigeonStats;
            if (pigeonStats == null)
            {
                Debug.LogWarning("포획된 비둘기 정보를 찾을 수 없습니다!");
                return;
            }

            // 먼저 인벤토리와 도감에 등록
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddPigeonToInventory(pigeonStats);
            }

            // 비둘기 상세정보 표시
            var inventoryUI = UnityEngine.Object.FindFirstObjectByType<UI.InventoryUI>();
            if (inventoryUI != null)
            {
                inventoryUI.ShowPigeonDetail(pigeonStats);
            }

            // 덫 오브젝트 제거
            Destroy(gameObject);
        }

        /// <summary>
        /// 포획된 비둘기를 수집하고 덫 초기화
        /// (이미 인벤토리에 등록된 후 호출됨)
        /// </summary>
        public void CollectCapturedPigeon()
        {
            if (!isCaptured || capturedPigeonStats == null)
                return;

            // 덫 초기화
            capturedPigeonStats = null;
            isCaptured = false;
            currentFeedAmount = trapData != null ? trapData.feedAmount : 20;
            
            // 시각적 상태 복원 (원래 스프라이트로)
            if (originalSprite != null)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = originalSprite;
                }
                else if (imageComponent != null)
                {
                    imageComponent.sprite = originalSprite;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 주변 비둘기들의 eatingRadius 표시 (각 비둘기마다 다를 수 있음)
            Gizmos.color = Color.yellow;
            PigeonAI[] allPigeons = FindObjectsByType<PigeonAI>(FindObjectsSortMode.None);
            
            foreach (var pigeon in allPigeons)
            {
                if (pigeon == null)
                    continue;

                PigeonMovement movement = pigeon.GetComponent<PigeonMovement>();
                if (movement == null)
                    continue;

                float distance = Vector2.Distance(transform.position, pigeon.transform.position);
                float pigeonEatingRadius = movement.GetEatingRadius();
                
                // 이 덫이 비둘기의 eatingRadius 내에 있으면 표시
                if (distance <= pigeonEatingRadius)
                {
                    Gizmos.DrawWireSphere(pigeon.transform.position, pigeonEatingRadius);
                }
            }
        }
    }
}

