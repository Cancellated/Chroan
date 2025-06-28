using System.Collections;
using System.Collections.Generic;
using MyGame.Managers;
using UnityEngine;


namespace MyGame.Data
{
    public class SaveData
    {
        #region 系统设置
        public float masterVolume;
        public float bgmVolume;
        public float sfxVolume;
        #endregion

        #region 进度映射
        public Dictionary<int, ChapterProgress> chapterProgressDict;
        public int currentChapterIndex;
        public int currentLevelIndex;
        #endregion

        public static SaveData FromProgress(GameProgress progress)
        {
            return new SaveData {
                chapterProgressDict = progress.chapterProgressDict,
                currentChapterIndex = progress.currentChapterIndex,
                currentLevelIndex = progress.currentLevelIndex,
            };
        }
    }

    // 章节进度
    public class ChapterProgress
    {
        public int completedLevels;      // 已完成的关卡数
        public bool isUnlocked;           // 是否已解锁
        public bool[] levelUnlockStates; // 每个关卡的解锁状态
    }
}