using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Diorama {
using TooltipPosition = TooltipManager.TooltipPosition;
public class ShowTooltip : MonoBehaviour
{
    public string text = "";
    public TooltipPosition position = TooltipPosition.Upper;

    const int PREVIEW_FONT_SIZE = 12; // in world units

    bool didEnable;

    void OnEnable() => didEnable = true;
    void OnDisable() => TooltipManager.Instance?.OnTooltipEnabled(this, false);

    void Update() {
        if (didEnable) {
            didEnable = false;
            TooltipManager.Instance?.OnTooltipEnabled(this, true);
        }
    }

    // Deprecated
    public Vector3 Placement { get => transform.position + Vector3.up * 20 * (position == TooltipPosition.Upper ? 1 : (position == TooltipPosition.Lower ? -1 : 0)); }

    void OnDrawGizmosSelected() {
        Gizmos.DrawWireCube(
            Placement,
            new Vector3(Mathf.Max(4, text.Length) * PREVIEW_FONT_SIZE, PREVIEW_FONT_SIZE, 0));
    }

}
}
