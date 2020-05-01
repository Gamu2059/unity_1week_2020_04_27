using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 草を生成するクラス
/// </summary>
public class GrassGenerator : MonoBehaviour
{
    #region Define

    [Serializable]
    private class GenerateData
    {
        [SerializeField]
        private Vector2 m_XRange;

        [SerializeField]
        private Vector2 m_NextGenerateTimeRange;

        public float GetX()
        {
            return m_XRange.GetRandomValue();
        }

        public float GetNextGenerateTime()
        {
            return m_NextGenerateTimeRange.GetRandomValue();
        }
    }

    [Serializable]
    private class GenerateActData
    {
        public GenerateData Data;
        public float NextGenerateTime;
        public float NextGenerateTimeCount;
    }

    #endregion

    #region Field Inspector

    [SerializeField]
    private GrassController m_GrassPrefab;

    [SerializeField]
    private GenerateData[] m_GenerateDatas;

    [SerializeField]
    private float m_InitZPos;

    [SerializeField]
    private int m_OnAwakeGenerateNum;

    [SerializeField]
    private Vector2 m_OnAwakeGenerateZRange;

    #endregion

    #region Field

    private GenerateActData[] m_GenerateActDatas;
    private List<GrassController> m_GrassPool;

    #endregion

    private void Awake()
    {
        m_GrassPool = new List<GrassController>();

        m_GenerateActDatas = new GenerateActData[m_GenerateDatas.Length];
        for (var i=0;i<m_GenerateDatas.Length;i++)
        {
            var data = new GenerateActData();
            data.Data = m_GenerateDatas[i];
            data.NextGenerateTime = 0;
            data.NextGenerateTimeCount = data.Data.GetNextGenerateTime();
            m_GenerateActDatas[i] = data;

            for (var j=0;j<m_OnAwakeGenerateNum;j++)
            {
                Generate(data.Data, m_OnAwakeGenerateZRange.GetRandomValue());
            }
        }
    }

    private void Update()
    {
        foreach (var d in m_GenerateActDatas)
        {
            if (d.NextGenerateTimeCount >= d.NextGenerateTime)
            {
                Generate(d.Data, m_InitZPos);
                d.NextGenerateTimeCount -= d.NextGenerateTime;
                d.NextGenerateTime = d.Data.GetNextGenerateTime();
            }

            d.NextGenerateTimeCount += Time.deltaTime;
        }
    }

    private GrassController GetGrassFromPool()
    {
        GrassController grass = null;
        if (m_GrassPool == null)
        {
            m_GrassPool = new List<GrassController>();
        }
        else
        {
            foreach (var g in m_GrassPool)
            {
                if (!g.gameObject.activeSelf)
                {
                    grass = g;
                    break;
                }
            }
        }

        if (grass == null)
        {
            grass = Instantiate(m_GrassPrefab);
            m_GrassPool.Add(grass);
        }
        else
        {
            grass.gameObject.SetActive(true);
        }

        return grass;
    }

    private void Generate(GenerateData data, float z)
    {
        var grass = GetGrassFromPool();
        if (grass == null)
        {
            return;
        }

        var grassT = grass.transform;
        grassT.SetParent(transform);

        // 絶対見えない場所に置く
        grassT.position = new Vector3(data.GetX(), -1000, z);
    }
}
