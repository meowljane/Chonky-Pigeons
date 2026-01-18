using UnityEngine;
using UnityEngine.InputSystem;

namespace PigeonGame.Gameplay
{
    /// <summary>
    /// 클릭 포커싱 시스템
    /// 상호작용 가능한 오브젝트를 클릭하면 범위를 표시
    /// </summary>
    public class ClickFocusManager : MonoBehaviour
    {
        public static ClickFocusManager Instance { get; private set; }

        [Header("Visual Settings")]
        [SerializeField] private Color pigeonRangeColor = new Color(1f, 0.5f, 0.5f, 0.8f); // 빨간색 (비둘기 감지 범위)
        [SerializeField] private Color buildingRangeColor = new Color(0.5f, 0.5f, 1f, 0.8f); // 파란색 (건물 상호작용 범위)
        [SerializeField] private Color trapRangeColor = new Color(0.5f, 1f, 0.5f, 0.8f); // 초록색 (덫 상호작용 범위)
        [SerializeField] private Color eatingRangeColor = new Color(1f, 0.8f, 0.5f, 0.6f); // 주황색 (비둘기 먹기 범위)

        private Camera mainCamera;
        private GameObject currentFocusObject;
        private UI.CircleRangeIndicator currentIndicator;
        private UI.CircleRangeIndicator currentSecondaryIndicator; // 비둘기 eatingRadius용

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }

        private void Update()
        {
            // 마우스 클릭 감지
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleClick();
            }
            // 터치 입력 감지
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                HandleTouch();
            }
        }

        private void HandleClick()
        {
            if (mainCamera == null)
                return;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            HandleInput(mousePosition);
        }

        private void HandleTouch()
        {
            if (mainCamera == null)
                return;

            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            HandleInput(touchPosition);
        }

        private void HandleInput(Vector2 screenPosition)
        {
            // UI를 클릭했는지 확인 (UI 클릭 시 무시)
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // 스크린 좌표를 월드 좌표로 변환
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, mainCamera.nearClipPlane));
            worldPosition.z = 0f;

            // 클릭한 위치에서 오브젝트 찾기
            Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);
            
            if (hitCollider != null)
            {
                GameObject clickedObject = hitCollider.gameObject;
                HandleObjectClick(clickedObject);
            }
            else
            {
                // 아무것도 클릭하지 않았으면 포커스 해제
                ClearFocus();
            }
        }

        private void HandleObjectClick(GameObject clickedObject)
        {
            // 비둘기 확인
            PigeonMovement pigeonMovement = clickedObject.GetComponent<PigeonMovement>();
            if (pigeonMovement != null)
            {
                ShowPigeonRange(clickedObject, pigeonMovement);
                return;
            }

            // 건물 확인
            WorldShop shop = clickedObject.GetComponent<WorldShop>();
            if (shop != null)
            {
                ShowBuildingRange(clickedObject, shop.InteractionRadius);
                return;
            }

            // 덫 확인 (포획 여부와 관계없이 표시)
            FoodTrap trap = clickedObject.GetComponent<FoodTrap>();
            if (trap != null)
            {
                ShowTrapRange(clickedObject, trap);
                return;
            }

            // 상호작용 가능한 오브젝트가 아니면 포커스 해제
            ClearFocus();
        }

        private void ShowPigeonRange(GameObject target, PigeonMovement pigeonMovement)
        {
            // DetectionRadius만 표시
            float detectionRadius = pigeonMovement.DetectionRadius;
            ShowRange(target, detectionRadius, pigeonRangeColor);
        }

        private void ShowBuildingRange(GameObject target, float radius)
        {
            ShowRange(target, radius, buildingRangeColor);
        }

        private void ShowTrapRange(GameObject target, FoodTrap trap)
        {
            // InteractionRadius 표시
            float interactionRadius = trap.InteractionRadius;
            ShowRange(target, interactionRadius, trapRangeColor);
            
            // eatingRadius도 표시 (비둘기가 덫에서 먹을 수 있는 범위)
            const float eatingRadius = 0.1f; // 비둘기의 eatingRadius
            ShowSecondaryRange(target, eatingRadius, eatingRangeColor);
        }

        private void ShowRange(GameObject target, float radius, Color color)
        {
            // 기존 포커스 해제
            ClearFocus();

            // 새 포커스 설정
            currentFocusObject = target;

            // 범위 표시 컴포넌트 가져오기 또는 생성
            UI.CircleRangeIndicator indicator = target.GetComponent<UI.CircleRangeIndicator>();
            if (indicator == null)
            {
                indicator = target.AddComponent<UI.CircleRangeIndicator>();
            }

            currentIndicator = indicator;
            indicator.SetRadius(radius);
            indicator.SetColor(color);
            indicator.SetVisible(true);
        }

        private void ShowSecondaryRange(GameObject target, float radius, Color color)
        {
            // 보조 범위 표시 (비둘기 eatingRadius용)
            // 별도의 GameObject에 표시하여 중첩 가능하게 함
            GameObject secondaryObj = new GameObject("SecondaryRangeIndicator");
            secondaryObj.transform.SetParent(target.transform);
            secondaryObj.transform.localPosition = Vector3.zero;
            
            UI.CircleRangeIndicator secondaryIndicator = secondaryObj.AddComponent<UI.CircleRangeIndicator>();
            currentSecondaryIndicator = secondaryIndicator;
            secondaryIndicator.SetRadius(radius);
            secondaryIndicator.SetColor(color);
            secondaryIndicator.SetVisible(true);
        }

        private void ClearFocus()
        {
            if (currentIndicator != null)
            {
                currentIndicator.SetVisible(false);
                currentIndicator = null;
            }
            
            if (currentSecondaryIndicator != null)
            {
                currentSecondaryIndicator.SetVisible(false);
                // 보조 인디케이터는 자식 오브젝트이므로 부모와 함께 제거될 수 있음
                if (currentSecondaryIndicator.gameObject != null)
                {
                    Destroy(currentSecondaryIndicator.gameObject);
                }
                currentSecondaryIndicator = null;
            }
            
            currentFocusObject = null;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
