﻿using System.Collections.Generic;
using MikeNspired.UnityXRHandPoser;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace MyNamespace
{
    public class InventoryGrabInteract : MonoBehaviour
    {
        [SerializeField] private InteractButton interactButton = InteractButton.grip;
        [SerializeField] private bool autoAddIfGripping = true;

        public bool leftIsGripped, rightIsGripped;
        private List<ActionBasedController> controllers = new List<ActionBasedController>();
        private InventorySlot inventorySlot;
        private InputDevice inputDevice;

        private enum InteractButton
        {
            trigger,
            grip
        };

        private void Start()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            if (!inventorySlot)
                inventorySlot = GetComponent<InventorySlot>();
        }

        private void Update()
        {
            if (controllers.Count == 0) return;

            foreach (var controller in controllers)
            {
                CheckController(controller);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            var controller = other.GetComponentInParent<ActionBasedController>();
            if (controller && !controllers.Contains(controller))
            {
                controllers.Add(controller);

                if (autoAddIfGripping)
                {
                    if (controller.GetComponentInParent<HandReference>().LeftRight == LeftRight.Left)
                        leftIsGripped = true;
                    else
                        rightIsGripped = true;
                }
                else
                {
                    if (controller.GetComponentInParent<HandReference>().LeftRight == LeftRight.Left)
                        leftIsGripped = false;
                    else
                        rightIsGripped = false;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var controller = other.GetComponentInParent<ActionBasedController>();
            if (controller)
                controllers.Remove(controller);
        }
    
        private void CheckController(ActionBasedController controller)
        {
            if (interactButton == InteractButton.trigger)
                CheckControllerTrigger(controller);
            else
            {
                if (controller.GetComponentInParent<HandReference>().LeftRight == LeftRight.Left)
                    CheckControllerGrip(controller, ref leftIsGripped);
                else
                    CheckControllerGrip(controller, ref rightIsGripped);
            }
        }

        private void CheckControllerGrip(ActionBasedController controller, ref bool isGripped)
        {
            bool gripValue = controller.selectAction.action.triggered;

            if (!isGripped && gripValue) 
            {
                isGripped = true;
                if (autoAddIfGripping || !IsControllerHoldingObject(controller))
                    inventorySlot.TryInteractWithSlot(controller.GetComponentInChildren<XRDirectInteractor>());
            }
            else if (isGripped && !gripValue)
            {
                isGripped = false;
                if (IsControllerHoldingObject(controller))
                    inventorySlot.TryInteractWithSlot(controller.GetComponentInChildren<XRDirectInteractor>());
            }
        }

        private bool IsControllerHoldingObject(ActionBasedController controller)
        {
            return controller.GetComponentInChildren<XRDirectInteractor>().selectTarget;
        }

        private void CheckControllerTrigger(ActionBasedController controller)
        {
            bool gripValue = controller.activateAction.action.triggered;

            if (gripValue)
            {
                if (!controller.GetComponentInChildren<XRDirectInteractor>().selectTarget)
                    inventorySlot.TryInteractWithSlot(controller.GetComponentInChildren<XRDirectInteractor>());
            }
        }
    }
}