#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 青プニを通じて赤プニを制御するための上位制御クラス
/// </summary>
public class PlayerOverallController : MonoBehaviour
{
    [SerializeField]
    private BluePuniController m_BluePuni;

    [SerializeField]
    private RedPuniController m_RedPuni;

    private void Start()
    {
        m_BluePuni.OnInitialize();
        m_RedPuni.OnInitialize();
        m_BluePuni.OnStart();
        m_RedPuni.OnStart();
    }

    private void OnDestroy()
    {
        m_RedPuni.OnFinalize();
        m_BluePuni.OnFinalize();
    }

    private void Update()
    {
        m_BluePuni.OnUpdate();
        m_RedPuni.OnUpdate();
    }

    private void LateUpdate()
    {
        m_BluePuni.OnLateUpdate();
        m_RedPuni.OnLateUpdate();
    }
}
