using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[ExecuteInEditMode(), RequireComponent(typeof(MaskableGraphic))]
public class ShadowCastingUI : MonoBehaviour {

    public enum ShadowMode {
        All,
        First,
        Last,
        None
    }
    public enum ColorWrapMode {
        Repeat,
        Clamp
    }

    public Mesh mesh;
    public Material material;
    public ShadowMode shadowCastMode = ShadowMode.All;
    public ShadowMode shadowReceiveMode = ShadowMode.All;
    public ColorWrapMode colorWrapMode = ColorWrapMode.Repeat;
    public bool squareUVs = false;
    public float UVsize = 1;
    public bool cutoff = false;
    public bool sdfMaterial = false;
    [Range(0, 1)] public float smoothness = 1;
    [Range(0, 1)] public float sdfThreshold = 0;
    [Range(0, 1)] public float sdfMarginOffset = 0;
    public Color[] colors = new Color[] {};
    [Range(-.15f,.3f)] public float margin = .1f;

    [Range(1,20)] public int planes = 4;
    [Range(0.01f, 1)] public float separation = .143f;
    [Range(0, .2f)] public float shuffleScale = .022f;
    [Range(0, 1)] public float shuffleSeparation = .342f;
    [Range(0, .03f)] public float shuffleRotation = 0;
    [Range(0.01f, 1)] public float sizeIncrease = .094f;
    [Range(0, 1)] public float cornerRandomize = 0;
    public int seed = 0;
    public Texture2D texture;
    public Texture2D textureBackground;

    MaterialPropertyBlock mpr;
    static Vector3[] _v3s;
    static Matrix4x4[] _ms;

    void OnEnable() {
        if (texture == null) {
            // texture = Resources.Load<Texture2D>("square-soft");
        }
    }

    void LateUpdate () {
        if (!mesh)
            return;
        if (mpr == null)
            mpr = new MaterialPropertyBlock();
        mpr.Clear();

        var img = GetComponent<MaskableGraphic>();
        var rt = GetComponent<RectTransform>();

        if (_v3s == null) _v3s = new Vector3[] { };
        System.Array.Resize(ref _v3s, 4);
        if (_ms == null) _ms = new Matrix4x4[] { };
        System.Array.Resize(ref _ms, planes);

        rt.GetLocalCorners(_v3s);
        var w = _v3s[2].x - _v3s[0].x;
        var h = _v3s[2].y - _v3s[0].y;

        var lastState = Random.state;
        Random.InitState(seed);
        //if (img is Image && (img as Image).sprite)
        //    mpr.SetTexture("_MainTex", (img as Image).sprite.texture);
        var m = transform.localToWorldMatrix;
        for(int i = 0; i < planes; i++) {
            var depth = .01f;
            if (i > 0) {
                depth += i * separation;
                depth += (-1 + Random.value * 2) * shuffleSeparation * separation;
            }
            var r = transform.rotation;
            if (i != 0 && shuffleRotation > 0)
                r = Quaternion.Euler(r.eulerAngles + Random.rotationUniform.eulerAngles * shuffleRotation);
            m = Matrix4x4.TRS(
                transform.position + transform.forward * depth * transform.localScale.z, 
                r,
                new Vector3(
                    (w * transform.lossyScale.x + margin + sdfMarginOffset) + (sizeIncrease * i),
                    (h * transform.lossyScale.y + margin + sdfMarginOffset) + (sizeIncrease * i),
                    1
                ) + Vector3.one * (-1 + Random.value * 2) * shuffleScale
            );
            var s = ShadowCastingMode.Off;
            if (shadowCastMode == ShadowMode.All ||
                shadowCastMode == ShadowMode.First && i == 0 ||
                shadowCastMode == ShadowMode.Last && i == planes - 1)
                s = ShadowCastingMode.TwoSided;
            bool receiveShadows = false;
            if (shadowReceiveMode == ShadowMode.All ||
                shadowReceiveMode == ShadowMode.First && i == 0 ||
                shadowReceiveMode == ShadowMode.Last && i == planes - 1)
                receiveShadows = true;

            var c = img.color;
            if (colors.Length > 0) {
                if (colorWrapMode == ColorWrapMode.Clamp)
                    c = colors[(int)Mathf.Clamp(i, 0, colors.Length - 1)];
                else if (colorWrapMode == ColorWrapMode.Repeat)
                    c = colors[(int)Mathf.Repeat(i, colors.Length)];
            }
            c.a = 1;
            mpr.SetColor("_Color", c);
            mpr.SetFloat("_SkewAmount", cornerRandomize);
            mpr.SetFloat("_SkewSeed", seed / 100f + i);

            var t = texture;
            if (textureBackground && i > 0)
                t = textureBackground;
            if (t)
                mpr.SetTexture("_MainTex", t);

            if (squareUVs) {
                var aspect = w / h;
                var size = UVsize;
                var offset = i * seed / 100f;
                mpr.SetVector("_MainTex_ST", new Vector4(aspect * size, size, offset, offset));
                mpr.SetVector("_BumpMap_ST", new Vector4(aspect * size, size, offset, offset));
            }

            if (cutoff)
                mpr.SetFloat("_Cutoff", 1);
            if (sdfMaterial) {
                mpr.SetFloat("_SDFThreshold", sdfThreshold);
                mpr.SetFloat("_Smoothness", smoothness);
            }

            Graphics.DrawMesh(mesh, m, material, gameObject.layer, null, 0, mpr, s, receiveShadows);
        }
        Random.state = lastState;
        
	}
}
