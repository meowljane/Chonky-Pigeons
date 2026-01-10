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

        public int CurrentMoney => currentMoney;
        public IReadOnlyList<PigeonInstanceStats> Inventory => inventory;
        public int InventoryCount => inventory.Count;

        public event System.Action<int> OnMoneyChanged;
        public event System.Action<PigeonInstanceStats> OnPigeonAddedToInventory;
        public event System.Action<string> OnTrapUnlocked;

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

            Debug.Log($"GameManager 초기화: 시작 돈 {currentMoney}, 해금된 덫 {unlockedTraps.Count}개");
        }

        /// <summary>
        /// 돈 추가
        /// </summary>
        public void AddMoney(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"음수 금액 추가 시도: {amount}");
                return;
            }

            currentMoney += amount;
            OnMoneyChanged?.Invoke(currentMoney);
            Debug.Log($"돈 추가: +{amount} (현재: {currentMoney})");
        }

        /// <summary>
        /// 돈 차감 (구매 등)
        /// </summary>
        public bool SpendMoney(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"음수 금액 차감 시도: {amount}");
                return false;
            }

            if (currentMoney < amount)
            {
                Debug.Log($"돈 부족: 필요 {amount}, 현재 {currentMoney}");
                return false;
            }

            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            Debug.Log($"돈 차감: -{amount} (현재: {currentMoney})");
            return true;
        }

        /// <summary>
        /// 비둘기를 인벤토리에 추가
        /// </summary>
        public void AddPigeonToInventory(PigeonInstanceStats stats)
        {
            if (stats == null)
            {
                Debug.LogWarning("null 비둘기를 인벤토리에 추가하려고 시도했습니다.");
                return;
            }

            // 복사본을 저장하여 원본이 삭제되어도 안전하게 보관
            var clonedStats = stats.Clone();
            inventory.Add(clonedStats);
            OnPigeonAddedToInventory?.Invoke(clonedStats);
            
            // 도감에 기록
            if (EncyclopediaManager.Instance != null)
            {
                EncyclopediaManager.Instance.RecordPigeon(clonedStats);
            }
            
            Debug.Log($"인벤토리에 추가: {clonedStats.speciesId} (가격: {clonedStats.price})");
        }

        /// <summary>
        /// 비둘기 판매
        /// </summary>
        public bool SellPigeon(int index)
        {
            if (index < 0 || index >= inventory.Count)
            {
                Debug.LogWarning($"잘못된 인벤토리 인덱스: {index}");
                return false;
            }

            var pigeon = inventory[index];
            int price = pigeon.price;
            
            inventory.RemoveAt(index);
            AddMoney(price);
            
            Debug.Log($"비둘기 판매: {pigeon.speciesId} (가격: {price})");
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
            
            Debug.Log($"모든 비둘기 판매: 총 {totalPrice}");
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
                Debug.Log($"이미 해금된 덫: {trapId}");
                return false;
            }

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
            {
                Debug.LogError("GameDataRegistry를 찾을 수 없습니다!");
                return false;
            }

            var trapData = registry.Traps.GetTrapById(trapId);
            if (trapData == null)
            {
                Debug.LogError($"덫을 찾을 수 없습니다: {trapId}");
                return false;
            }

            // 돈 차감
            if (!SpendMoney(trapData.cost))
            {
                return false;
            }

            // 해금
            unlockedTraps.Add(trapId);
            OnTrapUnlocked?.Invoke(trapId);
            Debug.Log($"덫 해금: {trapData.name} ({trapId})");
            return true;
        }

        /// <summary>
        /// 해금된 덫 목록 가져오기
        /// </summary>
        public HashSet<string> GetUnlockedTraps()
        {
            return new HashSet<string>(unlockedTraps);
        }
    }
}

