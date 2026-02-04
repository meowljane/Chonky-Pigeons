using UnityEngine;
using System.Collections;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 씬 시작 시 세이브 데이터 로드 및 적용 담당
    /// 중요한 이벤트 발생 시 자동 저장
    /// (씬에 한 개 배치해서 사용)
    /// </summary>
    public class SaveInitializer : MonoBehaviour
    {
        [Header("Auto Save Settings")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float saveCooldown = 0.5f; // 중복 저장 방지용 쿨다운 (초)

        private float lastSaveTime = 0f;
        private bool isInitialized = false;

        private void Start()
        {
            // 모든 매니저들의 Awake가 끝난 뒤에 호출됨
            SaveManager.LoadOrCreateAndApply();
            
            // 자동 저장 이벤트 구독
            if (enableAutoSave)
            {
                SubscribeToSaveEvents();
            }
            
            isInitialized = true;
        }

        private void SubscribeToSaveEvents()
        {
            // GameManager 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged += OnGameStateChanged;
                GameManager.Instance.OnPigeonAddedToInventory += OnPigeonAddedToInventory;
                GameManager.Instance.OnTrapUnlocked += OnTrapUnlocked;
                GameManager.Instance.OnSpeciesUnlocked += OnSpeciesUnlocked;
                GameManager.Instance.OnDoorUnlocked += OnDoorUnlocked;
                GameManager.Instance.OnPigeonAddedToExhibition += OnPigeonAddedToExhibition;
                GameManager.Instance.OnPigeonRemovedFromExhibition += OnPigeonRemovedFromExhibition;
            }
            else
            {
                // GameManager가 아직 초기화되지 않았을 수 있으므로 코루틴으로 재시도
                StartCoroutine(WaitForGameManagerAndSubscribe());
            }

            // UpgradeData 이벤트 구독
            if (UpgradeData.Instance != null)
            {
                UpgradeData.Instance.OnUpgradeChanged += OnUpgradeChanged;
            }
            else
            {
                StartCoroutine(WaitForUpgradeDataAndSubscribe());
            }
        }

        private IEnumerator WaitForGameManagerAndSubscribe()
        {
            while (GameManager.Instance == null)
            {
                yield return null;
            }

            GameManager.Instance.OnMoneyChanged += OnGameStateChanged;
            GameManager.Instance.OnPigeonAddedToInventory += OnPigeonAddedToInventory;
            GameManager.Instance.OnTrapUnlocked += OnTrapUnlocked;
            GameManager.Instance.OnSpeciesUnlocked += OnSpeciesUnlocked;
            GameManager.Instance.OnDoorUnlocked += OnDoorUnlocked;
            GameManager.Instance.OnPigeonAddedToExhibition += OnPigeonAddedToExhibition;
            GameManager.Instance.OnPigeonRemovedFromExhibition += OnPigeonRemovedFromExhibition;
        }

        private IEnumerator WaitForUpgradeDataAndSubscribe()
        {
            while (UpgradeData.Instance == null)
            {
                yield return null;
            }

            UpgradeData.Instance.OnUpgradeChanged += OnUpgradeChanged;
        }

        /// <summary>
        /// 게임 상태 변경 시 자동 저장 (쿨다운 적용)
        /// </summary>
        private void OnGameStateChanged(int money)
        {
            TriggerAutoSave();
        }

        private void OnPigeonAddedToInventory(PigeonInstanceStats stats)
        {
            TriggerAutoSave();
        }

        private void OnTrapUnlocked(TrapType trapType)
        {
            TriggerAutoSave();
        }

        private void OnSpeciesUnlocked(PigeonSpecies species)
        {
            TriggerAutoSave();
        }

        private void OnDoorUnlocked(DoorType doorType)
        {
            TriggerAutoSave();
        }

        private void OnPigeonAddedToExhibition(PigeonInstanceStats stats)
        {
            TriggerAutoSave();
        }

        private void OnPigeonRemovedFromExhibition(PigeonInstanceStats stats)
        {
            TriggerAutoSave();
        }

        private void OnUpgradeChanged()
        {
            TriggerAutoSave();
        }

        private void TriggerAutoSave()
        {
            // 초기화 전에는 저장하지 않음 (로드 중 이벤트 발생 방지)
            if (!isInitialized)
                return;

            // 쿨다운 체크 (너무 자주 저장하는 것 방지)
            if (Time.time - lastSaveTime < saveCooldown)
                return;

            SaveManager.SaveGame();
            lastSaveTime = Time.time;
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged -= OnGameStateChanged;
                GameManager.Instance.OnPigeonAddedToInventory -= OnPigeonAddedToInventory;
                GameManager.Instance.OnTrapUnlocked -= OnTrapUnlocked;
                GameManager.Instance.OnSpeciesUnlocked -= OnSpeciesUnlocked;
                GameManager.Instance.OnDoorUnlocked -= OnDoorUnlocked;
                GameManager.Instance.OnPigeonAddedToExhibition -= OnPigeonAddedToExhibition;
                GameManager.Instance.OnPigeonRemovedFromExhibition -= OnPigeonRemovedFromExhibition;
            }

            if (UpgradeData.Instance != null)
            {
                UpgradeData.Instance.OnUpgradeChanged -= OnUpgradeChanged;
            }
        }
    }
}

