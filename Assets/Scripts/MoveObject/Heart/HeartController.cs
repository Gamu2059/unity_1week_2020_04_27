#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ハート制御クラス
/// </summary>
public class HeartController : MonoBehaviour, IMoveObject
{
    #region Field

    private bool m_IsSpecial;
    private float m_MoveSpeed;
    private int m_Point;
    private int m_SpecialId;
    private int m_SpecialHeartMaxCombo;

    #endregion

    private void Update()
    {
        var ts = InGameManager.Instance.TimeScale;
        var pos = transform.position;
        pos.z += m_MoveSpeed * Time.deltaTime * ts;

        if (GroundManager.Instance != null)
        {
            pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
        }

        if (pos.z <= 0)
        {
            gameObject.SetActive(false);
        }

        transform.position = pos;
    }

    public void OnEnterPuni(IPuni puni)
    {
        if (InGameManager.Instance != null)
        {
            InGameManager.Instance.GainHeart(m_Point);

            if (m_IsSpecial)
            {
                InGameManager.Instance.GainSpecialHeart(m_SpecialId, m_SpecialHeartMaxCombo);
            }
        }

        gameObject.SetActive(false);
    }

    public void OnGeneratedAsHeart(float moveSpeed, int point)
    {
        m_IsSpecial = false;
        m_MoveSpeed = moveSpeed;
        m_Point = point;
    }

    public void OnGeneratedAsSpecialHeart(float moveSpeed, int point, int id, int maxCombo)
    {
        m_IsSpecial = true;
        m_MoveSpeed = moveSpeed;
        m_Point = point;
        m_SpecialId = id;
        m_SpecialHeartMaxCombo = maxCombo;
    }
}
