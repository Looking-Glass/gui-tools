using UnityEngine;
using UnityEditor;
using CatlikeCoding.SDFToolkit;

[CustomEditor(typeof(SdfIcon)), CanEditMultipleObjects()]
public class SdfIconEditor : Editor {

    override public void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
            (target as SdfIcon).SetDirty();

        EditorGUILayout.Space();
        if (GUILayout.Button("SDF Generator...", GUILayout.MaxWidth(175))) {
            SdfGeneratorWindow.OpenWindow((target as SdfIcon).gameObject);
        }
    }
    
}
