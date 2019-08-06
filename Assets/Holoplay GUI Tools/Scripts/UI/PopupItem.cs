using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DentedPixel;

namespace Diorama
{
public class PopupItem : MonoBehaviour
{

    LeanTweenType easeShow = LeanTweenType.easeOutElastic;
    LeanTweenType easeHide = LeanTweenType.easeInSine;
    float timeShow = 1;
    float timeHide = .2f;

    Vector3 originalScale;

    internal bool active;

    void OnEnable() {
        active = gameObject.activeSelf;
        originalScale = transform.localScale;
    }

    public void SetActive(bool bActive, bool setVisibility = true, bool tween = true) {
        active = bActive;
        if (setVisibility) {
            if (bActive)
                Show(tween);
            else
                Hide(tween);
        }
    }
    public void Reset(bool bActive) {
        SetActive(bActive, true, false);
    }

    public void Show(bool tween = true, float delay = 0) {
        if (!active)
            return;
        float t = timeShow;
        var ease = easeShow;

        LeanTween.cancel(gameObject);
        
        gameObject.SetActive(true);
        if (tween) {
            transform.localScale = originalScale * .25f;

            LeanTween.scaleX(gameObject, originalScale.x, t * 1.2f)
                .setEase(ease)
                .setDelay(delay);
                //.setOnComplete(() => { gameObject.SetActive(false); });
            LeanTween.scaleY(gameObject, originalScale.y, t)
                .setEase(ease)
                .setDelay(delay);
            LeanTween.scaleZ(gameObject, originalScale.z, t)
                .setEase(ease)
                .setDelay(delay);
        } else {
            transform.localScale = originalScale;
        }
    }
    public void Hide(bool tween = true, float delay = 0) {
        if (!gameObject.activeSelf)
            return;
        float t = timeHide;
        var ease = easeHide;

        LeanTween.cancel(gameObject);

        if (tween) {
            LeanTween.scaleX(gameObject, originalScale.x, t * 1.2f)
                .setEase(ease)
                .setDelay(delay)
                .setOnComplete(() => { gameObject.SetActive(false); });
            LeanTween.scaleY(gameObject, originalScale.y, t)
                .setEase(ease)
                .setDelay(delay);
            LeanTween.scaleZ(gameObject, originalScale.z, t)
                .setEase(ease)
                .setDelay(delay);
        } else {
            gameObject.SetActive(false);
        }
    }

}
}