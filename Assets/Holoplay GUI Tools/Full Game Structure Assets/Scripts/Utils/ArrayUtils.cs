using System.Collections;
using UnityEngine;

public static class ArrayUtils {

    public static T Pick<T>(this T[] array) {
        return array[Mathf.FloorToInt(Random.value * array.Length)];
    }
}