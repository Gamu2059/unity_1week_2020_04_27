using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = "Param/RandomGetGradientSet", fileName = "param.random_get_gradient_set.asset")]
public class RandomGetGradientSet : ScriptableObject
{
    [Serializable]
    private class Set
    {
        [SerializeField]
        private GradientSet m_GradientSet;
        public GradientSet GradientSet => m_GradientSet;

        [SerializeField]
        private int m_Weight;
        public int Weight => m_Weight;
    }

    [SerializeField]
    private Set[] m_Set;

    public GradientSet GetGradientSet()
    {
        if (m_Set == null)
        {
            return null;
        }

        int sum = m_Set.Sum(s => s.Weight);
        int value = UnityEngine.Random.Range(0, sum);

        int weightCount = 0;
        foreach (var s in m_Set)
        {
            var w = s.Weight;
            if (value >= weightCount && value < weightCount + w)
            {
                return s.GradientSet;
            }

            weightCount += w;
        }

        return null;
    }
}
