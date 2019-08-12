using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CatlikeCoding.SDFToolkit;

public class SdfGeneratorWindow : EditorWindow
{
    [MenuItem("Window/SDF Generator")]
    public static void OpenWindow() { OpenWindow(null); }
    public static void OpenWindow(GameObject parent)
    {
        // Get existing open window or if none, make a new one:
        var window = (SdfGeneratorWindow)EditorWindow.GetWindow(typeof(SdfGeneratorWindow));
        window.minSize = new Vector2(100, 300);
        window.maxSize = new Vector2(600, Screen.height);
        window.position = new Rect(100,100,Screen.width * .4f, Screen.height * .35f);
        window.Show();

        if (parent != null) {
            var img = parent.GetComponent<UnityEngine.UI.Image>();
            if (img)
                window.originalTex2D = img.sprite.texture;
        }
    }

    [SerializeField] internal Texture2D originalTex2D;
    [SerializeField] Texture2D generatedTex2D;
    [SerializeField] float distanceInner = 15;
    [SerializeField] float distanceOuter = 80;
    [SerializeField] bool AutoUpdate = true;

    const float distancePostProcess = 1.0f;
    const RGBFillMode rgbFillMode = RGBFillMode.Distance;
    const TextureFormat GeneratedTextureFormat = TextureFormat.ARGB32;
    const string extension = "png";
    bool dirty = false;

    void OnGUI() {
        var prevOG = originalTex2D;

        originalTex2D = (Texture2D)EditorGUILayout.ObjectField("Input Texture", originalTex2D, typeof(Texture2D), false);
        EditorGUI.BeginChangeCheck();
        distanceInner = EditorGUILayout.FloatField("Inside Distance", distanceInner);
        distanceOuter = EditorGUILayout.FloatField("Outer Distance", distanceOuter);
        dirty = originalTex2D && (EditorGUI.EndChangeCheck() || generatedTex2D == null || originalTex2D != prevOG);

        GUI.enabled = originalTex2D != null;
        AutoUpdate = EditorGUILayout.ToggleLeft("Generate Automatically", AutoUpdate);
        if (GUILayout.Button("Generate SDF Texture...") || (AutoUpdate && originalTex2D && dirty))
            CreateSDFTexture();
        GUI.enabled = true;

        EditorGUILayout.Separator();

        if (generatedTex2D != null) {
            var r = EditorGUILayout.GetControlRect(false, this.position.width, GUILayout.Width(this.position.width));
            GUI.color = Color.black;
            GUI.DrawTexture(r, EditorGUIUtility.whiteTexture);
            GUI.color = Color.white;
            GUI.DrawTexture(r, generatedTex2D);
        }

        EditorGUILayout.Separator();

        GUI.enabled = generatedTex2D != null;
        if (GUILayout.Button("Save SDF Texture as Asset..."))
            SaveSDFTexture();
        GUI.enabled = true;

    }

    void CreateSDFTexture() {
        if (generatedTex2D == null ||
                generatedTex2D.width != originalTex2D.width ||
                generatedTex2D.height != originalTex2D.height) {
            generatedTex2D = new Texture2D(originalTex2D.width, originalTex2D.height, GeneratedTextureFormat, false);
        }

        var sourcePath = AssetDatabase.GetAssetPath(originalTex2D);
        var texImporter = TextureImporter.GetAtPath(sourcePath) as TextureImporter;

        var compression = texImporter.textureCompression;
        bool uncompressed = compression == TextureImporterCompression.Uncompressed;
        bool readable = texImporter.isReadable;

        // mark as uncompressed and readable if necessary
        if (!readable || !uncompressed) {
            texImporter.isReadable = true;
            texImporter.textureCompression = TextureImporterCompression.Uncompressed;
            AssetDatabase.ImportAsset(sourcePath);
        }

        SDFTextureGenerator.Generate(originalTex2D, generatedTex2D, distanceInner, distanceOuter, distancePostProcess, rgbFillMode);

        // restore texture settings.
        if (!readable || !uncompressed) {
            texImporter.isReadable = readable;
            texImporter.textureCompression = compression;
            AssetDatabase.ImportAsset(sourcePath);
        }

        generatedTex2D.Apply();
    }

    void SaveSDFTexture() {
        var dirName = new System.IO.FileInfo(AssetDatabase.GetAssetPath(originalTex2D)).DirectoryName;
        var newFilename = originalTex2D.name + " SDF";
        string sdfFilePath = EditorUtility.SaveFilePanel("Save SDF Texture", dirName, newFilename, extension);
        if (string.IsNullOrEmpty(sdfFilePath)) return;

        var isNewTexture = !System.IO.File.Exists(sdfFilePath);
        System.IO.File.WriteAllBytes(sdfFilePath, generatedTex2D.EncodeToPNG());
        AssetDatabase.Refresh();

        int assetsIdx = sdfFilePath.Replace("\\","/").IndexOf("Assets/");
        string relToProject = assetsIdx >= 0 ? sdfFilePath.Substring(assetsIdx) : sdfFilePath;
        if (isNewTexture) {
            var texImporter = TextureImporter.GetAtPath(relToProject) as TextureImporter;
            if (texImporter != null) {
                texImporter.textureType = TextureImporterType.Sprite;
                texImporter.textureCompression = TextureImporterCompression.Uncompressed;
                AssetDatabase.ImportAsset(relToProject);
            }
        }
    }

}
