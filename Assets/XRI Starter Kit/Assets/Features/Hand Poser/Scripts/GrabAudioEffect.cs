﻿// Author MikeNspired. 

using UnityEngine;


namespace MikeNspired.UnityXRHandPoser
{
    public class GrabAudioEffect : AudioRandomize
    {
        public UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

        private void Start()
        {
            if (interactable)
                interactable.selectEntered.AddListener(x => Play());
            else
                Debug.Log("XRGrabInteractable not found on : " + gameObject.name + " to play hand grabbing sound effect");
        }

        protected new void OnValidate()
        {
            base.OnValidate();
            if (!interactable)
                interactable = GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        }
    }
}