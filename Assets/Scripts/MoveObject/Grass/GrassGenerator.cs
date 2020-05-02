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
    private class GenerateUnitActData
    {
        public GrassGenerateUnitParameter Data;
        public float NextGenerateTime;
        public float NextGenerateTimeCount;
    }

    #endregion

    #region Field Inspector

    [SerializeField]
    private GrassGenerateParameter m_Parameter;

    #endregion

    #region Field

    private GenerateUnitActData[] m_GenerateActDatas;
    private Dictionary<string, List<GrassController>> m_GrassPool;

    #endregion

    #region Game Cycle

    // マネージャ系が準備しきれていない可能性があるのでStartにしておく
    private void Start()
    {
        m_GrassPool = new Dictionary<string, List<GrassController>>();

        // Unitデータの準備
        var unitParams = m_Parameter.UnitParameters;
        m_GenerateActDatas = new GenerateUnitActData[unitParams.Length];
        for (var i = 0; i < unitParams.Length; i++)
        {
            var data = new GenerateUnitActData();
            data.Data = unitParams[i];
            data.NextGenerateTime = data.Data.GetNextGenerateTime(0);
            data.NextGenerateTimeCount = 0;
            m_GenerateActDatas[i] = data;
        }

        // Awakeデータの処理
        var awakeParams = m_Parameter.AwakePrameters;
        foreach (var p in awakeParams)
        {
            GenerateWithAwakeData(p);
        }
    }

    private void Update()
    {
        if (InGameManager.Instance == null)
        {
            return;
        }

        var progress = InGameManager.Instance.Progress.Value;
        foreach (var d in m_GenerateActDatas)
        {
            if (!d.Data.IsValidProgress(progress))
            {
                continue;
            }

            if (d.NextGenerateTimeCount >= d.NextGenerateTime)
            {
                GenerateWithUnitData(d.Data);
                d.NextGenerateTimeCount -= d.NextGenerateTime;
                d.NextGenerateTime = d.Data.GetNextGenerateTime(progress);
            }

            d.NextGenerateTimeCount += Time.deltaTime;
        }
    }

    #endregion

    private GrassController GetGrassFromPool(GrassController prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        var id = prefab.Id;
        GrassController grass = null;
        List<GrassController> list = null;
        m_GrassPool.TryGetValue(id, out list);
        if (list == null)
        {
            list = new List<GrassController>();
            m_GrassPool.Add(id, list);
        }
        else
        {
            foreach (var g in list)
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
            grass = Instantiate(prefab);
            list.Add(grass);
        }
        else
        {
            grass.gameObject.SetActive(true);
        }

        return grass;
    }

    private void GenerateWithAwakeData(GrassGenerateAwakeParameter data)
    {
        if (data == null)
        {
            return;
        }

        foreach (var i in new int[data.GenerateNum])
        {
            var grass = GetGrassFromPool(data.Prefab);
            if (grass == null)
            {
                continue;
            }

            grass.SetMoveSpeed(m_Parameter.MoveSpeed);
            grass.SetGradientSet(data.GetGradientSet());

            var grassT = grass.transform;
            grassT.SetParent(transform);

            // 絶対見えない場所に置く
            grassT.position = new Vector3(data.GetX(), -1000, data.GetZ());
        }
    }

    private void GenerateWithUnitData(GrassGenerateUnitParameter data)
    {
        if (data == null)
        {
            return;
        }

        var grass = GetGrassFromPool(data.Prefab);
        if (grass == null)
        {
            return;
        }

        grass.SetMoveSpeed(m_Parameter.MoveSpeed);
        grass.SetGradientSet(data.GetGradientSet());

        var grassT = grass.transform;
        grassT.SetParent(transform);

        // 絶対見えない場所に置く
        grassT.position = new Vector3(data.GetX(), -1000, m_Parameter.UnitDataZPos);
    }
}
