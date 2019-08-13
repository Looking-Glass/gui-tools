// Copyright 2019 Looking Glass Factory Inc
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LookingGlass
{
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
        if (material)  
            _modifiedMaterial = Instantiate(material);
    }
    
    void Update() {
        if (material != null && image.material != material)
            image.material = material;
    }

    public Material GetModifiedMaterial(Material baseMaterial) {
        if (_modifiedMaterial == null)
            _modifiedMaterial = Instantiate(material);
        _modifiedMaterial.SetFloat("_SDFThreshold", threshold);
        _modifiedMaterial.SetFloat("_Smoothness", smoothness);
        return _modifiedMaterial;
    }

    public void SetDirty() {
        if (image)
            image.SetMaterialDirty();
    }
}
}