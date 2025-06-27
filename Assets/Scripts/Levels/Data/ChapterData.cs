using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Chapter Data")]
public class ChapterData : ScriptableObject
{
    public string chapterName;
    public Sprite previewImage;
    public LevelData[] levels;
    public bool isUnlocked;

    [Header("解锁条件")]
    public int chapterIndex;
    public int requiredCompletedLevels;
}
