using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyPuniController : MonoBehaviour, IMoveObject
{
    #region Define

    private const string WALK = "Puni@walk";
    private const string DAMAGED = "Puni@damaged";

    private enum E_STATE
    {
        /// <summary>
        /// 待機状態
        /// </summary>
        STAY,

        /// <summary>
        /// 向こうから移動してくる
        /// </summary>
        WALK,

        /// <summary>
        /// ぶつかった
        /// </summary>
        DAMAGED,

        /// <summary>
        /// 逃げる
        /// </summary>
        RUN_AWAY,
    }

    private class StateCycle : StateCycleBase<EnemyPuniController, E_STATE> { }

    private class InnerState : State<E_STATE, EnemyPuniController>
    {
        public InnerState(E_STATE state, EnemyPuniController target) : base(state, target) { }
        public InnerState(E_STATE state, EnemyPuniController target, StateCycle cycle) : base(state, target, cycle) { }
    }

    #endregion

    #region Field Inspector

    [SerializeField]
    private PuniViewController m_ViewController;

    [SerializeField]
    private ColliderDetector m_Trigger;

    [SerializeField]
    private Animator m_Animator;

    [SerializeField]
    private float m_DamagedMoveSpeed;

    [SerializeField]
    private float m_RunawayMoveSpeed;

    #endregion

    #region Field

    private StateMachine<E_STATE, EnemyPuniController> m_StateMachine;
    private float m_MoveSpeed;
    private int m_DamageValue;

    private bool m_IsCollided;

    // ぶつかった相手のX座標
    private float m_CollidedXPosition;

    #endregion

    #region Game Cycle

    private void Awake()
    {
        m_StateMachine = new StateMachine<E_STATE, EnemyPuniController>();
        m_StateMachine.AddState(new InnerState(E_STATE.STAY, this, new StayState()));
        m_StateMachine.AddState(new InnerState(E_STATE.WALK, this, new WalkState()));
        m_StateMachine.AddState(new InnerState(E_STATE.DAMAGED, this, new DamagedState()));
        m_StateMachine.AddState(new InnerState(E_STATE.RUN_AWAY, this, new RunAwayState()));

        RequestChangeState(E_STATE.STAY);
    }

    private void OnDestroy()
    {
        m_StateMachine.OnFinalize();
        m_StateMachine = null;
    }

    private void Update()
    {
        m_StateMachine?.OnUpdate();
    }

    private void LateUpdate()
    {
        m_StateMachine?.OnLateUpdate();
    }

    #endregion

    #region Stay State
    
    private class StayState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.gameObject.SetActive(false);
        }
    }
    
    #endregion

    #region Walk State

    private class WalkState : StateCycle
    {
        public override void OnStart()
        {
            base.OnStart();
            Target.m_Animator.Play(WALK);
            Target.m_Trigger.SetEnableCollider(true);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var pos = Target.transform.position;
            pos.z += Target.m_MoveSpeed * Time.deltaTime;

            if (pos.z < 0)
            {
                Target.RequestChangeState(E_STATE.STAY);
                return;
            }

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;
        }
    }

    #endregion

    #region Damaged State

    private class DamagedState : StateCycle
    {
        private float m_AnimationWaitCount;
        private float m_DamagedMoveDir;

        public override void OnStart()
        {
            base.OnStart();
            Target.m_Trigger.SetEnableCollider(false);
            Target.m_Animator.Play(DAMAGED);
            Target.m_ViewController.SetEmote(E_PUNI_EMOTE.DAMAGED);
            m_AnimationWaitCount = 0;

            var pos = Target.transform.position;
            var delta = pos.x - Target.m_CollidedXPosition;
            m_DamagedMoveDir = Mathf.Sign(delta);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (m_AnimationWaitCount < 1f)
            {
                var pos = Target.transform.position;
                pos.x += m_DamagedMoveDir * Target.m_DamagedMoveSpeed * Time.deltaTime;
                Target.transform.position = pos;
            }
            else if (m_AnimationWaitCount >= 2f)
            {
                Target.RequestChangeState(E_STATE.RUN_AWAY);
                return;
            }
            else
            {
                var pos = Target.transform.position;
                pos.z += -Target.m_DamagedMoveSpeed * Time.deltaTime;

                if (GroundManager.Instance != null)
                {
                    pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
                }

                Target.transform.position = pos;
            }

            m_AnimationWaitCount += Time.deltaTime;
        }
    }

    #endregion

    #region Run Away State

    private class RunAwayState : StateCycle
    {
        private float m_MoveDir;
        private float m_TimeCount;

        public override void OnStart()
        {
            base.OnStart();
            m_MoveDir = Mathf.Sign(Target.transform.position.x);

            Target.m_ViewController.SetEmote(E_PUNI_EMOTE.ANGRY);
            Target.m_ViewController.SetLook(m_MoveDir > 0 ? E_PUNI_LOOK_DIR.RIGHT : E_PUNI_LOOK_DIR.LEFT);
            Target.m_Animator.Play(WALK);
            m_TimeCount = 0;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var pos = Target.transform.position;
            pos.x += m_MoveDir * Target.m_RunawayMoveSpeed * Time.deltaTime;

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;

            if (m_TimeCount >= 2f)
            {
                Target.RequestChangeState(E_STATE.STAY);
                return;
            }

            m_TimeCount += Time.deltaTime;
        }
    }

    #endregion

    private void RequestChangeState(E_STATE state)
    {
        m_StateMachine?.Goto(state);
    }

    /// <summary>
    /// 生成された時に呼び出される
    /// </summary>
    public void OnGenerated(EnemyPuniGeneratedData data, float moveSpeed, int damage)
    {
        m_MoveSpeed = moveSpeed;
        m_DamageValue = damage;
        m_IsCollided = false;

        m_ViewController.SetPartsColor(E_PUNI_PARTS.BODY_1, data.Body1Color);
        m_ViewController.SetPartsColor(E_PUNI_PARTS.BODY_2, data.Body2Color);
        m_ViewController.SetShadowColor(data.ShadowColor);
        m_ViewController.SetEmote(E_PUNI_EMOTE.NORMAL);
        m_ViewController.SetLook(E_PUNI_LOOK_DIR.BACK);
        m_ViewController.SetView(0);

        RequestChangeState(E_STATE.WALK);
    }

    public void OnEnterPuni(IPuni puni)
    {
        if (m_IsCollided)
        {
            return;
        }

        m_IsCollided = true;
        RequestChangeState(E_STATE.DAMAGED);

        if (InGameManager.Instance != null)
        {
            var damage = m_DamageValue * InGameManager.Instance.PlayerSkill.Value;
            InGameManager.Instance.Damaged(damage);
        }
    }
}
