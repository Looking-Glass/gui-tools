using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class FindEventCamera : MonoBehaviour {
	Canvas canvas;

	internal Camera cam;

	void OnEnable() => canvas = GetComponent<Canvas>();
	void Start () => FindEventCameraMarker();

    static Camera _FoundEventCamera;

	void FindEventCameraMarker() {
		if (canvas.worldCamera) return;

        if (!_FoundEventCamera) {
            Camera markedCamera = null;
            foreach(var ec in FindObjectsOfType<EventCameraMarker>()) {
                if (ec.enabled && ec.gameObject.activeSelf) {
                    markedCamera = ec.GetComponent<Camera>();
                    break;
                }
            }
            if (!markedCamera) {
                Debug.LogWarning("expected a EventCameraMarker component on a camera in the scene. please add it.");
                return;
            }
            _FoundEventCamera = markedCamera;
        }

		canvas.worldCamera = cam = _FoundEventCamera;
	}
	
}
