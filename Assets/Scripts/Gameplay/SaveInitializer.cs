using UnityEngine;
using System.Collections;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class SaveInitializer : MonoBehaviour
    {
        [SerializeField] private float saveCooldown = 0.5f; 

        private float lastSaveTime = 0f;
        private bool isInitialized = false;

        private void Start()
        {
            SaveManager.LoadOrCreateAndApply();

            SubscribeToSaveEvents();

            isInitialized = true;
        }

        private void SubscribeToSaveEvents()
        {
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
                StartCoroutine(WaitForGameManagerAndSubscribe());
            }

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
            if (!isInitialized)
                return;

            if (Time.time - lastSaveTime < saveCooldown)
                return;

            SaveManager.SaveGame();
            lastSaveTime = Time.time;
        }

        private void OnDestroy()
        {
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

