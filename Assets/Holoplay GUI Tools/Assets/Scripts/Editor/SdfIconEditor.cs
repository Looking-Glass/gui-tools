using UnityEngine;
using UnityEditor;
using CatlikeCoding.SDFToolkit.Editor;
using CatlikeCoding.SDFToolkit;

[CustomEditor(typeof(SdfIcon)), CanEditMultipleObjects()]
public class SdfIconEditor : Editor {
    [SerializeField] bool GeneratorOpen;
    Texture2D originalTex2D;
    Texture2D generatedTex2D;
    float distanceInner = 6f;
    float distanceOuter = 3f;

    const float distancePostProcess = 1.0f;
    const RGBFillMode rgbFillMode = RGBFillMode.Distance;
    const TextureFormat GeneratedTextureFormat = TextureFormat.ARGB32;
    const string extension = "png";
    const bool SaveAsSprite = true;


    override public void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
            (target as SdfIcon).SetDirty();

        EditorGUILayout.Space();
        /*
        if (GUILayout.Button("SDF Generator...", GUILayout.MaxWidth(175))) {
            SDFTextureGeneratorWindow.DidGenerateSDFImage += DidGenerate;
            SDFTextureGeneratorWindow.OpenWindow();
        }
        */

        GeneratorOpen = EditorGUILayout.Foldout(GeneratorOpen, "Generate an SDF...");
        if (GeneratorOpen) {
            originalTex2D = (Texture2D)EditorGUILayout.ObjectField("Input Texture", originalTex2D, typeof(Texture2D), false);
            distanceInner = EditorGUILayout.FloatField("Inside Distance", distanceInner);
            distanceOuter = EditorGUILayout.FloatField("Outer Distance", distanceOuter);

            GUI.enabled = originalTex2D != null;
            if (GUILayout.Button("Generate SDF Texture..."))
                CreateSDFTexture();
            GUI.enabled = true;

            GUI.enabled = generatedTex2D != null;
            if (GUILayout.Button("Save SDF Texture as Asset..."))
                SaveSDFTexture();
            GUI.enabled = true;

            if (generatedTex2D != null)
                GUILayout.Label(generatedTex2D);
        }
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
                texImporter.textureType = SaveAsSprite ? TextureImporterType.Sprite : TextureImporterType.SingleChannel;
                texImporter.textureCompression = TextureImporterCompression.Uncompressed;
                AssetDatabase.ImportAsset(relToProject);
            }
        }

        // assign the new image in our field
        var sdfIcon = target as SdfIcon;
        if (!sdfIcon) return;
        var image = sdfIcon.GetComponent<UnityEngine.UI.Image>();
        if (image && !image.sprite) {
            var newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(relToProject);
            if (newSprite)
                image.sprite = newSprite;
        }
    }

    
}
