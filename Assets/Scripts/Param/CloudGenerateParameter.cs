using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Param/CloudGenerate", fileName = "param.cloud_generate.asset")]
public class CloudGenerateParameter : ScriptableObject
{
    [SerializeField]
    private CloudGenerateAwakeParameter m_AwakePrameter;
    public CloudGenerateAwakeParameter AwakeParameter => m_AwakePrameter;

    [SerializeField]
    private CloudGenerateUnitParameter[] m_UnitParameters;
    public CloudGenerateUnitParameter[] UnitParameters => m_UnitParameters;

    [SerializeField]
    private Vector2 m_MoveSpeed;

    [SerializeField]
    private float m_UnitDataZPos;
    public float UnitDataZPos => m_UnitDataZPos;

    public float GetMoveSpeed()
    {
        return m_MoveSpeed.GetRandomValue();
    }
}

[Serializable]
public class CloudGenerateAwakeParameter
{
    [SerializeField]
    private CloudController[] m_Prefabs;
    public CloudController[] Prefabs => m_Prefabs;

    [SerializeField]
    private Vector2 m_XRange;

    [SerializeField]
    private Vector2 m_YRange;

    [SerializeField]
    private Vector2 m_ZRange;

    [SerializeField]
    private int m_GenerateNum;
    public int GenerateNum => m_GenerateNum;

    [SerializeField]
    private GradientSet m_GradientSet;
    public GradientSet GradientSet => m_GradientSet;

    public float GetX()
    {
        return m_XRange.GetRandomValue();
    }

    public float GetY()
    {
        return m_YRange.GetRandomValue();
    }

    public float GetZ()
    {
        return m_ZRange.GetRandomValue();
    }
}

[Serializable]
public class CloudGenerateUnitParameter
{
    [SerializeField]
    private float m_BeginProgress;

    [SerializeField]
    private float m_EndProgress;

    [SerializeField]
    private CloudController[] m_Prefabs;
    public CloudController[] Prefabs => m_Prefabs;

    [SerializeField]
    private Vector2 m_XRange;

    [SerializeField]
    private Vector2 m_YRange;

    [SerializeField]
    private AnimationCurve m_MinNextGenerateTime;

    [SerializeField]
    private AnimationCurve m_MaxNextGenerateTime;

    [SerializeField]
    private GradientSet m_GradientSet;
    public GradientSet GradientSet => m_GradientSet;

    public bool IsValidProgress(float progress)
    {
        return progress >= m_BeginProgress && progress <= m_EndProgress;
    }

    public float GetX()
    {
        return m_XRange.GetRandomValue();
    }

    public float GetY()
    {
        return m_YRange.GetRandomValue();
    }

    public float GetNextGenerateTime(float progress)
    {
        var min = m_MinNextGenerateTime.Evaluate(progress);
        var max = m_MaxNextGenerateTime.Evaluate(progress);
        return UnityEngine.Random.Range(min, max);
    }
}