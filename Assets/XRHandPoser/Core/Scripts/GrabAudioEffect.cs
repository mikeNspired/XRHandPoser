﻿// Copyright (c) MikeNspired. All Rights Reserved.

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace MikeNspired.UnityXRHandPoser
{
    public class GrabAudioEffect : AudioRandomize
    {
        public XRBaseInteractable interactable;

        private void Start()
        {
            if (interactable)
                interactable.selectEntered.AddListener(x => PlaySound());
            else
                Debug.Log("XRGrabInteractable not found on : " + gameObject.name + " to play hand grabbing sound effect");
        }

        protected new void OnValidate()
        {
            base.OnValidate();
            if (!interactable)
                interactable = GetComponentInParent<XRBaseInteractable>();
        }
    }
}