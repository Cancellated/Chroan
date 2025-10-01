using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.UI.SaveLoad
{
    /// <summary>
    /// 存档菜单相关常量定义
    /// </summary>
    public static class SaveLoadMenuConstants
    {
        /// <summary>
        /// 自动存档槽名称
        /// </summary>
        public const string AUTO_SAVE_SLOT = "auto_save";
        
        /// <summary>
        /// 默认存档槽数量
        /// </summary>
        public const int DEFAULT_SAVE_SLOT_COUNT = 3;
        
        /// <summary>
        /// 存档数据格式字符串
        /// </summary>
        public const string DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";
    }
}