using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Data
{
    /// <summary>
    /// 存档数据类
    /// 包含游戏进度和设置信息，用于统一管理和持久化
    /// </summary>
    [Serializable]
    public class SaveData
    {
        #region 进度数据

        /// <summary>
        /// 游戏进度信息
        /// </summary>
        public GameProgress gameProgress;

        #endregion

        #region 游戏设置

        public float musicVolume; // 音乐音量
        public float sfxVolume; // 音效音量
        public int qualityLevel; // 画质等级
        public bool fullscreen; // 是否全屏
        public int resolutionIndex; // 分辨率索引
        public bool invertYAxis; // 是否反转Y轴 

        #endregion

        #region 元数据

        /// <summary>
        /// 存档时间戳
        /// </summary>
        public string saveTime;

        /// <summary>
        /// 存档版本号
        /// </summary>
        public string version;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public SaveData()
        {
            gameProgress = new GameProgress();
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            version = "1.0.0"; // 使用默认版本号，避免在序列化期间调用Application.version
        }

        /// <summary>
        /// 构造函数，从游戏进度和设置创建存档数据
        /// </summary>
        /// <param name="progress">游戏进度</param>
        /// <param name="settings">游戏设置</param>
        public SaveData(GameProgress progress, GameSettings settings)
        {
            gameProgress = progress ?? new GameProgress();
            UpdateSettings(settings);
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            version = "1.0.0";
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 更新存档中的设置信息
        /// </summary>
        /// <param name="settings">游戏设置</param>
        public void UpdateSettings(GameSettings settings)
        {
            if (settings == null) return;

            musicVolume = settings.MusicVolume;
            sfxVolume = settings.SfxVolume;
            qualityLevel = settings.QualityLevel;
            fullscreen = settings.Fullscreen;
            resolutionIndex = settings.ResolutionIndex;
            invertYAxis = settings.InvertYAxis;
        }

        /// <summary>
        /// 创建游戏设置对象
        /// </summary>
        /// <returns>游戏设置对象</returns>
        public GameSettings CreateGameSettings()
        {
            return new GameSettings
            {
                MusicVolume = musicVolume,
                SfxVolume = sfxVolume,
                QualityLevel = qualityLevel,
                Fullscreen = fullscreen,
                ResolutionIndex = resolutionIndex,
                InvertYAxis = invertYAxis
            };
        }

        #endregion
    }
}