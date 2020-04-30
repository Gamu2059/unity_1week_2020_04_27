#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ハート制御クラス
/// </summary>
public class HeartController : MonoBehaviour
{
    [HideInInspector]
    public float MoveSpeed;

    private void Update()
    {
        var pos = transform.position;
        pos.z += MoveSpeed * Time.deltaTime;

        if (GroundManager.Instance != null)
        {
            pos.y = GroundManager.Instance.GetYPosition(pos.x, pos.z);
        }

        if (pos.z <= 0)
        {
            gameObject.SetActive(false);
        }

        transform.position = pos;
    }
}
