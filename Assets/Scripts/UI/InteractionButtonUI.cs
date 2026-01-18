using UnityEngine;
using UnityEngine.UI;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    /// <summary>
    /// 상호작용 버튼 UI
    /// 플레이어가 상호작용 가능한 오브젝트와 상호작용할 수 있는 버튼을 관리
    /// </summary>
    public class InteractionButtonUI : MonoBehaviour
    {
        [Header("Button")]
        [SerializeField] private Button interactionButton;

        private void Start()
        {
            // InteractionSystem 미리 초기화 (첫 클릭부터 작동하도록)
            if (InteractionSystem.Instance == null)
            {
                GameObject interactionObj = new GameObject("InteractionSystem");
                InteractionSystem interactionSystem = interactionObj.AddComponent<InteractionSystem>();
                interactionSystem.InitializeUIComponents();
            }
            else
            {
                // 이미 존재하는 경우에도 UI 컴포넌트가 초기화되었는지 확인
                InteractionSystem.Instance.InitializeUIComponents();
            }

            UIHelper.SafeAddListener(interactionButton, OnInteractionButtonClicked);
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
                // Start()가 다음 프레임에 실행되므로 즉시 초기화
                interactionSystem.InitializeUIComponents();
            }
            
            if (interactionSystem != null)
            {
                interactionSystem.OnInteract();
            }
        }

        private void OnDestroy()
        {
            UIHelper.SafeRemoveListener(interactionButton);
        }
    }
}
