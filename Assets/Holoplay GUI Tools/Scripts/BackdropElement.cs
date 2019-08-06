using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diorama {
[RequireComponent(typeof(Renderer))]
public class BackdropElement : MonoBehaviour
{
    public enum Type {
        Floor,
        Backdrop
    }

    [Header("This will get tinted by the Scene Settings")]
    public Type type = Type.Floor;

    public bool useCustomUVs = false;
    public Vector4 uvs;

    MaterialPropertyBlock mpr;
    Renderer r;
    string gifURL = "";
    //WebPAnimator animator;

    // void OnEnable() {
    //     if (animator == null) {
    //         animator = GetComponent<WebPAnimator>();
    //         if (!animator)
    //             animator = gameObject.AddComponent<WebPAnimator>();
    //         animator._targetComponent = GetComponent<Renderer>();
    //         animator.enabled = false;
    //         // animator.flipUVs = false;
    //         animator.setEmissionTexture = true;
    //     }

    //     if(!string.IsNullOrEmpty(gifURL)) {
    //         animator.enabled = false;
    //         animator.enabled = true;
    //         animator.LoadURL(gifURL);
    //     }
    // }

    /*
    public void Clear() {
        if (mpr != null) {
            mpr.Clear();
            r.SetPropertyBlock(mpr);
        }
        gifURL = "";
        //if (animator) {
            //animator.Reset();
            //animator.enabled = false;
        //}
    }
    */

    public void SetColor(string property, Color color) {
        if (mpr == null)
            mpr = new MaterialPropertyBlock();
        if (r == null)
            r = GetComponent<Renderer>();
        r.GetPropertyBlock(mpr);
        mpr.SetColor(property, color);
        r.SetPropertyBlock(mpr);
    }

    public void SetTexture(string property, Texture2D tex, Vector2 tiling, Vector2 offset) {
        if (tex == null)
            return;
        if (mpr == null)
            mpr = new MaterialPropertyBlock();
        if (r == null)
            r = GetComponent<Renderer>();
        r.GetPropertyBlock(mpr);
        mpr.SetTexture(property, tex);
        // mpr.SetVector(property + "_ST", new Vector4(tiling.x,tiling.y,offset.x,offset.y));
        r.SetPropertyBlock(mpr);
    }

    public void LoadGif(string url) {
        gifURL = url;
        // if (animator) animator.Reset();
        // if(gameObject.activeInHierarchy) {
        //     animator.enabled = false;
        //     animator.enabled = true;
        //     animator.LoadURL(gifURL);
        //     animator.ResetUVs();
        // }
    }
}
}
