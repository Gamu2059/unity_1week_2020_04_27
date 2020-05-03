using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵プニ生成クラス
/// </summary>
public class EnemyPuniGenerator : MonoBehaviour
{
    #region Define

    private class GenerateUnitActData
    {
        public EnemyPuniGenerateUnitParameter Data;
        public float NextGenerateTime;
        public float NextGenerateTimeCount;
    }

    #endregion

    #region Field Inspector

    [SerializeField]
    private EnemyPuniGenerateParameter m_Parameter;

    #endregion

    #region Field

    private GenerateUnitActData[] m_GenerateActDatas;
    private List<EnemyPuniController> m_PuniPool;
    private bool m_IsValid;

    #endregion

    #region Game Cycle

    // マネージャ系が準備しきれていない可能性があるのでStartにしておく
    private void Start()
    {
        m_PuniPool = new List<EnemyPuniController>();

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

    private EnemyPuniController GetPuniFromPool(EnemyPuniController prefab)
    {
        if (prefab == null)
        {
            return null;
        }

        EnemyPuniController puni = null;
        foreach (var p in m_PuniPool)
        {
            if (!p.gameObject.activeSelf)
            {
                puni = p;
                break;
            }
        }

        if (puni == null)
        {
            puni = Instantiate(prefab);
            m_PuniPool.Add(puni);
        }
        else
        {
            puni.gameObject.SetActive(true);
        }

        return puni;
    }

    private void GenerateWithUnitData(EnemyPuniGenerateUnitParameter data, int playerSkill)
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
        puniT.position = new Vector3(m_Parameter.GetX(), -1000, m_Parameter.InitZPos);

        puni.OnGenerated(m_Parameter.GetGeneratedData(), data.GetMoveSpeed(playerSkill), data.Damage);
    }

    private void OnChangeState(E_INGAME_STATE state)
    {
        m_IsValid = state == E_INGAME_STATE.GAME;
    }
}
