using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extension
{
    public static float GetRandomValue(this Vector2 v)
    {
        return Random.Range(v.x, v.y);
    }
}
