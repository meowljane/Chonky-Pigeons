using UnityEngine;
using UnityEngine.UI;
using PigeonGame.Gameplay;

namespace PigeonGame.UI
{
    public class InteractionButtonUI : MonoBehaviour
    {
        [Header("Button")]
        [SerializeField] private Button interactionButton;

        private void Start()
        {
            if (InteractionSystem.Instance == null)
            {
                GameObject interactionObj = new GameObject("InteractionSystem");
                InteractionSystem interactionSystem = interactionObj.AddComponent<InteractionSystem>();
                interactionSystem.InitializeUIComponents();
            }
            else
            {
                InteractionSystem.Instance.InitializeUIComponents();
            }

            UIHelper.SafeAddListener(interactionButton, OnInteractionButtonClicked);
        }

        private void OnInteractionButtonClicked()
        {
            InteractionSystem interactionSystem = InteractionSystem.Instance;

            if (interactionSystem == null)
            {
                GameObject interactionObj = new GameObject("InteractionSystem");
                interactionSystem = interactionObj.AddComponent<InteractionSystem>();
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
