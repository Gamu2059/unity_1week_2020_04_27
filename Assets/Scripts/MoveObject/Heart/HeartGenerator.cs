#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ハート生成クラス
/// </summary>
public class HeartGenerator : MonoBehaviour
{
    [SerializeField]
    private HeartController m_HeartPrefab;

    [SerializeField]
    private Vector2 m_XRange;

    [SerializeField]
    private float m_InitZPos;

    [SerializeField]
    private Vector2 m_HeartMoveSpeedRange;

    [SerializeField]
    private Vector2 m_NextGenerateTimeRange;

    private float m_NextGenerateTime;
    private float m_NextGenerateTimeCount;

    private List<HeartController> m_HeartPool;

    private void Awake()
    {
        m_HeartPool = new List<HeartController>();

        m_NextGenerateTime = Random.Range(m_NextGenerateTimeRange.x, m_NextGenerateTimeRange.y);
        m_NextGenerateTimeCount = 0;
    }

    private void Update()
    {
        if (m_NextGenerateTimeCount >= m_NextGenerateTime)
        {
            Generate();
            m_NextGenerateTimeCount -= m_NextGenerateTime;
            m_NextGenerateTime = Random.Range(m_NextGenerateTimeRange.x, m_NextGenerateTimeRange.y);
        }

        m_NextGenerateTimeCount += Time.deltaTime;
    }

    private HeartController GetHeartFromPool()
    {
        HeartController heart = null;
        if (m_HeartPool == null)
        {
            m_HeartPool = new List<HeartController>();
        }
        else
        {
            foreach (var h in m_HeartPool)
            {
                if (!h.gameObject.activeSelf)
                {
                    heart = h;
                    break;
                }
            }
        }

        if (heart == null)
        {
            heart = Instantiate(m_HeartPrefab);
            m_HeartPool.Add(heart);
        }
        else
        {
            heart.gameObject.SetActive(true);
        }

        return heart;
    }

    private void Generate()
    {
        var heart = GetHeartFromPool();
        if (heart == null)
        {
            return;
        }

        var heartT = heart.transform;
        heartT.SetParent(transform);

        // 絶対見えない場所に置く
        heartT.position = new Vector3(Random.Range(m_XRange.x, m_XRange.y), -1000, m_InitZPos);
        heart.MoveSpeed = Random.Range(m_HeartMoveSpeedRange.x, m_HeartMoveSpeedRange.y);
    }
}
