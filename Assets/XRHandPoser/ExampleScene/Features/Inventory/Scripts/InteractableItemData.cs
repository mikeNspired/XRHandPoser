﻿using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractableItemData : MonoBehaviour
{
    public bool canInventory = true;

    public bool canDistanceGrab = true;

    // public int maxInventoryStackSize = 0;

    #region TemporaryFixForSmoothMovementJerkyness

    public bool parentToPlayerOnGrabForSmoothMovement = true;

    private XRGrabInteractable interactable;

    private void Start()
    {
        if (!parentToPlayerOnGrabForSmoothMovement) return;
        interactable = GetComponent<XRGrabInteractable>();
        interactable.onSelectEntered.AddListener(x => Invoke(nameof(SetParentToPlayerForSmoothMovement), Time.deltaTime));
    }

    private void SetParentToPlayerForSmoothMovement()
    {
        if (interactable.selectingInteractor)
            transform.parent = interactable.selectingInteractor.GetComponentInParent<XRRig>().cameraFloorOffsetObject.transform;
    }

    #endregion
}