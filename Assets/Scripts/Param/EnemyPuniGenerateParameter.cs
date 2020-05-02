using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Param/EnemyPuniGenerate", fileName = "param.enemy_puni_generate.asset")]
public class EnemyPuniGenerateParameter : ScriptableObject
{
    [SerializeField]
    private EnemyPuniController m_Prefab;
    public EnemyPuniController Prefab => m_Prefab;

    [SerializeField]
    private EnemyPuniGenerateUnitParameter[] m_UnitParameters;
    public EnemyPuniGenerateUnitParameter[] UnitParameters => m_UnitParameters;

    [SerializeField]
    private EnemyPuniGeneratedData[] m_GeneratedDatas;

    [SerializeField]
    private Vector2 m_XRange;

    [SerializeField]
    private float m_InitZPos;
    public float InitZPos => m_InitZPos;

    public EnemyPuniGeneratedData GetGeneratedData()
    {
        var index = UnityEngine.Random.Range(0, m_GeneratedDatas.Length);
        return m_GeneratedDatas[index];
    }

    public float GetX()
    {
        return m_XRange.GetRandomValue();
    }
}

[Serializable]
public class EnemyPuniGenerateUnitParameter
{
    [SerializeField]
    private float m_BeginProgress;

    [SerializeField]
    private float m_EndProgress;

    [SerializeField]
    private AnimationCurve m_NextGenerateTimeWithPS;

    [SerializeField]
    private AnimationCurve m_MoveSpeedWithPS;

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
}
