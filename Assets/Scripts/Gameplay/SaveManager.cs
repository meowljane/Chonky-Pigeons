using UnityEngine;
using PigeonGame.Save;

namespace PigeonGame.Gameplay
{
    public static class SaveManager
    {
        private const string SaveKey = "GameSaveData";

        public static void SaveGame()
        {
            var save = new SaveData
            {
                version = 1
            };

            if (GameManager.Instance != null)
            {
                save.game = GameManager.Instance.CreateSaveData();
            }

            if (UpgradeData.Instance != null)
            {
                save.upgrades = UpgradeData.Instance.CreateSaveData();
            }

            if (EncyclopediaManager.Instance != null)
            {
                save.encyclopedia = EncyclopediaManager.Instance.CreateSaveData();
            }

            string json = JsonUtility.ToJson(save);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public static SaveData LoadGame()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return CreateDefaultSaveData();
            }

            string json = PlayerPrefs.GetString(SaveKey);
            if (string.IsNullOrEmpty(json))
            {
                return CreateDefaultSaveData();
            }

            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                return CreateDefaultSaveData();
            }

            return data;
        }

        public static void ApplyLoadedGame(SaveData data)
        {
            if (data == null)
                return;

            if (GameManager.Instance != null && data.game != null)
            {
                GameManager.Instance.ApplySaveData(data.game);
            }

            if (UpgradeData.Instance != null && data.upgrades != null)
            {
                UpgradeData.Instance.ApplySaveData(data.upgrades);
            }

            if (EncyclopediaManager.Instance != null && data.encyclopedia != null)
            {
                EncyclopediaManager.Instance.ApplySaveData(data.encyclopedia);
            }
        }

        public static void LoadOrCreateAndApply()
        {
            var data = LoadGame();
            ApplyLoadedGame(data);
        }

        public static void DeleteSave()
        {
            if (PlayerPrefs.HasKey(SaveKey))
            {
                PlayerPrefs.DeleteKey(SaveKey);
                PlayerPrefs.Save();
            }
        }

        private static SaveData CreateDefaultSaveData()
        {
            var data = new SaveData
            {
                version = 1
            };

            if (GameManager.Instance != null)
            {
                data.game = GameManager.Instance.CreateSaveData();
            }

            if (UpgradeData.Instance != null)
            {
                data.upgrades = UpgradeData.Instance.CreateSaveData();
            }

            if (EncyclopediaManager.Instance != null)
            {
                data.encyclopedia = EncyclopediaManager.Instance.CreateSaveData();
            }

            return data;
        }
    }
}

