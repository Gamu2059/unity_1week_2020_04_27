using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private bool m_IsBluePuni;

    [SerializeField]
    private float m_XSpeed;

    [SerializeField]
    private float m_ZSpeed;

    [SerializeField]
    private float m_MinX;

    [SerializeField]
    private float m_MaxX;

    [SerializeField]
    private Transform m_Shadow;

    [Space()]
    [Header("衝突系")]

    [SerializeField, Tooltip("プニに対する判定")]
    private ColliderDetector m_PuniTrigger;

    [SerializeField, Tooltip("動くオブジェクトに対する判定")]
    private ColliderDetector m_MoveObjectTrigger;

    public void OnUpdate(float x, float z)
    {
        if (m_IsBluePuni)
        {
            OnUpdateBluePuni(x, z);
        }
        else
        {
            OnUpdateRedPuni();
        }
    }

    private void OnUpdateBluePuni(float x, float z)
    {
        var pos = transform.position;
        pos.x += x * m_XSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, m_MinX, m_MaxX);
        pos.z += z * m_ZSpeed * Time.deltaTime;

        if (GroundManager.Instance != null)
        {
            pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
        }

        transform.position = pos;
        RotateShadow(pos.z);
    }

    private void OnUpdateRedPuni()
    {
        //pos.x += x * m_XSpeed * Time.deltaTime;
        //pos.x = Mathf.Clamp(pos.x, m_MinX, m_MaxX);
        //pos.z += z * m_ZSpeed * Time.deltaTime;

        var pos = transform.position;
        if (GroundManager.Instance != null)
        {
            pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
        }

        transform.position = pos;
        RotateShadow(transform.position.z);
    }

    public void RotateShadow(float zPos)
    {
    }

    /// <summary>
    /// ひきつける
    /// </summary>
    public void Follow()
    {
        if (m_IsBluePuni)
        {

        }
        else
        {

        }
    }

    /// <summary>
    /// 引きつけを解除する
    /// </summary>
    public void DisFollow()
    {
        if (m_IsBluePuni)
        {

        }
        else
        {

        }
    }
}
