﻿// Copyright (c) MikeNspired. All Rights Reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MikeNspired.UnityXRHandPoser
{
    public class MatchTransform : MonoBehaviour
    {
        public Transform positionToMatch;
        public bool unParent = false, matchPosition = true, matchRotation;

        private void Start()
        {
            if (unParent) transform.parent = null;
        }

        private void FixedUpdate()
        {
            if (!positionToMatch) return;
            if (matchPosition)
                transform.position = positionToMatch.position;
            if (matchRotation)
                transform.rotation = positionToMatch.rotation;
        }
    }
}