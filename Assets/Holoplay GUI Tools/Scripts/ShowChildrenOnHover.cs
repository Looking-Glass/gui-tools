// Copyright 2019 Looking Glass Factory Inc
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LookingGlass
{
public class ShowChildrenOnHover : MonoBehaviour {

    public GameObject button;
    public CanvasGroup children;
    public RectTransform hoverArea;
    public float scaleOnHover = 2.25f;

    void Start()
    {
        children.gameObject.SetActive(false);
        hoverArea.gameObject.SetActive(false);
    }

    public void ShowChildren() {
        float time = .2f;
        // Hover button
        LeanTween.cancel(button);
        LeanTween.moveLocalZ(button, 2, time).setEaseInOutCirc();
        // Show children
        LeanTween.cancel(children.gameObject);
        children.transform.localScale = Vector3.one * .75f;
        children.alpha = 1;
        //LeanTween.value(children.gameObject, OnChildAlphaUpdate, children.alpha, 1, time);
        LeanTween.scaleX(children.gameObject, 1, time).setEaseInOutCirc();
        LeanTween.scaleZ(children.gameObject, 1, time).setEaseInOutCirc();
        LeanTween.scaleY(children.gameObject, 1, time).setEaseInOutCirc().setDelay(time * .25f);
        children.gameObject.SetActive(true);

        hoverArea.gameObject.SetActive(true);
        // hoverArea.localScale = Vector3.one * scaleOnHover;
    }
    public void HideChildren() {
        float time = .2f;
        // Hover button
        LeanTween.cancel(button);
        LeanTween.moveLocalZ(button, 0, time).setEaseInOutCirc();
        // Hide children
        LeanTween.cancel(children.gameObject);
        LeanTween.scale(children.gameObject, Vector3.one, time).setEaseInOutCirc();
        // children.alpha = 0; // TODO: tween
        children.gameObject.SetActive(false);
        hoverArea.gameObject.SetActive(false);
        // hoverArea.localScale = Vector3.one;
    }
}
}