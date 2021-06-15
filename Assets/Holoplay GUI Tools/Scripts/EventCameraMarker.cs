// Copyright 2019 Looking Glass Factory Inc
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LookingGlass
{
public class EventCameraMarker : MonoBehaviour
{
    // This component is just used as a marker.

    public Camera cam { get { return Holoplay.Instance.cam; } }
}
}