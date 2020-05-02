using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 雲制御クラス
/// </summary>
public class CloudController : MonoBehaviour
{
    #region Field Inspector

    [SerializeField]
    private Renderer m_Renderer;

    #endregion

    #region Field

    private float m_MoveSpeed;
    private GradientSet m_GradientSet;
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
        transform.Translate(Vector3.forward * m_MoveSpeed * Time.deltaTime, Space.World);
        if (transform.position.z <= 0)
        {
            gameObject.SetActive(false);
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
                m_MaterialPropBlock.SetColor(ShaderPropertyID.Instance.GetID(s.Name), s.GetColor(progress));
            }
            m_Renderer.SetPropertyBlock(m_MaterialPropBlock);
        }
    }

    public void SetMoveSpeed(float speed)
    {
        m_MoveSpeed = speed;
    }

    public void SetGradientSet(GradientSet gradientSet)
    {
        m_GradientSet = gradientSet;
    }

    public void SetFlipX()
    {
        if (m_Renderer != null)
        {
            m_Renderer.transform.rotation = transform.position.x < 0 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
        }
    }
}
