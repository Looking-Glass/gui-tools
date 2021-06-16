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
    static internal int _lastCamGUID;

    // public bool useTargetTexture = false;

    void Start() {
        if (Holoplay.Instance == null) {
            Debug.LogWarning("[Holoplay] No holoplay detected for 3D cursor!");
            enabled = false;
            return;
        }
        DestroyCamera();
        if (!cam)
            CreateCamera();
    }

    void CreateCamera() {
        var hp = Holoplay.Instance;
        cam = new GameObject("UI Camera").AddComponent<Camera>();
        cam.gameObject.hideFlags = HideFlags.DontSave;
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
        
        cam.enabled = false;
        _lastCamGUID = cam.gameObject.GetInstanceID();

        // We assign a rendertexture to get to match viewport size to Holoplay size
        // if (useTargetTexture) {
        //     var uiRT = RenderTexture.GetTemporary(
        //         Mathf.FloorToInt(hp.quiltSettings.viewWidth),
        //         Mathf.FloorToInt(hp.quiltSettings.viewHeight)
        //     );
        //     cam.targetTexture = uiRT;
        // }

        GetComponent<Canvas>().worldCamera = cam;
    }

    void DestroyCamera() {
        var oldCam = GameObject.Find(_lastCamGUID.ToString());
        if (oldCam) {
            Debug.Log(oldCam);
            cam = oldCam.GetComponent<Camera>();
        }
        if (cam == null)
            return;
        if (cam.targetTexture)
            cam.targetTexture.Release();
        if (Application.isPlaying)
            Destroy(cam.gameObject);
        else
            DestroyImmediate(cam.gameObject);
        _lastCamGUID = -1;
        cam = null;
    }

    void Update() {
        if (!cam)
            CreateCamera();
        
        var hp = Holoplay.Instance;
        cam.fieldOfView = hp.cam.fieldOfView;
        cam.nearClipPlane = hp.cam.nearClipPlane;
        cam.farClipPlane = hp.cam.farClipPlane;

        cam.transform.SetPositionAndRotation(
            hp.cam.transform.position + hp.cam.transform.forward * -hp.GetCamDistance(), 
            hp.cam.transform.rotation
        );
    }
	
}
}