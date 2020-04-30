using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シングルトンパターンを実装したMonoBehaviorの基底クラス。
/// </summary>
public abstract class SingletonMonoBehavior<T> : MonoBehaviour where T : MonoBehaviour
{

    public static T Instance {
        get;
        private set;
    }

    /// <summary>
    /// Unityで制御される生成直後に呼び出される処理。
    /// </summary>
    private void Awake()
    {
        if (CheckExistInstance())
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = GetComponent<T>();
            OnAwake();
        }
    }

    protected virtual void OnAwake() { }

    /// <summary>
    /// Unityで制御される破棄直前に呼び出される処理。
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            OnDestroyed();
            Instance = null;
        }
    }

    protected virtual void OnDestroyed() { }

    /// <summary>
    /// このクラスのインスタンスが存在するかどうかを取得する。
    /// </summary>
    public static bool CheckExistInstance()
    {
        return Instance;
    }
}