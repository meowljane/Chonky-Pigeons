using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Save;

namespace PigeonGame.Gameplay
{
    public class UpgradeData : MonoBehaviour
    {
        public static UpgradeData Instance { get; private set; }

        public event System.Action OnUpgradeChanged; 

        public const float SPECIES_WEIGHT_CHANGE_AMOUNT = 0.2f;

        [Header("초기값 (스타팅 값)")]
        [SerializeField] private int baseMaxInventorySlots = 10; 
        [SerializeField] private int baseMaxTrapCount = 2; 
        [SerializeField] private int basePigeonsPerMap = 5; 

        [HideInInspector] [SerializeField] private int inventorySlotBonus = 0;

        [Header("비둘기 확률 조정 (현재 선택된 종)")]
        [SerializeField, Tooltip("증가 슬롯에 선택된 종 (1.2배 적용)")] 
        private PigeonSpecies? selectedIncreaseSpecies = null;
        [SerializeField, Tooltip("감소 슬롯에 선택된 종 (0.8배 적용)")] 
        private PigeonSpecies? selectedDecreaseSpecies = null;

        [SerializeField] private int maxTrapCount = 2; 

        [HideInInspector] [SerializeField] private int pigeonsPerMapUnlockedLevel = 0; 
        [SerializeField] private int pigeonsPerMapSelectedValue = 5; 

        public int BaseMaxInventorySlots => baseMaxInventorySlots;
        public int BaseMaxTrapCount => baseMaxTrapCount;
        public int BasePigeonsPerMap => basePigeonsPerMap;

        public int InventorySlotBonus => inventorySlotBonus;
        public int MaxTrapCount => maxTrapCount;
        public int PigeonsPerMapUnlockedLevel => pigeonsPerMapUnlockedLevel;
        public int PigeonsPerMapSelectedValue => pigeonsPerMapSelectedValue;
        public PigeonSpecies? SelectedIncreaseSpecies => selectedIncreaseSpecies;
        public PigeonSpecies? SelectedDecreaseSpecies => selectedDecreaseSpecies;

        public int MaxInventorySlots => baseMaxInventorySlots + inventorySlotBonus;
        public int MaxPigeonsPerMap => pigeonsPerMapSelectedValue; 

        [Header("최종 값 (Inspector 확인용)")]
        [SerializeField, Tooltip("인벤토리 최대 슬롯 수 (기본값 + 보너스)")] 
        private int inspectorMaxInventorySlots = 10;

        [SerializeField, Tooltip("맵당 비둘기 스폰 수 (선택된 값)")] 
        private int inspectorMaxPigeonsPerMap = 5;

        [SerializeField, Tooltip("동시 덫 설치 최대 개수")] 
        private int inspectorMaxTrapCount = 2;

        public void UpdateInspectorValues()
        {
            inspectorMaxInventorySlots = baseMaxInventorySlots + inventorySlotBonus;
            inspectorMaxPigeonsPerMap = pigeonsPerMapSelectedValue;
            inspectorMaxTrapCount = maxTrapCount;
        }

        private void NotifyUpgradeChanged()
        {
            UpdateInspectorValues();
            OnUpgradeChanged?.Invoke();
        }

        public void AddInventorySlotBonus(int bonus)
        {
            inventorySlotBonus += bonus;
            NotifyUpgradeChanged();
        }

        public void SetInventorySlotBonus(int bonus)
        {
            inventorySlotBonus = bonus;
            NotifyUpgradeChanged();
        }

        public void SetIncreaseSpecies(PigeonSpecies? species)
        {
            selectedIncreaseSpecies = species;
            OnUpgradeChanged?.Invoke();
        }

        public void SetDecreaseSpecies(PigeonSpecies? species)
        {
            selectedDecreaseSpecies = species;
            OnUpgradeChanged?.Invoke();
        }

        public float GetSpeciesWeightMultiplier(PigeonSpecies species)
        {
            float multiplier = 1.0f;

            if (selectedIncreaseSpecies.HasValue && selectedIncreaseSpecies.Value == species)
            {
                multiplier *= (1.0f + SPECIES_WEIGHT_CHANGE_AMOUNT); 
            }

            if (selectedDecreaseSpecies.HasValue && selectedDecreaseSpecies.Value == species)
            {
                multiplier *= (1.0f - SPECIES_WEIGHT_CHANGE_AMOUNT); 
            }

            return multiplier;
        }

        public void SetMaxTrapCount(int count)
        {
            maxTrapCount = count;
            NotifyUpgradeChanged();
        }

        public void SetPigeonsPerMapUnlockedLevel(int level)
        {
            pigeonsPerMapUnlockedLevel = level;
            NotifyUpgradeChanged();
        }

        public void SetPigeonsPerMapSelectedValue(int value)
        {
            pigeonsPerMapSelectedValue = value;
            NotifyUpgradeChanged();
        }

        public void Reset()
        {
            inventorySlotBonus = 0;

            selectedIncreaseSpecies = null;
            selectedDecreaseSpecies = null;

            maxTrapCount = baseMaxTrapCount;

            pigeonsPerMapUnlockedLevel = 0;
            pigeonsPerMapSelectedValue = basePigeonsPerMap;

            UpdateInspectorValues();
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                Reset();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public UpgradeSaveData CreateSaveData()
        {
            var data = new UpgradeSaveData
            {
                inventorySlotBonus = inventorySlotBonus,
                maxTrapCount = maxTrapCount,
                pigeonsPerMapUnlockedLevel = pigeonsPerMapUnlockedLevel,
                pigeonsPerMapSelectedValue = pigeonsPerMapSelectedValue,
                hasIncreaseSpecies = selectedIncreaseSpecies.HasValue,
                hasDecreaseSpecies = selectedDecreaseSpecies.HasValue
            };

            if (selectedIncreaseSpecies.HasValue)
            {
                data.increaseSpecies = selectedIncreaseSpecies.Value;
            }

            if (selectedDecreaseSpecies.HasValue)
            {
                data.decreaseSpecies = selectedDecreaseSpecies.Value;
            }

            return data;
        }

        public void ApplySaveData(UpgradeSaveData data)
        {
            if (data == null)
                return;

            inventorySlotBonus = data.inventorySlotBonus;
            maxTrapCount = data.maxTrapCount;
            pigeonsPerMapUnlockedLevel = data.pigeonsPerMapUnlockedLevel;
            pigeonsPerMapSelectedValue = data.pigeonsPerMapSelectedValue;

            selectedIncreaseSpecies = data.hasIncreaseSpecies ? (PigeonSpecies?)data.increaseSpecies : null;
            selectedDecreaseSpecies = data.hasDecreaseSpecies ? (PigeonSpecies?)data.decreaseSpecies : null;

            UpdateInspectorValues();
            OnUpgradeChanged?.Invoke();
        }
    }
}
