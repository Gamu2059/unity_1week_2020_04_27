using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using naichilab;

public class InGameUiManager : MonoBehaviour
{
    #region Define

    private const string START = "Start";
    private const string END = "End";
    private const string PERFECT_GAIN = "PerfectGain";

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

    [SerializeField]
    private Animator m_PerfectGainLabel;

    [SerializeField]
    private CanvasGroup m_ComboSpecialGroup;

    [SerializeField]
    private AnimationCurve m_ComboSpecialAlphaCurve;

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

    [Header("Other")]

    [SerializeField]
    private float m_WaitGameOverPerformanceTime;

    [SerializeField]

    private float m_WaitGameClearPerformanceTime;

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
        m_TitleGroup.alpha = 0;
        m_TitleGroup.blocksRaycasts = false;
        m_InGameGroup.alpha = 0;
        m_InGameGroup.blocksRaycasts = false;
        m_GameOverGroup.alpha = 0;
        m_GameOverGroup.blocksRaycasts = false;
        m_GameClearGroup.alpha = 0;
        m_GameClearGroup.blocksRaycasts = false;
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

    #region Title State

    private class TitleState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.m_TitleAnimator.Play(START);

            Observable.Timer(TimeSpan.FromSeconds(1.5f)).Subscribe(_ =>
            {
                Target.m_TitleGroup.blocksRaycasts = true;
                Target.m_TitlePlayButton.onClick.AddListener(OnClickPlay);
            }).AddTo(Target);
        }

        public override void OnEnd()
        {
            Target.m_TitleGroup.blocksRaycasts = false;
            Target.m_TitleAnimator.Play(END);
            base.OnEnd();
        }
        private void OnClickPlay()
        {
            InGameManager.Instance.GameStart();
        }
    }


    #endregion

    #region Game State

    private class GameState : StateCycle
    {
        private float m_TimeCount;
        private bool m_IsValidAlphaAnimation;

        public override void OnStart()
        {
            base.OnStart();
            Target.m_InGameAnimator.Play(START);
            Target.m_ProgressBar.value = 0;

            InGameManager.Instance.Progress.Subscribe(x => Target.m_ProgressBar.value = x);
            InGameManager.Instance.Closeness.Subscribe(x => Target.m_ClosenessIndicator.text = x.ToString());
            InGameManager.Instance.Combo.Subscribe(x => Target.m_ComboIndicator.text = x.ToString());
            InGameManager.Instance.SpecialHeartGainCount.Subscribe(x => IndicateComboSpecial(x));
            InGameManager.Instance.PerfectGainAction += () => Target.m_PerfectGainLabel.Play(PERFECT_GAIN);

            m_TimeCount = 0;
            m_IsValidAlphaAnimation = false;
            ApplyAlpha(0);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (m_IsValidAlphaAnimation)
            {
                m_TimeCount += Time.deltaTime;
                var a = Target.m_ComboSpecialAlphaCurve.Evaluate(m_TimeCount);
                ApplyAlpha(a);
            }
        }

        public override void OnEnd()
        {
            base.OnEnd();
            Target.m_InGameAnimator.Play(END);
        }

        private void IndicateComboSpecial(int x)
        {
            m_TimeCount = 0;
            m_IsValidAlphaAnimation = true;
            Target.m_ComboSpecialIndicator.text = x.ToString();
        }

        private void ApplyAlpha(float alpha)
        {
            Target.m_ComboSpecialGroup.alpha = alpha;
        }
    }

    #endregion

    #region Game Over State

    private class GameOverState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();

            Observable.Timer(TimeSpan.FromSeconds(Target.m_WaitGameOverPerformanceTime)).Subscribe(_ =>
            {
                Target.m_GameOverAnimator.Play(START);

                var progress = InGameManager.Instance.Progress.Value;
                Target.m_ProgressReport.text = string.Format("しんこうど\n{0}%", Mathf.RoundToInt(progress * 100f));

                Observable.Timer(TimeSpan.FromSeconds(1.5f)).Subscribe(__ =>
                {
                    AudioManager.Instance.PlayBGM(AudioManagerKeyWord.GameOver);
                    Target.m_GameOverGroup.blocksRaycasts = true;
                    Target.m_GameOverToTitleButton.onClick.AddListener(OnClickToTitle);
                    Target.m_GameOverTwitterButton.onClick.AddListener(OnClickTwitter);
                }).AddTo(Target);
            }).AddTo(Target);
        }

        private void OnClickToTitle()
        {
            InGameManager.Instance.ToTitle();
        }

        private void OnClickTwitter()
        {
            var id = InGameManager.Instance.UnityRoomId;
            var progress = InGameManager.Instance.Progress.Value;
            UnityRoomTweet.Tweet(id, string.Format("残念！ふられてしまいました...\nあなたは{0}%まで到達しました。\n", Mathf.RoundToInt(progress * 100f)), "unityroom", "uniry1week");
        }
    }

    #endregion

    #region Game Clear State

    private class GameClearState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();

            Observable.Timer(TimeSpan.FromSeconds(Target.m_WaitGameClearPerformanceTime)).Subscribe(_ =>
            {
                InGameManager.Instance.IsInvalidJumpSe = true;

                Target.StartCreateScreenShot(tex =>
                {
                    Target.m_RenderImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                    var closeness = InGameManager.Instance.Closeness.Value;
                    Target.m_ScoreReport.text = closeness.ToString();
                    Target.m_GameClearAnimator.Play(START);

                    Observable.Timer(TimeSpan.FromSeconds(3f)).Subscribe(__ =>
                    {
                        AudioManager.Instance.PlayBGM(AudioManagerKeyWord.GameClear);
                        Target.m_GameClearGroup.blocksRaycasts = true;
                        Target.m_GameClearToTitleButton.onClick.AddListener(OnClickToTitle);
                        Target.m_GameClearTwitterButton.onClick.AddListener(OnClickTwitter);
                        Target.m_RankingButton.onClick.AddListener(OnClickRanking);
                    }).AddTo(Target);
                });
            }).AddTo(Target);
        }

        private void OnClickToTitle()
        {
            InGameManager.Instance.ToTitle();
        }

        private void OnClickTwitter()
        {
            var id = InGameManager.Instance.UnityRoomId;
            var closeness = InGameManager.Instance.Closeness.Value;
            UnityRoomTweet.Tweet(id, string.Format("おめでとう！デートに成功しました！\nあなたの親密度は{0}です。\n", closeness), "unityroom", "uniry1week");
        }

        private void OnClickRanking()
        {
            InGameManager.Instance.SendScoreAndShowRanking();
        }
    }

    private void StartCreateScreenShot(Action<Texture2D> onComp = null)
    {
        StartCoroutine(CreateScreenShot(onComp));
    }

    private IEnumerator CreateScreenShot(Action<Texture2D> onComp = null)
    {
        yield return new WaitForEndOfFrame();

        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        var cam = InGameManager.Instance.Camera;
        cam.targetTexture = renderTexture;
        var tex = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        tex.Apply();
        cam.targetTexture = null;
        onComp?.Invoke(tex);
    }

    #endregion
}
