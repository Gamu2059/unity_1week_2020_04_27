using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 草制御クラス
/// </summary>
public class GrassController : MonoBehaviour
{
    #region Field Inspector

    [SerializeField, Tooltip("プールから取り出す時に手掛かりにする識別子")]
    private string m_Id;
    public string Id => m_Id;

    [SerializeField]
    private Renderer[] m_Renderers;

    #endregion

    #region Field

    private float m_MoveSpeed;
    private GradientSet m_GradientSet;
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
        ApplyProgress();
    }

    private void Update()
    {
        var pos = transform.position;
        pos.z += m_MoveSpeed * Time.deltaTime;

        if (GroundManager.Instance != null)
        {
            pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
        }

        if (pos.z <= 0)
        {
            gameObject.SetActive(false);
        }

        if (GroundManager.Instance != null)
        {
            var rot = Quaternion.Euler(GroundManager.Instance.GetXAngle(transform.position.z) - 90, 0, 0);
            transform.SetPositionAndRotation(pos, rot);
        }
        else
        {
            transform.position = pos;
        }

        ApplyProgress();
    }

    #endregion

    private void ApplyProgress()
    {
        if (InGameManager.Instance != null)
        {
            var progress = InGameManager.Instance.Progress.Value;
            for (var i = 0; i < m_Renderers.Length; i++)
            {
                foreach (var s in m_GradientSet.Set)
                {
                    m_MaterialPropBlocks[i].SetColor(ShaderPropertyID.Instance.GetID(s.Name), s.GetColor(progress));
                }
                m_Renderers[i].SetPropertyBlock(m_MaterialPropBlocks[i]);
            }
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
}
