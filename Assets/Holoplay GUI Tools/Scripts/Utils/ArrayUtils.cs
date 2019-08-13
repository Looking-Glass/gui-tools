// Copyright 2019 Looking Glass Factory Inc
using System.Collections;
using UnityEngine;

namespace LookingGlass
{
public static class ArrayUtils {

    public static T Pick<T>(this T[] array) {
        return array[Mathf.FloorToInt(Random.value * array.Length)];
    }
}
}