﻿// Copyright 2019 Looking Glass Factory Inc
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LookingGlass
{
public class SyncCamera : MonoBehaviour
{
    public Camera withCamera;
    Camera cam;

    void OnEnable() {
        cam = GetComponent<Camera>();
    }

    public bool SyncEveryFrameAutomatically;

    void Update() {
        if (SyncEveryFrameAutomatically) {
            Sync();
        }
    }

    public void Sync() {
        cam.fieldOfView = withCamera.fieldOfView;
        cam.nearClipPlane = withCamera.nearClipPlane;
        cam.farClipPlane = withCamera.farClipPlane;
    }
}
}