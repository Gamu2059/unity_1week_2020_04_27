using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Param/TreeGenerate", fileName = "param.tree_generate.asset")]
public class TreeGenerateParameter : ScriptableObject
{
    [SerializeField]
    private TreeGenerateAwakeParameter[] m_AwakeParameters;
    public TreeGenerateAwakeParameter[] AwakeParameters => m_AwakeParameters;

    [SerializeField]
    private TreeGenerateUnitParameter[] m_UnitParameters;
    public TreeGenerateUnitParameter[] UnitParameters => m_UnitParameters;

    [SerializeField]
    private float m_UnitDataZPos;
    public float UnitDataZPos => m_UnitDataZPos;
}

[Serializable]
public class TreeGenerateAwakeParameter
{
    [SerializeField]
    private TreeController m_Prefab;
    public TreeController Prefab => m_Prefab;

    [SerializeField]
    private Vector2 m_XRange;

    [SerializeField]
    private Vector2 m_YRange;

    [SerializeField]
    private Vector2 m_ZRange;

    [SerializeField]
    private Vector3 m_MoveSpeed;
    public Vector3 MoveSpeed => m_MoveSpeed;

    [SerializeField]
    private int m_GenerateNum;
    public int GenerateNum => m_GenerateNum;

    [SerializeField]
    private GradientSet m_GradientSet;
    public GradientSet GradientSet => m_GradientSet;

    [SerializeField]
    private AnimationCurve m_AlphaCurve;
    public AnimationCurve AlphaCurve => m_AlphaCurve;

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
public class TreeGenerateUnitParameter
{
    [SerializeField]
    private float m_BeginProgress;

    [SerializeField]
    private float m_EndProgress;

    [SerializeField]
    private TreeController m_Prefab;
    public TreeController Prefab => m_Prefab;

    [SerializeField]
    private Vector2 m_XRange;

    [SerializeField]
    private Vector2 m_YRange;

    [SerializeField]
    private Vector3 m_MoveSpeed;
    public Vector3 MoveSpeed => m_MoveSpeed;

    [SerializeField]
    private AnimationCurve m_MinNextGenerateTime;

    [SerializeField]
    private AnimationCurve m_MaxNextGenerateTime;

    [SerializeField]
    private GradientSet m_GradientSet;
    public GradientSet GradientSet => m_GradientSet;

    [SerializeField]
    private AnimationCurve m_AlphaCurve;
    public AnimationCurve AlphaCurve => m_AlphaCurve;

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