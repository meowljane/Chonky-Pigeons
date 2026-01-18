using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PigeonGame.Data;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 업그레이드 상점 UI - 모든 슬롯을 참조로 받아서 업데이트만 함
    /// </summary>
    public class UpgradeShopUI : MonoBehaviour
    {
        [System.Serializable]
        public class UpgradeSlotReferences
        {
            public TextMeshProUGUI nameText;
            public Button upgradeButton;
            public TextMeshProUGUI buttonText;
        }

        [System.Serializable]
        public class SpeciesWeightUnlockSlotReferences
        {
            public TextMeshProUGUI nameText;
            public Button unlockButton;
            public TextMeshProUGUI buttonText;
        }

        [System.Serializable]
        public class SpeciesWeightSlotReferences
        {
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI multiplierText;
            public Button leftButton;
            public Button rightButton;
        }

        [System.Serializable]
        public class PigeonsPerMapSelectPanelReferences
        {
            public TextMeshProUGUI valueText;
            public Button leftButton;
            public Button rightButton;
        }

        [Header("Main Panel")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("Upgrade Slots")]
        [SerializeField] private UpgradeSlotReferences inventorySlot;
        [SerializeField] private UpgradeSlotReferences pigeonsPerMapSlot;
        [SerializeField] private UpgradeSlotReferences maxTrapCountSlot;

        [Header("Species Weight Unlock Slots")]
        [SerializeField] private SpeciesWeightUnlockSlotReferences speciesWeightIncUnlockSlot;
        [SerializeField] private SpeciesWeightUnlockSlotReferences speciesWeightDecUnlockSlot;

        [Header("Species Weight Slots")]
        [SerializeField] private GameObject increaseSlot; // 증가 슬롯 (항상 활성화)
        [SerializeField] private GameObject increaseSlotBlockPanel; // 증가 슬롯 막는 패널
        [SerializeField] private SpeciesWeightSlotReferences increaseSlotRefs;
        [SerializeField] private GameObject decreaseSlot; // 감소 슬롯 (항상 활성화)
        [SerializeField] private GameObject decreaseSlotBlockPanel; // 감소 슬롯 막는 패널
        [SerializeField] private SpeciesWeightSlotReferences decreaseSlotRefs;

        [Header("Pigeons Per Map Select Panel")]
        [SerializeField] private GameObject pigeonsPerMapSelectPanel; // 항상 활성화
        [SerializeField] private GameObject pigeonsPerMapBlockPanel; // 비둘기 스폰 수 선택 패널 막는 패널
        [SerializeField] private PigeonsPerMapSelectPanelReferences pigeonsPerMapSelectPanelRefs;

        [SerializeField] private int speciesWeightUnlockCost = 200;

        private bool isSpeciesWeightIncUnlocked = false;
        private bool isSpeciesWeightDecUnlocked = false;
        private List<PigeonSpecies> availableSpecies = new List<PigeonSpecies>();

        // 업그레이드 정의 (GameDataRegistry에서 가져옴)
        private Dictionary<UpgradeType, UpgradeDefinition> upgradeDefinitions = new Dictionary<UpgradeType, UpgradeDefinition>();

        private void Start()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            UIHelper.SafeAddListener(closeButton, OnCloseButtonClicked);

            // GameManager 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
            }

            // UpgradeData 이벤트 구독
            if (UpgradeData.Instance != null)
            {
                UpgradeData.Instance.OnUpgradeChanged += OnUpgradeChanged;
            }

            InitializeUpgradeDefinitions();
            UpdateGoldText();
            UpdateShopDisplay();
        }

        private void InitializeUpgradeDefinitions()
        {
            upgradeDefinitions.Clear();

            var registry = GameDataRegistry.Instance;
            if (registry?.UpgradeDefinitions == null)
                return;

            var upgradeSet = registry.UpgradeDefinitions;
            if (upgradeSet.upgrades == null)
                return;

            foreach (var upgrade in upgradeSet.upgrades)
            {
                if (upgrade != null)
                {
                    upgradeDefinitions[upgrade.upgradeType] = upgrade;
                }
            }
        }

        public void OpenShopPanel()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
                UpdateGoldText();
                UpdateShopDisplay();
            }
        }

        private void OnMoneyChanged(int money)
        {
            UpdateGoldText();
            UpdateShopDisplay();
        }

        private void OnUpgradeChanged()
        {
            UpdateShopDisplay();
        }

        private void UpdateGoldText()
        {
            UIHelper.UpdateGoldText(goldText);
        }

        private void UpdateShopDisplay()
        {
            // 각 업그레이드 슬롯 업데이트
            if (inventorySlot != null)
                UpdateUpgradeSlot(inventorySlot, upgradeDefinitions[UpgradeType.InventorySlots]);
            
            if (pigeonsPerMapSlot != null)
            {
                UpdateUpgradeSlot(pigeonsPerMapSlot, upgradeDefinitions[UpgradeType.PigeonsPerMap]);
                UpdatePigeonsPerMapSlot();
            }
            
            if (maxTrapCountSlot != null)
                UpdateUpgradeSlot(maxTrapCountSlot, upgradeDefinitions[UpgradeType.MaxTrapCount]);

            // 비둘기 확률 조정 해금 슬롯 업데이트
            if (speciesWeightIncUnlockSlot != null)
                UpdateSpeciesWeightUnlockSlot(speciesWeightIncUnlockSlot, true);
            
            if (speciesWeightDecUnlockSlot != null)
                UpdateSpeciesWeightUnlockSlot(speciesWeightDecUnlockSlot, false);

            // 증가/감소 슬롯 막는 패널 제어 및 업데이트
            if (increaseSlotBlockPanel != null)
            {
                increaseSlotBlockPanel.SetActive(!isSpeciesWeightIncUnlocked);
            }
            
            if (increaseSlot != null && increaseSlotRefs != null && isSpeciesWeightIncUnlocked)
            {
                UpdateSpeciesWeightSlot(increaseSlotRefs, true);
            }

            if (decreaseSlotBlockPanel != null)
            {
                decreaseSlotBlockPanel.SetActive(!isSpeciesWeightDecUnlocked);
            }
            
            if (decreaseSlot != null && decreaseSlotRefs != null && isSpeciesWeightDecUnlocked)
            {
                UpdateSpeciesWeightSlot(decreaseSlotRefs, false);
            }
        }

        private void UpdateUpgradeSlot(UpgradeSlotReferences slotRefs, UpgradeDefinition upgradeDef)
        {
            if (slotRefs == null)
                return;

            int currentLevel = GetCurrentLevel(upgradeDef.upgradeType);
            int maxLevel = upgradeDef.values.Length;
            bool isMaxLevel = currentLevel >= maxLevel;

            // NameText 업데이트: "이름(현재단계/전체단계)"
            if (slotRefs.nameText != null)
            {
                slotRefs.nameText.text = $"{upgradeDef.upgradeName}({currentLevel}/{maxLevel})";
            }

            // 업그레이드 버튼 업데이트
            if (slotRefs.upgradeButton != null)
            {
                bool canAfford = GameManager.Instance != null && 
                                !isMaxLevel && 
                                GameManager.Instance.CurrentMoney >= upgradeDef.costs[currentLevel];
                
                slotRefs.upgradeButton.interactable = !isMaxLevel && canAfford;
                slotRefs.upgradeButton.onClick.RemoveAllListeners();
                
                if (!isMaxLevel)
                {
                    slotRefs.upgradeButton.onClick.AddListener(() => OnUpgradeClicked(upgradeDef));
                }

                if (slotRefs.buttonText != null)
                {
                    if (isMaxLevel)
                    {
                        slotRefs.buttonText.text = "최대 레벨";
                    }
                    else
                    {
                        int nextValue = upgradeDef.values[currentLevel];
                        int currentValue = GetCurrentValue(upgradeDef.upgradeType);
                        int increaseAmount = nextValue - currentValue;
                        slotRefs.buttonText.text = canAfford 
                            ? $"(+{increaseAmount})업그레이드\n{upgradeDef.costs[currentLevel]}G"
                            : $"돈부족\n{upgradeDef.costs[currentLevel]}G";
                    }
                }
            }
        }

        private void UpdatePigeonsPerMapSlot()
        {
            if (GameManager.Instance == null || pigeonsPerMapSelectPanel == null || pigeonsPerMapSelectPanelRefs == null)
                return;

            int unlockedLevel = UpgradeData.Instance.PigeonsPerMapUnlockedLevel;
            int selectedValue = UpgradeData.Instance.PigeonsPerMapSelectedValue;

            // 막는 패널 제어
            if (pigeonsPerMapBlockPanel != null)
            {
                pigeonsPerMapBlockPanel.SetActive(unlockedLevel == 0);
            }

            if (unlockedLevel > 0)
            {
                // 값 업데이트
                if (pigeonsPerMapSelectPanelRefs.valueText != null)
                {
                    pigeonsPerMapSelectPanelRefs.valueText.text = selectedValue.ToString();
                }

                // 버튼 활성화/비활성화 및 이벤트 연결
                int maxValue = upgradeDefinitions[UpgradeType.PigeonsPerMap].values[unlockedLevel - 1];
                
                if (pigeonsPerMapSelectPanelRefs.leftButton != null)
                {
                    pigeonsPerMapSelectPanelRefs.leftButton.interactable = selectedValue > 5;
                    pigeonsPerMapSelectPanelRefs.leftButton.onClick.RemoveAllListeners();
                    pigeonsPerMapSelectPanelRefs.leftButton.onClick.AddListener(OnPigeonsPerMapDecrease);
                }

                if (pigeonsPerMapSelectPanelRefs.rightButton != null)
                {
                    pigeonsPerMapSelectPanelRefs.rightButton.interactable = selectedValue < maxValue;
                    pigeonsPerMapSelectPanelRefs.rightButton.onClick.RemoveAllListeners();
                    pigeonsPerMapSelectPanelRefs.rightButton.onClick.AddListener(OnPigeonsPerMapIncrease);
                }
            }
        }

        private void UpdateSpeciesWeightUnlockSlot(SpeciesWeightUnlockSlotReferences slotRefs, bool isIncrease)
        {
            if (slotRefs == null)
                return;

            bool isUnlocked = isIncrease ? isSpeciesWeightIncUnlocked : isSpeciesWeightDecUnlocked;

            // NameText 업데이트
            if (slotRefs.nameText != null)
            {
                string slotName = isIncrease ? "비둘기 확률 증가" : "비둘기 확률 감소";
                slotRefs.nameText.text = isUnlocked ? $"{slotName}(해금됨)" : $"{slotName}(미해금)";
            }

            // 업그레이드 버튼 업데이트
            if (slotRefs.unlockButton != null)
            {
                bool canAfford = GameManager.Instance != null && 
                                !isUnlocked && 
                                GameManager.Instance.CurrentMoney >= speciesWeightUnlockCost;
                
                slotRefs.unlockButton.interactable = !isUnlocked && canAfford;
                slotRefs.unlockButton.onClick.RemoveAllListeners();
                slotRefs.unlockButton.onClick.AddListener(() => OnSpeciesWeightUnlockClicked(isIncrease));

                if (slotRefs.buttonText != null)
                {
                    if (isUnlocked)
                        slotRefs.buttonText.text = "해금됨";
                    else if (canAfford)
                        slotRefs.buttonText.text = $"해금\n{speciesWeightUnlockCost}G";
                    else
                        slotRefs.buttonText.text = $"돈부족\n{speciesWeightUnlockCost}G";
                }
            }
        }

        private void UpdateSpeciesWeightSlot(SpeciesWeightSlotReferences slotRefs, bool isIncrease)
        {
            if (slotRefs == null)
                return;

            UpdateAvailableSpecies();
            PigeonSpecies? selectedSpecies = UpgradeData.Instance != null
                ? (isIncrease ? UpgradeData.Instance.SelectedIncreaseSpecies : UpgradeData.Instance.SelectedDecreaseSpecies)
                : null;

            if (slotRefs.nameText != null)
            {
                if (selectedSpecies.HasValue)
                {
                    var registry = GameDataRegistry.Instance;
                    var species = registry?.SpeciesSet?.GetSpeciesById(selectedSpecies.Value);
                    slotRefs.nameText.text = species != null ? species.name : selectedSpecies.Value.ToString();
                }
                else
                {
                    slotRefs.nameText.text = "None";
                }
            }

            if (slotRefs.multiplierText != null)
            {
                if (selectedSpecies.HasValue && GameManager.Instance != null)
                {
                    float multiplier = UpgradeData.Instance.GetSpeciesWeightMultiplier(selectedSpecies.Value);
                    slotRefs.multiplierText.text = $"배율: {multiplier:F2}x";
                }
                else
                {
                    slotRefs.multiplierText.text = "배율: 1.00x (기본값)";
                }
            }

            if (slotRefs.leftButton != null)
            {
                slotRefs.leftButton.onClick.RemoveAllListeners();
                slotRefs.leftButton.onClick.AddListener(() => OnSpeciesChanged(-1, isIncrease));
            }

            if (slotRefs.rightButton != null)
            {
                slotRefs.rightButton.onClick.RemoveAllListeners();
                slotRefs.rightButton.onClick.AddListener(() => OnSpeciesChanged(1, isIncrease));
            }
        }

        private int GetCurrentLevel(UpgradeType upgradeType)
        {
            if (GameManager.Instance == null)
                return 0;

            var upgrades = UpgradeData.Instance;

            switch (upgradeType)
            {
                case UpgradeType.InventorySlots:
                    int inventoryBonus = upgrades.InventorySlotBonus;
                    if (inventoryBonus >= 15) return 3;
                    if (inventoryBonus >= 10) return 2;
                    if (inventoryBonus >= 5) return 1;
                    return 0;

                case UpgradeType.PigeonsPerMap:
                    return upgrades.PigeonsPerMapUnlockedLevel;

                case UpgradeType.MaxTrapCount:
                    int maxTraps = upgrades.MaxTrapCount;
                    if (maxTraps >= 10) return 3;
                    if (maxTraps >= 7) return 2;
                    if (maxTraps >= 5) return 1;
                    if (maxTraps >= 2) return 0;
                    return 0;

                default:
                    return 0;
            }
        }

        private int GetCurrentValue(UpgradeType upgradeType)
        {
            if (GameManager.Instance == null)
                return 0;

            switch (upgradeType)
            {
                case UpgradeType.InventorySlots:
                    return GameManager.Instance.MaxInventorySlots;

                case UpgradeType.PigeonsPerMap:
                    return UpgradeData.Instance.PigeonsPerMapSelectedValue;

                case UpgradeType.MaxTrapCount:
                    int maxTraps = UpgradeData.Instance.MaxTrapCount;
                    return maxTraps > 0 ? maxTraps : 2;

                default:
                    return 0;
            }
        }

        private void OnUpgradeClicked(UpgradeDefinition upgradeDef)
        {
            if (GameManager.Instance == null)
                return;

            int currentLevel = GetCurrentLevel(upgradeDef.upgradeType);
            if (currentLevel >= upgradeDef.values.Length)
                return;

            int cost = upgradeDef.costs[currentLevel];
            if (!GameManager.Instance.SpendMoney(cost))
                return;

            switch (upgradeDef.upgradeType)
            {
                case UpgradeType.InventorySlots:
                    int targetInventory = upgradeDef.values[currentLevel];
                    int baseInventory = UpgradeData.Instance.BaseMaxInventorySlots;
                    int inventoryBonus = targetInventory - baseInventory;
                    UpgradeData.Instance.SetInventorySlotBonus(inventoryBonus);
                    break;

                case UpgradeType.PigeonsPerMap:
                    int unlockLevel = GetCurrentLevel(UpgradeType.PigeonsPerMap) + 1;
                    UpgradeData.Instance.SetPigeonsPerMapUnlockedLevel(unlockLevel);
                    int defaultValue = upgradeDef.values[unlockLevel - 1];
                    UpgradeData.Instance.SetPigeonsPerMapSelectedValue(defaultValue);
                    break;

                case UpgradeType.MaxTrapCount:
                    int targetTraps = upgradeDef.values[currentLevel];
                    UpgradeData.Instance.SetMaxTrapCount(targetTraps);
                    break;
            }

            UpdateShopDisplay();
        }

        private void OnPigeonsPerMapDecrease()
        {
            if (UpgradeData.Instance == null)
                return;

            int selectedValue = UpgradeData.Instance.PigeonsPerMapSelectedValue;
            if (selectedValue > UpgradeData.Instance.BasePigeonsPerMap)
            {
                UpgradeData.Instance.SetPigeonsPerMapSelectedValue(selectedValue - 1);
                UpdateShopDisplay();
            }
        }

        private void OnPigeonsPerMapIncrease()
        {
            if (UpgradeData.Instance == null)
                return;

            int selectedValue = UpgradeData.Instance.PigeonsPerMapSelectedValue;
            int unlockedLevel = UpgradeData.Instance.PigeonsPerMapUnlockedLevel;
            
            if (unlockedLevel > 0 && upgradeDefinitions.ContainsKey(UpgradeType.PigeonsPerMap))
            {
                int maxValue = upgradeDefinitions[UpgradeType.PigeonsPerMap].values[unlockedLevel - 1];
                if (selectedValue < maxValue)
                {
                    UpgradeData.Instance.SetPigeonsPerMapSelectedValue(selectedValue + 1);
                    UpdateShopDisplay();
                }
            }
        }

        private void OnSpeciesWeightUnlockClicked(bool isIncrease)
        {
            bool isUnlocked = isIncrease ? isSpeciesWeightIncUnlocked : isSpeciesWeightDecUnlocked;
            
            if (isUnlocked)
                return;

            if (GameManager.Instance == null)
                return;

            if (!GameManager.Instance.SpendMoney(speciesWeightUnlockCost))
                return;

            if (isIncrease)
                isSpeciesWeightIncUnlocked = true;
            else
                isSpeciesWeightDecUnlocked = true;

            UpdateAvailableSpecies();
            UpdateShopDisplay();
        }

        private void OnSpeciesChanged(int direction, bool isIncrease)
        {
            UpdateAvailableSpecies();

            PigeonSpecies? currentSpecies = UpgradeData.Instance != null
                ? (isIncrease ? UpgradeData.Instance.SelectedIncreaseSpecies : UpgradeData.Instance.SelectedDecreaseSpecies)
                : null;
            
            // 이전 종에서 선택 해제
            if (currentSpecies.HasValue && UpgradeData.Instance != null)
            {
                if (isIncrease)
                    UpgradeData.Instance.SetIncreaseSpecies(null);
                else
                    UpgradeData.Instance.SetDecreaseSpecies(null);
            }

            PigeonSpecies? selectedSpecies = currentSpecies;

            if (direction < 0)
            {
                if (selectedSpecies == null)
                {
                    if (availableSpecies.Count > 0)
                        selectedSpecies = availableSpecies[availableSpecies.Count - 1];
                }
                else
                {
                    int currentIndex = availableSpecies.IndexOf(selectedSpecies.Value);
                    if (currentIndex <= 0)
                        selectedSpecies = null;
                    else
                        selectedSpecies = availableSpecies[currentIndex - 1];
                }
            }
            else
            {
                if (selectedSpecies == null)
                {
                    if (availableSpecies.Count > 0)
                        selectedSpecies = availableSpecies[0];
                }
                else
                {
                    int currentIndex = availableSpecies.IndexOf(selectedSpecies.Value);
                    if (currentIndex < 0 || currentIndex >= availableSpecies.Count - 1)
                        selectedSpecies = null;
                    else
                        selectedSpecies = availableSpecies[currentIndex + 1];
                }
            }

            // 새 종 선택
            if (UpgradeData.Instance != null)
            {
                if (isIncrease)
                    UpgradeData.Instance.SetIncreaseSpecies(selectedSpecies);
                else
                    UpgradeData.Instance.SetDecreaseSpecies(selectedSpecies);
            }

            UpdateShopDisplay();
        }

        private void UpdateAvailableSpecies()
        {
            availableSpecies.Clear();

            var registry = GameDataRegistry.Instance;
            if (registry == null || registry.SpeciesSet == null)
                return;

            foreach (var species in registry.SpeciesSet.species)
            {
                if (GameManager.Instance.IsSpeciesUnlocked(species.speciesType))
                {
                    if (!availableSpecies.Contains(species.speciesType))
                    {
                        availableSpecies.Add(species.speciesType);
                    }
                }
            }

            if (UpgradeData.Instance != null)
            {
                if (UpgradeData.Instance.SelectedIncreaseSpecies.HasValue && 
                    !availableSpecies.Contains(UpgradeData.Instance.SelectedIncreaseSpecies.Value))
                {
                    UpgradeData.Instance.SetIncreaseSpecies(null);
                }
                if (UpgradeData.Instance.SelectedDecreaseSpecies.HasValue && 
                    !availableSpecies.Contains(UpgradeData.Instance.SelectedDecreaseSpecies.Value))
                {
                    UpgradeData.Instance.SetDecreaseSpecies(null);
                }
            }
        }

        private void OnCloseButtonClicked()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
            }

            if (UpgradeData.Instance != null)
            {
                UpgradeData.Instance.OnUpgradeChanged -= OnUpgradeChanged;
            }

            UIHelper.SafeRemoveListener(closeButton);
        }
    }
}
