using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LookingGlass {
/*
An example class pairing with ObjectManipulation to change the 3D cursor's
visuals depending on what the user is doing (dragging, hovering).
*/
public class CursorVisuals : MonoBehaviour
{
    public ObjectManipulation objectManipulation;

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
        
        var isHovering = Cursor3D.Instance.GetHoveredObject() != null;
        var isDragging = objectManipulation != null && objectManipulation.IsDragging;
        var dragMode = objectManipulation != null ? objectManipulation.DragMode : CursorDragMode.MovingInXY;

        // Cursor visibility
        if (!isDragging) {
            arrowModel.SetActive(!isHovering);
            hoveringModel.SetActive(isHovering);
            grabModel.SetActive(false);
            grabZModel.SetActive(false);
        } else {
            arrowModel.SetActive(false);
            hoveringModel.SetActive(false);
            grabModel.SetActive(dragMode == CursorDragMode.MovingInXY);
            grabZModel.SetActive(dragMode == CursorDragMode.MovingInXZ);
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