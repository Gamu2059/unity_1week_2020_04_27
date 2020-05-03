using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 木制御クラス
/// </summary>
public class TreeController : MonoBehaviour
{
    #region Field Inspector

    [SerializeField, Tooltip("プールから取り出す時に手掛かりにする識別子")]
    private string m_Id;
    public string Id => m_Id;

    [SerializeField]
    private Renderer[] m_Renderers;

    #endregion

    #region Field

    private Vector3 m_MoveSpeed;
    private GradientSet m_GradientSet;
    private MaterialPropertyBlock[] m_MaterialPropBlocks;
    private AnimationCurve m_AlphaCurve;
    private float m_TimeCount;

    #endregion

    #region Game Cycle

    private void Awake()
    {
        m_MaterialPropBlocks = new MaterialPropertyBlock[m_Renderers.Length];
        for (var i = 0; i < m_Renderers.Length; i++)
        {
            var m = new MaterialPropertyBlock();
            m_Renderers[i].GetPropertyBlock(m);
            m_MaterialPropBlocks[i] = m;
        }
    }

    private void Start()
    {
        ApplyProgress();
        if (transform.position.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    private void Update()
    {
        var ts = InGameManager.Instance.TimeScale;
        var pos = transform.position;
        pos += m_MoveSpeed * Time.deltaTime * ts;

        if (pos.z <= 0)
        {
            gameObject.SetActive(false);
        }

        transform.position = pos;
        m_TimeCount += Time.deltaTime * ts;
        ApplyProgress();
    }

    #endregion

    public void ApplyProgress()
    {
        if (InGameManager.Instance != null)
        {
            var progress = InGameManager.Instance.Progress.Value;
            var alpha = m_AlphaCurve.Evaluate(m_TimeCount);
            for (var i = 0; i < m_Renderers.Length; i++)
            {
                foreach (var s in m_GradientSet.Set)
                {
                    var c = s.GetColor(progress);
                    c.a *= alpha;
                    m_MaterialPropBlocks[i].SetColor(ShaderPropertyID.Instance.GetID(s.Name), c);
                }
                m_Renderers[i].SetPropertyBlock(m_MaterialPropBlocks[i]);
            }
        }
    }

    public void SetMoveSpeed(Vector3 speed)
    {
        m_MoveSpeed = speed;
    }

    public void SetGradientSet(GradientSet gradientSet)
    {
        m_GradientSet = gradientSet;
    }

    public void SetAlphaCurve(AnimationCurve alphaCurve)
    {
        m_AlphaCurve = alphaCurve;
    }

    public void ResetTimeCount()
    {
        m_TimeCount = 0;
    }
}
