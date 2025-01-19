// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit.Interactors;
//
// namespace MikeNspired.UnityXRHandPoser
// {
//     public class InventoryTouchInteract : MonoBehaviour
//     {
//         private InventorySlot inventorySlot;
//         private NearFarInteractor leftInteractor;
//         private NearFarInteractor rightInteractor;
//
//         private void Awake()
//         {
//             // Cache the InventorySlot
//             inventorySlot = GetComponent<InventorySlot>();
//             if (!inventorySlot)
//             {
//                 Debug.LogWarning("InventorySlot is not assigned or found!");
//                 return;
//             }
//
//             // Cache the interactors from the InventoryManager
//             var inventoryManager = GetComponentInParent<InventoryManager>();
//             if (!inventoryManager)
//             {
//                 Debug.LogWarning("InventoryManager is not assigned or found!");
//                 return;
//             }
//
//             leftInteractor = inventoryManager.leftController?.GetComponentInChildren<NearFarInteractor>();
//             rightInteractor = inventoryManager.rightController?.GetComponentInChildren<NearFarInteractor>();
//         }
//
//         private void OnEnable()
//         {
//             if (inventorySlot == null) return;
//
//             // Subscribe to UnityActions for hover changes
//             inventorySlot.OnLeftControllerHoverChanged += HandleLeftControllerHover;
//             inventorySlot.OnRightControllerHoverChanged += HandleRightControllerHover;
//         }
//
//         private void OnDisable()
//         {
//             if (inventorySlot == null) return;
//
//             // Unsubscribe from UnityActions to avoid memory leaks
//             inventorySlot.OnLeftControllerHoverChanged -= HandleLeftControllerHover;
//             inventorySlot.OnRightControllerHoverChanged -= HandleRightControllerHover;
//         }
//
//         private void HandleLeftControllerHover(bool isHovering)
//         {
//             if (!enabled || !isHovering || leftInteractor == null) return;
//
//             if (inventorySlot.CurrentSlotItem || leftInteractor.hasSelection)
//                 inventorySlot.TryInteractWithSlot(leftInteractor);
//         }
//
//         private void HandleRightControllerHover(bool isHovering)
//         {
//             if (!enabled || !isHovering || rightInteractor == null) return;
//
//             if (inventorySlot.CurrentSlotItem || rightInteractor.hasSelection)
//                 inventorySlot.TryInteractWithSlot(rightInteractor);
//         }
//     }
// }
