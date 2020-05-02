using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// とげとげ制御クラス
/// </summary>
public class ThornController : MonoBehaviour, IMoveObject
{
    #region Define

    private enum E_STATE
    {
        /// <summary>
        /// 待機状態
        /// </summary>
        STAY,

        /// <summary>
        /// 落下
        /// </summary>
        FALL,

        /// <summary>
        /// 落下後移動
        /// </summary>
        MOVE,
    }

    private class StateCycle : StateCycleBase<ThornController, E_STATE> { }

    private class InnerState : State<E_STATE, ThornController>
    {
        public InnerState(E_STATE state, ThornController target) : base(state, target) { }
        public InnerState(E_STATE state, ThornController target, StateCycle cycle) : base(state, target, cycle) { }
    }

    #endregion

    #region Field Inspector

    [SerializeField]
    private Renderer m_Renderer;

    [SerializeField]
    private SpriteRenderer m_ShadowRenderer;

    [SerializeField]
    private AnimationCurve m_FallMovementCurve;

    [SerializeField]
    private AnimationCurve m_XMoveDecreaseCurve;

    [SerializeField]
    private float m_ThornRadius;

    #endregion

    #region Field

    private StateMachine<E_STATE, ThornController> m_StateMachine;
    private bool m_IsCollided;
    private float m_MoveSpeed;
    private int m_DamageValue;
    private float m_XMoveSpeed;

    private MaterialPropertyBlock m_MaterialPropBlock;
    private GradientSet m_GradientSet;
    private GradientSet m_ShadowGradientSet;

    #endregion

    #region Game Cycle

    private void Awake()
    {
        m_StateMachine = new StateMachine<E_STATE, ThornController>();
        m_StateMachine.AddState(new InnerState(E_STATE.STAY, this, new StayState()));
        m_StateMachine.AddState(new InnerState(E_STATE.FALL, this, new FallState()));
        m_StateMachine.AddState(new InnerState(E_STATE.MOVE, this, new MoveState()));

        RequestChangeState(E_STATE.STAY);

        m_MaterialPropBlock = new MaterialPropertyBlock();
        m_Renderer.GetPropertyBlock(m_MaterialPropBlock);
    }

    private void OnDestroy()
    {
        m_StateMachine.OnFinalize();
        m_StateMachine = null;
    }

    private void Update()
    {
        m_StateMachine?.OnUpdate();

        ApplyProgress();
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
            Target.m_Renderer.transform.rotation = Quaternion.identity;
        }
    }

    #endregion

    #region Fall State

    private class FallState : StateCycle
    {
        private float m_TimeCount;
        private float m_CurrentYPos;
        private float m_GroundYPos;

        public override void OnStart()
        {
            base.OnStart();
            m_TimeCount = 0;

            m_GroundYPos = 0;
            var pos = Target.transform.position;
            if (GroundManager.Instance != null)
            {
                m_GroundYPos = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }
            m_CurrentYPos = pos.y;

            Apply();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            m_TimeCount += Time.deltaTime;
            Apply();

            if (Target.transform.position.y <= m_GroundYPos)
            {
                Target.RequestChangeState(E_STATE.MOVE);
            }
        }

        private void Apply()
        {
            // 毎フレーム位置を固定させておかないといけない
            var shadowPos = Target.m_ShadowRenderer.transform.position;
            shadowPos.y = m_GroundYPos;
            Target.m_ShadowRenderer.transform.position = shadowPos;

            var rate = Target.m_FallMovementCurve.Evaluate(m_TimeCount);
            var pos = Target.transform.position;
            pos.y = Mathf.Lerp(m_CurrentYPos, m_GroundYPos, rate);
            Target.transform.position = pos;
        }
    }

    #endregion

    #region Move State

    private class MoveState : StateCycle
    {
        private float m_TimeCount;
        private float m_MoveDir;
        private float m_RotateBaseAmount;

        public override void OnStart()
        {
            base.OnStart();
            m_TimeCount = 0;
            Target.m_ShadowRenderer.transform.localPosition = Vector3.zero;
            m_MoveDir = -Mathf.Sign(Target.transform.position.x);
            m_RotateBaseAmount = 180 / (Mathf.PI * Target.m_ThornRadius);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            var pos = Target.transform.position;            
            if (pos.z < 0)
            {
                Target.RequestChangeState(E_STATE.STAY);
                return;
            }

            var move = Target.m_XMoveDecreaseCurve.Evaluate(m_TimeCount) * Target.m_XMoveSpeed;
            pos.x += m_MoveDir * move * Time.deltaTime;
            pos.z += Target.m_MoveSpeed * Time.deltaTime;

            if (GroundManager.Instance != null)
            {
                pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
            }

            Target.transform.position = pos;

            var deltaRotate = -m_MoveDir * move * m_RotateBaseAmount * Time.deltaTime;
            var rot = Target.m_Renderer.transform.localEulerAngles;
            rot.z += deltaRotate;
            Target.m_Renderer.transform.localEulerAngles = rot;

            m_TimeCount += Time.deltaTime;
        }
    }

    #endregion

    private void RequestChangeState(E_STATE state)
    {
        m_StateMachine?.Goto(state);
    }

    private void ApplyProgress()
    {
        if (InGameManager.Instance != null)
        {
            var progress = InGameManager.Instance.Progress.Value;
            foreach (var s in m_GradientSet.Set)
            {
                m_MaterialPropBlock.SetColor(ShaderPropertyID.Instance.GetID(s.Name), s.GetColor(progress));
            }
            m_Renderer.SetPropertyBlock(m_MaterialPropBlock);
            m_ShadowRenderer.color = m_ShadowGradientSet.Set[0].GetColor(progress);
        }
    }

    /// <summary>
    /// 生成された時に呼び出される
    /// </summary>
    public void OnGenerated(float moveSpeed, float xMoveSpeed, int damage, GradientSet gradientSet, GradientSet shadowGradientSet)
    {
        m_MoveSpeed = moveSpeed;
        m_XMoveSpeed = xMoveSpeed;
        m_DamageValue = damage;
        m_GradientSet = gradientSet;
        m_ShadowGradientSet = shadowGradientSet;
        m_IsCollided = false;

        RequestChangeState(E_STATE.FALL);
    }

    public void OnEnterPuni(IPuni puni)
    {
        if (m_IsCollided)
        {
            return;
        }

        m_IsCollided = true;
        RequestChangeState(E_STATE.STAY);

        if (InGameManager.Instance != null)
        {
            var damage = m_DamageValue * InGameManager.Instance.PlayerSkill.Value;
            InGameManager.Instance.Damaged(damage);
        }
    }
}
