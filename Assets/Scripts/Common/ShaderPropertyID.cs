using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シェーダーに用いるPropertyIDをキャッシュするためのクラス
/// </summary>
public class ShaderPropertyID : Singleton<ShaderPropertyID>
{
    private Dictionary<string, int> m_PropertyIDs;

    public ShaderPropertyID() : base()
    {
        m_PropertyIDs = new Dictionary<string, int>();
    }

    protected override void OnFinalizeInternal()
    {
        m_PropertyIDs?.Clear();
        m_PropertyIDs = null;
        base.OnFinalizeInternal();
    }

    public int GetID(string name)
    {
        int id;
        if (m_PropertyIDs.TryGetValue(name, out id))
        {
            return id;
        }

        id = Shader.PropertyToID(name);
        m_PropertyIDs.Add(name, id);
        return id;
    }
}
