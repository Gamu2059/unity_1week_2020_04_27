using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Param/GradientSet", fileName = "param.gradient.asset")]
public class GradientSet : ScriptableObject
{
    [SerializeField]
    private GradientData[] m_Set;
    public GradientData[] Set => m_Set;
}

[Serializable]
public class GradientData
{
    [SerializeField]
    private string m_Name;
    public string Name => m_Name;

    [SerializeField]
    private Gradient m_Gradient;

    public Color GetColor(float progress)
    {
        return m_Gradient.Evaluate(progress);
    }
}