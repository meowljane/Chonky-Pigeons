using UnityEngine;
using System.Collections.Generic;
using PigeonGame.UI;
using PigeonGame.Data;

namespace PigeonGame.Gameplay
{
    public class InteractionSystem : MonoBehaviour
    {
        public static InteractionSystem Instance { get; private set; }

        [Header("Highlight Settings")]
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0f, 0.3f); 

        private HashSet<IInteractable> nearbyInteractables = new HashSet<IInteractable>(); 
        private IInteractable currentInteractable; 
        private GameObject currentOutlineObject; 

        [Header("UI References")]
        [SerializeField] private InventoryUI inventoryUI;
        [SerializeField] private PigeonShopUI pigeonShopUI;
        [SerializeField] private TrapShopUI trapShopUI;
        [SerializeField] private UI.ExhibitionUI exhibitionUI;
        [SerializeField] private UI.PigeonResearchUI pigeonResearchUI;
        [SerializeField] private UpgradeShopUI upgradeShopUI;
        [SerializeField] private DoorPurchaseUI doorPurchaseUI;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }


        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void RegisterInteractable(IInteractable interactable)
        {
            if (interactable != null && interactable.CanInteract())
            {
                nearbyInteractables.Add(interactable);
                UpdateClosestInteractable();
            }
        }

        public void UnregisterInteractable(IInteractable interactable)
        {
            if (interactable != null)
            {
                nearbyInteractables.Remove(interactable);

                if (currentInteractable == interactable)
                {
                    HideOutline();
                    currentInteractable = null;
                    if (nearbyInteractables.Count > 0)
                        UpdateClosestInteractable();
                }
            }
        }

        private void UpdateClosestInteractable()
        {
            if (PlayerController.Instance == null)
            {
                if (currentInteractable != null)
                {
                    HideOutline();
                    currentInteractable = null;
                }
                return;
            }

            Vector2 playerPosition = PlayerController.Instance.Position;
            IInteractable closestInteractable = null;
            float closestSqrDistance = float.MaxValue; 

            nearbyInteractables.RemoveWhere(interactable => interactable == null || !interactable.CanInteract());

            foreach (var interactable in nearbyInteractables)
            {
                if (interactable is MonoBehaviour monoBehaviour)
                {
                    Vector2 toObject = (Vector2)monoBehaviour.transform.position - playerPosition;
                    float sqrDistance = toObject.sqrMagnitude; 
                    if (sqrDistance < closestSqrDistance)
                    {
                        closestSqrDistance = sqrDistance;
                        closestInteractable = interactable;
                    }
                }
            }

            if (closestInteractable != currentInteractable)
            {
                currentInteractable = closestInteractable;

                if (currentInteractable != null)
                    ShowOutline(currentInteractable);
                else
                    HideOutline();
            }
        }

        public void OnInteract()
        {
            if (currentInteractable?.CanInteract() == true)
            {
                currentInteractable.OnInteract();
            }
        }

        public void OpenPigeonShop()
        {
            pigeonShopUI.OpenShopPanel();
        }

        public void OpenTrapShop()
        {
            trapShopUI.OpenShopPanel();
        }

        public void OpenExhibition()
        {
            exhibitionUI.OpenExhibitionPanel();
        }

        public void OpenPigeonResearch()
        {
            pigeonResearchUI.OpenShopPanel();
        }

        public void OpenUpgradeShop()
        {
            upgradeShopUI.OpenShopPanel();
        }

        public void OpenDoorPurchase(Door door, DoorType doorType, int cost, MapType unlocksMap)
        {
            doorPurchaseUI.OpenPurchasePanel(door, doorType, cost, unlocksMap);
        }

        public bool CanInteract()
        {
            return currentInteractable != null && currentInteractable.CanInteract();
        }

        private void ShowOutline(IInteractable interactable)
        {
            HideOutline();

            if (!(interactable is MonoBehaviour monoBehaviour))
                return;

            GameObject targetObject = monoBehaviour.gameObject;
            SpriteRenderer spriteRenderer = targetObject.GetComponent<SpriteRenderer>() 
                ?? targetObject.GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer?.sprite == null)
                return;

            GameObject highlightObj = new GameObject("InteractionHighlight");
            highlightObj.transform.SetParent(targetObject.transform, false);
            highlightObj.transform.localPosition = Vector3.zero;
            highlightObj.transform.localScale = Vector3.one;
            highlightObj.transform.localRotation = Quaternion.identity;

            SpriteRenderer highlightRenderer = highlightObj.AddComponent<SpriteRenderer>();
            highlightRenderer.sprite = spriteRenderer.sprite;
            highlightRenderer.color = highlightColor;
            highlightRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
            highlightRenderer.sortingLayerName = spriteRenderer.sortingLayerName;

            currentOutlineObject = highlightObj;
        }

        private void HideOutline()
        {
            if (currentOutlineObject != null)
            {
                Destroy(currentOutlineObject);
                currentOutlineObject = null;
            }
        }
    }
}