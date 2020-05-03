using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 雨制御クラス
/// </summary>
public class RainController : MonoBehaviour
{
    #region Field Inspcetor

    [SerializeField]
    private AnimationCurve m_AlphaCurve;

    [SerializeField]
    private string m_AlphaPropName = "_Alpha";

    [SerializeField]
    private float m_DestroyTimingProgress;

    [SerializeField]
    private Renderer[] m_Renderers;

    #endregion

    #region Field

    private MaterialPropertyBlock[] m_MaterialPropBlocks;

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
        AudioManager.Instance.PlayBGM(AudioManagerKeyWord.Rain);
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
            var alpha = m_AlphaCurve.Evaluate(progress);
            for (var i = 0; i < m_Renderers.Length; i++)
            {
                m_MaterialPropBlocks[i].SetFloat(ShaderPropertyID.Instance.GetID(m_AlphaPropName), alpha);
                m_Renderers[i].SetPropertyBlock(m_MaterialPropBlocks[i]);
            }

            AudioManager.Instance.ChangeVolumeBGM(AudioManagerKeyWord.Rain, alpha);
        }
    }
}
