// Copyright 2019 Looking Glass Factory Inc
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LookingGlass
{
public class CustomInputModule : StandaloneInputModule {

    public static CustomInputModule Instance;

    public PointerEventData GetPointerData() {
        return m_PointerData.ContainsKey(kMouseLeftId) ? m_PointerData[kMouseLeftId] : null;
    }

    public static CustomInputModule current {
        get {
            return EventSystem.current.currentInputModule as CustomInputModule;
        }
    }

    public static bool IsRightClick() => ForceRightClick;

    public static bool ForceRightClick;

    static List<RaycastResult> _raycastResults = new List<RaycastResult>();
    static int _lastRaycastFrame = -1;

    public static void RaycastMouse(List<RaycastResult> results) {
        if (_lastRaycastFrame != Time.frameCount) {
            _lastRaycastFrame = Time.frameCount;
        
            var pointerData = new PointerEventData (EventSystem.current) { pointerId = -1 };
            pointerData.position = Input.mousePosition;

            _raycastResults.Clear();
            EventSystem.current.RaycastAll(pointerData, _raycastResults);
        }

        results.Clear();
        results.AddRange(_raycastResults);
    }

    public static bool InputFieldHasFocus() {
        var eventSystem = EventSystem.current;
        if (eventSystem != null) {
            var obj = eventSystem.currentSelectedGameObject;
            if (obj != null) {
                if (obj.GetComponent<UnityEngine.UI.InputField>() != null)
                    return true;
                if (obj.GetComponent<TMPro.TMP_InputField>() != null)
                    return true;
            }
        }
        return false;
    }
}
}