using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TooltipPosition = TooltipManager.TooltipPosition;
public class ShowTooltip : MonoBehaviour
{
    public string text = "";
    public TooltipPosition position = TooltipPosition.Upper;

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
