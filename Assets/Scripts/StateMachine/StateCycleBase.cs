using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateCycleBase<T, U> : ControllableObject
{
    protected T Target { get; private set; }
    protected U State { get; private set; }

    public virtual void OnEnd()
    {
    }

    public void SetTarget(T target)
    {
        Target = target;
    }

    public void SetState(U state)
    {
        State = state;
    }
}
