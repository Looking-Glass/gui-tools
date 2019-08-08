using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class RepeatRenderer : MonoBehaviour {

    public Color color = Color.white;

    [Range(1, 40)] public int planes = 4;
    [Range(0.001f, 1)] public float separation = .143f;
    public bool receiveShadows = false;
    public UnityEngine.Rendering.ShadowCastingMode shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    public bool invert = false;
    public bool animate = false;
    public float animateSpeed = 1;
    public AnimationCurve animateCurve = AnimationCurve.Linear(0,0,1,1);

    MaterialPropertyBlock mpr;
    List<Matrix4x4> _ms = new List<Matrix4x4>();
    MeshRenderer meshRenderer;
    Graphic uiGraphic;
    Mesh mesh;
    private void OnEnable() {
        if (mpr == null) mpr = new MaterialPropertyBlock();
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer)
            mesh = GetComponent<MeshFilter>().sharedMesh;
        uiGraphic = GetComponent<Graphic>();

        UpdateMatrices();
        //if (Application.isPlaying && uiGraphic) {
        //    for (int i = 0; i < planes; i++) {
        //        var go = Instantiate(transform);
        //        go.transform.SetParent(transform);
        //        var depth = separation + i * separation;
        //        go.transform.localPosition = Vector3.forward * depth;
        //    }
        //}
    }
    void UpdateMatrices() {
        var m = transform.localToWorldMatrix;
        _ms.Clear();
        for (int i = 0; i < planes; i++) {
            var depth = separation + i * separation;

            m = Matrix4x4.TRS(
                transform.TransformPoint(Vector3.forward * depth * (invert ? -1 : 1)),
                transform.rotation,
                transform.lossyScale
            );
            _ms.Add(m);
        }
    }

    void LateUpdate () {
        int p = planes;
        //if (!Application.isPlaying)p
        if (Application.isPlaying && animate) {
            p = Mathf.FloorToInt(Mathf.Lerp(0, planes, animateCurve.Evaluate(Time.time * animateSpeed)));
        }
        UpdateMatrices();
        
        if (meshRenderer && mesh) {
            meshRenderer.GetPropertyBlock(mpr);
            mpr.SetColor("_Color", color);
            for (int i = 0; i < p; i++) {
                Graphics.DrawMesh(mesh, _ms[i], meshRenderer.sharedMaterial, gameObject.layer, null, 0, mpr, shadowCastingMode, receiveShadows);
            }
        }

        
        
        //Graphics.DrawMeshInstanced(mesh, 0, meshRenderer.sharedMaterial, _ms, mpr, shadowCastingMode, receiveShadows);

    }
}
