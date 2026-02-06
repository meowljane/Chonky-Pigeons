using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class FoodTrap : InteractableBase
    {
        private TrapType trapId;
        private TrapDefinition trapData;

        public void SetTrapIdAndFeedAmount(TrapType trapType, int feedAmount = 0)
        {
            trapId = trapType;
            trapData = GameDataRegistry.Instance?.Traps?.GetTrapById(trapId);
            if (trapData != null)
            {
                if (feedAmount > 0)
            {
                currentFeedAmount = Mathf.Max(1, feedAmount);
                initialFeedAmount = currentFeedAmount;
            }
                else
                {
                    currentFeedAmount = trapData.feedAmount;
                }
        }
        }
        private int currentFeedAmount;
        private int initialFeedAmount; 
        private List<PigeonAI> nearbyPigeons = new List<PigeonAI>();
        private Dictionary<PigeonAI, float> pigeonEatTimers = new Dictionary<PigeonAI, float>();
        private HashSet<PigeonAI> currentlyEatingPigeons = new HashSet<PigeonAI>(); 
        private Dictionary<PigeonAI, float> eatingStateTimers = new Dictionary<PigeonAI, float>(); 
        private Dictionary<PigeonAI, PigeonController> pigeonControllerCache = new Dictionary<PigeonAI, PigeonController>(); 
        private Dictionary<PigeonAI, PigeonMovement> pigeonMovementCache = new Dictionary<PigeonAI, PigeonMovement>();
        private List<PigeonAI> reusablePigeonsToRemoveList = new List<PigeonAI>(); 

        private PigeonInstanceStats capturedPigeonStats; 
        private bool isCaptured = false; 
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Captured Pigeon Overlay")]
        [SerializeField] private SpriteRenderer pigeonIconSpriteRenderer; 
        [SerializeField] private SpriteRenderer pigeonFaceIconSpriteRenderer; 

        public TrapType TrapId => trapId;
        public int CurrentFeedAmount => currentFeedAmount;
        public int MaxFeedAmount => initialFeedAmount > 0 ? initialFeedAmount : trapData?.feedAmount ?? 20;
        public bool HasCapturedPigeon => isCaptured && capturedPigeonStats != null;
        public PigeonInstanceStats CapturedPigeonStats => capturedPigeonStats;
        public event System.Action<PigeonAI> OnCaptured;

        public bool IsPigeonEating(PigeonAI pigeon)
        {
            return currentlyEatingPigeons.Contains(pigeon);
        }

        protected override void Start()
        {
            base.Start();

            if (trapData != null)
            {
                if (currentFeedAmount <= 0)
                    currentFeedAmount = trapData.feedAmount;
                if (initialFeedAmount <= 0)
                    initialFeedAmount = currentFeedAmount;

                if (trapData.icon != null)
                    spriteRenderer.sprite = trapData.icon;
            }
        }

        private void Update()
        {
            if (isCaptured)
                return;

            UpdateNearbyPigeons();

            UpdateCompetitionAlert();

            UpdateEatingStateTimers();

            nearbyPigeons.RemoveAll(p => p == null);

            foreach (var pigeon in nearbyPigeons)
            {
                if (!pigeon.CanEat())
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

        private void UpdateCompetitionAlert()
        {
            if (nearbyPigeons.Count <= 1)
                return;

            int competingCount = 0;
            foreach (var pigeon in nearbyPigeons)
            {
                if (pigeon.CanEat())
                    competingCount++;
            }

            if (competingCount <= 1)
                return;

            float deltaTime = Time.deltaTime;
            int competitorCount = competingCount - 1;

            foreach (var pigeon in nearbyPigeons)
            {
                if (pigeon.CanEat() && pigeon.CurrentState != PigeonState.Flee)
                pigeon.AddCrowdAlert(competitorCount, deltaTime);
            }
        }

        private void UpdateEatingStateTimers()
        {
            const float EATING_STATE_DURATION = 0.5f;

            currentlyEatingPigeons.RemoveWhere(p => p == null);
            reusablePigeonsToRemoveList.Clear();

            foreach (var pigeon in currentlyEatingPigeons)
            {
                if (!pigeon.CanEat() || pigeon.CurrentState == PigeonState.Flee)
                {
                    reusablePigeonsToRemoveList.Add(pigeon);
                    continue;
                }

                if (!eatingStateTimers.ContainsKey(pigeon))
                    eatingStateTimers[pigeon] = 0f;

                eatingStateTimers[pigeon] += Time.deltaTime;

                if (eatingStateTimers[pigeon] >= EATING_STATE_DURATION)
                    reusablePigeonsToRemoveList.Add(pigeon);
            }

            foreach (var pigeon in reusablePigeonsToRemoveList)
            {
                currentlyEatingPigeons.Remove(pigeon);
                eatingStateTimers.Remove(pigeon);
                pigeon.SetEating(false);
            }
        }

        private void UpdateNearbyPigeons()
        {
            nearbyPigeons.Clear();

            float searchRadius = interactionRadius + 0.5f;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, searchRadius);

            foreach (var col in colliders)
            {
                PigeonAI pigeon = col.GetComponent<PigeonAI>();
                if (pigeon == null)
                    continue;

                if (!pigeonMovementCache.TryGetValue(pigeon, out PigeonMovement movement))
                {
                    movement = pigeon.GetComponent<PigeonMovement>();
                    if (movement == null)
                        continue;
                    pigeonMovementCache[pigeon] = movement;
                }

                Vector2 toPigeon = (Vector2)(pigeon.transform.position - transform.position);
                float sqrDistance = toPigeon.sqrMagnitude;
                float eatingRadius = movement.GetEatingRadius();
                float sqrRadius = eatingRadius * eatingRadius;

                if (sqrDistance <= sqrRadius)
                    nearbyPigeons.Add(pigeon);
            }
        }

        private bool TryEat(PigeonAI pigeon)
        {
            if (isCaptured || !pigeon.CanEat())
                return false;

            if (!pigeonControllerCache.TryGetValue(pigeon, out PigeonController controller))
            {
                controller = pigeon.GetComponent<PigeonController>();
                if (controller == null)
                    return false;
                pigeonControllerCache[pigeon] = controller;
            }

            if (controller.Stats == null || Random.value > pigeon.GetEatChance())
                return false;

            currentlyEatingPigeons.Add(pigeon);
            pigeon.SetEating(true);
            eatingStateTimers[pigeon] = 0f;

            var stats = controller.Stats;
            int bitePower = stats.bitePower;
            currentFeedAmount -= bitePower;

            if (currentFeedAmount <= 0)
            {
                capturedPigeonStats = stats.Clone();
                isCaptured = true;
                
                if (trapData?.capturedSprite != null)
                    spriteRenderer.sprite = trapData.capturedSprite;

                var registry = GameDataRegistry.Instance;
                if (registry != null)
                {
                    UpdateSpriteRenderer(pigeonIconSpriteRenderer, 
                        registry.SpeciesSet?.GetSpeciesById(capturedPigeonStats.speciesId)?.icon ?? 
                        registry.SpeciesSet?.GetSpeciesById(PigeonSpecies.SP01)?.icon);

                    UpdateSpriteRenderer(pigeonFaceIconSpriteRenderer,
                        registry.Faces?.GetFaceById(capturedPigeonStats.faceId)?.icon ?? 
                        registry.Faces?.GetFaceById(FaceType.F00)?.icon);
                }

                OnCaptured?.Invoke(pigeon);
                pigeonControllerCache.Remove(pigeon);
                pigeonMovementCache.Remove(pigeon);
                Destroy(pigeon.gameObject);
            }

            return true;
        }


        private void UpdateSpriteRenderer(SpriteRenderer renderer, Sprite sprite)
        {
            if (sprite != null)
            {
                renderer.sprite = sprite;
                renderer.enabled = true;
                Color color = renderer.color;
                    color.a = 0.7f;
                renderer.color = color;
                }
                else
                {
                renderer.enabled = false;
            }
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);
        }

        protected override void OnTriggerExit2D(Collider2D other)
        {
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

