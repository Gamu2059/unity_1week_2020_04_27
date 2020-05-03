using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// とげとげの生成クラス
/// </summary>
public class ThornGenerator : MonoBehaviour
{
    #region Define

    private class GenerateUnitActData
    {
        public ThornGenerateUnitParameter Data;
        public float NextGenerateTime;
        public float NextGenerateTimeCount;
    }

    #endregion

    #region Field Inspector

    [SerializeField]
    private ThornGenerateParameter m_Parameter;

    #endregion

    #region Field

    private GenerateUnitActData[] m_GenerateActDatas;
    private List<ThornController> m_ThornPool;
    private bool m_IsValid;

    #endregion

    #region Game Cycle

    // マネージャ系が準備しきれていない可能性があるのでStartにしておく
    private void Start()
    {
        m_ThornPool = new List<ThornController>();

        int ps = 0;
        if (InGameManager.Instance != null)
        {
            ps = InGameManager.Instance.PlayerSkill.Value;
        }

        // Unitデータの準備
        var unitParams = m_Parameter.UnitParameters;
        m_GenerateActDatas = new GenerateUnitActData[unitParams.Length];
        for (var i = 0; i < unitParams.Length; i++)
        {
            var data = new GenerateUnitActData();
            data.Data = unitParams[i];
            data.NextGenerateTime = data.Data.GetNextGenerateTime(ps);
            data.NextGenerateTimeCount = 0;
            m_GenerateActDatas[i] = data;
        }

        InGameManager.Instance.ChangeStateAction += OnChangeState;
    }

    private void Update()
    {
        if (InGameManager.Instance == null || !m_IsValid)
        {
            return;
        }

        var progress = InGameManager.Instance.Progress.Value;
        var playerSkill = InGameManager.Instance.PlayerSkill.Value;
        foreach (var d in m_GenerateActDatas)
        {
            if (!d.Data.IsValidProgress(progress))
            {
                continue;
            }

            if (d.NextGenerateTimeCount >= d.NextGenerateTime)
            {
                GenerateWithUnitData(d.Data, playerSkill);
                d.NextGenerateTimeCount -= d.NextGenerateTime;
                d.NextGenerateTime = d.Data.GetNextGenerateTime(playerSkill);
            }

            d.NextGenerateTimeCount += Time.deltaTime;
        }
    }

    #endregion

    private ThornController GetPuniFromPool(ThornController prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        ThornController thorn = null;
        foreach (var t in m_ThornPool)
        {
            if (!t.gameObject.activeSelf)
            {
                thorn = t;
                break;
            }
        }

        if (thorn == null)
        {
            thorn = Instantiate(prefab);
            m_ThornPool.Add(thorn);
        }
        else
        {
            thorn.gameObject.SetActive(true);
        }

        return thorn;
    }

    private void GenerateWithUnitData(ThornGenerateUnitParameter data, int playerSkill)
    {
        if (data == null)
        {
            return;
        }

        var puni = GetPuniFromPool(m_Parameter.Prefab);
        if (puni == null)
        {
            return;
        }

        var puniT = puni.transform;
        puniT.SetParent(transform);
        puniT.position = new Vector3(data.GetX(), m_Parameter.InitYPos, m_Parameter.GetZ());

        puni.OnGenerated(data.GetMoveSpeed(playerSkill), data.GetXMoveSpeed(), data.Damage, m_Parameter.GradientSet, m_Parameter.ShadowGradientSet);
    }

    private void OnChangeState(E_INGAME_STATE state)
    {
        m_IsValid = state == E_INGAME_STATE.GAME;
    }
}
