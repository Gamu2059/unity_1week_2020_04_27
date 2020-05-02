using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// 雲を生成するクラス
/// </summary>
public class CloudGenerator : MonoBehaviour
{
    #region Define

    [Serializable]
    private class GenerateUnitActData
    {
        public CloudGenerateUnitParameter Data;
        public float NextGenerateTime;
        public float NextGenerateTimeCount;
        public int[] m_IndexWeights;
    }

    #endregion

    #region Field Inspector

    [SerializeField]
    private CloudGenerateParameter m_Parameter;

    #endregion

    #region Field

    private GenerateUnitActData[] m_GenerateActDatas;
    private Dictionary<int, List<CloudController>> m_CloudPool;

    #endregion

    #region Game Cycle

    // マネージャ系が準備しきれていない可能性があるのでStartにしておく
    private void Start()
    {
        m_CloudPool = new Dictionary<int, List<CloudController>>();

        // Unitデータの準備
        var unitParams = m_Parameter.UnitParameters;
        m_GenerateActDatas = new GenerateUnitActData[unitParams.Length];
        for (var i = 0; i < unitParams.Length; i++)
        {
            var data = new GenerateUnitActData();
            data.Data = unitParams[i];
            data.NextGenerateTime = data.Data.GetNextGenerateTime(0);
            data.NextGenerateTimeCount = 0;
            data.m_IndexWeights = new int[data.Data.Prefabs.Length];
            m_GenerateActDatas[i] = data;
        }

        // Awakeデータの処理
        var indexWeights = new int[m_Parameter.AwakeParameter.Prefabs.Length];
        GenerateWithAwakeData(m_Parameter.AwakeParameter, indexWeights);
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
                GenerateWithUnitData(d.Data, d.m_IndexWeights);
                d.NextGenerateTimeCount -= d.NextGenerateTime;
                d.NextGenerateTime = d.Data.GetNextGenerateTime(progress);
            }

            d.NextGenerateTimeCount += Time.deltaTime;
        }
    }

    #endregion

    private CloudController GetCloudFromPool(int index, CloudController prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        CloudController cloud = null;
        List<CloudController> list = null;
        m_CloudPool.TryGetValue(index, out list);
        if (list == null)
        {
            list = new List<CloudController>();
            m_CloudPool.Add(index, list);
        }
        else
        {
            foreach (var c in list)
            {
                if (!c.gameObject.activeSelf)
                {
                    cloud = c;
                    break;
                }
            }
        }

        if (cloud == null)
        {
            cloud = Instantiate(prefab);
            list.Add(cloud);
        }
        else
        {
            cloud.gameObject.SetActive(true);
        }

        return cloud;
    }

    private void GenerateWithAwakeData(CloudGenerateAwakeParameter data, int[] indexWeights)
    {
        if (data == null)
        {
            return;
        }

        foreach (var i in new int[data.GenerateNum])
        {
            var index = GetIndex(indexWeights);
            if (index < 0 || index >= data.Prefabs.Length)
            {
                continue;
            }

            var cloud = GetCloudFromPool(index, data.Prefabs[index]);
            if (cloud == null)
            {
                continue;
            }

            var cloudT = cloud.transform;
            cloudT.SetParent(transform);
            cloudT.position = new Vector3(data.GetX(), data.GetY(), data.GetZ());

            cloud.SetFlipX();
            cloud.SetMoveSpeed(m_Parameter.GetMoveSpeed());
            cloud.SetGradientSet(data.GradientSet);
        }
    }

    private void GenerateWithUnitData(CloudGenerateUnitParameter data, int[] indexWeights)
    {
        if (data == null)
        {
            return;
        }

        var index = GetIndex(indexWeights);
        if (index < 0 || index >= data.Prefabs.Length)
        {
            return;
        }

        var cloud = GetCloudFromPool(index, data.Prefabs[index]);
        if (cloud == null)
        {
            return;
        }

        var cloudT = cloud.transform;
        cloudT.SetParent(transform);
        cloudT.position = new Vector3(data.GetX(), data.GetY(), m_Parameter.UnitDataZPos);

        cloud.SetFlipX();
        cloud.SetMoveSpeed(m_Parameter.GetMoveSpeed());
        cloud.SetGradientSet(data.GradientSet);
    }

    private int GetIndex(int[] indexWeights)
    {
        if (indexWeights == null)
        {
            return -1;
        }

        var max = indexWeights.Max();
        var invertWeight = new int[indexWeights.Length];
        for (var i = 0; i < invertWeight.Length; i++)
        {
            invertWeight[i] = max - indexWeights[i];
        }

        var sum = invertWeight.Sum();
        int value = UnityEngine.Random.Range(0, sum);
        if (sum == 0)
        {
            var index = UnityEngine.Random.Range(0, indexWeights.Length);
            indexWeights[index]++;
            return index;
        }

        int weightCount = 0;
        for (var i = 0; i < invertWeight.Length; i++)
        {
            var w = invertWeight[i];
            if (value >= weightCount && value < weightCount + w)
            {
                indexWeights[i]++;
                return i;
            }

            weightCount += w;
        }

        return -1;
    }
}
