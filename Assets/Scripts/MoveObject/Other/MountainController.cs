using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 山制御クラス
/// </summary>
public class MountainController : MonoBehaviour
{
    #region Field Inspector

    [SerializeField]
    private Renderer m_Renderer;

    [SerializeField]
    private GradientSet m_GradientSet;

    [SerializeField]
    private float m_DestroyTimingProgress;

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
        if (InGameManager.Instance != null)
        {
            var progress = InGameManager.Instance.Progress.Value;
            if (progress >= m_DestroyTimingProgress)
            {
                Destroy(gameObject);
                return;
            }
        }

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
                    var c = s.GetColor(progress);
                    m_MaterialPropBlock.SetColor(ShaderPropertyID.Instance.GetID(s.Name), c);
                }
                m_Renderer.SetPropertyBlock(m_MaterialPropBlock);
        }
    }
}
