using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    /// <summary>
    /// 音效条目
    /// </summary>
    [System.Serializable]
    public class SoundEntry
    {
        public string name;
        public AudioClip clip;
    }

    /// <summary>
    /// 音效列表
    /// </summary>
    public SoundEntry[] sounds;

    /// <summary>
    /// 获取音效
    /// </summary>
    /// <param name="clipName"></param>
    /// <returns></returns>
    public AudioClip GetClip(string clipName)
    {
        foreach (var entry in sounds)
        {
            if (entry.name == clipName)
                return entry.clip;
        }
        return null;
    }
}