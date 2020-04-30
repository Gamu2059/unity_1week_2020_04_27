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

    [SerializeField]
    private Collider2D m_Collider2D;

    public Action<Collision, Collider> CollisionEnterAction;
    public Action<Collision, Collider> CollisionStayAction;
    public Action<Collision, Collider> CollisionExitAction;
    public Action<Collider, Collider> TriggerEnterAction;
    public Action<Collider, Collider> TriggerStayAction;
    public Action<Collider, Collider> TriggerExitAction;
    public Action<Collision2D, Collider2D> CollisionEnter2DAction;
    public Action<Collision2D, Collider2D> CollisionStay2DAction;
    public Action<Collision2D, Collider2D> CollisionExit2DAction;
    public Action<Collider2D, Collider2D> TriggerEnter2DAction;
    public Action<Collider2D, Collider2D> TriggerStay2DAction;
    public Action<Collider2D, Collider2D> TriggerExit2DAction;

    private void OnDestroy()
    {
        RemoveAllAction();
    }

    public void RemoveAllAction()
    {
        CollisionEnterAction = null;
        CollisionStayAction = null;
        CollisionExitAction = null;
        TriggerEnterAction = null;
        TriggerStayAction = null;
        TriggerExitAction = null;
        CollisionEnter2DAction = null;
        CollisionStay2DAction = null;
        CollisionExit2DAction = null;
        TriggerEnter2DAction = null;
        TriggerStay2DAction = null;
        TriggerExit2DAction = null;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CollisionEnter2DAction?.Invoke(collision, m_Collider2D);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        CollisionStay2DAction?.Invoke(collision, m_Collider2D);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        CollisionExit2DAction?.Invoke(collision, m_Collider2D);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TriggerEnter2DAction?.Invoke(collision, m_Collider2D);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TriggerStay2DAction?.Invoke(collision, m_Collider2D);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        TriggerExit2DAction?.Invoke(collision, m_Collider2D);
    }
}
