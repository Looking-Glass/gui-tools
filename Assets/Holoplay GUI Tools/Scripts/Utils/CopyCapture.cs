using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloPlay;

namespace Diorama {

// When using multiple Holoplay captures, this class helps you keep a secondary capture's 
// values synced up with the "main" capture.
[ExecuteInEditMode]
public class CopyCapture : MonoBehaviour
{
    public Capture capture;
    public bool copyTransform = true;
    Capture me;

    void OnEnable() {
        me = GetComponent<Capture>();
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

        me.Fov = capture.Fov;
        me.Size = capture.Size;
        me.nearClipFactor = capture.nearClipFactor;
        me.farClipFactor = capture.farClipFactor;
    }
}

}