#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BluePuniController : ControllableMonoBehavior, IPuni
{
    #region Define

    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

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
        ALONE_LEAVE
    }

    private class StateCycle : StateCycleBase<BluePuniController, E_STATE> { }

    private class InnerState : State<E_STATE, BluePuniController>
    {
        public InnerState(E_STATE state, BluePuniController target) : base(state, target) { }
        public InnerState(E_STATE state, BluePuniController target, StateCycle cycle) : base(state, target, cycle) { }
    }

    #endregion

    #region Field Inspector

    [Header("Collision")]

    [SerializeField]
    private ColliderDetector m_PuniTrigger;

    [Header("Parameter")]

    [SerializeField]
    private RedPuniController m_RedPuni;

    [SerializeField]
    private float m_AloneSpeed;

    [SerializeField]
    private float m_CoupleSpeed;

    [SerializeField]
    private float m_CoupleSlideSpeed;

    [SerializeField]
    private E_STATE m_State;

    [SerializeField]
    private bool m_IsForwardAtract;

    #endregion

    #region Field

    private StateMachine<E_STATE, BluePuniController> m_StateMachine;

    public float XMoveSign { get; private set; }
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

        m_PuniTrigger.TriggerEnterAction += OnEnterMoveObjectTrigger;

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
            Target.XMoveSign = x == 0 ? 0 : Mathf.Sign(x);

            var pos = Target.transform.position;
            pos.x += x * Target.m_AloneSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -7.3f, 7.3f);

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;
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
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var x = Input.GetAxis(HORIZONTAL);
            Target.XMoveSign = x == 0 ? 0 : Mathf.Sign(x);

            var pos = Target.transform.position;
            pos.x += x * Target.m_AloneSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -7.3f, 7.3f);

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;
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
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var x = Input.GetAxis(HORIZONTAL);
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
        }

        public override void OnEnd()
        {
            Target.SetEnablePuniTrigger(false);
            base.OnEnd();
        }
    }

    #endregion

    private void RequestChangeState(E_STATE state)
    {
        m_StateMachine?.Goto(state);
        m_State = state;
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

    #endregion

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
