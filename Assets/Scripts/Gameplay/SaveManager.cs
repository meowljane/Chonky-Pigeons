using UnityEngine;
using PigeonGame.Save;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// JSON 기반 세이브/로드/삭제 관리
    /// </summary>
    public static class SaveManager
    {
        private const string SaveKey = "GameSaveData";

        /// <summary>
        /// 현재 게임 상태를 저장
        /// </summary>
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

        /// <summary>
        /// 저장된 데이터를 불러오거나, 없으면 기본값으로 생성
        /// </summary>
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

        /// <summary>
        /// 저장 데이터를 현재 싱글톤 매니저들에 반영
        /// </summary>
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

        /// <summary>
        /// 세이브가 있으면 로드 후 적용, 없으면 현재 상태를 기본 세이브로 간주
        /// </summary>
        public static void LoadOrCreateAndApply()
        {
            var data = LoadGame();
            ApplyLoadedGame(data);
        }

        /// <summary>
        /// 저장 데이터 삭제 (새 게임 시작용)
        /// </summary>
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

            // 현재 초기 상태를 그대로 기본 세이브로 사용
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

