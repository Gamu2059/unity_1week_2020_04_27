using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲームサイクルの任意のタイミングで制御を呼び出せるメソッドを提供するインタフェース。
/// </summary>
public interface IControllableGameCycle
{
	/// <summary>
	/// このコンポーネントの初期化処理。
	/// OnAwakeとは異なる。
	/// このメソッドの呼び出しは、Unityではなく任意の制御下で行われる。
	/// </summary>
	void OnInitialize();

	/// <summary>
	/// このコンポーネントの終了処理。
	/// OnDestroyedとは異なる。
	/// このメソッドの呼び出しは、Unityではなく任意の制御下で行われる。
	/// </summary>
	void OnFinalize();

	/// <summary>
	/// 最初のフレームで呼び出される処理。
	/// このメソッドの呼び出しは、Unityではなく任意の制御下で行われる。
	/// </summary>
	void OnStart();

	/// <summary>
	/// 毎フレームで呼び出される処理。
	/// このメソッドの呼び出しは、Unityではなく任意の制御下で行われる。
	/// </summary>
	void OnUpdate();

	/// <summary>
	/// OnUpdateの後に呼び出される処理。
	/// このメソッドの呼び出しは、Unityではなく任意の制御下で行われる。
	/// </summary>
	void OnLateUpdate();

	/// <summary>
	/// 固定フレームで呼び出される処理。
	/// このメソッドの呼び出しは、Unityではなく任意の制御下で行われる。
	/// </summary>
	void OnFixedUpdate();
}
