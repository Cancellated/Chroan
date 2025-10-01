using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyGame.UI
{
    /// <summary>
    /// UI面板类型枚举，用于UIManager进行状态管理和面板分类
    /// </summary>
    public enum UIType
    {
        None,
        MainMenu,
        SaveLoadMenu,
        PauseMenu,
        ResultPanel,
        HUD,
        Loading,
        Console,
        Inventory,
        SettingsPanel,
        AboutPanel,
    }
}