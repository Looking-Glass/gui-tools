using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Diorama {
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SdfIcon)), CanEditMultipleObjects()]
public class SdfIconEditor : Editor {
    override public void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
            (target as SdfIcon).SetDirty();
    }
}
#endif
[RequireComponent(typeof(Image)), ExecuteInEditMode()]
public class SdfIcon : MonoBehaviour, IMaterialModifier
{

    [Range(0,1)] public float threshold = .5f;
    [Range(0,1)] public float smoothness = .2f;

    public Material material;
    Material _modifiedMaterial;
    
    Image image;
    void OnEnable() {
        image = GetComponent<Image>();
        _modifiedMaterial = Instantiate(material);
    }
    
    void Update() {
        if (material) {
            if (image.material != material)
                image.material = material;
            
        }
    }

    public Material GetModifiedMaterial(Material baseMaterial) {
        // Debug.Log("boop");
        /// TODO: do better
        if (_modifiedMaterial == null)
            _modifiedMaterial = Instantiate(material);
        _modifiedMaterial.SetFloat("_SDFThreshold", threshold);
        _modifiedMaterial.SetFloat("_Smoothness", smoothness);
        return _modifiedMaterial;
    }

    internal void SetDirty() {
        if (image)
            image.SetMaterialDirty();
    }
}
}