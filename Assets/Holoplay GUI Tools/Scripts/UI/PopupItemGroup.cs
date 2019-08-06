using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DentedPixel;
using System.Linq;

namespace Diorama {
[RequireComponent(typeof(CanvasGroup))]
public class PopupItemGroup : MonoBehaviour
{
    [SerializeField] public List<PopupItem> children = new List<PopupItem>();
    
    bool active = true;
    internal bool Active { get { return active; } }

    CanvasGroup cgr;
    bool Initialized { get { return cgr; } }

    /*
    void OnEnable() {
        return;
        EnsureInitialization();
    }
    */

    void EnsureInitialization() {
        if (Initialized)
            return;
        cgr = GetComponent<CanvasGroup>();
        active = gameObject.activeSelf;
        gameObject.SetActive(true);

        if (children.Count == 0)
            children = GetComponentsInChildren<PopupItem>().ToList();
        if (children.Count == 0) {
            foreach(Transform child in transform) {
                var item = child.GetComponent<PopupItem>();
                if (!item)
                    item = child.gameObject.AddComponent<PopupItem>();
                children.Add(item);
            }
        }
    }

    public void SetActive(bool bActive, bool bSetVisibility = true) {
        Debug.LogError("Using a deprecated function!");
        gameObject.SetActive(bActive);
        /*
        EnsureInitialization();
        // cgr.interactable = bActive;
        active = bActive;
        foreach(var i in children)
            i.SetActive(bActive, bSetVisibility);
        */
    }
    
    public void ShowIfActive(bool immediate = false) {
        EnsureInitialization();
        if (!Active)
            return;
        Show(immediate);
    }
    public void Show(bool immediate = false) {
        EnsureInitialization();
        // cgr.interactable = true;
        foreach(var i in children)
            i.Show(!immediate);
    }

    public void Hide(bool immediate = false) {
        EnsureInitialization();
        // cgr.interactable = false;
        float d = 0;
        foreach(var i in children) {
            i.Hide(!immediate, d += .1f);
        }
    }
}
}
