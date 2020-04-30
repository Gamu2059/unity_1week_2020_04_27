#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 雲を生成するクラス
/// </summary>
public class CloudGenerator : MonoBehaviour
{
    [SerializeField]
    private CloudController[] m_CloudPrefabs;

    [SerializeField]
    private Vector2 m_XRange;

    [SerializeField]
    private Vector2 m_YRange;

    [SerializeField]
    private Vector2 m_CloudMoveSpeedRange;

    [SerializeField]
    private Vector2 m_NextGenerateTimeRange;

    [SerializeField]
    private int m_OnAwakeGenerateNum;

    private int m_PreGenerateIndex;
    private float m_NextGenerateTime;
    private float m_NextGenerateTimeCount;

    private Dictionary<int, List<CloudController>> m_CloudPool;

    private void Awake()
    {
        m_CloudPool = new Dictionary<int, List<CloudController>>();

        m_PreGenerateIndex = -1;
        for (var i = 0; i < m_OnAwakeGenerateNum; i++)
        {
            Generate(Random.Range(200, 900));
        }

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

    private CloudController GetCloudFromPool(int index)
    {
        if (index < 0 || index >= m_CloudPrefabs.Length)
        {
            return null;
        }

        CloudController cloud = null;
        List<CloudController> list = null;
        m_CloudPool.TryGetValue(index, out list);
        if (list == null)
        {
            list = new List<CloudController>();
            m_CloudPool.Add(index, list);
        }
        else
        {
            foreach (var c in list)
            {
                if (!c.gameObject.activeSelf)
                {
                    cloud = c;
                    break;
                }
            }
        }

        if (cloud == null)
        {
            cloud = Instantiate(m_CloudPrefabs[index]);
            list.Add(cloud);
        }
        else
        {
            cloud.gameObject.SetActive(true);
        }

        return cloud;
    }

    private void Generate(float z = 1000)
    {
        var index = Random.Range(0, m_CloudPrefabs.Length);
        for (var i = 0; i < m_CloudPrefabs.Length && index == m_PreGenerateIndex; i++)
        {
            index = Random.Range(0, m_CloudPrefabs.Length);
        }

        m_PreGenerateIndex = index;
        var cloud = GetCloudFromPool(index);
        if (cloud == null)
        {
            return;
        }

        var cloudT = cloud.transform;
        cloudT.SetParent(transform);
        cloudT.position = new Vector3(Random.Range(m_XRange.x, m_XRange.y), Random.Range(m_YRange.x, m_YRange.y), z);

        cloud.SetFlipX();
        cloud.MoveSpeed = Random.Range(m_CloudMoveSpeedRange.x, m_CloudMoveSpeedRange.y);
    }
}
