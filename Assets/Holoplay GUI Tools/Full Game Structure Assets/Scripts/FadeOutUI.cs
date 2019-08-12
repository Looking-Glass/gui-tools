using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutUI : MonoBehaviour
{

    public float idleTime = 5;
    public float fadeOutTime = 5;

    public float scaleIdle = .01f;
    public float scaleAway = .18f;
    public float offsetYAway = -2;

    float lastInputTime;

    UIState state;
    Canvas canvas;
    CanvasGroup canvasGroup;
    Vector3 lastMousePos;
    bool tempFullscreenForced = false;
    public bool hideCursor = true;

    public bool fullscreen = false;
    public System.Action<bool> OnFullscreenModeChange;

    void OnEnable() {
        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        state = FindObjectOfType<UIState>();
        lastMousePos = Input.mousePosition;
    }   

    void Update() {
        if (Vector3.Distance(lastMousePos, Input.mousePosition) > 0 || Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            lastInputTime = Time.time;
        
        if (tempFullscreenForced) {
            float t = Time.time - lastInputTime - idleTime;
            t = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / fadeOutTime));
            canvasGroup.alpha = t;
            // transform.localScale = Vector3.one * Mathf.Lerp(scaleIdle, scaleAway, t);
            // transform.localPosition = Vector3.down * Mathf.Lerp(0, offsetYAway, t);
            bool show = Time.time - lastInputTime < idleTime;
            if (hideCursor)
                Cursor.visible = show;
            canvas.enabled = show;
            transform.localPosition = show ? Vector3.zero : Vector3.right * 999;
            if (show) {
                tempFullscreenForced = false;
                fullscreen = false;
                OnFullscreenModeChange?.Invoke(fullscreen);
            }
            
        } else {
            canvasGroup.alpha = 1;
            transform.localScale = Vector3.one * scaleIdle;
            transform.localPosition = Vector3.zero;
            canvas.enabled = true;
            if (hideCursor && !Cursor.visible)
                Cursor.visible = true;
        }

        lastMousePos = Input.mousePosition;
    }

    public void ForceFullscreen() {
        // transform.localScale = Vector3.one * scaleAway;
        if (hideCursor)
            Cursor.visible = false;
        lastInputTime = Time.time - idleTime - fadeOutTime;
        // transform.localPosition = Vector3.down * offsetYAway;
        transform.localPosition = Vector3.right * 999;
        canvas.enabled = false;
        tempFullscreenForced = true;
        fullscreen = true;
        OnFullscreenModeChange?.Invoke(fullscreen);
    }
}
