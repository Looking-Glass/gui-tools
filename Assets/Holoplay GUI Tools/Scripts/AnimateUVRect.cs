using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage)), ExecuteInEditMode()]
public class AnimateUVRect : MonoBehaviour
{
    public float size = .05f;
    public float speed = .5f;

    RawImage rawImage;
    Vector2 offset;
    void OnEnable() {
        rawImage = GetComponent<RawImage>();
        offset = new Vector2(Random.value, Random.value);
    }
    void Update() {
        var r = rawImage.uvRect;
        float t = Time.time * speed;
        r.center = offset + new Vector2(t % 1,t % 1);
        r.size = Vector2.one * size;
        rawImage.uvRect = r;
    }
}
