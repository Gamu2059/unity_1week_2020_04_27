using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;

/// <summary>
/// インゲームのデータを保持するマネージャ
/// </summary>
public class InGameManager : SingletonMonoBehavior<InGameManager>
{
    [SerializeField, Tooltip("ゲーム時間")]
    private float m_InGameDuration;

    [SerializeField, Tooltip("親密度")]
    private IntReactiveProperty m_Closeness;
    public IntReactiveProperty Closeness => m_Closeness;

    [SerializeField, Tooltip("進行度")]
    private FloatReactiveProperty m_Progress;
    public FloatReactiveProperty Progress => m_Progress;

    protected override void OnAwake()
    {
        base.OnAwake();

        ShaderPropertyID.Create();

        m_Closeness = new IntReactiveProperty(100);
        m_Progress = new FloatReactiveProperty(0);
    }

    protected override void OnDestroyed()
    {
        m_Progress.Dispose();
        m_Closeness.Dispose();

        ShaderPropertyID.Instance?.OnFinalize();

        base.OnDestroyed();
    }

    private void Update()
    {
        m_Progress.Value = Mathf.Clamp01(m_Progress.Value + 1f / m_InGameDuration * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (m_Closeness.Value <= 0)
        {
            Debug.Log("ゲームオーバー");
            SceneManager.LoadScene("InGame");
        }
    }
}
