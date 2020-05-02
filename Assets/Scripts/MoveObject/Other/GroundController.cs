using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 地面制御クラス
/// </summary>
public class GroundController : MonoBehaviour
{
    #region Field Inspector

    [SerializeField]
    private Renderer m_Renderer;

    [SerializeField]
    private GradientSet m_GradientSet;

    #endregion

    #region Field

    private MaterialPropertyBlock m_MaterialPropBlock;

    #endregion

    #region Game Cycle

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

    #endregion

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
        }
    }
}
