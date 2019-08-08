using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineMove : MonoBehaviour
{
    public float frequency = 1;
    public Vector3 amount = Vector3.zero;
    public float timeOffset;

    Vector3 _originalPos;

    void Start()
    {
        _originalPos = transform.localPosition;
    }

    void Update()
    {
        transform.localPosition = _originalPos + amount * Mathf.Sin((Time.time + timeOffset) * frequency) * transform.localScale.magnitude;
    }
}
