using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ステートマシンで使用するステート。
/// Created by Sho Yamagami.
/// </summary>
[Serializable]
public class State<T, U> : ControllableObject
{
    public T Key { get; private set; }
    public U Target { get; private set; }

    public Action m_OnStart;
    public Action m_OnUpdate;
    public Action m_OnLateUpdate;
    public Action m_OnFixedUpdate;
    public Action m_OnEnd;

    private StateCycleBase<U, T> m_StateCycle;

    public State(T key, U target)
    {
        Key = key;
        Target = target;
        m_StateCycle = null;
    }

    public State(T key, U target, StateCycleBase<U, T> stateCycle)
    {
        Key = key;
        Target = target;
        m_StateCycle = stateCycle;
        m_StateCycle.SetState(Key);
        m_StateCycle.SetTarget(Target);
    }

    public override void OnInitialize()
    {
        base.OnInitialize();

        m_StateCycle?.OnInitialize();
    }

    public override void OnFinalize()
    {
        m_StateCycle?.OnFinalize();
        m_StateCycle = null;
        m_OnStart = null;
        m_OnUpdate = null;
        m_OnLateUpdate = null;
        m_OnFixedUpdate = null;
        m_OnEnd = null;
    }

    public override void OnStart()
    {
        if (m_StateCycle != null)
        {
            m_StateCycle.OnStart();
        }
        else
        {
            m_OnStart?.Invoke();
        }

        if (Target != null && Target is IStateCallback<T> callback)
        {
            callback.ChangeStateAction?.Invoke(Key);
        }
    }

    public void OnEnd()
    {
        if (m_StateCycle != null)
        {
            m_StateCycle.OnEnd();
        }
        else
        {
            m_OnEnd?.Invoke();
        }
    }

    public override void OnUpdate()
    {
        if (m_StateCycle != null)
        {
            m_StateCycle.OnUpdate();
        }
        else
        {
            m_OnUpdate?.Invoke();
        }
    }

    public override void OnLateUpdate()
    {
        if (m_StateCycle != null)
        {
            m_StateCycle.OnLateUpdate();
        }
        else
        {
            m_OnLateUpdate?.Invoke();
        }
    }

    public override void OnFixedUpdate()
    {
        if (m_StateCycle != null)
        {
            m_StateCycle.OnFixedUpdate();
        }
        else
        {
            m_OnFixedUpdate?.Invoke();
        }
    }
}
