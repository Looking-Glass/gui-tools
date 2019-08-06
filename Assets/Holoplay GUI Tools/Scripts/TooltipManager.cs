using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Diorama {

public class TooltipManager : MonoBehaviour
{
    public GameObject tooltip;
    public TextMeshProUGUI text;
    public TextMeshProUGUI text2;
    public Vector3 offset = Vector3.up * 20;
    public bool moveToObject = true;

    public enum TooltipPosition {
        Upper,
        Center,
        Lower
    }

    public static TooltipManager Instance;

    ShowTooltip lastInfo;
    bool dirty = false;

    void OnEnable() {
        Instance = this;
        tooltip.SetActive(false);
    }

    void OnDisable() {
        tooltipGameObjects.Clear();
    }

    Dictionary<GameObject, ShowTooltip> tooltipGameObjects = new Dictionary<GameObject, ShowTooltip>();

    internal void OnTooltipEnabled(ShowTooltip showTooltip, bool enabled) {
        if (enabled)
            tooltipGameObjects.Add(showTooltip.gameObject, showTooltip);
        else
            tooltipGameObjects.Remove(showTooltip.gameObject);
    }

    List<RaycastResult> _raycasts = new List<RaycastResult>();

    void LateUpdate() {
        CustomInputModule.RaycastMouse(_raycasts);
        var didFind = false;
        foreach (var r in _raycasts) {
            ShowTooltip showTooltip = null;
            if (tooltipGameObjects.TryGetValue(r.gameObject, out showTooltip)) {
                if (showTooltip != lastInfo) {
                    lastInfo = showTooltip;
                    dirty = true;
                }
                didFind = true;
                break;
            }
        }
        if (!didFind && lastInfo != null) {
            lastInfo = null;
            dirty = true;
        }
        
        if (!dirty) return;

        dirty = false;

        tooltip.SetActive(lastInfo != null);
        if (!lastInfo) return;

        text.text = text2.text = lastInfo.text;
        if (moveToObject) {
            var p = lastInfo.GetComponent<RectTransform>().position;
            p += transform.TransformPoint(
                offset * (lastInfo.position == TooltipPosition.Upper ? 1 : (lastInfo.position == TooltipPosition.Lower ? -1 : 0))
            );
            tooltip.transform.position = p;
        }
    }
}
}
