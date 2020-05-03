#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// ハート生成クラス
/// </summary>
public class HeartGenerator : MonoBehaviour
{
    #region Define

    private class GenerateActData
    {
        public HeartGenerateUnitParameter Data;
        public float NextGenerateTime;
        public float NextGenerateTimeCount;
    }

    private class GenerateActSpecialData
    {
        public SpecialHeartGenerateUnitParameter Data;
        public float NextGenerateTimeCount;
        public int Index;
        public int SpecialId;
        
        public bool IsValid(float progress)
        {
            if (Data == null)
            {
                return false;
            }

            return Data.FireTimingProgress <= progress && Index < Data.XPositions.Length;
        }

        public bool ShouldRemove()
        {
            return Index >= Data.XPositions.Length;
        }
    }

    #endregion

    #region Field Inspector

    [SerializeField]
    private HeartGenerateParameter m_Parameter;

    #endregion

    #region Field

    private GenerateActData m_GenerateActData;
    private List<GenerateActSpecialData> m_GenerateActSpecialDatas;
    private List<HeartController> m_HeartPool;
    private List<HeartController> m_SpecialHeartPool;
    private bool m_IsValid;

    #endregion

    #region Game Cycle

    // マネージャ系が準備しきれていない可能性があるのでStartにしておく
    private void Start()
    {
        m_HeartPool = new List<HeartController>();
        m_SpecialHeartPool = new List<HeartController>();

        int ps = 0;
        if (InGameManager.Instance != null)
        {
            ps = InGameManager.Instance.PlayerSkill.Value;
        }

        // Unitデータの準備
        var unitParam = m_Parameter.UnitParameter;
        m_GenerateActData = new GenerateActData();
        m_GenerateActData.Data = unitParam;
        m_GenerateActData.NextGenerateTime = unitParam.GetNextGenerateTime(ps);
        m_GenerateActData.NextGenerateTimeCount = 0;

        // SpecialUnitデータの準備
        var specialUnitParams = m_Parameter.SpecialUnitParameters;
        m_GenerateActSpecialDatas = new List<GenerateActSpecialData>();
        for (var i = 0; i < specialUnitParams.Length; i++)
        {
            var data = new GenerateActSpecialData();
            data.Data = specialUnitParams[i];
            data.NextGenerateTimeCount = 0;
            data.Index = 0;
            data.SpecialId = i;
            m_GenerateActSpecialDatas.Add(data);
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

        if (progress >= m_Parameter.EndProgress)
        {
            return;
        }

        if (m_GenerateActData != null)
        {
            if (m_GenerateActData.NextGenerateTimeCount >= m_GenerateActData.NextGenerateTime)
            {
                GenerateWithUnitData(m_GenerateActData.Data, playerSkill);
                m_GenerateActData.NextGenerateTimeCount -= m_GenerateActData.NextGenerateTime;
                m_GenerateActData.NextGenerateTime = m_GenerateActData.Data.GetNextGenerateTime(playerSkill);
            }

            m_GenerateActData.NextGenerateTimeCount += Time.deltaTime;
        }

        foreach (var d in m_GenerateActSpecialDatas)
        {
            if (!d.IsValid(progress))
            {
                continue;
            }

            if (d.NextGenerateTimeCount >= d.Data.GenerateInterval)
            {
                GenerateWithSpecialUnitData(d, playerSkill);
                d.NextGenerateTimeCount -= d.Data.GenerateInterval;
                d.Index++;
            }

            d.NextGenerateTimeCount += Time.deltaTime;
        }

        m_GenerateActSpecialDatas.RemoveAll(d => d.ShouldRemove());
    }

    #endregion

    private HeartController GetHeartFromPool(bool isSpecial)
    {
        HeartController heart = null;
        var pool = isSpecial ? m_SpecialHeartPool : m_HeartPool;
        foreach (var h in pool)
        {
            if (!h.gameObject.activeSelf)
            {
                heart = h;
                break;
            }
        }

        if (heart == null)
        {
            heart = Instantiate(isSpecial ? m_Parameter.SpecialHeartPrefab : m_Parameter.HeartPrefab);
            pool.Add(heart);
        }
        else
        {
            heart.gameObject.SetActive(true);
        }

        return heart;
    }

    private void GenerateWithUnitData(HeartGenerateUnitParameter data, int playerSkill)
    {
        if (data == null)
        {
            return;
        }

        var heart = GetHeartFromPool(false);
        if (heart == null)
        {
            return;
        }

        var heartT = heart.transform;
        heartT.SetParent(transform);

        // 絶対見えない場所に置く
        heartT.position = new Vector3(data.GetX(), -1000, m_Parameter.InitZPos);
        var moveSpeed = m_Parameter.GetMoveSpeed(playerSkill); ;
        heart.OnGeneratedAsHeart(moveSpeed, m_Parameter.SpecialHeartPoint);
    }

    private void GenerateWithSpecialUnitData(GenerateActSpecialData data, int playerSkill)
    {
        if (data == null)
        {
            return;
        }
        var heart = GetHeartFromPool(true);
        if (heart == null)
        {
            return;
        }

        var heartT = heart.transform;
        heartT.SetParent(transform);

        // 絶対見えない場所に置く
        heartT.position = new Vector3(data.Data.XPositions[data.Index], -1000, m_Parameter.InitZPos);

        var moveSpeed = m_Parameter.GetMoveSpeed(playerSkill); ;
        heart.OnGeneratedAsSpecialHeart(moveSpeed, m_Parameter.SpecialHeartPoint, data.SpecialId, data.Data.XPositions.Length);
    }

    private void OnChangeState(E_INGAME_STATE state)
    {
        m_IsValid = state == E_INGAME_STATE.GAME;
    }
}
