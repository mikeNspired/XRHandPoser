using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace MikeNspired.UnityXRHandPoser
{
    public class InventoryGrabInteract : MonoBehaviour
    {
        [SerializeField] private InputActionReference leftControllerInput;
        [SerializeField] private InputActionReference rightControllerInput;

        // Reference to the InventoryManager in the scene/parent
        [SerializeField] private InventoryManager inventoryManager;

        private void Awake()
        {
            // If not assigned in Inspector, try to find one in parent or anywhere in the scene
            if (!inventoryManager)
                inventoryManager = GetComponentInParent<InventoryManager>();

            if (!inventoryManager)
                Debug.LogWarning("InventoryManager is missing or not assigned in Inspector!", this);

            if (leftControllerInput == null || leftControllerInput.action == null)
                Debug.LogWarning("LeftControllerInput or its action is null.", this);
            if (rightControllerInput == null || rightControllerInput.action == null)
                Debug.LogWarning("RightControllerInput or its action is null.", this);
        }

        private void OnEnable()
        {
            if (leftControllerInput?.action != null)
            {
                leftControllerInput.action.performed += OnLeftControllerAction;
                leftControllerInput.action.canceled += OnLeftControllerAction;

            }

            if (rightControllerInput?.action != null)
            {
                rightControllerInput.action.performed += OnRightControllerAction;
                rightControllerInput.action.canceled += OnRightControllerAction;
            }
        }

        private void OnDisable()
        {
            if (leftControllerInput?.action != null)
            {
                leftControllerInput.action.performed -= OnLeftControllerAction;
                leftControllerInput.action.canceled -= OnLeftControllerAction;

            }

            if (rightControllerInput?.action != null)
            {
                rightControllerInput.action.performed -= OnRightControllerAction;
                rightControllerInput.action.canceled -= OnRightControllerAction;
            }
        }

        private void OnLeftControllerAction(InputAction.CallbackContext context)
        {
            if (!inventoryManager) return;

            // Which slot is hovered by the left hand?
            var slot = inventoryManager.ActiveLeftSlot;
            if (!slot)
                return; // No hovered slot, do nothing

            // Get the interactor from the manager's left controller
            var leftInteractor = inventoryManager.leftController.GetComponentInChildren<NearFarInteractor>();
            if (leftInteractor != null)
            {
                slot.TryInteractWithSlot(leftInteractor);
            }
            else
            {
                Debug.LogWarning("NearFarInteractor not found on the left controller.");
            }
        }

        private void OnRightControllerAction(InputAction.CallbackContext context)
        {
            if (!inventoryManager) return;

            // Which slot is hovered by the right hand?
            var slot = inventoryManager.ActiveRightSlot;
            if (!slot)
                return; // No hovered slot, do nothing

            // Get the interactor from the manager's right controller
            var rightInteractor = inventoryManager.rightController.GetComponentInChildren<NearFarInteractor>();
            if (rightInteractor != null)
            {
                slot.TryInteractWithSlot(rightInteractor);
            }
            else
            {
                Debug.LogWarning("NearFarInteractor not found on the right controller.");
            }
        }
    }
}
