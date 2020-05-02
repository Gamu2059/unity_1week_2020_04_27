using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Param/ThornGenerate", fileName = "param.thorn_generate.asset")]
public class ThornGenerateParameter : ScriptableObject
{
    [SerializeField]
    private ThornController m_Prefab;
    public ThornController Prefab => m_Prefab;

    [SerializeField]
    private ThornGenerateUnitParameter[] m_UnitParameters;
    public ThornGenerateUnitParameter[] UnitParameters => m_UnitParameters;

    [SerializeField]
    private GradientSet m_GradientSet;
    public GradientSet GradientSet => m_GradientSet;

    [SerializeField]
    private GradientSet m_ShadowGradientSet;
    public GradientSet ShadowGradientSet => m_ShadowGradientSet;

    [SerializeField]
    private Vector2 m_ZRange;

    [SerializeField]
    private float m_InitYPos;
    public float InitYPos => m_InitYPos;
    
    public float GetZ()
    {
        return m_ZRange.GetRandomValue();
    }
}

[Serializable]
public class ThornGenerateUnitParameter
{
    [SerializeField]
    private float m_BeginProgress;

    [SerializeField]
    private float m_EndProgress;

    [SerializeField]
    private Vector2 m_XRange;

    [SerializeField]
    private AnimationCurve m_NextGenerateTimeWithPS;

    [SerializeField]
    private AnimationCurve m_MoveSpeedWithPS;

    [SerializeField]
    private Vector2 m_XMoveSpeed;

    [SerializeField]
    private int m_Damage;
    public int Damage => m_Damage;

    public bool IsValidProgress(float progress)
    {
        return progress >= m_BeginProgress && progress <= m_EndProgress;
    }

    public float GetNextGenerateTime(float playerSkill)
    {
        return m_NextGenerateTimeWithPS.Evaluate(playerSkill);
    }

    public float GetMoveSpeed(float playerSkill)
    {
        return m_MoveSpeedWithPS.Evaluate(playerSkill);
    }

    public float GetX()
    {
        return m_XRange.GetRandomValue();
    }

    public float GetXMoveSpeed()
    {
        return m_XMoveSpeed.GetRandomValue();
    }
}
