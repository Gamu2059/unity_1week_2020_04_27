using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 影の制御クラス
/// </summary>
public class ShadowController : MonoBehaviour
{
    private void LateUpdate()
    {
        AdjustShadow();
    }

    private void AdjustShadow()
    {
        if (GroundManager.Instance != null)
        {
            transform.rotation = Quaternion.Euler(GroundManager.Instance.GetXAngle(transform.position.z), 0, 0);
        }
    }
}
