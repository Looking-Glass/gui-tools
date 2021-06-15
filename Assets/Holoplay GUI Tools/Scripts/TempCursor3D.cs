//Copyright 2017-2019 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

// #define USE_DEBUG_TEXTURE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Cursor3D.cs from Holoplay with added support for pixel-perfect object ID
///     This class needs to be merged into Holoplay SDK, does not contain latest SDK changes
/// </summary>

namespace LookingGlass {
    [HelpURL("https://docs.lookingglassfactory.com/Unity/Scripts/Cursor3D/")]
	public class TempCursor3D : MonoBehaviour {

		private static TempCursor3D instance;
		public static TempCursor3D Instance {
			get {
				if (instance != null) return instance;
				instance = FindObjectOfType<TempCursor3D>();
				return instance;
			}
		}
		Holoplay holoplay { get{ return Holoplay.Instance; } }
		[Tooltip("Disables the OS cursor at the start")]
		public bool disableSystemCursor = true;
		[Tooltip("Should the cursor scale follow the size of the Holoplay?")]
		public bool relativeScale = true;
		[System.NonSerialized] public Texture2D depthNormals;
		[System.NonSerialized] public Texture2D objectIDPixel;
		[System.NonSerialized] public Shader depthOnlyShader;
		[System.NonSerialized] public Shader readDepthPixelShader;
		[System.NonSerialized] public Material readDepthPixelMat;
		public GameObject cursorGameObject;
		private bool cursorGameObjectExists;
		private bool frameRendered;
		private Camera cursorCam;

		public Shader replacementShader;
		public LayerMask cullingMask;

		private Vector3 worldPos;
		private Vector3 localPos;
		private Vector3 normal;
		private Quaternion rotation;
		private Quaternion localRotation;
		private bool overObject;
		MeshRenderer cursorMeshRenderer;

		#if USE_DEBUG_TEXTURE
		public RenderTexture debugTexture;
		#endif

		// returnable coordinates and normals
		public Vector3 GetWorldPos() { Update(); return worldPos; }
		public Vector3 GetLocalPos() { Update(); return localPos; }
		public Vector3 GetNormal() { Update(); return normal; }
		public Quaternion GetRotation() { Update(); return rotation; }
		public Quaternion GetLocalRotation() { Update(); return localRotation; }
		public bool GetOverObject() { Update(); return overObject; }

		void Start() {
			if (disableSystemCursor) Cursor.visible = false;
			cursorGameObjectExists = cursorGameObject != null;
		}

		void OnEnable() {
			depthOnlyShader = Shader.Find("Holoplay/DepthOnly");
			readDepthPixelShader = Shader.Find("Holoplay/ReadDepthPixel");
			if (readDepthPixelShader != null) 
				readDepthPixelMat = new Material(readDepthPixelShader);
			depthNormals = new Texture2D( 1, 1, TextureFormat.ARGB32, false, true);
			objectIDPixel = new Texture2D(1, 1, ObjectIDTexFormat, mipChain: false, linear: true);
			cursorCam = new GameObject("cursorCam").AddComponent<Camera>();
			cursorCam.transform.SetParent(transform);
			// cursorCam.gameObject.hideFlags = HideFlags.DontSave;
			cursorMeshRenderer = GetComponentInChildren<MeshRenderer>();
		}

		void OnDisable() {
			if (cursorCam.gameObject != null) {
				if (Application.isPlaying)
					Destroy(cursorCam.gameObject);
				else
					DestroyImmediate(cursorCam.gameObject);
			}
		}

		const RenderTextureFormat ObjectIDRTFormat = RenderTextureFormat.RFloat;
		const TextureFormat ObjectIDTexFormat =      TextureFormat.RFloat;

		Vector2 GetMousePos01() {
			// copy single pixel and sample it
			// this keeps the ReadPixels from taking forever
			Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
			float monitorW = Screen.width;
			float monitorH = Screen.height;
			int activeDisplays = 0; // check if multiple displays are active
			foreach (var d in Display.displays) {
				if (d.active) activeDisplays++;	
			}

			int monitor = 0;
			if (Application.platform == RuntimePlatform.WindowsPlayer && activeDisplays > 1) {
				mousePos = Display.RelativeMouseAt(new Vector3(Input.mousePosition.x, Input.mousePosition.y));
				monitor = Mathf.RoundToInt(mousePos.z);
				if (Display.displays.Length > monitor) {
					monitorW = Display.displays[monitor].renderingWidth;
					monitorH = Display.displays[monitor].renderingHeight;
				}
			}
			return new Vector2(mousePos.x / monitorW, mousePos.y / monitorH);
		}

		// raw values from the depth buffer
		float worldUnitsFromCamera;
		Vector3 decodedViewNormal;
		Vector2 mousePos01;

		// state while locked
		float lockedWorldUnitsFromCamera;
		Vector3 lockedNormal;
		Vector2 lockedMousePos01;

		LockMode lockMode;

		// the program can request that our 3D cursor "locks" in different ways
		internal enum LockMode {
			None,		// act normally
			Depth,		// act normally, but stay at exactly the same depth value
			XY,			// lock in X and Y
			Y			// lock in Y
		}

		internal void SetLockMode(LockMode lockMode) {
			this.lockMode = lockMode;
			if (lockMode == LockMode.Depth) {
				lockedWorldUnitsFromCamera = worldUnitsFromCamera;
				lockedNormal = decodedViewNormal;
			} else if (lockMode == LockMode.XY || lockMode == LockMode.Y) {
				lockedMousePos01 = mousePos01;
			}
		}

		void Update() {
			if (holoplay == null) {
				Debug.LogWarning("[Holoplay] No holoplay detected for 3D cursor!");
				enabled = false;
				return;
			}

			if (frameRendered) return; // don't update if frame's been rendered already
			cursorCam.CopyFrom(holoplay.cam);

			mousePos01 = GetMousePos01();
			if (lockMode == LockMode.XY)
				mousePos01 = lockedMousePos01;
			else if (lockMode == LockMode.Y)
				mousePos01.y = lockedMousePos01.y;

			// // Hide the cursor mesh if the mouse is over a different monitor.
			// if (mousePos01.x < 0 || mousePos01.y < 0 || mousePos01.x > 1 || mousePos01.y > 1) {
			// 	cursorMeshRenderer.enabled = false;
			// 	return;
			// }
			// cursorMeshRenderer.enabled = true;

			var w = holoplay.quiltSettings.viewWidth;
			var h = holoplay.quiltSettings.viewHeight;
			var colorRT    = RenderTexture.GetTemporary(w, h, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1);
			var objectIDRT = RenderTexture.GetTemporary(w, h, 0, ObjectIDRTFormat, RenderTextureReadWrite.Linear, 1);
			colorRT.filterMode = FilterMode.Point; // important to avoid some weird edge cases
			objectIDRT.filterMode = FilterMode.Point;
			var depthNormalsRT  = RenderTexture.GetTemporary(1, 1, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			var objectIDPixelRT = RenderTexture.GetTemporary(1, 1, 0, ObjectIDRTFormat, RenderTextureReadWrite.Linear);

			// we set multiple render targets on the cursor cam. See
			// DepthOnly.shader's MRTOut struct for where we write to these
			// RenderTextures.
			var bufs = new RenderBuffer [] {
				colorRT.colorBuffer,
				objectIDRT.colorBuffer,
			};
			cursorCam.SetTargetBuffers(bufs, colorRT.depthBuffer);

			cursorCam.allowMSAA = false;
			float halfNormal = 0.5f;
			Color bgColor = new Color(halfNormal, halfNormal, 1f, 1f);
			cursorCam.backgroundColor = QualitySettings.activeColorSpace == ColorSpace.Gamma ? 
				bgColor : bgColor.gamma;	
			cursorCam.clearFlags = CameraClearFlags.SolidColor;
			cursorCam.cullingMask &= ~Physics.IgnoreRaycastLayer;
			cursorCam.cullingMask = cullingMask;
			// disable cursor game object before rendering
			bool cursorObjectEnabled = true;
			if (cursorGameObjectExists) {
				cursorObjectEnabled = cursorGameObject.activeSelf;
				if (cursorObjectEnabled) {
					cursorGameObject.SetActive(false);
				}
			}

			{
				var mpr = new MaterialPropertyBlock();
				foreach (var r in FindObjectsOfType<Renderer>()) {
					// var placableProp = r.GetComponentInParent<PlaceableProp>();
					// if (placableProp == null) continue;

					// float hoverFloat = placableProp.GetHoverFloat();
					float hoverFloat = r.gameObject.GetInstanceID() / 1000f;
					//Debug.Log(hoverFloat + " " + r);

					r.GetPropertyBlock(mpr);
					mpr.SetFloat("propID", hoverFloat);
					// mpr.SetInt("uvInverted", placableProp.UVSAreInverted() ? 1 : 0);
					
					// var mergedTex = placableProp.GetMergedTexture();
					// mpr.SetInt("hasMergedTexture", mergedTex != null ? 1 : 0);
					// if (mergedTex)
					// 	mpr.SetTexture("mergedTexture", mergedTex);

					r.SetPropertyBlock(mpr);
				}

			}
			cursorCam.RenderWithShader(depthOnlyShader, "RenderType");
			cursorCam.enabled = false;
			cursorCam.targetTexture = null;
			// turn cursor object back on
			if (cursorGameObjectExists && cursorObjectEnabled) {
				cursorGameObject.SetActive(true);
			}
				
			var samplePoint = new Vector4(mousePos01.x, mousePos01.y);
			if (mousePos01.x <= 0 || mousePos01.y <= 0 || mousePos01.x >= 1 || mousePos01.y >= 1)
				samplePoint = new Vector4(float.NegativeInfinity, float.NegativeInfinity);

			readDepthPixelMat.SetVector("samplePoint", samplePoint);

			// read the depth and normal from the first render target.
			Color enc;
			{
				Graphics.Blit(colorRT, depthNormalsRT, readDepthPixelMat);
				RenderTexture.active = depthNormalsRT;
				depthNormals.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);
				depthNormals.Apply();
				enc = depthNormals.GetPixel(0, 0);
			}

			// the second render target contains our object ids.
			// here we blit the object id texture to a 1x1 texture, reading the
			// pixel we're interested in.
			{
				Graphics.Blit(objectIDRT, objectIDPixelRT, readDepthPixelMat);

				#if USE_DEBUG_TEXTURE
				if (debugTexture == null)
					debugTexture = new RenderTexture(objectIDRT.width, objectIDRT.height, 0, objectIDRT.format, RenderTextureReadWrite.Linear);
				Graphics.Blit(objectIDRT, debugTexture);
				#endif

				RenderTexture.active = objectIDPixelRT;
				objectIDPixel.ReadPixels(new Rect(0, 0, 1, 1), 0, 0, false);
				objectIDPixel.Apply();
				var objectIDCol = objectIDPixel.GetPixel(0, 0);
				RenderTexture.active = null;
				float hoverIndex = objectIDCol.r;
				// Debug.Log("testing r hover index " + hoverIndex);
				// var hovered = PlaceableProp.GetForHoverIndex(hoverIndex);
				// PropPlacer.Instance._SetCursor3DHovered(hovered);
			}

			// find world pos from depth
			float depth = DecodeFloatRG(enc);
			overObject = depth < 1f;
			if (!overObject) {
				depth = holoplay.nearClipFactor / (holoplay.nearClipFactor + holoplay.farClipFactor);
			}
			// bool hit = true;
			// depth = hit ? depth : 0.5f; // if nothing hit, default depth
			depth = cursorCam.nearClipPlane + depth * (cursorCam.farClipPlane - cursorCam.nearClipPlane);
			worldUnitsFromCamera = depth;
			if (lockMode == LockMode.Depth)
				depth = lockedWorldUnitsFromCamera;
			Vector3 screenPoint = new Vector3(mousePos01.x, mousePos01.y, depth);
			worldPos = cursorCam.ViewportToWorldPoint(screenPoint);
			localPos = holoplay.transform.InverseTransformPoint(worldPos);
			if (isActiveAndEnabled)
				transform.position = worldPos;


			// find world normal based on view normal
			normal = DecodeViewNormalStereo(enc);
			// normals = hit ? normals : Vector3.forward; // if nothing hit, default normal
			decodedViewNormal = normal;
			if (lockMode == LockMode.Depth)
				normal = lockedNormal;
			normal = cursorCam.cameraToWorldMatrix * normal;
			rotation = Quaternion.LookRotation(-normal);
			localRotation = Quaternion.Inverse(holoplay.transform.rotation) * rotation;
			if (isActiveAndEnabled) {
				transform.rotation = rotation;
				// might as well set size here as well
				if (relativeScale) 
					transform.localScale = Vector3.one * holoplay.size * 0.1f;
			}

			// reset settings
			RenderTexture.ReleaseTemporary(colorRT);
			RenderTexture.ReleaseTemporary(depthNormalsRT);
			RenderTexture.ReleaseTemporary(objectIDRT);
			RenderTexture.ReleaseTemporary(objectIDPixelRT);
			// set frame rendered
			frameRendered = true;
		}

		void LateUpdate() {
			frameRendered = false;
		}

		// copied from UnityCG.cginc
		Vector3 DecodeViewNormalStereo(Color enc4) {
			float kScale = 1.7777f;
			Vector3 enc4xyz = new Vector3(enc4.r, enc4.g, enc4.b);
			Vector3 asdf = Vector3.Scale(enc4xyz, new Vector3(2f*kScale, 2f*kScale, 0f));
			Vector3 nn = asdf + new Vector3(-kScale, -kScale, 1f);
			float g = 2.0f / Vector3.Dot(nn, nn);
			Vector2 nnxy = new Vector3(nn.x, nn.y) * g;
			Vector3 n = new Vector3(nnxy.x, nnxy.y, g - 1f);
			return n;
		}

		// copied from UnityCG.cginc
		float DecodeFloatRG(Color enc) {
			Vector2 encxy = new Vector2(enc.b, enc.a);
			Vector2 kDecodeDot = new Vector2(1.0f, 1.0f/255.0f);
			return Vector2.Dot(encxy, kDecodeDot);
		}
	}
}
