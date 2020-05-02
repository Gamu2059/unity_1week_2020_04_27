using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゴール地点の制御クラス
/// </summary>
public class GoalSpotController : MonoBehaviour
{
    [SerializeField, Tooltip("進行度に応じてゴール地点のY座標を制御する")]
    private AnimationCurve m_YPosCurve;

    [SerializeField]
    private Transform m_ReflectStarrySky;

    [SerializeField]
    private float m_ReflectStarrySkyBaseY = -300;

    [SerializeField, Tooltip("ゴール地点の移動と湖面の移動の関係 ゴール地点の移動量の参考値")]
    private float m_GoalSpotMoveAmountBase = -3.26f;

    [SerializeField, Tooltip("GoalSpotMoveAmountBase動いている時、この値が基準Y座標に加算される")]
    private float m_ReflectStarrySkyMoveAmountBase = 200;

    private void Update()
    {
        if (InGameManager.Instance != null)
        {
            var progress = InGameManager.Instance.Progress.Value;
            var yPos = m_YPosCurve.Evaluate(progress);
            var pos = transform.position;
            pos.y = yPos;
            transform.position = pos;

            var starrySkyPos = m_ReflectStarrySky.localPosition;
            var rate = m_ReflectStarrySkyMoveAmountBase / m_GoalSpotMoveAmountBase;
            starrySkyPos.y = m_ReflectStarrySkyBaseY + yPos * rate;
            m_ReflectStarrySky.localPosition = starrySkyPos;
        }
    }
}
