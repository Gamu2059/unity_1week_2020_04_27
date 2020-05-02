using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 星空制御クラス
/// </summary>
public class StarrySkyController : MonoBehaviour
{
    #region Field Inspector

    [SerializeField]
    private AnimationCurve m_AlphaCurve;

    [SerializeField]
    private string m_AlphaPropName = "_Alpha";

    [SerializeField]
    private Renderer m_Renderer;

    #endregion

    #region Field

    private MaterialPropertyBlock m_MaterialPropBlock;

    #endregion

    private void Awake()
    {
        m_MaterialPropBlock = new MaterialPropertyBlock();
        m_Renderer.GetPropertyBlock(m_MaterialPropBlock);
    }

    private void Start()
    {
        ApplyProgress();
    }

    private void Update()
    {
        ApplyProgress();
    }

    private void ApplyProgress()
    {
        if (InGameManager.Instance != null)
        {
            var progress = InGameManager.Instance.Progress.Value;
            var alpha = m_AlphaCurve.Evaluate(progress);
            m_MaterialPropBlock.SetFloat(ShaderPropertyID.Instance.GetID(m_AlphaPropName), alpha);
            m_Renderer.SetPropertyBlock(m_MaterialPropBlock);
        }
    }
}
