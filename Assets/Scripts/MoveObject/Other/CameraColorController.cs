using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraColorController : MonoBehaviour
{
    [SerializeField]
    private Camera m_Camera;

    [SerializeField]
    private GradientSet m_GradientSet;

    private void Start()
    {
        ApplyProgress();
    }

    private void Update()
    {
        ApplyProgress();
    }

    private void ApplyProgress()
    {
        if (InGameManager.Instance != null)
        {
            var progress = InGameManager.Instance.Progress.Value;
            var set = m_GradientSet.Set[0];
            m_Camera.backgroundColor = set.GetColor(progress);
        }
    }
}
