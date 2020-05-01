using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class InGameUiManager : MonoBehaviour
{
    #region Field Inspector

    [SerializeField]
    private Text m_Closeness;

    [SerializeField]
    private Scrollbar m_Progress;

    #endregion

    private void Start()
    {
        InGameManager.Instance.Closeness.Subscribe(x => m_Closeness.text = x.ToString());
        InGameManager.Instance.Progress.Subscribe(x => m_Progress.value = x);
    }
}
