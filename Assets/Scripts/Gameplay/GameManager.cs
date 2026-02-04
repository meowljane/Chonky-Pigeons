using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Save;
using PigeonGame.UI;

namespace PigeonGame.Gameplay
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private int startingMoney = 100;
        [SerializeField] private TrapType[] startingUnlockedTraps = { TrapType.BREAD }; 
        [SerializeField] private PigeonSpecies[] startingUnlockedSpecies = { };

        private int currentMoney;
        private HashSet<TrapType> unlockedTraps = new HashSet<TrapType>();
        private HashSet<PigeonSpecies> unlockedSpecies = new HashSet<PigeonSpecies>(); 
        private HashSet<DoorType> unlockedDoors = new HashSet<DoorType>(); 
        private List<PigeonInstanceStats> inventory = new List<PigeonInstanceStats>();
        private List<PigeonInstanceStats> exhibition = new List<PigeonInstanceStats>(); 

        public int CurrentMoney => currentMoney;
        public int MaxInventorySlots => UpgradeData.Instance?.MaxInventorySlots ?? 10;
        public int MaxPigeonsPerMap => UpgradeData.Instance?.MaxPigeonsPerMap ?? 5;
        public IReadOnlyList<PigeonInstanceStats> Inventory => inventory;
        public int InventoryCount => inventory.Count;
        public IReadOnlyList<PigeonInstanceStats> Exhibition => exhibition;
        public int ExhibitionCount => exhibition.Count;

        public event System.Action<int> OnMoneyChanged;
        public event System.Action<PigeonInstanceStats> OnPigeonAddedToInventory;
        public event System.Action<TrapType> OnTrapUnlocked;
        public event System.Action<PigeonSpecies> OnSpeciesUnlocked; 
        public event System.Action<PigeonInstanceStats> OnPigeonAddedToExhibition;
        public event System.Action<PigeonInstanceStats> OnPigeonRemovedFromExhibition;
        public event System.Action<DoorType> OnDoorUnlocked; 

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
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

            foreach (var trapType in startingUnlockedTraps)
            {
                unlockedTraps.Add(trapType);
                OnTrapUnlocked?.Invoke(trapType);
            }

            foreach (var speciesType in startingUnlockedSpecies)
            {
                unlockedSpecies.Add(speciesType);
                OnSpeciesUnlocked?.Invoke(speciesType);
            }

            OnMoneyChanged?.Invoke(currentMoney);
        }

        public void Reset()
        {
            currentMoney = startingMoney;

            unlockedTraps.Clear();
            unlockedSpecies.Clear();
            unlockedDoors.Clear();

            foreach (var trapType in startingUnlockedTraps)
            {
                unlockedTraps.Add(trapType);
                OnTrapUnlocked?.Invoke(trapType);
            }

            foreach (var speciesType in startingUnlockedSpecies)
            {
                unlockedSpecies.Add(speciesType);
                OnSpeciesUnlocked?.Invoke(speciesType);
            }

            inventory.Clear();
            exhibition.Clear();

            OnMoneyChanged?.Invoke(currentMoney);
        }

        public void AddMoney(int amount)
        {
            if (amount < 0)
                return;

            currentMoney += amount;
            OnMoneyChanged?.Invoke(currentMoney);
        }

        public bool SpendMoney(int amount)
        {
            if (amount < 0)
                return false;

            if (currentMoney < amount)
            {
                ToastNotificationManager.ShowWarning("골드가 부족합니다!");
                return false;
            }

            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            return true;
        }

        public void AddPigeonToInventory(PigeonInstanceStats stats)
        {
            if (stats == null)
                return;

            if (inventory.Count >= MaxInventorySlots)
            {
                ToastNotificationManager.ShowWarning("인벤토리가 가득 찼습니다!");
                return;
            }

            var clonedStats = stats.Clone();
            inventory.Add(clonedStats);
            OnPigeonAddedToInventory?.Invoke(clonedStats);
            EncyclopediaManager.Instance.RecordPigeon(clonedStats);
            ToastNotificationManager.ShowSuccess("포획 성공!");
        }

        public bool SellPigeon(int index)
        {
            if (index < 0 || index >= inventory.Count)
                return false;

            var pigeon = inventory[index];
            int price = pigeon.price;

            inventory.RemoveAt(index);
            AddMoney(price);
            ToastNotificationManager.ShowSuccess("판매 완료!");

            return true;
        }

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

        public bool IsTrapUnlocked(TrapType trapType)
        {
            return unlockedTraps.Contains(trapType);
        }

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
            ToastNotificationManager.ShowSuccess("해금 성공!");
            return true;
        }

        public bool IsSpeciesUnlocked(PigeonSpecies speciesType)
        {
            return unlockedSpecies.Contains(speciesType);
        }

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
            ToastNotificationManager.ShowSuccess("해금 성공!");
            return true;
        }

        public bool AddPigeonToExhibition(int inventoryIndex)
        {
            if (inventoryIndex < 0 || inventoryIndex >= inventory.Count)
                return false;

            const int MAX_EXHIBITION_SLOTS = 50;
            if (exhibition.Count >= MAX_EXHIBITION_SLOTS)
            {
                ToastNotificationManager.ShowWarning("전시관이 가득 찼습니다!");
                return false;
            }

            var clonedStats = inventory[inventoryIndex].Clone();
            exhibition.Add(clonedStats);
            inventory.RemoveAt(inventoryIndex);
            OnPigeonAddedToExhibition?.Invoke(clonedStats);
            ToastNotificationManager.ShowSuccess("전시 완료!");
            return true;
        }

        public bool RemovePigeonFromExhibition(int exhibitionIndex)
        {
            if (exhibitionIndex < 0 || exhibitionIndex >= exhibition.Count)
                return false;

            if (inventory.Count >= MaxInventorySlots)
            {
                ToastNotificationManager.ShowWarning("인벤토리가 가득 찼습니다!");
                return false;
            }

            var pigeon = exhibition[exhibitionIndex];
            var clonedStats = pigeon.Clone();
            inventory.Add(clonedStats);
            exhibition.RemoveAt(exhibitionIndex);
            OnPigeonRemovedFromExhibition?.Invoke(pigeon);
            OnPigeonAddedToInventory?.Invoke(clonedStats);
            ToastNotificationManager.ShowSuccess("꺼내기 완료!");
            return true;
        }

        public int CalculateTrapInstallCost(TrapType trapType, int feedAmount)
        {
            var registry = GameDataRegistry.Instance;
            var trapData = registry.Traps.GetTrapById(trapType);

            int totalCost = trapData.installCost;

            int additionalFeed = feedAmount - trapData.feedAmount;
            if (additionalFeed > 0)
            {
                totalCost += additionalFeed * trapData.feedCostPerUnit;
            }

            return totalCost;
        }

        public bool PurchaseTrapInstallation(TrapType trapType, int feedAmount)
        {
            if (!IsTrapUnlocked(trapType))
                return false;

            int totalCost = CalculateTrapInstallCost(trapType, feedAmount);

            if (currentMoney < totalCost)
            {
                ToastNotificationManager.ShowWarning("골드가 부족합니다!");
                return false;
            }

            SpendMoney(totalCost);
            return true;
        }

        public bool IsDoorUnlocked(DoorType doorType)
        {
            return unlockedDoors.Contains(doorType);
        }

        public bool UnlockDoor(DoorType doorType, int cost)
        {
            if (unlockedDoors.Contains(doorType))
                return false;

            if (!SpendMoney(cost))
                return false;

            unlockedDoors.Add(doorType);
            OnDoorUnlocked?.Invoke(doorType);

            ToastNotificationManager.ShowSuccess("문 해금 성공!");
            return true;
        }

        public GameManagerSaveData CreateSaveData()
        {
            var data = new GameManagerSaveData
            {
                currentMoney = currentMoney
            };

            data.unlockedTraps.AddRange(unlockedTraps);
            data.unlockedSpecies.AddRange(unlockedSpecies);
            data.unlockedDoors.AddRange(unlockedDoors);

            foreach (var pigeon in inventory)
            {
                if (pigeon == null) continue;
                data.inventory.Add(new PigeonInstanceSaveData
                {
                    speciesId = pigeon.speciesId,
                    faceId = pigeon.faceId,
                    weight = pigeon.weight
                });
            }

            foreach (var pigeon in exhibition)
            {
                if (pigeon == null) continue;
                data.exhibition.Add(new PigeonInstanceSaveData
                {
                    speciesId = pigeon.speciesId,
                    faceId = pigeon.faceId,
                    weight = pigeon.weight
                });
            }

            return data;
        }

        public void ApplySaveData(GameManagerSaveData data)
        {
            if (data == null)
                return;

            currentMoney = data.currentMoney;
            OnMoneyChanged?.Invoke(currentMoney);

            unlockedTraps.Clear();
            unlockedSpecies.Clear();
            unlockedDoors.Clear();

            foreach (var trap in data.unlockedTraps)
            {
                unlockedTraps.Add(trap);
                OnTrapUnlocked?.Invoke(trap);
            }

            foreach (var species in data.unlockedSpecies)
            {
                unlockedSpecies.Add(species);
                OnSpeciesUnlocked?.Invoke(species);
            }

            foreach (var door in data.unlockedDoors)
            {
                unlockedDoors.Add(door);
                OnDoorUnlocked?.Invoke(door);
            }

            inventory.Clear();
            exhibition.Clear();

            foreach (var entry in data.inventory)
            {
                if (entry == null) continue;

                int obesity = Mathf.RoundToInt(entry.weight);
                var stats = PigeonInstanceFactory.CreateInstanceStats(entry.speciesId, obesity, entry.weight, entry.faceId);
                if (stats == null) continue;

                inventory.Add(stats);
                OnPigeonAddedToInventory?.Invoke(stats);
            }

            foreach (var entry in data.exhibition)
            {
                if (entry == null) continue;

                int obesity = Mathf.RoundToInt(entry.weight);
                var stats = PigeonInstanceFactory.CreateInstanceStats(entry.speciesId, obesity, entry.weight, entry.faceId);
                if (stats == null) continue;

                exhibition.Add(stats);
                OnPigeonAddedToExhibition?.Invoke(stats);
            }
        }

    }
}

