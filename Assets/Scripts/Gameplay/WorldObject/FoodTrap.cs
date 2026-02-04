using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class FoodTrap : InteractableBase
    {
        [SerializeField] private TrapType trapId;
        private TrapDefinition trapData;

        public void SetTrapId(TrapType trapType)
        {
            trapId = trapType;
            LoadTrapData();
            if (trapData != null)
            {
                currentFeedAmount = trapData.feedAmount;
            }
        }

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
        private SpriteRenderer spriteRenderer;

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

                if (trapData.icon != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = trapData.icon;
                }
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

            float deltaTime = Time.deltaTime;

            int competingCount = 0;
            foreach (var pigeon in nearbyPigeons)
            {
                if (pigeon != null && pigeon.CanEat())
                {
                    competingCount++;
                }
            }

            if (competingCount <= 1)
                return;

            int competitorCount = competingCount - 1;
            foreach (var pigeon in nearbyPigeons)
            {
                if (pigeon == null || !pigeon.CanEat() || pigeon.CurrentState == PigeonState.Flee)
                    continue;

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
                if (pigeon == null)
                {
                    reusablePigeonsToRemoveList.Add(pigeon);
                    continue;
                }

                if (!pigeon.CanEat() || pigeon.CurrentState == PigeonState.Flee)
                {
                    reusablePigeonsToRemoveList.Add(pigeon);
                    continue;
                }

                if (!eatingStateTimers.ContainsKey(pigeon))
                {
                    eatingStateTimers[pigeon] = 0f;
                }

                eatingStateTimers[pigeon] += Time.deltaTime;

                if (eatingStateTimers[pigeon] >= EATING_STATE_DURATION)
                {
                    reusablePigeonsToRemoveList.Add(pigeon);
                }
            }

            foreach (var pigeon in reusablePigeonsToRemoveList)
            {
                currentlyEatingPigeons.Remove(pigeon);
                eatingStateTimers.Remove(pigeon);
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
                float pigeonEatingRadius = movement.GetEatingRadius();
                float sqrRadius = pigeonEatingRadius * pigeonEatingRadius;

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

            if (!pigeonControllerCache.TryGetValue(pigeon, out PigeonController controller))
            {
                controller = pigeon.GetComponent<PigeonController>();
                if (controller == null || controller.Stats == null)
                    return false;
                pigeonControllerCache[pigeon] = controller;
            }

            if (controller.Stats == null)
                return false;

            var stats = controller.Stats;

            if (Random.value > pigeon.GetEatChance())
                return false;

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
                pigeonControllerCache.Remove(pigeon);
                pigeonMovementCache.Remove(pigeon);
                Destroy(pigeon.gameObject);
                return true;
            }

            return true;
        }

        private void ChangeToCapturedState()
        {
            if (trapData != null && trapData.capturedSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = trapData.capturedSprite;
            }

            ShowCapturedPigeonOverlay();
        }

        private void ShowCapturedPigeonOverlay()
        {
            if (capturedPigeonStats == null)
                return;

            var registry = GameDataRegistry.Instance;
            if (registry == null)
                return;

            if (pigeonIconSpriteRenderer != null)
            {
                var species = registry.SpeciesSet?.GetSpeciesById(capturedPigeonStats.speciesId);
                var defaultSpecies = registry.SpeciesSet?.GetSpeciesById(PigeonSpecies.SP01);
                var iconToUse = species?.icon ?? defaultSpecies?.icon;

                if (iconToUse != null)
                {
                    pigeonIconSpriteRenderer.sprite = iconToUse;
                    pigeonIconSpriteRenderer.enabled = true;

                    Color color = pigeonIconSpriteRenderer.color;
                    color.a = 0.7f;
                    pigeonIconSpriteRenderer.color = color;
                }
                else
                {
                    pigeonIconSpriteRenderer.enabled = false;
                }
            }

            if (pigeonFaceIconSpriteRenderer != null)
            {
                var face = registry.Faces?.GetFaceById(capturedPigeonStats.faceId);
                var defaultFace = registry.Faces?.GetFaceById(FaceType.F00);
                var faceIconToUse = face?.icon ?? defaultFace?.icon;

                if (faceIconToUse != null)
                {
                    pigeonFaceIconSpriteRenderer.sprite = faceIconToUse;
                    pigeonFaceIconSpriteRenderer.enabled = true;

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

