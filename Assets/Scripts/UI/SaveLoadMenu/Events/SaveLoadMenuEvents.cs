using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Data;

namespace MyGame.UI.SaveLoad.Events
{
    /// <summary>
    /// 存档菜单相关事件定义
    /// 用于在UI组件和游戏逻辑之间传递消息
    /// </summary>
    public static class SaveLoadMenuEvents
    {
        /// <summary>
        /// 存档操作事件
        /// </summary>
        public delegate void SaveGameDelegate(string slotName);
        public static event SaveGameDelegate OnSaveGame;
        
        /// <summary>
        /// 加载游戏事件
        /// </summary>
        public delegate void LoadGameDelegate(string slotName);
        public static event LoadGameDelegate OnLoadGame;
        
        /// <summary>
        /// 删除存档事件
        /// </summary>
        public delegate void DeleteSaveDelegate(string slotName);
        public static event DeleteSaveDelegate OnDeleteSave;
        
        /// <summary>
        /// 创建新游戏事件
        /// </summary>
        public delegate void CreateNewGameDelegate();
        public static event CreateNewGameDelegate OnCreateNewGame;
        
        /// <summary>
        /// 返回主菜单事件
        /// </summary>
        public delegate void BackToMainMenuDelegate();
        public static event BackToMainMenuDelegate OnBackToMainMenu;
        
        /// <summary>
        /// 存档槽选中事件
        /// </summary>
        public delegate void SaveSlotSelectedDelegate(string slotName, SaveData saveData = null);
        public static event SaveSlotSelectedDelegate OnSaveSlotSelected;
        
        /// <summary>
        /// 触发存档操作
        /// </summary>
        /// <param name="slotName">存档槽名称</param>
        public static void TriggerSaveGame(string slotName)
        {
            OnSaveGame?.Invoke(slotName);
        }
        
        /// <summary>
        /// 触发加载游戏操作
        /// </summary>
        /// <param name="slotName">存档槽名称</param>
        public static void TriggerLoadGame(string slotName)
        {
            OnLoadGame?.Invoke(slotName);
        }
        
        /// <summary>
        /// 触发删除存档操作
        /// </summary>
        /// <param name="slotName">存档槽名称</param>
        public static void TriggerDeleteSave(string slotName)
        {
            OnDeleteSave?.Invoke(slotName);
        }
        
        /// <summary>
        /// 触发创建新游戏操作
        /// </summary>
        public static void TriggerCreateNewGame()
        {
            OnCreateNewGame?.Invoke();
        }
        
        /// <summary>
        /// 触发返回主菜单操作
        /// </summary>
        public static void TriggerBackToMainMenu()
        {
            OnBackToMainMenu?.Invoke();
        }
        
        /// <summary>
        /// 触发存档槽选中操作
        /// </summary>
        /// <param name="slotName">存档槽名称</param>
        /// <param name="saveData">存档数据</param>
        public static void TriggerSaveSlotSelected(string slotName, SaveData saveData = null)
        {
            OnSaveSlotSelected?.Invoke(slotName, saveData);
        }
        
        /// <summary>
        /// 清理所有事件订阅
        /// </summary>
        public static void ClearAllEvents()
        {
            OnSaveGame = null;
            OnLoadGame = null;
            OnDeleteSave = null;
            OnCreateNewGame = null;
            OnBackToMainMenu = null;
            OnSaveSlotSelected = null;
        }
    }
}