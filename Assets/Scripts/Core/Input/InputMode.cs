using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Input
{
    /// <summary>
    /// 输入模式枚举，定义不同的输入模式类型
    /// 用于统一管理游戏中的输入状态切换
    /// </summary>
    public enum InputMode
    {
        /// <summary>
        /// 游戏玩法模式 - 启用游戏控制输入，禁用UI输入
        /// </summary>
        GamePlay,
        
        /// <summary>
        /// UI交互模式 - 启用UI输入，禁用游戏控制输入
        /// </summary>
        UI,
        
        /// <summary>
        /// 混合模式 - 同时启用游戏和UI输入
        /// </summary>
        Both,
        
        /// <summary>
        /// 禁用所有输入 - 禁用所有输入模式
        /// </summary>
        None
    }
}