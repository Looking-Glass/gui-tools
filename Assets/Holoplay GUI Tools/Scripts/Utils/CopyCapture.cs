// Copyright 2019 Looking Glass Factory Inc
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LookingGlass
{
// When using multiple Holoplay captures, this class helps you keep a secondary capture's 
// values synced up with the "main" capture.
[ExecuteInEditMode]
public class CopyCapture : MonoBehaviour
{
    public Holoplay capture;
    public bool copyTransform = true;
    Holoplay me;

    void OnEnable() {
        me = GetComponent<Holoplay>();
    }

    void Update()
    {
        if (!capture) return;

        var t = capture.transform;

        if (copyTransform) {
            transform.position = t.position;
            transform.rotation = t.rotation;
            transform.localScale = t.localScale;
        }

        me.cam.fieldOfView = capture.cam.fieldOfView;
        me.size = capture.size;
        me.nearClipFactor = capture.nearClipFactor;
        me.farClipFactor = capture.farClipFactor;
    }
}
}