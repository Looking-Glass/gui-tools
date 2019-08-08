using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using CatlikeCoding.SDFToolkit;

namespace Diorama {

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

/**
 * This class generates an SDF texture from a black and white icon, so that you can use it
 * in UI and tweak its "smoothness" value.
 */
[RequireComponent(typeof(Image)), ExecuteInEditMode]
public class SoftIcon : MonoBehaviour, IMaterialModifier
{
    [Header("Runtime Softness Values")]
    [Range(0,1)] public float threshold = .5f;
    [Range(0,1)] public float smoothness = .2f;

    [Header("SDF Generation")]
    public Texture2D sourceTexture;
    public Texture2D generatedTexture;
    public float insideDistance = 2.3f;
    public float outsideDistance = 6f;
    public float postProcessDistance = 0f;
    public RGBFillMode rgbFillMode = RGBFillMode.Distance;

    public Material material;

    [SerializeField] string lastGuid;
    [SerializeField] long lastLocalId;
    [SerializeField] float lastInsideDistance = -1f;
    [SerializeField] float lastOutsideDistance = -1f;
    [SerializeField] RGBFillMode lastRgbFillMode;

    Material _modifiedMaterial;
    Image image;

    void OnEnable() {
        image = GetComponent<Image>();
        if (material)
            _modifiedMaterial = Instantiate(material);
    }
    
    void Update() {
        if (material && image.material != material)
            image.material = material;

        if (generatedTexture && (image.sprite == null || image.sprite.texture != generatedTexture)) {
            var sprite = Sprite.Create(generatedTexture, new Rect(0.0f, 0.0f, generatedTexture.width, generatedTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
            sprite.name = generatedTexture.name;
            image.sprite = sprite;
        }
#if UNITY_EDITOR
        if (updateQueueTime < 0 && NeedsUpdate()) {
            updateQueueTime = EditorApplication.timeSinceStartup + 0.5;
            EditorApplication.update += OnEditorUpdate;
        }
#endif
    }

#if UNITY_EDITOR
    double updateQueueTime = -1.0;

    internal void MaybeQueueGenerate() {
        if (!NeedsUpdate()) return;

        if (updateQueueTime < 0)
            EditorApplication.update += OnEditorUpdate;

        updateQueueTime = EditorApplication.timeSinceStartup + 0.5;
    }

    void OnEditorUpdate() {
        if (updateQueueTime < 0) {
            EditorApplication.update -= OnEditorUpdate;
            return;
        }

        if (EditorApplication.timeSinceStartup >= updateQueueTime) {
            Generate();
            Assert.IsFalse(NeedsUpdate());
            updateQueueTime = -1.0;
        }
    }

    void Generate () {
        if (generatedTexture == null || generatedTexture.width != sourceTexture.width || generatedTexture.height != sourceTexture.height) {
            generatedTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.ARGB32, false);
        }
        generatedTexture.name = "SDF of " + sourceTexture.name;

        string path = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) {
            Debug.LogError("Cannot work with built-in textures.");
            return;
        }

        if (importer.crunchedCompression) {
            Debug.LogError("You have to disable crunch compression while generating the SDF texture.");
            return;
        }

        bool isReadable = importer.isReadable;
        TextureImporterCompression compression = importer.textureCompression;

        bool uncompressed = compression == TextureImporterCompression.Uncompressed;

        if (!isReadable || !uncompressed) {
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            AssetDatabase.ImportAsset(path);
        }

        // Actually generate the new SDF texture.
        SDFTextureGenerator.Generate(
            sourceTexture, generatedTexture, insideDistance, outsideDistance, postProcessDistance, rgbFillMode);

        if (!isReadable || !uncompressed) {
            importer.isReadable = isReadable;
            importer.textureCompression = compression;
            AssetDatabase.ImportAsset(path);
        }

        generatedTexture.Apply();

        // Now save the texture out to an asset.
        string filePath = Path.Combine(
            new FileInfo(AssetDatabase.GetAssetPath(sourceTexture)).DirectoryName,
            sourceTexture.name + " SDF.png");

        bool isNewTexture = !File.Exists(filePath);
        File.WriteAllBytes(filePath, generatedTexture.EncodeToPNG());
        AssetDatabase.Refresh();

        if (isNewTexture) {
            int relativeIndex = filePath.Replace("\\", "/").IndexOf("Assets/");
            if (relativeIndex >= 0) {
                filePath = filePath.Substring(relativeIndex);
                TextureImporter _importer = TextureImporter.GetAtPath(filePath) as TextureImporter;
                if (_importer != null) {
                    _importer.textureType = TextureImporterType.SingleChannel;
                    _importer.textureCompression = TextureImporterCompression.Uncompressed;
                    AssetDatabase.ImportAsset(filePath);
                }
            }
        }

        string guid;
        long localId;
        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sourceTexture, out guid, out localId)) {
            lastGuid = guid;
            lastLocalId = localId;
        } else {
            Debug.LogError("Exepcted to be able to find GUIDs for source texture.");
        }
        lastOutsideDistance = outsideDistance;
        lastInsideDistance = insideDistance;
        lastRgbFillMode = rgbFillMode;
    }
#endif

    internal bool NeedsUpdate() {
        if (sourceTexture == null)
            return false;

        string guid;
        long localId;
        if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sourceTexture, out guid, out localId))
            return false;

        return (lastGuid != guid || lastLocalId != localId) ||
            !Mathf.Approximately(lastInsideDistance, insideDistance) ||
            !Mathf.Approximately(lastOutsideDistance, outsideDistance) ||
            lastRgbFillMode != rgbFillMode;
    }

    public Material GetModifiedMaterial(Material baseMaterial) {
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

#if UNTIY_EDITOR
[CustomEditor(typeof(SoftIcon))]
public class SoftIconEditor : Editor {
    SerializedProperty
        threshold,
        smoothness,
        rgbFillMode,
        insideDistance,
        outsideDistance,
        postProcessDistance,
        source,
        generatedTexture;

    void OnEnable() {
        threshold = serializedObject.FindProperty("threshold");
        smoothness = serializedObject.FindProperty("smoothness");
        source = serializedObject.FindProperty("sourceTexture");
        generatedTexture = serializedObject.FindProperty("generatedTexture");
        rgbFillMode = serializedObject.FindProperty("rgbFillMode");
        insideDistance = serializedObject.FindProperty("insideDistance");
        outsideDistance = serializedObject.FindProperty("outsideDistance");
        postProcessDistance = serializedObject.FindProperty("postProcessDistance");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(threshold);
        EditorGUILayout.PropertyField(smoothness);

        EditorGUILayout.PropertyField(source);
        //EditorGUILayout.PropertyField(generatedTexture);
        //EditorGUILayout.PropertyField(rgbFillMode);
        EditorGUILayout.PropertyField(insideDistance);
        EditorGUILayout.PropertyField(outsideDistance);
        //EditorGUILayout.PropertyField(postProcessDistance);

        var generated = (Texture2D)generatedTexture.objectReferenceValue;
        if (generated != null) {
            GUILayout.Label("Generated SDF texture:");
            GUILayout.Label(generated, GUILayout.MaxHeight(200));
        }

        if (serializedObject.ApplyModifiedProperties()) {
            var softIcon = target as SoftIcon;
            softIcon.MaybeQueueGenerate();
            softIcon.SetDirty();
        }
    }
}

#endif // UNITY_EDITOR

}
