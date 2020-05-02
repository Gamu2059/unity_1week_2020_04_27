using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Param/GrassGenerate", fileName = "param.grass_generate.asset")]
public class GrassGenerateParameter : ScriptableObject
{
    [SerializeField]
    private GrassGenerateAwakeParameter[] m_AwakeParameters;
    public GrassGenerateAwakeParameter[] AwakePrameters => m_AwakeParameters;

    [SerializeField, Tooltip("通常の生成パラメータ")]
    private GrassGenerateUnitParameter[] m_UnitParameters;
    public GrassGenerateUnitParameter[] UnitParameters => m_UnitParameters;

    [SerializeField, Tooltip("移動速度")]
    private float m_MoveSpeed;
    public float MoveSpeed => m_MoveSpeed;

    [SerializeField, Tooltip("生成位置のZ座標")]
    private float m_UnitDataZPos;
    public float UnitDataZPos => m_UnitDataZPos;
}

[Serializable]
public class GrassGenerateAwakeParameter
{
    [SerializeField]
    private GrassController m_Prefab;
    public GrassController Prefab => m_Prefab;

    [SerializeField]
    private Vector2 m_XRange;

    [SerializeField]
    private Vector2 m_ZRange;

    [SerializeField]
    private int m_GenerateNum;
    public int GenerateNum => m_GenerateNum;

    [SerializeField]
    private RandomGetGradientSet m_RandomGetGradientSet;

    public float GetX()
    {
        return m_XRange.GetRandomValue();
    }

    public float GetZ()
    {
        return m_ZRange.GetRandomValue();
    }

    public GradientSet GetGradientSet()
    {
        return m_RandomGetGradientSet.GetGradientSet();
    }
}

[Serializable]
public class GrassGenerateUnitParameter
{
    [SerializeField]
    private float m_BeginProgress;

    [SerializeField]
    private float m_EndProgress;

    [SerializeField]
    private GrassController m_Prefab;
    public GrassController Prefab => m_Prefab;

    [SerializeField]
    private Vector2 m_XRange;

    [SerializeField]
    private AnimationCurve m_MinNextGenerateTime;

    [SerializeField]
    private AnimationCurve m_MaxNextGenerateTime;

    [SerializeField]
    private RandomGetGradientSet m_RandomGetGradientSet;

    public bool IsValidProgress(float progress)
    {
        return progress >= m_BeginProgress && progress <= m_EndProgress;
    }

    public float GetX()
    {
        return m_XRange.GetRandomValue();
    }

    public float GetNextGenerateTime(float progress)
    {
        var min = m_MinNextGenerateTime.Evaluate(progress);
        var max = m_MaxNextGenerateTime.Evaluate(progress);
        return UnityEngine.Random.Range(min, max);
    }

    public GradientSet GetGradientSet()
    {
        return m_RandomGetGradientSet.GetGradientSet();
    }
}