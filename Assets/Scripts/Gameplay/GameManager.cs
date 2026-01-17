using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 게임 전반의 상태를 관리하는 싱글톤 매니저
    /// 돈, 인벤토리, 덫 해금 상태 등을 관리
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private int startingMoney = 100;
        [SerializeField] private TrapType[] startingUnlockedTraps = { TrapType.BREAD }; // 시작 시 해금된 덫
        [SerializeField] private PigeonSpecies[] startingUnlockedSpecies = { };

        private int currentMoney;
        private HashSet<TrapType> unlockedTraps = new HashSet<TrapType>();
        private HashSet<PigeonSpecies> unlockedSpecies = new HashSet<PigeonSpecies>(); // 해금된 비둘기 종
        private List<PigeonInstanceStats> inventory = new List<PigeonInstanceStats>();
        private List<PigeonInstanceStats> exhibition = new List<PigeonInstanceStats>(); // 전시관

        public int CurrentMoney => currentMoney;
        public IReadOnlyList<PigeonInstanceStats> Inventory => inventory;
        public int InventoryCount => inventory.Count;
        public IReadOnlyList<PigeonInstanceStats> Exhibition => exhibition;
        public int ExhibitionCount => exhibition.Count;

        public event System.Action<int> OnMoneyChanged;
        public event System.Action<PigeonInstanceStats> OnPigeonAddedToInventory;
        public event System.Action<TrapType> OnTrapUnlocked;
        public event System.Action<PigeonSpecies> OnSpeciesUnlocked; // 종 해금 이벤트
        public event System.Action<PigeonInstanceStats> OnPigeonAddedToExhibition;
        public event System.Action<PigeonInstanceStats> OnPigeonRemovedFromExhibition;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            currentMoney = startingMoney;
            
            // 시작 덫 해금
            foreach (var trapType in startingUnlockedTraps)
            {
                unlockedTraps.Add(trapType);
                OnTrapUnlocked?.Invoke(trapType);
            }

            // 시작 비둘기 종 해금
            foreach (var speciesType in startingUnlockedSpecies)
            {
                unlockedSpecies.Add(speciesType);
                OnSpeciesUnlocked?.Invoke(speciesType);
            }

            OnMoneyChanged?.Invoke(currentMoney);
        }

        /// <summary>
        /// 돈 추가
        /// </summary>
        public void AddMoney(int amount)
        {
            if (amount < 0)
                return;

            currentMoney += amount;
            OnMoneyChanged?.Invoke(currentMoney);
        }

        /// <summary>
        /// 돈 차감 (구매 등)
        /// </summary>
        public bool SpendMoney(int amount)
        {
            if (amount < 0)
                return false;

            if (currentMoney < amount)
            {
                return false;
            }

            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }

        /// <summary>
        /// 비둘기를 인벤토리에 추가
        /// </summary>
        public void AddPigeonToInventory(PigeonInstanceStats stats)
        {
            if (stats == null)
                return;

            var clonedStats = stats.Clone();
            inventory.Add(clonedStats);
            OnPigeonAddedToInventory?.Invoke(clonedStats);
            EncyclopediaManager.Instance.RecordPigeon(clonedStats);
        }

        /// <summary>
        /// 비둘기 판매
        /// </summary>
        public bool SellPigeon(int index)
        {
            if (index < 0 || index >= inventory.Count)
                return false;

            var pigeon = inventory[index];
            int price = pigeon.price;
            
            inventory.RemoveAt(index);
            AddMoney(price);
            
            return true;
        }

        /// <summary>
        /// 모든 비둘기 판매
        /// </summary>
        public int SellAllPigeons()
        {
            int totalPrice = 0;
            foreach (var pigeon in inventory)
            {
                totalPrice += pigeon.price;
            }

            inventory.Clear();
            AddMoney(totalPrice);
            
            return totalPrice;
        }

        /// <summary>
        /// 덫이 해금되어 있는지 확인
        /// </summary>
        public bool IsTrapUnlocked(TrapType trapType)
        {
            return unlockedTraps.Contains(trapType);
        }

        /// <summary>
        /// 덫 구매/해금
        /// </summary>
        public bool UnlockTrap(TrapType trapType)
        {
            if (unlockedTraps.Contains(trapType))
                return false;

            var registry = GameDataRegistry.Instance;
            var trapData = registry.Traps.GetTrapById(trapType);

            if (!SpendMoney(trapData.unlockCost))
                return false;

            unlockedTraps.Add(trapType);
            OnTrapUnlocked?.Invoke(trapType);
            return true;
        }

        /// <summary>
        /// 비둘기 종이 해금되어 있는지 확인
        /// </summary>
        public bool IsSpeciesUnlocked(PigeonSpecies speciesType)
        {
            return unlockedSpecies.Contains(speciesType);
        }

        /// <summary>
        /// 비둘기 종 구매/해금
        /// </summary>
        public bool UnlockSpecies(PigeonSpecies speciesType)
        {
            if (unlockedSpecies.Contains(speciesType))
                return false;

            var registry = GameDataRegistry.Instance;
            var speciesData = registry.SpeciesSet.GetSpeciesById(speciesType);

            if (!SpendMoney(speciesData.unlockCost))
                return false;

            unlockedSpecies.Add(speciesType);
            OnSpeciesUnlocked?.Invoke(speciesType);
            return true;
        }

        /// <summary>
        /// 전시관에 비둘기 추가 (인벤토리에서)
        /// </summary>
        public bool AddPigeonToExhibition(int inventoryIndex)
        {
            if (inventoryIndex < 0 || inventoryIndex >= inventory.Count)
                return false;

            var clonedStats = inventory[inventoryIndex].Clone();
            exhibition.Add(clonedStats);
            inventory.RemoveAt(inventoryIndex);
            OnPigeonAddedToExhibition?.Invoke(clonedStats);
            return true;
        }

        /// <summary>
        /// 전시관에서 비둘기 제거 (인벤토리로)
        /// </summary>
        public bool RemovePigeonFromExhibition(int exhibitionIndex)
        {
            if (exhibitionIndex < 0 || exhibitionIndex >= exhibition.Count)
                return false;

            var pigeon = exhibition[exhibitionIndex];
            var clonedStats = pigeon.Clone();
            inventory.Add(clonedStats);
            exhibition.RemoveAt(exhibitionIndex);
            OnPigeonRemovedFromExhibition?.Invoke(pigeon);
            OnPigeonAddedToInventory?.Invoke(clonedStats);
            return true;
        }

        /// <summary>
        /// 덫 설치 비용 계산 (설치 비용 + 모이 비용)
        /// </summary>
        public int CalculateTrapInstallCost(TrapType trapType, int feedAmount)
        {
            var registry = GameDataRegistry.Instance;
            var trapData = registry.Traps.GetTrapById(trapType);

            int totalCost = trapData.installCost;

            // 모이 비용 (기본 20을 제외한 추가 모이)
            int additionalFeed = feedAmount - trapData.feedAmount;
            if (additionalFeed > 0)
            {
                totalCost += additionalFeed * trapData.feedCostPerUnit;
            }

            return totalCost;
        }

        /// <summary>
        /// 덫 설치 구매 (설치 + 모이) - 이미 해금된 덫만 설치 가능
        /// </summary>
        public bool PurchaseTrapInstallation(TrapType trapType, int feedAmount)
        {
            if (!IsTrapUnlocked(trapType))
                return false;

            int totalCost = CalculateTrapInstallCost(trapType, feedAmount);

            if (currentMoney < totalCost)
                return false;

            SpendMoney(totalCost);
            return true;
        }
    }
}

