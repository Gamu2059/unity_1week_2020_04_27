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

    [SerializeField]
    private string m_UnityRoomId = "puni_dating";
    public string UnityRoomId => m_UnityRoomId;

    [SerializeField]
    private Camera m_Camera;
    public Camera Camera => m_Camera;

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
    public IntReactiveProperty SpecialCombo => m_SpecialCombo;

    [SerializeField, Tooltip("プレイ指標")]
    private IntReactiveProperty m_PlayerSkill;
    public IntReactiveProperty PlayerSkill => m_PlayerSkill;

    private int m_SpecialHeartId;
    private int m_SpecialHeartGainCount;

    private StateMachine<E_INGAME_STATE, InGameManager> m_StateMachine;

    public float TimeScale { get; private set; }

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
        m_Progress.Subscribe(_ => CheckGameClear());
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
        public override void OnStart()
        {
            base.OnStart();
            Target.TimeScale = 1;
            AudioManager.Instance.PlayBGM(AudioManagerKeyWord.Game);
        }
    }

    #endregion

    #region Game State

    private class GameState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.TimeScale = 1;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            var progress = Target.m_Progress;
            progress.Value = Mathf.Clamp01(progress.Value + 1f / Target.m_InGameDuration * Time.deltaTime);
        }

        public override void OnEnd()
        {
            AudioManager.Instance.StopBGM(AudioManagerKeyWord.Game, 1f);
            base.OnEnd();
        }
    }

    #endregion

    #region Game Over State

    private class GameOverState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.TimeScale = 0;
        }
    }

    #endregion

    #region Game Clear State

    private class GameClearState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.TimeScale = 0;
        }
    }

    #endregion

    private void RequestChangeState(E_INGAME_STATE state)
    {
        m_StateMachine?.Goto(state);
    }

    public void ToTitle()
    {
        SceneManager.LoadScene("InGame");
    }

    public void GameStart()
    {
        RequestChangeState(E_INGAME_STATE.GAME);
    }


    public void GainHeart(int point)
    {
        var gain = point * (int)Mathf.Log(m_Combo.Value + 2, 2);
        m_Closeness.Value += gain;
        m_Combo.Value++;
        AudioManager.Instance.PlaySE(AudioManagerKeyWord.GainHeart);
    }

    public void GainSpecialHeart(int id, int maxCombo)
    {

    }

    public void Damaged(int damage)
    {
        m_Closeness.Value -= damage;
        m_Combo.Value = 0;
        AudioManager.Instance.PlaySE(AudioManagerKeyWord.Damage);
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
            RequestChangeState(E_INGAME_STATE.GAME_OVER);
        }
    }

    private void CheckGameClear()
    {
        if (m_Progress.Value >= 1)
        {
            RequestChangeState(E_INGAME_STATE.GAME_CLEAR);
        }
    }

    public void SendScoreAndShowRanking()
    {
        naichilab.RankingLoader.Instance.SendScoreAndShowRanking(Closeness.Value);
    }
}
