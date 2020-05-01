using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// プニの見た目を制御するクラス
/// </summary>
public class PuniViewController : MonoBehaviour
{
    #region Define

    [Serializable]
    private class SpriteRendererSet
    {
        [SerializeField]
        private E_PUNI_PARTS m_Parts;
        public E_PUNI_PARTS Parts => m_Parts;

        [SerializeField]
        private SpriteRenderer m_Renderer;
        public SpriteRenderer Renderer => m_Renderer;

        [SerializeField]
        private float m_LookForwardOffsetZ;
        public float LookForwardOffsetZ => m_LookForwardOffsetZ;

        [SerializeField]
        private float m_LookBackOffsetZ;
        public float LookBackOffsetZ => m_LookBackOffsetZ;
    }

    #endregion

    #region Field Inspector

    [SerializeField]
    private PuniSpriteSet m_SpriteSet;

    [SerializeField]
    private List<SpriteRendererSet> m_Renderers;

    [SerializeField]
    private SpriteRenderer m_ShadowRenderer;

    [Space()]
    [Header("Init Option")]

    [SerializeField]
    private bool m_IsLookHighRight = true;

    [SerializeField]
    private bool m_IsLookRibbon = true;

    [SerializeField]
    private Color m_InitBody1Color;

    [SerializeField]
    private Color m_InitBody2Color;

    [SerializeField]
    private Color m_InitShadowColor;

    #endregion

    #region Field

    private E_PUNI_EMOTE m_Emote;
    private E_PUNI_LOOK_DIR m_LookDir;
    private int m_SpriteIndex;

    #endregion

    #region Game Cycle

    private void Start()
    {
        SetEnableRenderer(E_PUNI_PARTS.HIGH_LIGHT, m_IsLookHighRight);
        SetEnableRenderer(E_PUNI_PARTS.RIBBON, m_IsLookRibbon);

        var body1 = GetRenderer(E_PUNI_PARTS.BODY_1);
        var body2 = GetRenderer(E_PUNI_PARTS.BODY_2);
        body1.color = m_InitBody1Color;
        body2.color = m_InitBody2Color;
        m_ShadowRenderer.color = m_InitShadowColor;
    }

    #endregion

    private SpriteRenderer GetRenderer(E_PUNI_PARTS parts)
    {
        var renderer = m_Renderers.Find(r => r.Parts == parts);
        return renderer.Renderer;
    }

    public void SetEnableRenderer(E_PUNI_PARTS target, bool isEnable)
    {
        var renderer = GetRenderer(target);
        if (renderer != null)
        {
            renderer.enabled = isEnable;
        }
    }

    public void SetView(int spriteIndex)
    {
        m_SpriteIndex = spriteIndex;
        UpdateView();
    }

    public void SetEmote(E_PUNI_EMOTE emote, bool update = false)
    {
        m_Emote = emote;
        if (update)
        {
            UpdateView();
        }
    }

    public void SetLook(E_PUNI_LOOK_DIR lookDir, bool update = false)
    {
        m_LookDir = lookDir;
        if (update)
        {
            UpdateView();
        }
    }

    public void UpdateView()
    {
        if (m_SpriteSet == null)
        {
            return;
        }

        var isLookForwardOrBack = m_LookDir == E_PUNI_LOOK_DIR.FORWARD || m_LookDir == E_PUNI_LOOK_DIR.BACK;
        var isLookLeft = !isLookForwardOrBack && m_LookDir == E_PUNI_LOOK_DIR.LEFT;
        var isLookForward = m_LookDir == E_PUNI_LOOK_DIR.FORWARD;
        foreach (var r in m_Renderers)
        {
            var sprite = m_SpriteSet.GetSprite(m_SpriteIndex, r.Parts, m_Emote, isLookForwardOrBack);
            r.Renderer.sprite = sprite;
            r.Renderer.flipX = isLookLeft;

            var pos = r.Renderer.transform.localPosition;
            pos.z = isLookForward ? r.LookForwardOffsetZ : r.LookBackOffsetZ;
            r.Renderer.transform.localPosition = pos;
        }
    }
}
