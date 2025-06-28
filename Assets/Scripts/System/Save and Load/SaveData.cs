using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.System
{
    public class SaveData : MonoBehaviour
    {
        #region 玩家数据
        public int currentChapterIndex;
        public int currentLevelIndex;
        #endregion

        #region 音频设置
        public float masterVolume;
        public float bgmVolume;
        public float sfxVolume;
        #endregion
    }
}