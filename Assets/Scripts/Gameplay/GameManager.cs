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
        [SerializeField] private string[] startingUnlockedTraps = { "BREAD" }; // 시작 시 해금된 덫

        private int currentMoney;
        private HashSet<string> unlockedTraps = new HashSet<string>();
        private List<PigeonInstanceStats> inventory = new List<PigeonInstanceStats>();
        private List<PigeonInstanceStats> exhibition = new List<PigeonInstanceStats>(); // 전시관

        public int CurrentMoney => currentMoney;
        public IReadOnlyList<PigeonInstanceStats> Inventory => inventory;
        public int InventoryCount => inventory.Count;
        public IReadOnlyList<PigeonInstanceStats> Exhibition => exhibition;
        public int ExhibitionCount => exhibition.Count;

        public event System.Action<int> OnMoneyChanged;
        public event System.Action<PigeonInstanceStats> OnPigeonAddedToInventory;
        public event System.Action<string> OnTrapUnlocked;
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
            foreach (var trapId in startingUnlockedTraps)
            {
                unlockedTraps.Add(trapId);
            }

            // 초기 돈 값 이벤트 발생 (UI 업데이트용)
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

            // 복사본을 저장하여 원본이 삭제되어도 안전하게 보관
            var clonedStats = stats.Clone();
            inventory.Add(clonedStats);
            OnPigeonAddedToInventory?.Invoke(clonedStats);
            
            // 도감에 기록
            if (EncyclopediaManager.Instance != null)
            {
                EncyclopediaManager.Instance.RecordPigeon(clonedStats);
            }
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
        public bool IsTrapUnlocked(string trapId)
        {
            return unlockedTraps.Contains(trapId);
        }

        /// <summary>
        /// 덫 구매/해금
        /// </summary>
        public bool UnlockTrap(string trapId)
        {
            if (unlockedTraps.Contains(trapId))
            {
                return false;
            }

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return false;

            var trapData = registry.Traps.GetTrapById(trapId);
            if (trapData == null)
                return false;

            // 돈 차감
            if (!SpendMoney(trapData.unlockCost))
            {
                return false;
            }

            // 해금
            unlockedTraps.Add(trapId);
            OnTrapUnlocked?.Invoke(trapId);
            return true;
        }

        /// <summary>
        /// 해금된 덫 목록 가져오기
        /// </summary>
        public HashSet<string> GetUnlockedTraps()
        {
            return new HashSet<string>(unlockedTraps);
        }

        /// <summary>
        /// 전시관에 비둘기 추가 (인벤토리에서)
        /// </summary>
        public bool AddPigeonToExhibition(int inventoryIndex)
        {
            if (inventoryIndex < 0 || inventoryIndex >= inventory.Count)
                return false;

            var pigeon = inventory[inventoryIndex];
            if (pigeon == null)
                return false;

            // 전시관에 추가
            var clonedStats = pigeon.Clone();
            exhibition.Add(clonedStats);

            // 인벤토리에서 제거
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
            if (pigeon == null)
                return false;

            // 인벤토리에 추가
            var clonedStats = pigeon.Clone();
            inventory.Add(clonedStats);

            // 전시관에서 제거
            exhibition.RemoveAt(exhibitionIndex);

            OnPigeonRemovedFromExhibition?.Invoke(pigeon);
            OnPigeonAddedToInventory?.Invoke(clonedStats);
            return true;
        }

        /// <summary>
        /// 덫 설치 비용 계산 (설치 비용 + 모이 비용)
        /// </summary>
        public int CalculateTrapInstallCost(string trapId, int feedAmount)
        {
            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return 0;

            var trapData = registry.Traps.GetTrapById(trapId);
            if (trapData == null)
                return 0;

            int totalCost = 0;

            // 설치 비용
            totalCost += trapData.installCost;

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
        public bool PurchaseTrapInstallation(string trapId, int feedAmount)
        {
            // 해금되지 않은 덫은 설치 불가
            if (!IsTrapUnlocked(trapId))
                return false;

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return false;

            var trapData = registry.Traps.GetTrapById(trapId);
            if (trapData == null)
                return false;

            // 설치 비용과 모이 비용만 계산
            int totalCost = CalculateTrapInstallCost(trapId, feedAmount);

            // 돈이 부족하면 실패
            if (currentMoney < totalCost)
                return false;

            // 돈 차감
            SpendMoney(totalCost);
            return true;
        }
    }
}

