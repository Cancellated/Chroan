using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Chapter Data")]
public class ChapterData : ScriptableObject
{
    public string chapterName;    // 章节名称
    public Sprite previewImage;   // 预览图片
    public LevelData[] levels;    // 关卡数组
    public bool isUnlocked;       // 是否解锁

    [Header("解锁条件")]
    public int chapterIndex;      // 章节索引（用于存档）
    public int requiredCompletedLevels; // 完成所需关卡数量
}
