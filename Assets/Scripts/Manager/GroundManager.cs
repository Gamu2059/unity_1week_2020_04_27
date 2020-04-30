using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundManager : SingletonMonoBehavior<GroundManager>
{
    private const float DELTA = 0.001f;

    [SerializeField, Tooltip("Z座標に対するY座標の関数")]
    private AnimationCurve m_YPositionFunc;

    [SerializeField, Tooltip("X座標に対するY座標の補正関数")]
    private AnimationCurve m_YPositionCorrectFunc;

    /// <summary>
    /// X座標とZ座標からY座標を取得する
    /// </summary>
    public float GetYPosition(float x, float z)
    {
        return m_YPositionFunc.Evaluate(z) + m_YPositionCorrectFunc.Evaluate(x);
    }

    /// <summary>
    /// Z座標からX軸角度を取得する(本当はX座標も参照したい)
    /// </summary>
    public float GetXAngle(float z)
    {
        // 影の角度を計算する
        var z1 = m_YPositionFunc.Evaluate(z - DELTA);
        var z2 = m_YPositionFunc.Evaluate(z + DELTA);

        // 斜面の法線方向の角度
        var normalAngle = Mathf.Atan2(z2 - z1, DELTA * 2) * Mathf.Rad2Deg;

        // オブジェクトは上向きが0度なので、数値を補正する
        return 90 - normalAngle;
    }
}
