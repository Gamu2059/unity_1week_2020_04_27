using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtility
{
    /// <summary>
    /// [a, b]区間を1として、valueがどの割合にあるかを計算する。
    /// </summary>
    public static float CalcRate(float a, float b, float value)
    {
        return (value - a) / (b - a);
    }
}
