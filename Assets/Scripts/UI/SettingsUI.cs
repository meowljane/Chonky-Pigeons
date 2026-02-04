using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 설정 UI
    /// 상단 버튼으로 열고, 데이터 초기화 기능 제공
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button settingsButton; // 상단 설정 버튼
        [SerializeField] private Button closeButton;

        [Header("Sound Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI volumeValueText;

        [Header("Version Info")]
        [SerializeField] private TextMeshProUGUI versionText;

        [Header("Data Reset")]
        [SerializeField] private Button resetDataButton;
        [SerializeField] private GameObject confirmDialog; // 확인 다이얼로그
        [SerializeField] private Button confirmResetButton;
        [SerializeField] private Button cancelResetButton;

        private const string MasterVolumeKey = "MasterVolume";
        private const float DefaultMasterVolume = 1f;

        private void Start()
        {
            // 패널 초기화
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }

            // 버튼 이벤트 연결
            UIHelper.SafeAddListener(settingsButton, OpenSettings);
            UIHelper.SafeAddListener(closeButton, CloseSettings);
            UIHelper.SafeAddListener(resetDataButton, OnResetDataButtonClicked);
            UIHelper.SafeAddListener(confirmResetButton, OnConfirmReset);
            UIHelper.SafeAddListener(cancelResetButton, OnCancelReset);

            // 사운드 설정 초기화
            InitializeSoundSettings();

            // 버전 정보 설정
            InitializeVersionInfo();
        }

        /// <summary>
        /// 사운드 설정 초기화
        /// </summary>
        private void InitializeSoundSettings()
        {
            // 저장된 볼륨 값 로드
            float savedVolume = PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
            SetMasterVolume(savedVolume);

            // 슬라이더 이벤트 연결
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = savedVolume;
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
        }

        /// <summary>
        /// 버전 정보 초기화
        /// </summary>
        private void InitializeVersionInfo()
        {
            if (versionText != null)
            {
                string version = Application.version;
                if (string.IsNullOrEmpty(version))
                {
                    version = "1.0.0"; // 기본 버전
                }
                versionText.text = $"버전: {version}";
            }
        }

        /// <summary>
        /// 마스터 볼륨 변경
        /// </summary>
        private void OnMasterVolumeChanged(float value)
        {
            SetMasterVolume(value);
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 마스터 볼륨 설정
        /// </summary>
        private void SetMasterVolume(float volume)
        {
            // AudioListener를 통해 마스터 볼륨 제어
            AudioListener.volume = Mathf.Clamp01(volume);

            // 볼륨 값 텍스트 업데이트
            if (volumeValueText != null)
            {
                int volumePercent = Mathf.RoundToInt(volume * 100f);
                volumeValueText.text = $"{volumePercent}%";
            }
        }

        /// <summary>
        /// 설정 패널 열기
        /// </summary>
        public void OpenSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 설정 패널 닫기
        /// </summary>
        public void CloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // 확인 다이얼로그도 닫기
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
        }

        /// <summary>
        /// 데이터 초기화 버튼 클릭
        /// </summary>
        private void OnResetDataButtonClicked()
        {
            // 확인 다이얼로그 표시
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(true);
            }
        }

        /// <summary>
        /// 데이터 초기화 확인
        /// </summary>
        private void OnConfirmReset()
        {
            // 저장 데이터 삭제
            SaveManager.DeleteSave();

            // 확인 다이얼로그 닫기
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }

            // 설정 패널 닫기
            CloseSettings();

            // 토스트 알림 표시
            ToastNotificationManager.ShowSuccess("게임 데이터가 초기화되었습니다.\n게임을 재시작합니다...");

            // 씬 재시작 (게임 완전히 리셋)
            RestartGame();
        }

        /// <summary>
        /// 게임 재시작 (현재 씬 다시 로드)
        /// </summary>
        private void RestartGame()
        {
            // 현재 씬을 다시 로드하여 게임 완전히 재시작
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// 데이터 초기화 취소
        /// </summary>
        private void OnCancelReset()
        {
            if (confirmDialog != null)
            {
                confirmDialog.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            // 버튼 이벤트 해제
            UIHelper.SafeRemoveListener(settingsButton);
            UIHelper.SafeRemoveListener(closeButton);
            UIHelper.SafeRemoveListener(resetDataButton);
            UIHelper.SafeRemoveListener(confirmResetButton);
            UIHelper.SafeRemoveListener(cancelResetButton);
        }
    }
}
