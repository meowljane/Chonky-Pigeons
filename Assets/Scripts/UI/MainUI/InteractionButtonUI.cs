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
            UIHelper.SafeAddListener(interactionButton, OnInteractionButtonClicked);
        }

        private void OnInteractionButtonClicked()
        {
            InteractionSystem.Instance?.OnInteract();
        }

        private void OnDestroy()
        {
            UIHelper.SafeRemoveListener(interactionButton);
        }
    }
}
