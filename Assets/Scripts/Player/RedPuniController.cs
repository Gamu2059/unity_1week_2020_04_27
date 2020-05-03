using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;

public class RedPuniController : ControllableMonoBehavior, IPuni
{
    #region Define

    private const string WALK = "Puni@walk";
    private const string DAMAGED = "Puni@damaged";
    private const string STOP = "Puni@stop";

    private enum E_STATE
    {
        // 1人でいる
        ALONE,

        COUPLE_SLIDE,

        // 2人でいる
        COUPLE,

        // 1人でいるが、呼び寄せられている
        ALONE_CALLED,

        // 1人でいるが、離された直後
        ALONE_LEFT_OUT,

        // ダメージ受けた時
        DAMAGED,

        // ゲームオーバー
        GAME_OVER,

        // ゲームクリア
        GAME_CLEAR,
    }

    private class StateCycle : StateCycleBase<RedPuniController, E_STATE> { }

    private class InnerState : State<E_STATE, RedPuniController>
    {
        public InnerState(E_STATE state, RedPuniController target) : base(state, target) { }
        public InnerState(E_STATE state, RedPuniController target, StateCycle cycle) : base(state, target, cycle) { }
    }

    #endregion

    #region Field Inspector

    [Header("Component")]

    [SerializeField]
    private PuniViewController m_ViewController;

    [SerializeField]
    private Animator m_Animator;

    [SerializeField]
    private BluePuniController m_BluePuni;
    
    [Header("Collision")]

    [SerializeField]
    private ColliderDetector m_PuniTrigger;

    [Header("Parameter")]

    [SerializeField]
    private float m_CoupleCalledSpeed;

    [SerializeField]
    private float m_CoupleSlideSpeed;

    [SerializeField]
    private float m_CoupleDistance;

    [SerializeField]
    private float m_GameOverRunawaySpeed;

    [SerializeField]
    private float m_GameClearLeftSidePosX;

    [SerializeField]
    private float m_GameClearRightSidePosX;

    [SerializeField]
    private float m_GameClearMoveDuration;

    [SerializeField]
    private float m_GameClearWaitDuration;

    [SerializeField]
    private GameObject m_HeartAnimationObject;

    #endregion

    #region Field

    private StateMachine<E_STATE, RedPuniController> m_StateMachine;
    public bool IsBluePuni => false;

    #endregion

    #region Game Cycle

    public override void OnInitialize()
    {
        base.OnInitialize();

        m_StateMachine = new StateMachine<E_STATE, RedPuniController>();
        m_StateMachine.AddState(new InnerState(E_STATE.ALONE, this, new AloneState()));
        m_StateMachine.AddState(new InnerState(E_STATE.ALONE_CALLED, this, new AloneCalledState()));
        m_StateMachine.AddState(new InnerState(E_STATE.ALONE_LEFT_OUT, this, new AloneLeftOutState()));
        m_StateMachine.AddState(new InnerState(E_STATE.COUPLE, this, new CoupleState()));
        m_StateMachine.AddState(new InnerState(E_STATE.COUPLE_SLIDE, this, new CoupleSlideState()));
        m_StateMachine.AddState(new InnerState(E_STATE.DAMAGED, this, new DamagedState()));
        m_StateMachine.AddState(new InnerState(E_STATE.GAME_OVER, this, new GameOverState()));
        m_StateMachine.AddState(new InnerState(E_STATE.GAME_CLEAR, this, new GameClearState()));

        m_PuniTrigger.TriggerEnterAction += OnEnterMoveObjectTrigger;

        m_ViewController.SetEmote(E_PUNI_EMOTE.NORMAL);
        m_ViewController.SetLook(E_PUNI_LOOK_DIR.FORWARD);
        m_ViewController.SetView(0);

        InGameManager.Instance.ChangeStateAction += OnChangeState;

        RequestChangeState(E_STATE.COUPLE);
    }

    public override void OnFinalize()
    {
        m_PuniTrigger.TriggerEnterAction -= OnEnterMoveObjectTrigger;
        m_StateMachine.OnFinalize();
        base.OnFinalize();
    }

    public override void OnStart()
    {
        base.OnStart();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        m_StateMachine.OnUpdate();
    }

    public override void OnLateUpdate()
    {
        base.OnLateUpdate();
        m_StateMachine.OnLateUpdate();
    }

    #endregion

    #region Alone State

    private class AloneState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.m_ViewController.SetLook(E_PUNI_LOOK_DIR.FORWARD, true);
            Target.Walk();
        }
    }

    #endregion

    #region Couple Slide State

    private class CoupleSlideState : StateCycle
    {
        private float m_MoveDir;

        public override void OnStart()
        {
            base.OnStart();

            var bluePuni = Target.m_BluePuni;
            if (bluePuni == null || bluePuni.XMoveSign == 0)
            {
                Target.RequestChangeState(E_STATE.COUPLE);
                return;
            }

            // 青プニとは逆方向に動くため
            m_MoveDir = -bluePuni.XMoveSign;

            Target.SetLookDirFromXMoveSign(m_MoveDir);
            Target.Walk();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var pos = Target.transform.position;
            pos.x += m_MoveDir * Target.m_CoupleSlideSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -7.3f, 7.3f);

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;
        }
    }

    #endregion

    #region Couple State

    private class CoupleState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.m_ViewController.SetLook(E_PUNI_LOOK_DIR.FORWARD, true);
            Target.Walk();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Target.m_BluePuni == null)
            {
                return;
            }

            var baseX = Target.m_BluePuni.transform.position.x;

            var pos = Target.transform.position;
            pos.x = baseX + Target.m_CoupleDistance * Target.m_BluePuni.PuniRelativePositionSign;
            pos.x = Mathf.Clamp(pos.x, -7.3f, 7.3f);

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;
        }
    }

    #endregion

    #region Alone Called State

    private class AloneCalledState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.Walk();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Target.m_BluePuni == null)
            {
                return;
            }

            var pos = Target.transform.position;
            var delta = Mathf.Sign(Target.m_BluePuni.transform.position.x - pos.x);
            pos.x += delta * Target.m_CoupleCalledSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -7.3f, 7.3f);

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;

            Target.SetLookDirFromXMoveSign(delta);
        }
    }

    #endregion

    #region Alone Left Out State

    private class AloneLeftOutState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.Alone();
            Target.Walk();
        }
    }

    #endregion

    #region Damaged State

    private class DamagedState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.m_PuniTrigger.SetEnableCollider(false);
            Target.m_Animator.Play(DAMAGED);
            Target.m_ViewController.SetEmote(E_PUNI_EMOTE.DAMAGED);
            Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(_ =>
            {
                Target.m_PuniTrigger.SetEnableCollider(true);
                Target.m_ViewController.SetEmote(E_PUNI_EMOTE.NORMAL);
                Target.RequestChangeState(E_STATE.ALONE);
                Target.m_BluePuni.Alone();
            }).AddTo(Target);
        }
    }

    #endregion

    #region Game Over State

    private class GameOverState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.m_PuniTrigger.SetEnableCollider(false);
            Target.m_Animator.Play(WALK);
            Target.m_ViewController.SetEmote(E_PUNI_EMOTE.ANGRY);
            Target.m_ViewController.SetLook(E_PUNI_LOOK_DIR.BACK);
            Target.m_ViewController.UpdateView();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            var pos = Target.transform.position;
            pos.z += Target.m_GameOverRunawaySpeed * Time.deltaTime;

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;
        }
    }

    #endregion

    #region Game Clear State

    private class GameClearState : StateCycle
    {

        private float m_MoveSpeed;
        private float m_MoveAfterLookDir;
        private bool m_IsLoving;

        public override void OnStart()
        {
            base.OnStart();

            var targetX = Target.m_BluePuni.PuniRelativePositionSign < 0 ? Target.m_GameClearLeftSidePosX : Target.m_GameClearRightSidePosX;
            var pos = Target.transform.position;
            m_MoveSpeed = (targetX - pos.x) / Target.m_GameClearMoveDuration;

            // 青プニにとっての向きなので、赤プニではそのまま使う
            m_MoveAfterLookDir = -Target.m_BluePuni.PuniRelativePositionSign;

            m_IsLoving = false;

            Observable.Timer(TimeSpan.FromSeconds(Target.m_GameClearMoveDuration)).Subscribe(_ =>
            {
                m_IsLoving = true;
                Target.m_Animator.Play(STOP);
                Target.SetLookDirFromXMoveSign(m_MoveAfterLookDir);

                Observable.Timer(TimeSpan.FromSeconds(Target.m_GameClearWaitDuration)).Subscribe(__ =>
                {
                    Target.m_ViewController.SetEmote(E_PUNI_EMOTE.LOVE, true);
                    // ハートパーティクルを発火
                    Target.m_HeartAnimationObject.SetActive(true);
                }).AddTo(Target);
            }).AddTo(Target);

            Target.m_ViewController.SetEmote(E_PUNI_EMOTE.NORMAL);
            Target.Walk();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (!m_IsLoving)
            {
                var pos = Target.transform.position;
                pos.x += m_MoveSpeed * Time.deltaTime;
                Target.transform.position = pos;
            }
        }
    }

    #endregion

    #region Collider & Trigger

    private void OnEnterMoveObjectTrigger(Collider other, Collider self)
    {
        IMoveObject moveObj = null;
        var t = other.transform;
        while (!t.TryGetComponent<IMoveObject>(out moveObj))
        {
            if (t.parent == null)
            {
                break;
            }

            t = t.parent;
        }

        moveObj?.OnEnterPuni(this);
    }

    public void KnockBack()
    {
        RequestChangeState(E_STATE.DAMAGED);
    }

    #endregion

    #region View

    private void SetLookDirFromXMoveSign(float xMoveSign)
    {
        if (xMoveSign == 0)
        {
            m_ViewController.SetLook(E_PUNI_LOOK_DIR.FORWARD, true);
        }
        else if (xMoveSign > 0)
        {
            m_ViewController.SetLook(E_PUNI_LOOK_DIR.RIGHT, true);
        }
        else
        {
            m_ViewController.SetLook(E_PUNI_LOOK_DIR.LEFT, true);
        }
    }

    #endregion

    private void RequestChangeState(E_STATE state)
    {
        m_StateMachine?.Goto(state);
    }

    private void OnChangeState(E_INGAME_STATE state)
    {
        if (state == E_INGAME_STATE.GAME_CLEAR)
        {
            RequestChangeState(E_STATE.GAME_CLEAR);
        }
        else if (state == E_INGAME_STATE.GAME_OVER)
        {
            RequestChangeState(E_STATE.GAME_OVER);
        }
    }

    private void Walk()
    {
        m_Animator.Play(WALK);
    }

    public void Alone()
    {
        RequestChangeState(E_STATE.ALONE);
    }

    /// <summary>
    /// 青プニに呼び寄せられる
    /// </summary>
    public void CalledWithBluePuni()
    {
        RequestChangeState(E_STATE.ALONE_CALLED);
    }

    /// <summary>
    /// 青プニに離される
    /// </summary>
    public void LeftOutWithBluePuni()
    {
        RequestChangeState(E_STATE.ALONE_LEFT_OUT);
    }

    /// <summary>
    /// カップル状態にする
    /// </summary>
    public void Couple()
    {
        RequestChangeState(E_STATE.COUPLE);
    }

    /// <summary>
    /// カップル状態の時に位置を入れ替える
    /// </summary>
    public void SlideCouple()
    {
        RequestChangeState(E_STATE.COUPLE_SLIDE);
    }
}
