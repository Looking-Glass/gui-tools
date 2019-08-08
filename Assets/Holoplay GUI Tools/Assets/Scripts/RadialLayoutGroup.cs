using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Diorama
{
    // Source: https://forum.unity.com/threads/radial-ui-layout-in-unity-code-included.355676/
    [ExecuteInEditMode]
    public class RadialLayoutGroup : LayoutGroup {
 
        public float radius;
        public bool clockwise;
        [Range (0f, 360f)]
        public float minAngle;
        [Range (0f, 360f)]
        public float maxAngle = 360f;
        [Range (0f, 360f)]
        public float startAngle;
        public bool customRebuild;
 
        [Header ("Child rotation")]
        [Range (0f, 360f)]
        public float startElementAngle;
        public bool rotateElements;
 
        [Header("Child size")]
        public bool expandChildSize;
        public float childSize = 50f;
 
        #region Properties
 
        public float Radius {
            get {
                return radius;
            }
            set {
                if (radius != value) {
                    radius = value;
                    OnValueChanged ();
                }
            }
        }
 
        public bool Clockwise {
            get {
                return clockwise;
            }
            set {
                if (clockwise != value) {
                    clockwise = value;
                    OnValueChanged ();
                }
            }
        }
 
        public float MinAngle {
            get {
                return minAngle;
            }
            set {
                if (minAngle != value) {
                    minAngle = value;
                    OnValueChanged ();
                }
            }
        }
 
        public float MaxAngle {
            get {
                return maxAngle;
            }
            set {
                if (maxAngle != value) {
                    maxAngle = value;
                    OnValueChanged ();
                }
            }
        }
 
        public float StartAngle {
            get {
                return startAngle;
            }
            set {
                if (startAngle != value) {
                    startAngle = value;
                    OnValueChanged ();
                }
            }
        }
 
        public bool CustomRebuild {
            get {
                return customRebuild;
            }
            set {
                if (customRebuild != value) {
                    customRebuild = value;
                    OnValueChanged ();
                }
            }
        }
 
        public bool ExpandChildSize {
            get {
                return expandChildSize;
            }
            set {
                if (expandChildSize != value) {
                    expandChildSize = value;
                    OnValueChanged ();
                }
            }
        }
 
        public float ChildSize {
            get {
                return childSize;
            }
            set {
                if (childSize != value) {
                    childSize = value;
                    OnValueChanged ();
                }
            }
        }
 
        public RectTransform SelfTransform {
            get {
                return rectTransform;
            }
        }
 
        public void OnValueChanged () {
            if (customRebuild) {
                CalculateRadial ();
            }
        }
 
        #endregion
 
        protected override void OnEnable () {
            base.OnEnable ();
            CalculateRadial ();
        }
 
        public override void SetLayoutHorizontal () {
 
        }
 
        public override void SetLayoutVertical () {
 
        }
 
        public override void CalculateLayoutInputVertical () {
            CalculateRadial ();
        }
 
        public override void CalculateLayoutInputHorizontal () {
            CalculateRadial ();
        }

#if UNITY_EDITOR
        protected override void OnValidate () {
            base.OnValidate ();
            needToCalculate = true;
        }

        bool needToCalculate;

        void Update() {
            if (needToCalculate) {
                needToCalculate = false;
                CalculateRadial();
            }
        }
#endif
 
        public void CalculateRadial () {
            int activeChildCount = 0;
            List<RectTransform> childList = new List<RectTransform> ();
            for (int i = 0; i < transform.childCount; i++) {
                RectTransform child = transform.GetChild (i) as RectTransform;
                LayoutElement childLayout = child.GetComponent<LayoutElement> ();
                if (child == null || !child.gameObject.activeSelf || (childLayout != null && childLayout.ignoreLayout)) {
                    continue;
                }
                childList.Add (child);
                activeChildCount++;
            }
 
            m_Tracker.Clear ();
            if (activeChildCount == 0) {
                return;
            }
 
            rectTransform.sizeDelta = new Vector2 (radius, radius) * 2f;
            float sAngle = ((360f) / activeChildCount) * (activeChildCount - 1f);
 
            float anglOffset = minAngle;
            if (anglOffset > sAngle) {
                anglOffset = sAngle;
            }
 
            float buff = sAngle - anglOffset;
 
            float maxAngl = 360f - maxAngle;
            if (maxAngl > sAngle) {
                maxAngl = sAngle;
            }
 
            if (anglOffset > sAngle) {
                anglOffset = sAngle;
            }
 
            buff = sAngle - anglOffset;
 
            float fOffsetAngle = ((buff - maxAngl)) / (activeChildCount - 1f);
            float fAngle = startAngle + anglOffset;
 
            bool expandChilds = expandChildSize;
            DrivenTransformProperties drivenTransformProperties = DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Pivot;
            if (expandChildSize) {
                drivenTransformProperties |= DrivenTransformProperties.SizeDeltaX;
                drivenTransformProperties |= DrivenTransformProperties.SizeDeltaY;
            }
            if (rotateElements) {
                drivenTransformProperties |= DrivenTransformProperties.Rotation;
            }
 
            if (clockwise) {
                fOffsetAngle *= -1f;
            }
 
            for (int i = 0; i < childList.Count; i++) {
                RectTransform child = childList[i];
                if (child != null && child.gameObject.activeSelf) {
                    //Adding the elements to the tracker stops the user from modifiying their positions via the editor.
                    m_Tracker.Add (this, child, drivenTransformProperties);
                    Vector3 vPos = new Vector3 (Mathf.Cos (fAngle * Mathf.Deg2Rad), Mathf.Sin (fAngle * Mathf.Deg2Rad), 0);
                    child.localPosition = vPos * radius;
                    //Force objects to be center aligned, this can be changed however I'd suggest you keep all of the objects with the same anchor points.
                    child.anchorMin = child.anchorMax = child.pivot = new Vector2 (0.5f, 0.5f);
 
                    float elementAngle = startElementAngle;
                    if (rotateElements) {
                        elementAngle += fAngle;
                        child.localEulerAngles = new Vector3 (0f, 0f, elementAngle);
                    } else {
                        child.localEulerAngles = new Vector3 (0f, 0f, elementAngle);
                    }
 
                    if (expandChilds) {
                        Vector2 expandSize = child.sizeDelta;
                        if (expandChildSize)
                            expandSize.x = expandSize.y = childSize;
                        child.sizeDelta = expandSize;
                    }
 
                    fAngle += fOffsetAngle;
                }
 
            }
 
        }
 
    }
}
