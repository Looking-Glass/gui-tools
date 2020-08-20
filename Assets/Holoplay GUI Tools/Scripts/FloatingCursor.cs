// Copyright 2019 Looking Glass Factory Inc
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DentedPixel;

namespace LookingGlass
{
public class FloatingCursor : MonoBehaviour {

    public bool collideWithUI = true;
    public bool collideWithColliders = true;
    public bool hideWhenNoHit = false;
    public float speed = 2.0f;
    public float defaultDistance = 3;
    public LayerMask layerMask;
    public Camera cam;
    public float zWhenNoHit = -0.4f;

    public Vector2 offset;
    public float zOffset = 0;

    GameObject current;

    RaycastHit rh;

    void OnEnable() {
        Cursor.visible = false;
    }
    void OnDisable() {
        Cursor.visible = true;
    }
	void Start () {
        var c = FindObjectOfType<Holoplay>();
        if (c)
            cam = c.cam;
        
		
	}
    List<RaycastResult> _rl = new List<RaycastResult>();
	void LateUpdate () {
        var es = EventSystem.current;
        var currentCam = cam ? cam : Camera.current;
        if (es == null || currentCam == null)
            return;

        var targetPos = transform.position;

        // Get plane position
        targetPos = currentCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x + offset.x, Input.mousePosition.y + offset.y, defaultDistance));
        
        
        if (collideWithColliders && Physics.Raycast(currentCam.ScreenPointToRay(Input.mousePosition), out rh, 30, layerMask.value)) {
            var bounds = rh.collider.bounds;
            //Debug.Log(rh.collider.gameObject.name);
            targetPos = rh.point;
            current = rh.collider.gameObject;
        } else {
            current = null;
        }
        
        if (collideWithUI) {

            // CustomInputModule.Instance.GetPointerData().pointerCurrentRaycast
            var pointerData = new PointerEventData (EventSystem.current) { pointerId = -1 };
            pointerData.position = Input.mousePosition;

		    _rl.Clear();
		    EventSystem.current.RaycastAll(pointerData, _rl);
            if (_rl.Count > 0) {

                current = _rl[0].gameObject;
                targetPos = _rl[0].worldPosition;
            }
            //Debug.Log(CustomInputModule.current.GetPointerData().pointerCurrentRaycast.worldPosition);
            // TODO collide with UI elements
        }
        //transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
        var p = targetPos + -currentCam.transform.forward * zOffset;
        bool hit = current != null || p.z < -0.1f;
        foreach(Transform t in transform) {
            if (hideWhenNoHit)
                t.gameObject.SetActive(hit);
            else
                t.gameObject.SetActive(true);
        }
        if (!hideWhenNoHit && !hit) {
            targetPos = currentCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x + offset.x, Input.mousePosition.y + offset.y, zWhenNoHit));
            p = targetPos;
        }
        transform.position = p;

    }
}

}