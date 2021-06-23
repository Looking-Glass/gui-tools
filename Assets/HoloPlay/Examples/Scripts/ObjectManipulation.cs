using UnityEngine;

namespace LookingGlass {

/*
An example class for manipulating objects in the scene by dragging them (using Cursor3D).
*/

[RequireComponent(typeof(Cursor3D))]
class ObjectManipulation : MonoBehaviour {
    const float Z_DRAG_SPEED = 0.003f;

    internal enum DragMode {
        MovingInXZ,
        MovingInXY,
    }

    Cursor3D cursor;
    DragMode dragMode;
    Vector3 originalCursorWorldPos;
    Vector3 originalPos;
    Vector2 originalMousePos;
    GameObject draggingGameObject;

    void OnEnable() {
        cursor = GetComponent<Cursor3D>();
    }

    void Update() {
        var hoveredGameObject = cursor.GetHoveredObject();

        var leftDownThisFrame = Input.GetMouseButtonDown(0);
        var rightDownThisFrame = Input.GetMouseButtonDown(1);

        // Start a drag.
        if ((leftDownThisFrame || rightDownThisFrame) && hoveredGameObject != null) {
            draggingGameObject = hoveredGameObject;

            // Drag in and out of the screen if using the right mouse button, or holding shift.
            var shiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            dragMode = (shiftDown || rightDownThisFrame) ? DragMode.MovingInXZ : DragMode.MovingInXY;

            originalPos = draggingGameObject.transform.position;
            originalCursorWorldPos = cursor.GetWorldPos();
            originalMousePos = Input.mousePosition;

            cursor.SetLockMode(dragMode == DragMode.MovingInXY ? Cursor3D.LockMode.Depth : Cursor3D.LockMode.Y);

            Debug.Log(dragMode + " " + draggingGameObject);
        }

        // End a drag.
        if (!(Input.GetMouseButton(0) || Input.GetMouseButton(1)) && draggingGameObject != null) {
            Debug.Log("Released " + draggingGameObject);
            draggingGameObject = null;
            cursor.SetLockMode(Cursor3D.LockMode.None);
        }

        // Update an object being dragged.
        if (draggingGameObject) {
            if (dragMode == DragMode.MovingInXY) {
                var newPos = originalPos + (cursor.GetWorldPos() - originalCursorWorldPos);
                newPos.z = originalPos.z;
                draggingGameObject.transform.position = newPos;
            } else if (dragMode == DragMode.MovingInXZ) {
                var newPos = originalPos + (cursor.GetWorldPos() - originalCursorWorldPos);
                newPos.y = originalPos.y;
                newPos.z = originalPos.z + (Input.mousePosition.y - originalMousePos.y) * Z_DRAG_SPEED;
                draggingGameObject.transform.position = newPos;
            }
        }
    }

}
}
