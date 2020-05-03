using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 木を生成するクラス
/// </summary>
public class TreeGenerator : MonoBehaviour
{
    #region Define

    [Serializable]
    private class GenerateUnitActData
    {
        public TreeGenerateUnitParameter Data;
        public float NextGenerateTime;
        public float NextGenerateTimeCount;
    }

    #endregion

    #region Field Inspector

    [SerializeField]
    private TreeGenerateParameter m_Parameter;

    #endregion

    #region Field

    private GenerateUnitActData[] m_GenerateActDatas;
    private Dictionary<string, List<TreeController>> m_TreePool;

    #endregion

    #region Game Cycle

    // マネージャ系が準備しきれていない可能性があるのでStartにしておく
    private void Start()
    {
        m_TreePool = new Dictionary<string, List<TreeController>>();

        // Unitデータの準備
        var unitParams = m_Parameter.UnitParameters;
        m_GenerateActDatas = new GenerateUnitActData[unitParams.Length];
        for (var i=0;i<unitParams.Length;i++)
        {
            var data = new GenerateUnitActData();
            data.Data = unitParams[i];
            data.NextGenerateTime = data.Data.GetNextGenerateTime(0);
            data.NextGenerateTimeCount = 0;
            m_GenerateActDatas[i] = data;
        }

        // Awakeデータの準備
        var awakeParams = m_Parameter.AwakeParameters;
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

    private TreeController GetTreeFromPool(TreeController prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        var id = prefab.Id;
        TreeController tree = null;
        List<TreeController> list = null;
        m_TreePool.TryGetValue(id, out list);
        if (list == null)
        {
            list = new List<TreeController>();
            m_TreePool.Add(id, list);
        }
        else
        {
            foreach (var g in list)
            {
                if (!g.gameObject.activeSelf)
                {
                    tree = g;
                    break;
                }
            }
        }

        if (tree == null)
        {
            tree = Instantiate(prefab);
            list.Add(tree);
        }

        return tree;
    }

    private void GenerateWithAwakeData(TreeGenerateAwakeParameter data)
    {
        if (data == null)
        {
            return;
        }

        foreach (var i in new int[data.GenerateNum])
        {
            var tree = GetTreeFromPool(data.Prefab);
            if (tree == null)
            {
                continue;
            }

            tree.SetMoveSpeed(data.MoveSpeed);
            tree.SetGradientSet(data.GradientSet);
            tree.SetAlphaCurve(data.AlphaCurve);
            tree.ResetTimeCount();
            tree.ApplyProgress();

            var grassT = tree.transform;
            grassT.SetParent(transform);

            // 絶対見えない場所に置く
            grassT.position = new Vector3(data.GetX(), data.GetY(), data.GetZ());

            tree.gameObject.SetActive(true);
        }
    }

    private void GenerateWithUnitData(TreeGenerateUnitParameter data)
    {
        if (data == null)
        {
            return;
        }

        var tree = GetTreeFromPool(data.Prefab);
        if (tree == null)
        {
            return;
        }

        tree.SetMoveSpeed(data.MoveSpeed);
        tree.SetGradientSet(data.GradientSet);
        tree.SetAlphaCurve(data.AlphaCurve);
        tree.ResetTimeCount();
        tree.ApplyProgress();

        var grassT = tree.transform;
        grassT.SetParent(transform);

        // 絶対見えない場所に置く
        grassT.position = new Vector3(data.GetX(), data.GetY(), m_Parameter.UnitDataZPos);

        tree.gameObject.SetActive(true);
    }
}
