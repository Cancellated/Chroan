using MyGame.Events;
using MyGame.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Logger;

namespace MyGame.Data
{
    /// <summary>
    /// 游戏存档管理器，负责处理游戏数据的保存、加载和删除操作。
    /// 作为单例类提供全局访问点，并与游戏事件系统集成。
    /// </summary>
    public class SaveManager : Singleton<SaveManager>
    {
        private const string LOG_MODULE = "SaveManager";
        private const string DEFAULT_SAVE_SLOT = "AutoSave";
        
        private ISaveSystem m_saveSystem;
        private SaveData m_currentSaveData;
        
        #region 属性
        
        /// <summary>
        /// 当前加载的存档数据。
        /// </summary>
        public SaveData CurrentSaveData
        {
            get { return m_currentSaveData; }
        }
        
        /// <summary>
        /// 获取所有可用的存档槽名称。
        /// </summary>
        public List<string> AvailableSaves
        {
            get { return m_saveSystem?.GetAvailableSaves() ?? new List<string>(); }
        }
        
        #endregion
        
        #region 生命周期
        
        /// <summary>
        /// 初始化存档管理器。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            // 初始化存档系统实现（使用JSON文件存储）
            m_saveSystem = new JsonSaveSystem();
            
            // 初始化当前存档数据为默认值
            m_currentSaveData = new SaveData();
            
            Log.Info(LOG_MODULE, "存档管理器已初始化");
        }
        
        private void OnEnable()
        {
            // 注册游戏事件监听器
            GameEvents.OnGameOver += HandleGameOver;
            GameEvents.OnCreateNewGame += HandleCreateNewGame;
            GameEvents.OnSaveGame += HandleSaveGame;
            GameEvents.OnLoadGame += HandleLoadGame;
            GameEvents.OnDeleteSave += HandleDeleteSave;
            GameEvents.OnAutoSave += HandleAutoSave;
        }
        
        private void OnDisable()
        {
            // 注销游戏事件监听器
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnCreateNewGame -= HandleCreateNewGame;
            GameEvents.OnSaveGame -= HandleSaveGame;
            GameEvents.OnLoadGame -= HandleLoadGame;
            GameEvents.OnDeleteSave -= HandleDeleteSave;
            GameEvents.OnAutoSave -= HandleAutoSave;
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 加载存档数据但不应用游戏设置，仅用于预览存档信息。
        /// 包含延迟初始化逻辑，确保在第一次访问时初始化存档系统。
        /// </summary>
        /// <param name="slotName">存档槽名称，如果为空则使用默认存档槽。</param>
        /// <returns>加载的存档数据，如果失败则返回null。</returns>
        public SaveData LoadSaveData(string slotName = null)
        {
            // 延迟初始化存档系统
            if (m_saveSystem == null)
            {
                InitializeSaveSystem();
            }
            
            // 使用默认存档槽如果未指定
            string saveSlot = string.IsNullOrEmpty(slotName) ? DEFAULT_SAVE_SLOT : slotName;
            
            Log.Info(LOG_MODULE, $"开始加载存档数据: {saveSlot}");
            
            // 直接从存档系统加载数据但不更新当前游戏数据
            SaveData loadedData = m_saveSystem.LoadGame(saveSlot);
            
            if (loadedData != null)
            {
                Log.Info(LOG_MODULE, "存档数据加载成功");
            }
            else
            {
                Log.Warning(LOG_MODULE, "存档数据加载失败或存档不存在");
            }
            
            return loadedData;
        }
        
        /// <summary>
        /// 保存当前游戏数据到指定存档槽。
        /// 包含延迟初始化逻辑，确保在第一次访问时初始化存档系统。
        /// </summary>
        /// <param name="slotName">存档槽名称，如果为空则使用默认存档槽。</param>
        /// <returns>保存操作是否成功。</returns>
        public bool SaveCurrentGame(string slotName = null)
        {
            // 延迟初始化存档系统
            if (m_saveSystem == null)
            {
                InitializeSaveSystem();
            }
            
            // 使用默认存档槽如果未指定
            string saveSlot = string.IsNullOrEmpty(slotName) ? DEFAULT_SAVE_SLOT : slotName;
            
            Log.Info(LOG_MODULE, $"开始保存游戏到存档槽: {saveSlot}");
            
            // 保存数据
            bool success = m_saveSystem.SaveGame(m_currentSaveData, saveSlot);
            
            if (success)
            {
                Log.Info(LOG_MODULE, "游戏保存成功");
            }
            else
            {
                Log.Error(LOG_MODULE, "游戏保存失败");
            }
            
            return success;
        }
        
        
        /// <summary>
        /// 从指定存档槽加载游戏数据。
        /// 包含延迟初始化逻辑，确保在第一次访问时初始化存档系统。
        /// </summary>
        /// <param name="slotName">存档槽名称，如果为空则使用默认存档槽。</param>
        /// <returns>加载操作是否成功。</returns>
        public bool LoadGame(string slotName = null)
        {
            // 延迟初始化存档系统
            if (m_saveSystem == null)
            {
                InitializeSaveSystem();
            }
            
            // 使用默认存档槽如果未指定
            string saveSlot = string.IsNullOrEmpty(slotName) ? DEFAULT_SAVE_SLOT : slotName;
            
            Log.Info(LOG_MODULE, $"开始加载游戏存档: {saveSlot}");
            
            // 加载数据
            SaveData loadedData = m_saveSystem.LoadGame(saveSlot);
            
            // 检查加载结果
            if (loadedData != null)
            {
                // 更新当前存档数据
                m_currentSaveData = loadedData;
                
                // 应用加载的设置
                ApplyLoadedSettings();
                
                Log.Info(LOG_MODULE, "游戏加载成功");
                return true;
            }
            else
            {
                Log.Error(LOG_MODULE, "游戏加载失败或存档不存在");
                return false;
            }
        }
        
        
        /// <summary>
        /// 删除指定存档槽的游戏数据。
        /// 包含延迟初始化逻辑，确保在第一次访问时初始化存档系统。
        /// </summary>
        /// <param name="slotName">存档槽名称，如果为空则使用默认存档槽。</param>
        /// <returns>删除操作是否成功。</returns>
        public bool DeleteSave(string slotName = null)
        {
            // 延迟初始化存档系统
            if (m_saveSystem == null)
            {
                InitializeSaveSystem();
            }
            
            // 使用默认存档槽如果未指定
            string saveSlot = string.IsNullOrEmpty(slotName) ? DEFAULT_SAVE_SLOT : slotName;
            
            Log.Info(LOG_MODULE, $"开始删除游戏存档: {saveSlot}");
            
            // 删除存档
            bool success = m_saveSystem.DeleteGame(saveSlot);
            
            // 触发删除事件
            if (success)
            {
                Log.Info(LOG_MODULE, "存档删除成功");
            }
            else
            {
                Log.Error(LOG_MODULE, "存档删除失败");
            }
            
            return success;
        }
        
        /// <summary>
        /// 检查指定存档槽是否存在游戏数据。
        /// 包含延迟初始化逻辑，确保在第一次访问时初始化存档系统。
        /// </summary>
        /// <param name="slotName">存档槽名称，如果为空则使用默认存档槽。</param>
        /// <returns>存档是否存在。</returns>
        public bool DoesSaveExist(string slotName = null)
        {
            // 延迟初始化存档系统（防止Awake顺序问题导致的未初始化错误）
            if (m_saveSystem == null)
            {
                InitializeSaveSystem();
            }
            
            // 使用默认存档槽如果未指定
            string saveSlot = string.IsNullOrEmpty(slotName) ? DEFAULT_SAVE_SLOT : slotName;
            
            return m_saveSystem.DoesSaveExist(saveSlot);
        }
        
        /// <summary>
        /// 初始化存档系统实现
        /// 确保与Awake方法中的初始化逻辑保持一致
        /// </summary>
        private void InitializeSaveSystem()
        {
            if (m_saveSystem == null)
            {
                m_saveSystem = new JsonSaveSystem();
                
                // 确保当前存档数据已初始化
                m_currentSaveData ??= new SaveData();
                
                Log.Info(LOG_MODULE, "存档系统初始化完成");
            }
        }
        
        /// <summary>
        /// 创建一个新的游戏存档。
        /// </summary>
        public void NewGame()
        {
            // 初始化新的存档数据
            m_currentSaveData = new SaveData();
            
            Log.Info(LOG_MODULE, "创建了新游戏存档");
        }
        
        #endregion
        
        #region 私有方法
        
        /// <summary>
        /// 应用加载的游戏设置。
        /// </summary>
        private void ApplyLoadedSettings()
        {
            if (m_currentSaveData == null)
            {
                return;
            }
            
            // 从存档创建游戏设置对象并应用到游戏中
            GameSettings settings = m_currentSaveData.CreateGameSettings();
            settings.ApplyToGame();
            Log.Info(LOG_MODULE, "已应用加载的游戏设置");
        }
        
        /// <summary>
        /// 处理游戏结束事件。
        /// </summary>   
        private void HandleGameOver(bool isWin)
        {
            // 游戏结束时自动保存
            Log.Info(LOG_MODULE, "游戏结束，触发自动保存");
            SaveCurrentGame();
        }
        
        #endregion
        
        #region 存档事件触发方法
        
        /// <summary>
        /// 处理新游戏创建事件。
        /// </summary>
        private void HandleCreateNewGame()
        {
            // 新游戏创建时自动保存
            Log.Info(LOG_MODULE, "新游戏创建，触发自动保存");
            NewGame();
            SaveCurrentGame();
        }

        private void HandleSaveGame(string slotName)
        {
            SaveCurrentGame(slotName);
            Log.Info(LOG_MODULE, $"游戏存档已保存到槽位: {slotName}");
        }

        private void HandleLoadGame(string slotName)
        {
            if(!DoesSaveExist(slotName))
            {
                Log.Error(LOG_MODULE, $"要加载的存档文件不存在: {slotName}");
                return;
            }
            LoadGame(slotName);
            Log.Info(LOG_MODULE, $"游戏从槽位 {slotName} 加载完成");
        }
        
        private void HandleDeleteSave(string slotName)
        {
            DeleteSave(slotName);
            Log.Info(LOG_MODULE, $"游戏存档已删除槽位: {slotName}");
        }
        
        private void HandleAutoSave(string slotName = DEFAULT_SAVE_SLOT)
        {
            SaveCurrentGame(slotName);
            Log.Info(LOG_MODULE, $"游戏自动保存到槽位: {slotName}");
        }
        
        #endregion
    }
}