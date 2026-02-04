using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    public class SettingsUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button settingsButton; 
        [SerializeField] private Button closeButton;

        [Header("Sound Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI volumeValueText;

        [Header("Version Info")]
        [SerializeField] private TextMeshProUGUI versionText;

        [Header("Data Reset")]
        [SerializeField] private Button resetDataButton;
        [SerializeField] private GameObject confirmDialog; 
        [SerializeField] private Button confirmResetButton;
        [SerializeField] private Button cancelResetButton;

        private const string MasterVolumeKey = "MasterVolume";
        private const float DefaultMasterVolume = 1f;

        private void Start()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }

            UIHelper.SafeAddListener(settingsButton, OpenSettings);
            UIHelper.SafeAddListener(closeButton, CloseSettings);
            UIHelper.SafeAddListener(resetDataButton, OnResetDataButtonClicked);
            UIHelper.SafeAddListener(confirmResetButton, OnConfirmReset);
            UIHelper.SafeAddListener(cancelResetButton, OnCancelReset);

            InitializeSoundSettings();

            InitializeVersionInfo();
        }

        private void InitializeSoundSettings()
        {
            float savedVolume = PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
            SetMasterVolume(savedVolume);

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = savedVolume;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
        }

        private void InitializeVersionInfo()
        {
            if (versionText != null)
            {
                string version = Application.version;
                if (string.IsNullOrEmpty(version))
                {
                    version = "1.0.0"; 
                }
                versionText.text = $"버전: {version}";
            }
        }

        private void OnMasterVolumeChanged(float value)
        {
            SetMasterVolume(value);
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
            PlayerPrefs.Save();
        }

        private void SetMasterVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);

            if (volumeValueText != null)
            {
                int volumePercent = Mathf.RoundToInt(volume * 100f);
                volumeValueText.text = $"{volumePercent}%";
            }
        }

        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
        }

        private void OnResetDataButtonClicked()
        {
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(true);
            }
        }

        private void OnConfirmReset()
        {
            SaveManager.DeleteSave();

            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }

            CloseSettings();

            ToastNotificationManager.ShowSuccess("게임 데이터가 초기화되었습니다.\n게임을 재시작합니다...");

            RestartGame();
        }

        private void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnCancelReset()
        {
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            UIHelper.SafeRemoveListener(settingsButton);
            UIHelper.SafeRemoveListener(closeButton);
            UIHelper.SafeRemoveListener(resetDataButton);
            UIHelper.SafeRemoveListener(confirmResetButton);
            UIHelper.SafeRemoveListener(cancelResetButton);
        }
    }
}
