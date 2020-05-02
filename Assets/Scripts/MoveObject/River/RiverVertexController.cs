using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverVertexController : MonoBehaviour
{
    [SerializeField]
    private MeshFilter m_MeshFilter;

    [SerializeField]
    private float m_Speed;

    private Mesh m_CopiedMesh;
    private Vector3 m_PrePosition;

    private void Start()
    {
        m_CopiedMesh = m_MeshFilter.mesh;
        m_PrePosition = transform.position;
        ApplyGround();
    }

    private void OnDestroy()
    {
        Destroy(m_CopiedMesh);
        m_CopiedMesh = null;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * m_Speed * Time.deltaTime, Space.World);
    }

    private void LateUpdate()
    {
        var pos = transform.position;
        if (m_PrePosition.x == pos.x && m_PrePosition.z == pos.z)
        {
            return;
        }

        ApplyGround();
        m_PrePosition = transform.position;
    }

    private void ApplyGround()
    {
        if (GroundManager.Instance == null)
        {
            return;
        }

        List<Vector3> vertices = new List<Vector3>();
        m_CopiedMesh.GetVertices(vertices);
        var pos = transform.position;
        var scale = transform.lossyScale;
        for (var i = 0; i < m_CopiedMesh.vertexCount; i++)
        {
            // vはmeshの存在する位置からの相対座標になっている
            var v = vertices[i];
            v.y = GroundManager.Instance.GetYPosition(v.x * scale.x + pos.x, v.z * scale.z + pos.z);
            vertices[i] = v;
        }

        m_CopiedMesh.SetVertices(vertices);
        m_CopiedMesh.RecalculateBounds();
    }
}
