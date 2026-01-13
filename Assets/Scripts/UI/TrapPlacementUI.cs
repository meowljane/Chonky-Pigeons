using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PigeonGame.Gameplay;
using PigeonGame.Data;
using System.Collections.Generic;

namespace PigeonGame.UI
{
    /// <summary>
    /// 우측 하단 덫 설치 UI
    /// 덫 설치 버튼과 상호작용 버튼, 덫 선택 그리드를 관리
    /// </summary>
    public class TrapPlacementUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button trapPlacementButton;
        [SerializeField] private Button interactionButton;

        [Header("Trap Selection Panel")]
        [SerializeField] private GameObject trapSelectionPanel;
        [SerializeField] private Transform trapGridContainer;
        [SerializeField] private GameObject trapItemPrefab;
        [SerializeField] private Button closeButton;

        [Header("Trap Item UI")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = Color.gray;

        private TrapPlacer trapPlacer;
        private List<GameObject> trapItemObjects = new List<GameObject>();

        private void Start()
        {
            // TrapPlacer 찾기
            trapPlacer = FindFirstObjectByType<TrapPlacer>();
            if (trapPlacer == null)
            {
                Debug.LogError("TrapPlacementUI: TrapPlacer를 찾을 수 없습니다!");
                enabled = false;
                return;
            }

            // 버튼 이벤트 연결
            if (trapPlacementButton != null)
            {
                trapPlacementButton.onClick.AddListener(OnTrapPlacementButtonClicked);
            }

            if (interactionButton != null)
            {
                interactionButton.onClick.AddListener(OnInteractionButtonClicked);
            }

            // 덫 선택 패널 초기화
            if (trapSelectionPanel != null)
            {
                trapSelectionPanel.SetActive(false);
            }

            // 닫기 버튼 찾기 및 연결
            if (closeButton == null && trapSelectionPanel != null)
            {
                closeButton = trapSelectionPanel.GetComponentInChildren<Button>();
                if (closeButton == null)
                {
                    closeButton = trapSelectionPanel.transform.Find("CloseButton")?.GetComponent<Button>();
                }
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            // 덫 그리드 생성
            CreateTrapGrid();
        }

        private void CreateTrapGrid()
        {
            if (trapGridContainer == null || trapItemPrefab == null)
            {
                if (trapItemPrefab == null)
                    Debug.LogError("TrapPlacementUI: trapItemPrefab이 설정되지 않았습니다!");
                return;
            }

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
            {
                Debug.LogError("GameDataRegistry를 찾을 수 없습니다!");
                return;
            }

            ClearItemList(trapItemObjects);

            // 모든 덫 표시
            var allTraps = registry.Traps.traps;
            foreach (var trapData in allTraps)
            {
                GameObject itemObj = Instantiate(trapItemPrefab, trapGridContainer, false);
                SetupTrapItem(itemObj, trapData);
                trapItemObjects.Add(itemObj);
            }
        }

        private void SetupTrapItem(GameObject itemObj, TrapDefinition trapData)
        {
            // 버튼 클릭 이벤트
            Button button = itemObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnTrapSelected(trapData.id));
            }

            // 해금 상태에 따른 색상 설정
            bool isUnlocked = GameManager.Instance != null && 
                             GameManager.Instance.IsTrapUnlocked(trapData.id);

            Image bg = itemObj.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = isUnlocked ? unlockedColor : lockedColor;
            }

            // 텍스트 업데이트
            TextMeshProUGUI text = itemObj.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (text == null)
                text = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (text != null)
            {
                if (isUnlocked)
                {
                    text.text = trapData.name;
                    text.color = Color.white;
                }
                else
                {
                    text.text = $"{trapData.name}\n(해금 필요)";
                    text.color = Color.gray;
                }
            }

            // 버튼 활성화/비활성화
            if (button != null)
            {
                button.interactable = isUnlocked;
            }
        }

        private void OnTrapPlacementButtonClicked()
        {
            if (trapSelectionPanel != null)
            {
                bool isActive = trapSelectionPanel.activeSelf;
                trapSelectionPanel.SetActive(!isActive);

                // 패널이 열릴 때 덫 상태 업데이트
                if (!isActive)
                {
                    UpdateTrapItems();
                }
            }
        }

        private void OnInteractionButtonClicked()
        {
            // 통합 상호작용 시스템 사용
            InteractionSystem interactionSystem = InteractionSystem.Instance;
            
            // 인스턴스가 없으면 자동 생성
            if (interactionSystem == null)
            {
                GameObject interactionObj = new GameObject("InteractionSystem");
                interactionSystem = interactionObj.AddComponent<InteractionSystem>();
            }
            
            if (interactionSystem != null)
            {
                interactionSystem.OnInteract();
            }
            else
            {
                Debug.LogWarning("InteractionSystem을 찾을 수 없습니다!");
            }
        }

        private void OnCloseButtonClicked()
        {
            if (trapSelectionPanel != null)
            {
                trapSelectionPanel.SetActive(false);
            }
        }

        private void OnTrapSelected(string trapId)
        {
            // 덫 선택 패널 닫기
            if (trapSelectionPanel != null)
            {
                trapSelectionPanel.SetActive(false);
            }

            // 플레이어 위치에 덫 설치
            if (trapPlacer != null)
            {
                bool success = trapPlacer.PlaceTrapAtPlayerPosition(trapId);
                if (!success)
                {
                    Debug.LogWarning($"덫 설치 실패: {trapId}");
                }
            }
        }

        private void UpdateTrapItems()
        {
            // 모든 덫 아이템 상태 업데이트
            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.Traps == null)
                return;

            var allTraps = registry.Traps.traps;
            for (int i = 0; i < allTraps.Length && i < trapItemObjects.Count; i++)
            {
                if (trapItemObjects[i] != null)
                {
                    SetupTrapItem(trapItemObjects[i], allTraps[i]);
                }
            }
        }

        private void ClearItemList(List<GameObject> list)
        {
            foreach (var item in list)
            {
                if (item != null)
                    Destroy(item);
            }
            list.Clear();
        }

        private void OnDestroy()
        {
            if (trapPlacementButton != null)
            {
                trapPlacementButton.onClick.RemoveAllListeners();
            }

            if (interactionButton != null)
            {
                interactionButton.onClick.RemoveAllListeners();
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
            }
        }
    }
}

