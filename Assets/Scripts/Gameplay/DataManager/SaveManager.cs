using UnityEngine;
using System.Collections;
using PigeonGame.Data;
using PigeonGame.Save;

namespace PigeonGame.Gameplay
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SaveKey = "GameSaveData";

        [SerializeField] private float saveCooldown = 0.5f;

        private float lastSaveTime = 0f;
        private bool isInitialized = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            LoadOrCreateAndApply();
            SubscribeToSaveEvents();
            isInitialized = true;
        }

        private void SubscribeToSaveEvents()
        {
            SubscribeToGameManagerEvents();
            SubscribeToUpgradeDataEvents();
        }

        private void SubscribeToGameManagerEvents()
        {
            if (GameManager.Instance != null)
                DoSubscribeToGameManagerEvents();
            else
                StartCoroutine(WaitForGameManagerAndSubscribe());
        }

        private void SubscribeToUpgradeDataEvents()
        {
            if (UpgradeData.Instance != null)
                UpgradeData.Instance.OnUpgradeChanged += OnUpgradeChanged;
            else
                StartCoroutine(WaitForUpgradeDataAndSubscribe());
        }

        private void OnUpgradeChanged() => TriggerAutoSave();

        private void DoSubscribeToGameManagerEvents()
        {
            var gm = GameManager.Instance;
            gm.OnMoneyChanged += OnGameStateChanged;
            gm.OnPigeonAddedToInventory += OnPigeonStateChanged;
            gm.OnTrapUnlocked += OnUnlockStateChanged;
            gm.OnSpeciesUnlocked += OnSpeciesUnlockStateChanged;
            gm.OnDoorUnlocked += OnDoorUnlockStateChanged;
            gm.OnPigeonAddedToExhibition += OnPigeonStateChanged;
            gm.OnPigeonRemovedFromExhibition += OnPigeonStateChanged;
        }

        private void OnGameStateChanged(int _) => TriggerAutoSave();
        private void OnPigeonStateChanged(PigeonInstanceStats _) => TriggerAutoSave();
        private void OnUnlockStateChanged(TrapType _) => TriggerAutoSave();
        private void OnSpeciesUnlockStateChanged(PigeonSpecies _) => TriggerAutoSave();
        private void OnDoorUnlockStateChanged(DoorType _) => TriggerAutoSave();

        private IEnumerator WaitForGameManagerAndSubscribe()
        {
            while (GameManager.Instance == null)
                yield return null;
            DoSubscribeToGameManagerEvents();
        }

        private IEnumerator WaitForUpgradeDataAndSubscribe()
        {
            while (UpgradeData.Instance == null)
                yield return null;
            UpgradeData.Instance.OnUpgradeChanged += OnUpgradeChanged;
        }

        private void TriggerAutoSave()
        {
            if (!isInitialized)
                return;

            if (Time.time - lastSaveTime < saveCooldown)
                return;

            SaveGame();
            lastSaveTime = Time.time;
        }

        public void SaveGame()
        {
            var save = CreateSaveData();
            string json = JsonUtility.ToJson(save);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        private SaveData CreateSaveData()
        {
            var save = new SaveData { version = 1 };

            if (GameManager.Instance != null)
                save.game = GameManager.Instance.CreateSaveData();

            if (UpgradeData.Instance != null)
                save.upgrades = UpgradeData.Instance.CreateSaveData();

            if (EncyclopediaManager.Instance != null)
                save.encyclopedia = EncyclopediaManager.Instance.CreateSaveData();

            return save;
        }

        public SaveData LoadGame()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
                return CreateSaveData();

            string json = PlayerPrefs.GetString(SaveKey);
            if (string.IsNullOrEmpty(json))
                return CreateSaveData();

            var data = JsonUtility.FromJson<SaveData>(json);
            return data ?? CreateSaveData();
        }

        public void ApplyLoadedGame(SaveData data)
        {
            if (data == null)
                return;

            if (GameManager.Instance != null && data.game != null)
                GameManager.Instance.ApplySaveData(data.game);

            if (UpgradeData.Instance != null && data.upgrades != null)
                UpgradeData.Instance.ApplySaveData(data.upgrades);

            if (EncyclopediaManager.Instance != null && data.encyclopedia != null)
                EncyclopediaManager.Instance.ApplySaveData(data.encyclopedia);
        }

        public void LoadOrCreateAndApply()
        {
            var data = LoadGame();
            ApplyLoadedGame(data);
        }

        public void DeleteSave()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                PlayerPrefs.DeleteKey(SaveKey);
                PlayerPrefs.Save();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                var gm = GameManager.Instance;
                gm.OnMoneyChanged -= OnGameStateChanged;
                gm.OnPigeonAddedToInventory -= OnPigeonStateChanged;
                gm.OnTrapUnlocked -= OnUnlockStateChanged;
                gm.OnSpeciesUnlocked -= OnSpeciesUnlockStateChanged;
                gm.OnDoorUnlocked -= OnDoorUnlockStateChanged;
                gm.OnPigeonAddedToExhibition -= OnPigeonStateChanged;
                gm.OnPigeonRemovedFromExhibition -= OnPigeonStateChanged;
            }

            if (UpgradeData.Instance != null)
                UpgradeData.Instance.OnUpgradeChanged -= OnUpgradeChanged;
        }
    }
}

