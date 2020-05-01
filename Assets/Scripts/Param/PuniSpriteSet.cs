using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[CreateAssetMenu(menuName = "Param/PuniSpriteSet", fileName = "param.puni_sprite_set.asset")]
public class PuniSpriteSet : ScriptableObject
{
    #region Define

    [Serializable]
    private class EmoteSpriteSet
    {
        [SerializeField]
        private E_PUNI_EMOTE m_EmoteType;
        public E_PUNI_EMOTE EmoteType => m_EmoteType;

        [SerializeField]
        private Sprite m_Eye;
        public Sprite Eye => m_Eye;

        [SerializeField]
        private Sprite m_EyeRight;
        public Sprite EyeRight => m_EyeRight;
    }

    [Serializable]
    private class SpriteSet
    {
        [SerializeField]
        private Sprite m_Body1;
        public Sprite Body1 => m_Body1;

        [SerializeField]
        private Sprite m_Body2;
        public Sprite Body2 => m_Body2;

        [SerializeField]
        private Sprite m_Cheek;
        public Sprite Cheek => m_Cheek;

        [SerializeField]
        private Sprite m_CheekRight;
        public Sprite CheekRight => m_CheekRight;

        [SerializeField]
        private Sprite m_HighLight;
        public Sprite HighLight => m_HighLight;

        [SerializeField]
        private Sprite m_HighLightRight;
        public Sprite HighLightRight => m_HighLightRight;

        [SerializeField]
        private Sprite m_Ribbon;
        public Sprite Ribbon => m_Ribbon;

        [SerializeField]
        private Sprite m_RibbonRight;
        public Sprite RibbonRight => m_RibbonRight;

        [SerializeField]
        private List<EmoteSpriteSet> m_EmoteSets;
        public List<EmoteSpriteSet> EmoteSets => m_EmoteSets;
    }

    #endregion

    #region Field Inspector

    [SerializeField]
    private SpriteSet[] m_SpriteSets;

    #endregion

    public Sprite GetSprite(int index, E_PUNI_PARTS parts, E_PUNI_EMOTE emote, bool isLookForward)
    {
        if (index < 0 || index >= m_SpriteSets.Length)
        {
            Debug.LogWarningFormat("indexが範囲外です。 index : {0}, max size : {1}", index, m_SpriteSets.Length);
            return null;
        }

        var spriteSets = m_SpriteSets[index];
        var emoteSets = spriteSets.EmoteSets.Find(e => e.EmoteType == emote);
        switch (parts)
        {
            case E_PUNI_PARTS.BODY_1:
                return spriteSets.Body1;
            case E_PUNI_PARTS.BODY_2:
                return spriteSets.Body2;
            case E_PUNI_PARTS.CHEEK:
                return isLookForward ? spriteSets.Cheek : spriteSets.CheekRight;
            case E_PUNI_PARTS.EYE:
                return isLookForward ? emoteSets.Eye : emoteSets.EyeRight;
            case E_PUNI_PARTS.HIGH_LIGHT:
                return isLookForward ? spriteSets.HighLight : spriteSets.HighLightRight;
            case E_PUNI_PARTS.RIBBON:
                return isLookForward ? spriteSets.Ribbon : spriteSets.RibbonRight;
        }

        return null;
    }
}
