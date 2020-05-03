using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using System;

/// <summary>
/// インゲームのデータを保持するマネージャ
/// </summary>
public class InGameManager : SingletonMonoBehavior<InGameManager>, IStateCallback<E_INGAME_STATE>
{
    #region Define

    private class StateCycle : StateCycleBase<InGameManager, E_INGAME_STATE> { }

    private class InnerState : State<E_INGAME_STATE, InGameManager>
    {
        public InnerState(E_INGAME_STATE state, InGameManager target) : base(state, target) { }
        public InnerState(E_INGAME_STATE state, InGameManager target, StateCycle cycle) : base(state, target, cycle) { }
    }

    #endregion

    #region Field Inspector


    #endregion

    #region Field

    public Action<E_INGAME_STATE> ChangeStateAction { get; set; }

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

    private StateMachine<E_INGAME_STATE, InGameManager> m_StateMachine;

    #endregion

    #region Game Cycle

    protected override void OnAwake()
    {
        base.OnAwake();

        if (ShaderPropertyID.Instance == null)
        {
            ShaderPropertyID.Create();
        }

        m_StateMachine = new StateMachine<E_INGAME_STATE, InGameManager>();
        m_StateMachine.AddState(new InnerState(E_INGAME_STATE.TITLE, this, new TitleState()));
        m_StateMachine.AddState(new InnerState(E_INGAME_STATE.GAME, this, new GameState()));
        m_StateMachine.AddState(new InnerState(E_INGAME_STATE.GAME_OVER, this, new GameOverState()));
        m_StateMachine.AddState(new InnerState(E_INGAME_STATE.GAME_CLEAR, this, new GameClearState()));

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

        RequestChangeState(E_INGAME_STATE.TITLE);
    }

    protected override void OnDestroyed()
    {
        m_SpecialCombo.Dispose();
        m_Combo.Dispose();
        m_PlayerSkill.Dispose();
        m_Progress.Dispose();
        m_Closeness.Dispose();

        m_StateMachine.OnFinalize();
        m_StateMachine = null;

        ShaderPropertyID.Instance?.OnFinalize();

        base.OnDestroyed();
    }

    private void Update()
    {
        m_StateMachine?.OnUpdate();
    }

    #endregion

    #region Title State

    private class TitleState : StateCycle
    {

    }

    #endregion

    #region Game State

    private class GameState : StateCycle
    {
        public override void OnUpdate()
        {
            base.OnUpdate();
            var progress = Target.m_Progress;
            progress.Value = Mathf.Clamp01(progress.Value + 1f / Target.m_InGameDuration * Time.deltaTime);
        }
    }

    #endregion

    #region Game Over State

    private class GameOverState : StateCycle
    {

    }

    #endregion

    #region Game Clear State

    private class GameClearState : StateCycle
    {

    }

    #endregion

    private void RequestChangeState(E_INGAME_STATE state)
    {
        m_StateMachine?.Goto(state);
    }


    public void GainHeart(int point)
    {
        var gain = point * (int)Mathf.Log(m_Combo.Value + 2, 2);
        m_Closeness.Value += gain;
        m_Combo.Value++;
    }

    public void GainSpecialHeart(int id, int maxCombo)
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
        var closeness = (int)Mathf.Max(0, Mathf.Log10(m_Closeness.Value + 1));
        var combo = (int)Mathf.Max(0, Mathf.Log(m_Combo.Value + 1, 2));
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
