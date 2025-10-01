using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Data
{
    /// <summary>
    /// 游戏进度数据结构，存储玩家的游戏进度信息
    /// </summary>
    [Serializable]
    public class GameProgress
    {
        public Vector3 playerPosition;
        public int currentLevel;
        public List<QuestStatus> activeQuests;
        public List<LevelStatus> completedLevels;
        public Dictionary<string, int> playerStats; // 存储玩家统计数据，如生命、体力等

        /// <summary>
        /// 构造函数
        /// </summary>
        public GameProgress()
        {
            playerPosition = Vector3.zero;
            currentLevel = 1;
            activeQuests = new List<QuestStatus>();
            completedLevels = new List<LevelStatus>();
            playerStats = new Dictionary<string, int>();
        }

        /// <summary>
        /// 更新玩家位置
        /// </summary>
        /// <param name="position">新的玩家位置</param>
        public void UpdatePlayerPosition(Vector3 position)
        {
            playerPosition = position;
        }

        /// <summary>
        /// 更新当前关卡
        /// </summary>
        /// <param name="level">新的关卡编号</param>
        public void UpdateCurrentLevel(int level)
        {
            currentLevel = level;
        }

        /// <summary>
        /// 添加或更新任务进度
        /// </summary>
        /// <param name="questId">任务ID</param>
        /// <param name="step">任务进度步骤</param>
        public void UpdateQuestProgress(string questId, int step)
        {
            QuestStatus existingQuest = activeQuests.Find(q => q.questId == questId);
            if (existingQuest.questId != null)
            {
                // 更新现有任务
                activeQuests.Remove(existingQuest);
            }
            activeQuests.Add(new QuestStatus { questId = questId, progressStep = step });
        }

        /// <summary>
        /// 标记关卡为已完成
        /// </summary>
        /// <param name="levelId">关卡ID</param>
        public void MarkLevelAsCompleted(int levelId)
        {
            LevelStatus levelStatus = completedLevels.Find(l => l.levelId == levelId);
            if (levelStatus.levelId != 0)
            {
                // 更新现有关卡状态
                completedLevels.Remove(levelStatus);
            }
            completedLevels.Add(new LevelStatus { levelId = levelId, isCompleted = true });
        }

        /// <summary>
        /// 设置玩家统计数据
        /// </summary>
        /// <param name="statName">统计数据名称</param>
        /// <param name="value">统计数据值</param>
        public void SetPlayerStat(string statName, int value)
        {
            if (playerStats.ContainsKey(statName))
            {
                playerStats[statName] = value;
            }
            else
            {
                playerStats.Add(statName, value);
            }
        }

        /// <summary>
        /// 获取玩家统计数据
        /// </summary>
        /// <param name="statName">统计数据名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>统计数据值</returns>
        public int GetPlayerStat(string statName, int defaultValue = 0)
        {
            if (playerStats.TryGetValue(statName, out int value))
            {
                return value;
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// 任务状态结构，存储任务的ID和进度步骤
    /// </summary>
    [Serializable]
    public struct QuestStatus
    {
        public string questId;
        public int progressStep;
    }

    /// <summary>
    /// 关卡状态结构，存储关卡的ID和完成状态
    /// </summary>
    [Serializable]
    public struct LevelStatus
    {
        public int levelId;
        public bool isCompleted;
    }
}