#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// コライダーやトリガーの判定を検出するクラス。
/// </summary>
public class ColliderDetector : MonoBehaviour
{
    [SerializeField]
    private Collider m_Collider;

    public Action<Collision, Collider> CollisionEnterAction;
    public Action<Collision, Collider> CollisionStayAction;
    public Action<Collision, Collider> CollisionExitAction;
    public Action<Collider, Collider> TriggerEnterAction;
    public Action<Collider, Collider> TriggerStayAction;
    public Action<Collider, Collider> TriggerExitAction;

    public void RemoveAllAction()
    {
        CollisionEnterAction = null;
        CollisionStayAction = null;
        CollisionExitAction = null;
        TriggerEnterAction = null;
        TriggerStayAction = null;
        TriggerExitAction = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CollisionEnterAction?.Invoke(collision, m_Collider);
    }

    private void OnCollisionStay(Collision collision)
    {
        CollisionStayAction?.Invoke(collision, m_Collider);
    }

    private void OnCollisionExit(Collision collision)
    {
        CollisionExitAction?.Invoke(collision, m_Collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerEnterAction?.Invoke(other, m_Collider);
    }

    private void OnTriggerStay(Collider other)
    {
        TriggerStayAction?.Invoke(other, m_Collider);
    }

    private void OnTriggerExit(Collider other)
    {
        TriggerExitAction?.Invoke(other, m_Collider);
    }
}
