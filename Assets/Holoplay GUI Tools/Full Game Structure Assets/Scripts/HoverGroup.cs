using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//
// Determines if we're hovering over this object or any of its children.
//
[System.Serializable]
public class GameObjectEvent : UnityEvent<GameObject> {}

public class HoverGroup : MonoBehaviour {
	public bool Verbose;

	public UnityEvent HoverBegin;
	public GameObjectEvent HoverOver;
	public UnityEvent HoverEnd;

	bool hoveringThisFrame;

	public bool MouseIsHoveringOver { get => isActiveAndEnabled && hoveringThisFrame; }

	internal static List<HoverGroup> hovering = new List<HoverGroup>();

	internal GameObject CurrentHoveredChild;

	void Update() {
		var lastHover = hoveringThisFrame;

		GameObject obj;
		hoveringThisFrame = IsHoveringOverMe(out obj);

		if (!lastHover && hoveringThisFrame) {
			if (Verbose) Debug.Log("begin hover " + this);
			HoverBegin?.Invoke();
			hovering.Add(this);
		}

		CurrentHoveredChild = obj;
		if (obj) {
			HoverOver?.Invoke(obj);
		}

		if (lastHover && !hoveringThisFrame) {
			if (Verbose) Debug.Log("finish hover " + this);
			FinishHover();
		}
	}

	void OnDisable() {
		if (hoveringThisFrame)
			FinishHover();
	}

	void FinishHover() {
		HoverEnd?.Invoke();
		hovering.Remove(this);
	}

	int AncestorCount(Transform t) {
		int count = 0;

		while (t) {
			t = t.parent;
			count++;
		}

		return count;
	}

	bool IsInTree(Transform t) {
		while (t) {
			if (t == transform) return true;
			t = t.parent;
		}
		return false;
	}


	public GameObject DeepestCurrentHover() {
		var pointerData = CustomInputModule.current.GetPointerData();
		return pointerData == null ? null : pointerData.hovered
			.Where(o => IsInTree(o.transform))
			.OrderBy(o => -AncestorCount(o.transform))
			.FirstOrDefault();
	}


	bool IsHoveringOverMe(out GameObject overObj) {
		var pointerData = CustomInputModule.current.GetPointerData();
		if (Verbose) Debug.Log(pointerData);
		var raycast = new RaycastResult();
		if (pointerData != null) raycast = pointerData.pointerCurrentRaycast;

		overObj = null;
		if (Verbose) Debug.Log(raycast.gameObject);

		// Walk up the hierarchy and see if the current raycast hit is a child of ours.
		if (raycast.isValid && IsInTree(raycast.gameObject.transform)) {
			overObj = raycast.gameObject;
		}

		if (Verbose) Debug.Log("* " + overObj);

		return overObj != null;
	}
}
