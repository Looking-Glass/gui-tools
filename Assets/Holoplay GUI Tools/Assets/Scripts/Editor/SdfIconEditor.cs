using UnityEngine;
using UnityEditor;
using CatlikeCoding.SDFToolkit.Editor;

[CustomEditor(typeof(SdfIcon)), CanEditMultipleObjects()]
public class SdfIconEditor : Editor {
    override public void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
            (target as SdfIcon).SetDirty();

        EditorGUILayout.Space();
        if (GUILayout.Button("SDF Generator...", GUILayout.MaxWidth(175))) {
            SDFTextureGeneratorWindow.DidGenerateSDFImage += DidGenerate;
            SDFTextureGeneratorWindow.OpenWindow();
        }
    }

    void DidGenerate(string assetPath)  {
        SDFTextureGeneratorWindow.DidGenerateSDFImage -= DidGenerate;
        var sdfIcon = target as SdfIcon;
        if (!sdfIcon) return;
        var image = sdfIcon.GetComponent<UnityEngine.UI.Image>();
        if (image && !image.sprite) {
            var newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            image.sprite = newSprite;
        }
    }

    
    void OnDisable() {
        SDFTextureGeneratorWindow.DidGenerateSDFImage -= DidGenerate;
    }
}
