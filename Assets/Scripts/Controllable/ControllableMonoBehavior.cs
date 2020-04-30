using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// コンポーネントを制御しやすいようにするための拡張です。
/// </summary>
public class ControllableMonoBehavior : MonoBehaviour, IControllableGameCycle
{
	protected virtual void Awake()
	{
		OnAwake();
	}

	protected virtual void OnDestroy()
	{
		OnDestroyed();
	}

	/// <summary>
	/// Awakeで呼び出されるメソッド。
	/// </summary>
	protected virtual void OnAwake() { }

	/// <summary>
	/// OnDestroyで呼び出されるメソッド。
	/// </summary>
	protected virtual void OnDestroyed() { }


	public virtual void OnInitialize()
	{
	}

	public virtual void OnFinalize()
	{
	}

	public virtual void OnStart()
	{
	}

	public virtual void OnUpdate()
	{
    }

	public virtual void OnLateUpdate()
	{
	}

	public virtual void OnFixedUpdate()
	{
	}

}
