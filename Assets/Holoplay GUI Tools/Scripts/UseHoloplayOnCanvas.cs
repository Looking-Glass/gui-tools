// Copyright 2019 Looking Glass Factory Inc
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LookingGlass
{
[RequireComponent(typeof(Canvas)), ExecuteAlways()]
public class UseHoloplayOnCanvas : MonoBehaviour {

    /// <summary>
    /// Creates a Camera just for using on UI events and applies it to a Canvas
    /// <summary>

	static internal Camera cam;

    void Start() {
        if (Holoplay.Instance == null) {
            Debug.LogWarning("[Holoplay] No holoplay detected for 3D cursor!");
            enabled = false;
            return;
        }
        
        if (!cam)
            CreateCamera();
    }

    void CreateCamera() {
        var hp = Holoplay.Instance;
        cam = new GameObject("UI Camera").AddComponent<Camera>();
        cam.gameObject.hideFlags = HideFlags.HideAndDontSave;
        // cam.transform.SetParent(hp.transform);
        cam.transform.SetPositionAndRotation(
            hp.cam.transform.position + hp.cam.transform.forward * -hp.camDist, 
            hp.cam.transform.rotation
        );
        // cam.CopyFrom(Holoplay.Instance.cam);
        cam.fieldOfView = hp.cam.fieldOfView;
        cam.nearClipPlane = hp.cam.nearClipPlane;
        cam.farClipPlane = hp.cam.farClipPlane;
        cam.cullingMask = 0;

        GetComponent<Canvas>().worldCamera = cam;
    }

    void Update() {
        if (!cam)
            CreateCamera();

        var hp = Holoplay.Instance;
        cam.transform.SetPositionAndRotation(
            hp.cam.transform.position + hp.cam.transform.forward * -hp.camDist, 
            hp.cam.transform.rotation
        );
    }
	
}
}