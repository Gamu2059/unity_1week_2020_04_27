#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 雲を制御するクラス
/// </summary>
public class CloudController : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer m_SpriteRenderer;

    [HideInInspector]
    public float MoveSpeed;

    private void Update()
    {
        transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
        if (transform.position.z <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetFlipX()
    {
        if (m_SpriteRenderer != null)
        {
            m_SpriteRenderer.flipX = transform.position.x < 0;
        }
    }
}
