using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Diorama {

public class ButtonOtherClicks : MonoBehaviour, IPointerClickHandler {

    Button button;

    void OnEnable() {
        button = GetComponent<Button>();
    }
 

    public bool RightClickAlsoClicks;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            //Debug.Log("Left click");
        }
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            //Debug.Log("Middle click");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (RightClickAlsoClicks && button) {
                CustomInputModule.ForceRightClick = true;
                try {
                    ExecuteEvents.Execute(button.gameObject, eventData, ExecuteEvents.submitHandler);
                } finally {
                    CustomInputModule.ForceRightClick = false;
                }
        }
        }
    }
}

}
