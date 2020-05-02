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
    #region Field Inspector

    #endregion

    #region Field

    [SerializeField, Tooltip("ゲーム時間")]
    private float m_InGameDuration;

    [SerializeField, Tooltip("親密度")]
    private IntReactiveProperty m_Closeness;
    public IntReactiveProperty Closeness => m_Closeness;

    [SerializeField, Tooltip("進行度")]
    private FloatReactiveProperty m_Progress;
    public FloatReactiveProperty Progress => m_Progress;

    [SerializeField, Tooltip("コンボ数")]
    private IntReactiveProperty m_Combo;
    public IntReactiveProperty Combo => m_Combo;

    [SerializeField, Tooltip("スペシャルコンボ数")]
    private IntReactiveProperty m_SpecialCombo;

    [SerializeField, Tooltip("プレイ指標")]
    private IntReactiveProperty m_PlayerSkill;
    public IntReactiveProperty PlayerSkill => m_PlayerSkill;

    private int m_SpecialHeartId;
    private int m_SpecialHeartGainCount;

    #endregion

    #region Game Cycle

    protected override void OnAwake()
    {
        base.OnAwake();

        ShaderPropertyID.Create();

        m_Closeness = new IntReactiveProperty(100);
        m_Progress = new FloatReactiveProperty(0);
        m_PlayerSkill = new IntReactiveProperty();
        m_Combo = new IntReactiveProperty();
        m_SpecialCombo = new IntReactiveProperty();
        m_SpecialHeartId = 0;

        m_Closeness.Subscribe(_ => UpdatePlayerSkill());
        m_Closeness.Subscribe(_ => CheckGameOver());
        m_Combo.Subscribe(_ => UpdatePlayerSkill());

        UpdatePlayerSkill();
    }

    protected override void OnDestroyed()
    {
        m_SpecialCombo.Dispose();
        m_Combo.Dispose();
        m_PlayerSkill.Dispose();
        m_Progress.Dispose();
        m_Closeness.Dispose();

        ShaderPropertyID.Instance?.OnFinalize();

        base.OnDestroyed();
    }

    private void Update()
    {
        m_Progress.Value = Mathf.Clamp01(m_Progress.Value + 1f / m_InGameDuration * Time.deltaTime);
    }

    #endregion

    public void GainHeart(int point)
    {
        var gain = point * (int)Mathf.Log(m_Combo.Value + 2, 2);
        m_Closeness.Value += gain;
        m_Combo.Value++;
    }

    public void GainSpecialHeart(int id)
    {

    }

    public void Damaged(int damage)
    {
        m_Closeness.Value -= damage;
        m_Combo.Value = 0;
    }

    /// <summary>
    /// プレイ指標を更新する
    /// </summary>
    private void UpdatePlayerSkill()
    {
        var closeness = (int) Mathf.Max(0, Mathf.Log10(m_Closeness.Value + 1));
        var combo = (int)Mathf.Max(0, Mathf.Log(m_Combo.Value + 1, 2));
        Debug.LogFormat("{0} {1}", closeness, combo);
        m_PlayerSkill.Value = closeness + combo;
    }

    private void CheckGameOver()
    {
        if (m_Closeness.Value <= 0)
        {
            Debug.Log("ゲームオーバー");
            SceneManager.LoadScene("InGame");
        }
    }
}
