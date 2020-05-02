#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ハート制御クラス
/// </summary>
public class HeartController : MonoBehaviour, IMoveObject
{
    [SerializeField]
    private int m_ClosenessValue;

    [HideInInspector]
    public float MoveSpeed;

    private void Update()
    {
        var pos = transform.position;
        pos.z += MoveSpeed * Time.deltaTime;

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
            InGameManager.Instance.GainHeart(m_ClosenessValue);
        }

        gameObject.SetActive(false);
    }
}
