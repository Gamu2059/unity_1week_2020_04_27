using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Param/HeartGenerate", fileName = "param.heart_generate.asset")]
public class HeartGenerateParameter : ScriptableObject
{
    [SerializeField]
    private HeartController m_HeartPrefab;
    public HeartController HeartPrefab => m_HeartPrefab;

    [SerializeField]
    private HeartController m_SpecialHeartPrefab;
    public HeartController SpecialHeartPrefab => m_SpecialHeartPrefab;

    [SerializeField]
    private int m_HeartPoint;
    public int HeartPoint => m_HeartPoint;

    [SerializeField]
    private int m_SpecialHeartPoint;
    public int SpecialHeartPoint => m_SpecialHeartPoint;

    [SerializeField]
    private HeartGenerateUnitParameter m_UnitParameter;
    public HeartGenerateUnitParameter UnitParameter => m_UnitParameter;

    [SerializeField]
    private SpecialHeartGenerateUnitParameter[] m_SpecialUnitParameters;
    public SpecialHeartGenerateUnitParameter[] SpecialUnitParameters => m_SpecialUnitParameters;

    [SerializeField]
    private float m_InitZPos;
    public float InitZPos => m_InitZPos;

    [SerializeField]
    private AnimationCurve m_MoveSpeedWithPS;

    public float GetMoveSpeed(float playerSkill)
    {
        return m_MoveSpeedWithPS.Evaluate(playerSkill);
    }
}

[Serializable]
public class HeartGenerateUnitParameter
{
    [SerializeField]
    private Vector2 m_XRange;

    [SerializeField]
    private AnimationCurve m_NextGenerateTimeWithPS;

    public float GetNextGenerateTime(float playerSkill)
    {
        return m_NextGenerateTimeWithPS.Evaluate(playerSkill);
    }

    public float GetX()
    {
        return m_XRange.GetRandomValue();
    }
}

[Serializable]
public class SpecialHeartGenerateUnitParameter
{
    [SerializeField]
    private float m_FireTimingProgress;
    public float FireTimingProgress => m_FireTimingProgress;

    [SerializeField]
    private float[] m_XPositions;
    public float[] XPositions => m_XPositions;

    [SerializeField]
    private float m_GenerateInterval;
    public float GenerateInterval => m_GenerateInterval;
}
