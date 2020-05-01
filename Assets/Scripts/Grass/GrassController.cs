using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 草制御クラス
/// </summary>
public class GrassController : MonoBehaviour
{
    [SerializeField]
    private float m_MoveSpeed;

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
    }
}
