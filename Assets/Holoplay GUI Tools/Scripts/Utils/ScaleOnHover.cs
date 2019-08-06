using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DentedPixel;

public class ScaleOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public bool invertZ = false;
    public float moveZ = 1;
    public Vector3 scaleAmount = Vector3.one;
    float scale = .1f;
    float move = 6f;
    float time = .18f;

    Vector3 _originalPos;
    Vector3 _originalScale;

    bool initialized = false;

    bool scaledUp = false;
    int checkLater;
    bool hovering;

    void OnEnable() {
        checkLater = int.MaxValue;
        if (!initialized) {
            _originalPos = transform.localPosition;
            _originalScale = transform.localScale;
            initialized = true;
        }
	}

    void OnDisable() {
        LeanTween.cancel(gameObject);
        transform.localScale = _originalScale;
        transform.localPosition = _originalPos;        
    }

    void Update() {
        // This is a bit complicated so that even if we encounter a PointerExit
        // while tweening bigger, we do the right thing (which is to finish
        // tweening, and then check if we're hovering or not later.)
        if (Time.frameCount >= checkLater && !LeanTween.isTweening(gameObject)) {
            checkLater = int.MaxValue;
            if (hovering && !scaledUp)
                TweenBigger();
            else if (!hovering && scaledUp)
                TweenSmaller();
        }
    }

    public void OnPointerEnter(PointerEventData data) {
        hovering = true;

        if (!LeanTween.isTweening(gameObject)) {
            TweenBigger();
        } else {
            checkLater = Time.frameCount + 1;
        }
    }

    public void OnPointerExit(PointerEventData data) {
        hovering = false;
        if (!LeanTween.isTweening(gameObject)) {
            TweenSmaller();
        } else {
            checkLater = Time.frameCount + 1;
        }
    }

    void TweenBigger() {
        Assert.IsFalse(LeanTween.isTweening(gameObject));
        LeanTween.scaleX(gameObject, _originalScale.x + scale * scaleAmount.x, time).setEaseInOutCirc();
        LeanTween.scaleZ(gameObject, _originalScale.z + scale * scaleAmount.z, time).setEaseInOutCirc();
        LeanTween.scaleY(gameObject, _originalScale.y + scale * scaleAmount.y, time).setEaseInOutCirc().setDelay(time * .25f);
        LeanTween.moveLocalZ(gameObject, _originalPos.z + move * (invertZ ? -1 : 1) * moveZ, time).setEaseInOutCirc();
        scaledUp = true;
    }

    void TweenSmaller() {
        Assert.IsFalse(LeanTween.isTweening(gameObject));
        LeanTween.scale(gameObject, _originalScale, time).setEaseInOutCirc();
        LeanTween.moveLocalZ(gameObject, _originalPos.z, time).setEaseInOutCirc();
        scaledUp = false;
    }
}
