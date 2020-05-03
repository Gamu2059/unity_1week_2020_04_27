#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;

public class BluePuniController : ControllableMonoBehavior, IPuni
{
    #region Define

    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    private const string WALK = "Puni@walk";
    private const string DAMAGED = "Puni@damaged";
    private const string STOP = "Puni@stop";

    private enum E_STATE
    {
        /// <summary>
        /// 1人でいる時
        /// </summary>
        ALONE,

        /// <summary>
        /// 2人でいる時に、違う方向に移動する
        /// </summary>
        COUPLE_SLIDE,

        /// <summary>
        /// 2人でいる時
        /// </summary>
        COUPLE,

        /// <summary>
        /// 1人でいる時に、赤プニを呼んでいる
        /// </summary>
        ALONE_CALL,

        /// <summary>
        /// 1人でいる時に、赤プニを離した
        /// </summary>
        ALONE_LEAVE,

        /// <summary>
        /// 障害物に当たった時
        /// </summary>
        DAMAGED,

        /// <summary>
        /// ゲームオーバーになった時
        /// </summary>
        GAME_OVER,

        /// <summary>
        /// ゲームクリアになった時
        /// </summary>
        GAME_CLEAR,
    }

    private class StateCycle : StateCycleBase<BluePuniController, E_STATE> { }

    private class InnerState : State<E_STATE, BluePuniController>
    {
        public InnerState(E_STATE state, BluePuniController target) : base(state, target) { }
        public InnerState(E_STATE state, BluePuniController target, StateCycle cycle) : base(state, target, cycle) { }
    }

    #endregion

    #region Field Inspector

    [Header("Component")]

    [SerializeField]
    private PuniViewController m_ViewController;

    [SerializeField]
    private Animator m_Animator;

    [SerializeField]
    private RedPuniController m_RedPuni;

    [Header("Collision")]

    [SerializeField]
    private ColliderDetector m_PuniTrigger;

    [Header("Parameter")]

    [SerializeField]
    private float m_AloneSpeed;

    [SerializeField]
    private float m_CoupleSpeed;

    [SerializeField]
    private float m_CoupleSlideSpeed;

    [SerializeField]
    private bool m_IsForwardAtract;

    [SerializeField]
    private float m_GameClearLeftSidePosX;

    [SerializeField]
    private float m_GameClearRightSidePosX;

    [SerializeField]
    private float m_GameClearMoveDuration;

    [SerializeField]
    private float m_GameClearWaitDuration;

    #endregion

    #region Field

    private StateMachine<E_STATE, BluePuniController> m_StateMachine;

    public float XMoveSign { get; private set; }

    // 赤プニに対して青プニが左右のどちらにいるか

    public float PuniRelativePositionSign { get; private set; }

    public bool IsBluePuni => true;

    #endregion

    #region Game Cycle

    public override void OnInitialize()
    {
        base.OnInitialize();

        m_StateMachine = new StateMachine<E_STATE, BluePuniController>();
        m_StateMachine.AddState(new InnerState(E_STATE.ALONE, this, new AloneState()));
        m_StateMachine.AddState(new InnerState(E_STATE.ALONE_CALL, this, new AloneCallState()));
        m_StateMachine.AddState(new InnerState(E_STATE.ALONE_LEAVE, this, new AloneLeaveState()));
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
            if (Target.m_IsForwardAtract)
            {
                Target.SetEnablePuniTrigger(true);
            }
            Target.SetLookDirFromXMoveSign(Target.XMoveSign);
            Target.Walk();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (Input.GetKeyDown(KeyCode.C))
            {
                // 呼び寄せる
                Target.CallRedPuni();
                Target.m_RedPuni.CalledWithBluePuni();
                return;
            }

            var x = Input.GetAxis(HORIZONTAL);
            var preXMoveSign = Target.XMoveSign;
            Target.XMoveSign = x == 0 ? 0 : Mathf.Sign(x);

            var pos = Target.transform.position;
            pos.x += x * Target.m_AloneSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -7.3f, 7.3f);

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;

            if (preXMoveSign != Target.XMoveSign)
            {
                Target.SetLookDirFromXMoveSign(Target.XMoveSign);
            }
        }

        public override void OnEnd()
        {
            if (Target.m_IsForwardAtract)
            {
                Target.SetEnablePuniTrigger(false);
            }
            base.OnEnd();
        }
    }

    #endregion

    #region Couple Slide State

    private class CoupleSlideState : StateCycle
    {
        private float m_TargetXPosition;
        private float m_MoveDir;

        public override void OnStart()
        {
            base.OnStart();

            if (Target.m_RedPuni == null || Target.XMoveSign == 0)
            {
                Target.RequestChangeState(E_STATE.COUPLE);
                Target.m_RedPuni.Couple();
                return;
            }

            m_TargetXPosition = Target.m_RedPuni.transform.position.x;
            m_MoveDir = Target.XMoveSign;

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

            if (Target.m_RedPuni != null)
            {
                var redPuniPos = Target.m_RedPuni.transform.position;
                var delta = redPuniPos.x - pos.x;
                Target.PuniRelativePositionSign = delta == 0 ? 0 : Mathf.Sign(delta);
            }

            Target.transform.position = pos;

            if (m_MoveDir > 0 && m_TargetXPosition <= pos.x || m_MoveDir < 0 && m_TargetXPosition >= pos.x)
            {
                Target.RequestChangeState(E_STATE.COUPLE);
                Target.m_RedPuni.Couple();
            }
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

            if (Input.GetKeyDown(KeyCode.X))
            {
                // 引きはがし
                Target.LeaveRedPuni();
                Target.m_RedPuni.LeftOutWithBluePuni();
                return;
            }

            var x = Input.GetAxis(HORIZONTAL);
            Target.XMoveSign = x == 0 ? 0 : Mathf.Sign(x);

            var pos = Target.transform.position;
            if (Target.m_RedPuni != null)
            {
                var redPuniPos = Target.m_RedPuni.transform.position;
                var delta = redPuniPos.x - pos.x;
                Target.PuniRelativePositionSign = delta == 0 ? 0 : Mathf.Sign(delta);
            }

            // 移動方向と位置関係が逆になっていれば
            if (Target.XMoveSign * Target.PuniRelativePositionSign > 0)
            {
                Target.SlideCouple();
                Target.m_RedPuni.SlideCouple();
                return;
            }

            pos.x += x * Target.m_CoupleSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -7.3f, 7.3f);

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;
        }
    }

    #endregion

    #region Alone Call State

    private class AloneCallState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.SetEnablePuniTrigger(true);
            Target.SetLookDirFromXMoveSign(Target.XMoveSign);
            Target.Walk();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var x = Input.GetAxis(HORIZONTAL);
            var preXMoveSign = Target.XMoveSign;
            Target.XMoveSign = x == 0 ? 0 : Mathf.Sign(x);

            var pos = Target.transform.position;
            pos.x += x * Target.m_AloneSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -7.3f, 7.3f);

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;

            if (preXMoveSign != Target.XMoveSign)
            {
                Target.SetLookDirFromXMoveSign(Target.XMoveSign);
            }
        }

        public override void OnEnd()
        {
            Target.SetEnablePuniTrigger(false);
            base.OnEnd();
        }
    }

    #endregion

    #region Alone Leave State

    private class AloneLeaveState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.SetEnablePuniTrigger(true);
            Target.SetLookDirFromXMoveSign(Target.XMoveSign);
            Target.Walk();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var x = Input.GetAxis(HORIZONTAL);
            var preXMoveSign = Target.XMoveSign;
            Target.XMoveSign = x == 0 ? 0 : Mathf.Sign(x);

            var pos = Target.transform.position;
            pos.x += x * Target.m_AloneSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -7.3f, 7.3f);

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;

            Target.RequestChangeState(E_STATE.ALONE);
            Target.m_RedPuni.Alone();

            if (preXMoveSign != Target.XMoveSign)
            {
                Target.SetLookDirFromXMoveSign(Target.XMoveSign);
            }
        }

        public override void OnEnd()
        {
            Target.SetEnablePuniTrigger(false);
            base.OnEnd();
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
                Target.m_RedPuni.Alone();
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
            Target.m_Animator.Play(STOP);
            Target.m_ViewController.SetEmote(E_PUNI_EMOTE.STUNNED);
            Target.m_ViewController.SetLook(E_PUNI_LOOK_DIR.BACK);
            Target.m_ViewController.UpdateView();
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

            // 負の時は左、0以上は右
            var targetX = -Target.PuniRelativePositionSign < 0 ? Target.m_GameClearLeftSidePosX : Target.m_GameClearRightSidePosX;
            var pos = Target.transform.position;
            m_MoveSpeed = (targetX - pos.x) / Target.m_GameClearMoveDuration;
            
            m_MoveAfterLookDir = Target.PuniRelativePositionSign;

            m_IsLoving = false;

            Observable.Timer(TimeSpan.FromSeconds(Target.m_GameClearMoveDuration)).Subscribe(_ =>
            {
                m_IsLoving = true;
                Target.m_Animator.Play(STOP);
                Target.SetLookDirFromXMoveSign(m_MoveAfterLookDir);

                Observable.Timer(TimeSpan.FromSeconds(Target.m_GameClearWaitDuration)).Subscribe(__ =>
                {
                    Target.m_ViewController.SetEmote(E_PUNI_EMOTE.LOVE);
                    Target.Walk();
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

    #region Collider & Trigger

    private void SetEnablePuniTrigger(bool isEnable)
    {
        if (isEnable)
        {
            // 重複登録を避けるため、一度削除する
            if (m_PuniTrigger.TriggerEnterAction != null && m_PuniTrigger.TriggerEnterAction.GetInvocationList().Length > 0)
            {
                m_PuniTrigger.TriggerEnterAction -= OnEnterPuniTrigger;
            }
            m_PuniTrigger.TriggerEnterAction += OnEnterPuniTrigger;
        }
        else
        {
            m_PuniTrigger.TriggerEnterAction -= OnEnterPuniTrigger;
        }
    }

    private void OnEnterPuniTrigger(Collider other, Collider self)
    {
        // お互いに触れたら
        if (other.tag == TagName.Puni)
        {
            RequestChangeState(E_STATE.COUPLE);
            m_RedPuni.Couple();
        }
    }

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

    private void Walk()
    {
        m_Animator.Play(WALK);
    }

    public void Alone()
    {
        RequestChangeState(E_STATE.ALONE);
    }

    /// <summary>
    /// 赤プニを呼び寄せる
    /// </summary>
    public void CallRedPuni()
    {
        RequestChangeState(E_STATE.ALONE_CALL);
    }

    /// <summary>
    /// 赤プニを離す
    /// </summary>
    public void LeaveRedPuni()
    {
        RequestChangeState(E_STATE.ALONE_LEAVE);
    }

    /// <summary>
    /// カップル状態の時に位置を入れ替える
    /// </summary>
    public void SlideCouple()
    {
        RequestChangeState(E_STATE.COUPLE_SLIDE);

    }
}
