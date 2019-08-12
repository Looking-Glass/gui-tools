using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShowTooltip : MonoBehaviour
{
    public string text = "";
    bool didEnable;

    void OnEnable() => didEnable = true;
    void OnDisable() => TooltipManager.Instance?.OnTooltipEnabled(this, false);

    void Update() {
        if (didEnable) {
            didEnable = false;
            TooltipManager.Instance?.OnTooltipEnabled(this, true);
        }
    }

}
