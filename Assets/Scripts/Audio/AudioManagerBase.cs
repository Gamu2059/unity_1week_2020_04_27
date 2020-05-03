#pragma warning disable 0649

using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;
using System.Linq;
using DG.Tweening;
using YS_Tool;

#if UNITY_EDITOR
using UnityEditor;
using System.Text;
using System.IO;
#endif

/// <summary>
/// オーディオの再生などを管理する。
/// 一点からの再生になるため、スマホアプリ用。
/// </summary>
public class AudioManagerBase<T> : SingletonMonoBehavior<T> where T : MonoBehaviour
{

    #region Field Inspector

    [SerializeField]
    private AudioMixerGroup m_BgmMixer = null;

    [SerializeField]
    private AudioMixerGroup m_SeMixer = null;

    [SerializeField]
    private int m_MaxBGMSource;

    [SerializeField]
    private List<AudioInfo> m_AudioInfos = null;

    #endregion



    #region Field Private

    private AudioSource[] m_BgmSources;

    /// <summary>
    /// SE は基本的に PlayOneShotで鳴らすので複数持つ必要がないため 1つ しかソースを持たない。
    /// </summary>
    private AudioSource m_SeSource;

    private float[] m_BgmTimeStamps;

    #endregion



    #region Property Public

    public AudioMixerGroup BgmMixer {
        get {
            return m_BgmMixer;
        }
    }

    public AudioMixerGroup SeMixer {
        get {
            return m_SeMixer;
        }
    }

    public List<AudioInfo> AudioInfos {
        get {
            return m_AudioInfos;
        }
    }

    #endregion



    #region Method Override

    protected override void OnAwake()
    {
        base.OnAwake();

        if (m_MaxBGMSource < 1)
        {
            m_MaxBGMSource = 1;
        }

        m_BgmSources = GenerateAudioSourceList(m_MaxBGMSource, m_BgmMixer, "BGM");

        AudioSource[] temp = GenerateAudioSourceList(1, m_SeMixer, "SE");

        if (temp != null)
        {
            m_SeSource = temp[0];
        }

        // BGMはループ対応
        for (int i = 0; i < m_BgmSources.Length; i++)
        {
            m_BgmSources[i].loop = true;
        }

        // プレイ開始時のタイムスタンプを初期化
        m_BgmTimeStamps = new float[m_MaxBGMSource];

        for (int i = 0; i < m_MaxBGMSource; i++)
        {
            m_BgmTimeStamps[i] = Time.unscaledTime;
        }
    }

    #endregion



    #region Method Public

    public AudioClip GetAudioClip(string audioClipName)
    {
        if (m_AudioInfos == null)
        {
#if DEBUG_ON
			Debug.LogError( "Audio Infos が null です。" );
#endif
            return null;
        }

        for (int i = 0; i < m_AudioInfos.Count; i++)
        {
            var info = m_AudioInfos[i];

            if (audioClipName == info.Name)
                return info.Clip;
        }

        return null;
    }

    /// <summary>
    /// 指定されたクリップをBGMとして再生する。
    /// 再生数に余裕がない場合、最も古くに再生されたものに置き換える。
    /// </summary>
    public void PlayBGM(AudioClip bgmClip, float duration = 0)
    {
        if (!bgmClip)
            return;

        // 空いているソースがあればそれで再生
        for (int i = 0; i < m_BgmSources.Length; i++)
        {
            if (!CheckExistBGMSource(i))
                continue;

            var src = m_BgmSources[i];

            if (src.isPlaying)
                continue;

            PlayBGMClip(i, bgmClip, duration);
            return;
        }

        // 空いていない場合はタイムスタンプが一番古いソースで再生
        float oldestTimeStamp = m_BgmTimeStamps[0];
        int idx = 0;

        for (int i = 1; i < m_BgmTimeStamps.Length; i++)
        {
            if (m_BgmTimeStamps[i] < oldestTimeStamp)
            {
                oldestTimeStamp = m_BgmTimeStamps[i];
                idx = i;
            }
        }

        PlayBGMClip(idx, bgmClip, duration);
    }

    /// <summary>
    /// 指定された名前のBGMを再生する。
    /// 再生数に余裕がない場合、最も古くに再生されたものに置き換える。
    /// </summary>
    public void PlayBGM(string bgmName, float duration = 0)
    {
        var clip = GetAudioClip(bgmName);
        PlayBGM(clip, duration);
    }

    public void StopBGM(AudioClip bgmClip, float duration = 0)
    {
        if (!bgmClip)
            return;

        for (int i = 0; i < m_BgmSources.Length; i++)
        {
            if (!CheckExistBGMSource(i))
                continue;

            var src = m_BgmSources[i];
            if (src.clip != bgmClip || !src.isPlaying)
                continue;

            src.DOFade(0, duration).OnComplete(() => src.Stop());
        }
    }

    public void StopBGM(string bgmName, float duration = 0)
    {
        var clip = GetAudioClip(bgmName);
        StopBGM(clip, duration);
    }

    public void StopAllBGM()
    {
        for (int i = 0; i < m_BgmSources.Length; i++)
        {
            m_BgmSources[i].Stop();
        }
    }

    public bool IsPlayingBGM(AudioClip bgmClip)
    {
        if (!bgmClip)
            return false;

        for (int i = 0; i < m_BgmSources.Length; i++)
        {
            if (!CheckExistBGMSource(i))
                continue;

            var src = m_BgmSources[i];

            if (!src.isPlaying || src.clip != bgmClip)
                continue;

            return true;
        }

        return false;
    }

    public bool IsPlayingBGM(string bgmName)
    {
        var clip = GetAudioClip(bgmName);
        return IsPlayingBGM(clip);
    }

    public void ChangeVolumeBGM(AudioClip bgmClip, float value, float duration = 0)
    {
        if (!bgmClip)
            return;

        for (int i = 0; i < m_BgmSources.Length; i++)
        {
            if (!CheckExistBGMSource(i))
                continue;

            var src = m_BgmSources[i];
            if (src.clip != bgmClip || !src.isPlaying)
                continue;

            src.DOFade(value, duration);
        }
    }

    public void ChangeVolumeBGM(string bgmName, float value, float duration = 0)
    {
        var clip = GetAudioClip(bgmName);
        ChangeVolumeBGM(clip, value, duration);
    }

    /// <summary>
    /// 指定されたクリップをSEとして再生する。
    /// </summary>
    public void PlaySE(AudioClip seClip)
    {
        PlaySEClip(seClip);
    }

    /// <summary>
    /// 指定された名前のSEを再生する。
    /// </summary>
    public void PlaySE(string seName)
    {
        var clip = GetAudioClip(seName);
        PlaySEClip(clip);
    }

    public void StopSE()
    {
        if (!CheckExistSESource())
            return;

        m_SeSource.Stop();
    }

    public bool IsPlayingSE()
    {
        if (!CheckExistSESource())
            return false;

        return m_SeSource.isPlaying;
    }

    #endregion



    #region Method Private

    /// <summary>
    /// マネージャの直下に複数のオーディオソースを生成する。
    /// 生成前にある同名の子オブジェクトは自動的に削除される。
    /// </summary>
    private AudioSource[] GenerateAudioSourceList(int maxNum, AudioMixerGroup mixer, string listName)
    {
        if (maxNum < 1)
            return null;

        var sources = new AudioSource[maxNum];

        // 子オブジェクトに同名のオーディオリストがあれば削除する
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);

            if (child.name == listName)
                Destroy(child);
        }

        // オーディオリストオブジェクトを生成する
        var listObj = new GameObject(listName);
        listObj.transform.SetParent(transform);

        for (int i = 0; i < maxNum; i++)
        {
            sources[i] = listObj.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].outputAudioMixerGroup = mixer;
        }

        return sources;
    }

    private void PlayBGMClip(int idx, AudioClip clip, float duration = 0)
    {
        if (!CheckExistBGMSource(idx))
            return;

        var src = m_BgmSources[idx];
        src.clip = clip;
        src.volume = 0;
        src.Play();
        src.DOFade(1, duration);
        m_BgmTimeStamps[idx] = Time.unscaledTime;
    }

    private void PlaySEClip(AudioClip clip)
    {
        if (!CheckExistSESource())
            return;

        m_SeSource.PlayOneShot(clip);
    }

    private bool CheckExistBGMSource(int idx)
    {
        if (m_BgmSources == null)
        {
#if DEBUG_ON
			Debug.LogErrorFormat( "BGM AudioSource Array is null!" );
#endif
            return false;
        }

        if (YS_Tool.ArrayUtility.IsOutOfArray(m_BgmSources, idx))
        {
#if DEBUG_ON
			Debug.LogErrorFormat( "BGM AudioSources Out of Index! Size : {0}, Idx : {1}", m_BgmSources.Length, idx );
#endif
            return false;
        }

        if (!m_BgmSources[idx])
        {
#if DEBUG_ON
			Debug.LogErrorFormat( "BGM AudioSource is null! Idx : {0}", idx );
#endif
            return false;
        }

        return true;
    }

    private bool CheckExistSESource()
    {
        if (m_SeSource == null)
        {
#if DEBUG_ON
			Debug.LogError( "SE AudioSource is null!" );
#endif
            return false;
        }

        return true;
    }

    #endregion



#if UNITY_EDITOR
    public class AudioManagerEditor : Editor
    {

        /// <summary>
        /// 保存場所のパス
        /// </summary>
        private static readonly string _path = "Assets/Constant/" + typeof(T).Name + "KeyWord.cs";

        /// <summary>
        /// 無効な文字の配列
        /// </summary>
        private static readonly string[] INVALUD_CHARS =
        {
            " ", "!", "\"", "#", "$",
            "%", "&", "\'", "(", ")",
            "-", "=", "^", "~", "\\",
            "|", "[", "{", "@", "`",
            "]", "}", ":", "*", ";",
            "+", "/", "?", ".", ">",
            ",", "<"
        };

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audio Keyword Option");
            EditorGUILayout.LabelField("Path", _path);

            if (GUILayout.Button("Generate Audio Keyword"))
            {
                if (!CanGenerate())
                {
                    Debug.LogError("It cannot generate audio keyword now!");
                    return;
                }

                var editor = target as AudioManagerBase<T>;

                if (GenerateScript(editor.AudioInfos))
                {
                    EditorUtility.DisplayDialog("AudioManager", "作成が完了しました", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("AudioManager", "作成が失敗しました", "OK");
                }
            }
        }

        /// <summary>
        /// プレハブ名を定数で管理するクラスを作成できるかどうかを取得します。
        /// </summary>
        private static bool CanGenerate()
        {
            return !EditorApplication.isPlaying && !Application.isPlaying && !EditorApplication.isCompiling;
        }

        private static bool GenerateScript(List<AudioInfo> infos)
        {
            try
            {
                var builder = new StringBuilder();
                builder.AppendLine("/// <summary>");
                builder.AppendLine("/// オーディオクリップ名を定数として保持するクラス");
                builder.AppendLine("/// </summary>");
                builder.AppendFormat("public static class {0}", typeof(T).Name + "KeyWord").AppendLine();
                builder.AppendLine("{");

                foreach (var info in infos.Select(c => new
                {
                    var = RemoveInvalidChars(c.Name)
                         ,
                    val = c.Name
                }))
                {
                    builder.Append("\t").AppendFormat(@"public const string {0} = ""{1}"";", info.var, info.val).AppendLine();
                }

                builder.AppendLine("}");

                var directoryName = Path.GetDirectoryName(_path);

                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.WriteAllText(_path, builder.ToString(), Encoding.UTF8);
                AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }


        /// <summary>
        /// 無効な文字を削除します。
        /// </summary>
        private static string RemoveInvalidChars(string str)
        {
            Array.ForEach(INVALUD_CHARS, c => str = str.Replace(c, string.Empty));
            return str;
        }
    }
#endif

}

#region Definition

[Serializable]
public class AudioInfo
{
    [SerializeField]
    private string m_Name;

    [SerializeField]
    private AudioClip m_Clip;

    public string Name {
        get {
            return m_Name;
        }
    }

    public AudioClip Clip {
        get {
            return m_Clip;
        }
    }
}

#endregion

