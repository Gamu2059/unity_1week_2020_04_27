using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioManager : AudioManagerBase<AudioManager>
{
#if UNITY_EDITOR
    [CustomEditor(typeof(AudioManager))]
    public class AudioEditor : AudioManagerEditor { }
#endif
}
