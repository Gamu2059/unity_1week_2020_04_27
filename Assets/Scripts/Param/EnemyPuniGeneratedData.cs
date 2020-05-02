using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class EnemyPuniGeneratedData
{
    [SerializeField]
    private bool m_IsLookRibbon;
    public bool IsLookRibbon => m_IsLookRibbon;

    [SerializeField]
    private Color m_Body1Color;
    public Color Body1Color => m_Body1Color;

    [SerializeField]
    private Color m_Body2Color;
    public Color Body2Color => m_Body2Color;

    [SerializeField]
    private Color m_ShadowColor;
    public Color ShadowColor => m_ShadowColor;
}
