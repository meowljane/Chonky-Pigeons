using UnityEngine;
using System.Collections.Generic;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 업그레이드 데이터 저장 클래스 (싱글톤)
    /// </summary>
    public class UpgradeData : MonoBehaviour
    {
        public static UpgradeData Instance { get; private set; }

        public event System.Action OnUpgradeChanged; // 업그레이드 변경 이벤트
        
        // 종별 가중치 변경량 (20% = 0.2, 곱셈 방식: 1.0 + 0.2 = 1.2배, 1.0 - 0.2 = 0.8배)
        public const float SPECIES_WEIGHT_CHANGE_AMOUNT = 0.2f;

        // 초기값 (스타팅 값)
        [Header("초기값 (스타팅 값)")]
        [SerializeField] private int baseMaxInventorySlots = 10; // 기본 인벤토리 최대 슬롯 수
        [SerializeField] private int baseMaxTrapCount = 2; // 기본 덫 설치 최대 개수
        [SerializeField] private int basePigeonsPerMap = 5; // 기본 맵당 비둘기 수

        // 1. 인벤토리 슬롯 증가
        [HideInInspector] [SerializeField] private int inventorySlotBonus = 0;
        
        // 2. 특정 비둘기 확률 증가/감소 (증가 슬롯과 감소 슬롯에 선택된 종만 저장)
        [Header("비둘기 확률 조정 (현재 선택된 종)")]
        [SerializeField, Tooltip("증가 슬롯에 선택된 종 (1.2배 적용)")] 
        private PigeonSpecies? selectedIncreaseSpecies = null;
        [SerializeField, Tooltip("감소 슬롯에 선택된 종 (0.8배 적용)")] 
        private PigeonSpecies? selectedDecreaseSpecies = null;
        
        // 3. 동시 덫 설치 개수 제한
        [SerializeField] private int maxTrapCount = 2; // 최종값 (기본값 + 업그레이드, 게임 시작 시 baseMaxTrapCount로 초기화됨)
        
        // 4. 맵당 비둘기 스폰 제한 증가
        [HideInInspector] [SerializeField] private int pigeonsPerMapUnlockedLevel = 0; // 해금된 레벨 (0=기본, 1=10까지, 2=15까지, 3=20까지)
        [SerializeField] private int pigeonsPerMapSelectedValue = 5; // 현재 선택된 값 (최종값, 게임 시작 시 basePigeonsPerMap으로 초기화됨)

        // 초기값 접근
        public int BaseMaxInventorySlots => baseMaxInventorySlots;
        public int BaseMaxTrapCount => baseMaxTrapCount;
        public int BasePigeonsPerMap => basePigeonsPerMap;

        // 보너스 접근
        public int InventorySlotBonus => inventorySlotBonus;
        public int MaxTrapCount => maxTrapCount;
        public int PigeonsPerMapUnlockedLevel => pigeonsPerMapUnlockedLevel;
        public int PigeonsPerMapSelectedValue => pigeonsPerMapSelectedValue;
        public PigeonSpecies? SelectedIncreaseSpecies => selectedIncreaseSpecies;
        public PigeonSpecies? SelectedDecreaseSpecies => selectedDecreaseSpecies;

        // 최종 값 (Inspector에서 확인용, 외부에서 사용)
        public int MaxInventorySlots => baseMaxInventorySlots + inventorySlotBonus;
        public int MaxPigeonsPerMap => pigeonsPerMapSelectedValue; // 선택된 값이 최종 값

        // Inspector에서 표시할 최종값 (읽기 전용)
        [Header("최종 값 (Inspector 확인용)")]
        [SerializeField, Tooltip("인벤토리 최대 슬롯 수 (기본값 + 보너스)")] 
        private int inspectorMaxInventorySlots = 10;
        
        [SerializeField, Tooltip("맵당 비둘기 스폰 수 (선택된 값)")] 
        private int inspectorMaxPigeonsPerMap = 5;
        
        [SerializeField, Tooltip("동시 덫 설치 최대 개수")] 
        private int inspectorMaxTrapCount = 2;

        /// <summary>
        /// Inspector 표시용 최종값 업데이트
        /// </summary>
        public void UpdateInspectorValues()
        {
            inspectorMaxInventorySlots = baseMaxInventorySlots + inventorySlotBonus;
            inspectorMaxPigeonsPerMap = pigeonsPerMapSelectedValue;
            inspectorMaxTrapCount = maxTrapCount;
        }


        /// <summary>
        /// 인벤토리 슬롯 증가 (보너스 추가)
        /// </summary>
        public void AddInventorySlotBonus(int bonus)
        {
            inventorySlotBonus += bonus;
            UpdateInspectorValues();
            OnUpgradeChanged?.Invoke();
        }

        /// <summary>
        /// 인벤토리 슬롯 보너스 설정
        /// </summary>
        public void SetInventorySlotBonus(int bonus)
        {
            inventorySlotBonus = bonus;
            UpdateInspectorValues();
            OnUpgradeChanged?.Invoke();
        }

        /// <summary>
        /// 증가 슬롯에 종 선택
        /// </summary>
        public void SetIncreaseSpecies(PigeonSpecies? species)
        {
            selectedIncreaseSpecies = species;
            OnUpgradeChanged?.Invoke();
        }

        /// <summary>
        /// 감소 슬롯에 종 선택
        /// </summary>
        public void SetDecreaseSpecies(PigeonSpecies? species)
        {
            selectedDecreaseSpecies = species;
            OnUpgradeChanged?.Invoke();
        }

        /// <summary>
        /// 특정 비둘기 확률 배율 가져오기 (SPECIES_WEIGHT_CHANGE_AMOUNT로 계산)
        /// </summary>
        public float GetSpeciesWeightMultiplier(PigeonSpecies species)
        {
            float multiplier = 1.0f;
            
            // 증가 슬롯에 선택된 종이면 1.2배
            if (selectedIncreaseSpecies.HasValue && selectedIncreaseSpecies.Value == species)
            {
                multiplier *= (1.0f + SPECIES_WEIGHT_CHANGE_AMOUNT); // 1.2배
            }
            
            // 감소 슬롯에 선택된 종이면 0.8배
            if (selectedDecreaseSpecies.HasValue && selectedDecreaseSpecies.Value == species)
            {
                multiplier *= (1.0f - SPECIES_WEIGHT_CHANGE_AMOUNT); // 0.8배
            }
            
            return multiplier;
        }

        /// <summary>
        /// 동시 덫 설치 개수 제한 설정
        /// </summary>
        public void SetMaxTrapCount(int count)
        {
            maxTrapCount = count;
            UpdateInspectorValues();
            OnUpgradeChanged?.Invoke();
        }


        /// <summary>
        /// 맵당 비둘기 스폰 해금 레벨 설정
        /// </summary>
        public void SetPigeonsPerMapUnlockedLevel(int level)
        {
            pigeonsPerMapUnlockedLevel = level;
            UpdateInspectorValues();
            OnUpgradeChanged?.Invoke();
        }

        /// <summary>
        /// 맵당 비둘기 스폰 선택된 값 설정
        /// </summary>
        public void SetPigeonsPerMapSelectedValue(int value)
        {
            pigeonsPerMapSelectedValue = value;
            UpdateInspectorValues();
            OnUpgradeChanged?.Invoke();
        }

        /// <summary>
        /// 초기화 (모든 업그레이드 값 리셋)
        /// </summary>
        public void Reset()
        {
            inventorySlotBonus = 0;
            
            // 종별 가중치 초기화 (증가/감소 슬롯 모두 해제)
            selectedIncreaseSpecies = null;
            selectedDecreaseSpecies = null;
            
            // 덫 설치 개수 초기화
            maxTrapCount = baseMaxTrapCount;
            
            // 맵당 비둘기 수 초기화
            pigeonsPerMapUnlockedLevel = 0;
            pigeonsPerMapSelectedValue = basePigeonsPerMap;
            
            UpdateInspectorValues();
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                
                // 게임 시작 시 모든 업그레이드 값 초기화 (이전 저장된 값 제거)
                Reset();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
