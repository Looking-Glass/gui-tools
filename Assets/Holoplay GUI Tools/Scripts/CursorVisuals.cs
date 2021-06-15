using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LookingGlass {
public class CursorVisuals : MonoBehaviour
{
    [Header("References")]
    public GameObject arrowModel;
    public GameObject hoveringModel;
    public GameObject grabModel;
    public GameObject grabZModel;

    [Header("Easing")]
    public bool easePosition = false;
    public float moveSpeed = 0.2f;
    public bool easeRotation = true;
    public float rotateSpeed = 0.2f;
    public Vector3 clampAngle = new Vector3(0,0,0);

    // TODO: Use EventSystem to toggle these
    [Header("Debug")]
    public bool IsDragging = false;
    public bool IsHovering = false;
    public enum DragMode { MovingInXY, MovingInXZ }
    public DragMode dragMode;

    // private
    Vector3 finalPosition;
    Quaternion finalRotation;

    void Start() 
    {
        if (!Cursor3D.Instance)
        {
            Debug.LogWarning("[Holoplay] No 3D cursor detected, add the Cursor3D script to your scene.");
            enabled = false;
            return;
        }
        finalPosition = Cursor3D.Instance.GetWorldPos();
        finalRotation = Cursor3D.Instance.GetRotation();
    }

    void LateUpdate()
    {
        if (!Cursor3D.Instance)
            return;

        // Cursor visibility
        if (!IsDragging) {
            arrowModel.SetActive(!IsHovering);
            hoveringModel.SetActive(IsHovering);
            grabModel.SetActive(false);
            grabZModel.SetActive(false);
        } else {
            arrowModel.SetActive(false);
            hoveringModel.SetActive(false);
            grabModel.SetActive(dragMode == DragMode.MovingInXY);
            grabZModel.SetActive(dragMode == DragMode.MovingInXZ);
        }

        // Cursor position with easing
        var targetPosition = Cursor3D.Instance.GetWorldPos();
        var targetRotation = Cursor3D.Instance.GetRotation();

        if (easePosition)
            finalPosition = Vector3.Slerp(finalPosition, targetPosition, Time.deltaTime * moveSpeed);
        else
            finalPosition = targetPosition;

        if (easeRotation)
            finalRotation = Quaternion.SlerpUnclamped(finalRotation, targetRotation, Time.deltaTime * rotateSpeed);
        else   
            finalRotation = targetRotation;
        
        transform.position = finalPosition;
        transform.rotation = finalRotation;

        // Grabbing cursors should not be eased
        grabModel.transform.position = grabZModel.transform.position = targetPosition;
        grabModel.transform.rotation = grabZModel.transform.rotation = Quaternion.identity;
    }
}
}