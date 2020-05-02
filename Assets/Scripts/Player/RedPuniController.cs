using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RedPuniController : ControllableMonoBehavior, IPuni
{
    #region Define

    private const string WALK = "Puni@walk";

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
    private float m_AloneSpeed;

    [SerializeField]
    private float m_CoupleCalledSpeed;

    [SerializeField]
    private float m_CoupleSlideSpeed;

    [SerializeField]
    private float m_CoupleDistance;

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

        m_PuniTrigger.TriggerEnterAction += OnEnterMoveObjectTrigger;

        m_ViewController.SetEmote(E_PUNI_EMOTE.NORMAL);
        m_ViewController.SetLook(E_PUNI_LOOK_DIR.FORWARD);
        m_ViewController.SetView(0);

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
