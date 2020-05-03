using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class InGameUiManager : MonoBehaviour
{
    #region Define

    private class StateCycle : StateCycleBase<InGameUiManager, E_INGAME_STATE> { }

    private class InnerState : State<E_INGAME_STATE, InGameUiManager>
    {
        public InnerState(E_INGAME_STATE state, InGameUiManager target) : base(state, target) { }
        public InnerState(E_INGAME_STATE state, InGameUiManager target, StateCycle cycle) : base(state, target, cycle) { }
    }

    #endregion

    #region Field Inspector

    [Header("Title")]

    [SerializeField]
    private CanvasGroup m_TitleGroup;

    [SerializeField]
    private Animator m_TitleAnimator;

    [SerializeField]
    private Button m_TitlePlayButton;

    [Header("In Game")]

    [SerializeField]
    private CanvasGroup m_InGameGroup;

    [SerializeField]
    private Animator m_InGameAnimator;

    [SerializeField]
    private Text m_ClosenessIndicator;

    [SerializeField]
    private Text m_ComboIndicator;

    [SerializeField]
    private Text m_ComboSpecialIndicator;

    [SerializeField]
    private Slider m_ProgressBar;

    [Header("Game Over")]

    [SerializeField]
    private CanvasGroup m_GameOverGroup;

    [SerializeField]
    private Animator m_GameOverAnimator;

    [SerializeField]
    private Text m_ProgressReport;

    [SerializeField]
    private Button m_GameOverToTitleButton;

    [SerializeField]
    private Button m_GameOverTwitterButton;

    [Header("Game Clear")]

    [SerializeField]
    private CanvasGroup m_GameClearGroup;

    [SerializeField]
    private Animator m_GameClearAnimator;

    [SerializeField]
    private Text m_ScoreReport;

    [SerializeField]
    private Button m_GameClearToTitleButton;

    [SerializeField]
    private Button m_RankingButton;

    [SerializeField]
    private Button m_GameClearTwitterButton;

    [SerializeField]
    private Image m_RenderImage;

    #endregion

    #region Field

    private StateMachine<E_INGAME_STATE, InGameUiManager> m_StateMachine;

    #endregion

    #region Game Cycle

    private void Awake()
    {
        m_StateMachine = new StateMachine<E_INGAME_STATE, InGameUiManager>();
        m_StateMachine.AddState(new InnerState(E_INGAME_STATE.TITLE, this, new TitleState()));
        m_StateMachine.AddState(new InnerState(E_INGAME_STATE.GAME, this, new GameState()));
        m_StateMachine.AddState(new InnerState(E_INGAME_STATE.GAME_OVER, this, new GameOverState()));
        m_StateMachine.AddState(new InnerState(E_INGAME_STATE.GAME_CLEAR, this, new GameClearState()));
    }

    private void OnDestroy()
    {
        m_StateMachine.OnFinalize();
    }

    private void Start()
    {
        InGameManager.Instance.ChangeStateAction += OnChangeState;

        //InGameManager.Instance.Closeness.Subscribe(x => m_ClosenessIndicator.text = x.ToString());
        //InGameManager.Instance.Progress.Subscribe(x => m_Progress.value = x);
    }

    private void Update()
    {
        m_StateMachine?.OnUpdate();
    }

    #endregion

    private void OnChangeState(E_INGAME_STATE state)
    {
        m_StateMachine?.Goto(state);
    }

    private void ChangeToTitle()
    {

    }

    private void ChangeToGame()
    {

    }

    private void ChangeToGameOver()
    {

    }

    private void ChangeToGameClear()
    {

    }
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
            //var progress = Target.m_Progress;
            //progress.Value = Mathf.Clamp01(progress.Value + 1f / Target.m_InGameDuration * Time.deltaTime);
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
}
