using UnityEngine;
using System.Collections.Generic;
using Logger;
using Items.LevelEntrance;

namespace MyGame.Managers
{
    /// <summary>
    /// 关卡入口管理器
    /// 统一管理所有关卡入口物品，提供全局访问和状态管理
    /// 注意：此管理器只在选关场景中存在，不会跨场景存在
    /// </summary>
    public class LevelEntranceManager : MonoBehaviour
    {
        #region 配置字段
        
        [Header("管理器设置")]
        [Tooltip("是否启用自动状态更新")]
        [SerializeField] private bool enableAutoStateUpdate = true;
        
        [Tooltip("状态更新间隔（秒）")]
        [SerializeField] private float stateUpdateInterval = 5f;
        
        #endregion
        
        #region 私有字段
        
        private Dictionary<string, LevelEntrance> levelEntrances = new Dictionary<string, LevelEntrance>();
        private LevelEntrance currentSelectedEntrance;
        private float lastStateUpdateTime = 0f;
        
        private const string LOG_MODULE = "LevelEntranceManager";
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 所有关卡入口的数量
        /// </summary>
        public int EntranceCount => levelEntrances.Count;
        
        /// <summary>
        /// 当前选中的关卡入口
        /// </summary>
        public LevelEntrance CurrentSelectedEntrance => currentSelectedEntrance;
        
        #endregion
        
        #region 生命周期方法
        
        /// <summary>
        /// 初始化
        /// </summary>
        private void Awake()
        {
            InitializeEventListeners();
            Log.Info(LOG_MODULE, "关卡入口管理器初始化完成");
        }
        
        /// <summary>
        /// 初始化事件监听
        /// </summary>
        private void InitializeEventListeners()
        {
            // 监听关卡状态变化事件
            MyGame.Events.GameEvents.OnLevelUnlocked += HandleLevelUnlocked;
            MyGame.Events.GameEvents.OnLevelCompleted += HandleLevelCompleted;
            MyGame.Events.GameEvents.OnLevelStatusUpdate += HandleLevelStatusUpdate;
            
            Log.Info(LOG_MODULE, "事件监听器初始化完成");
        }
        
        /// <summary>
        /// 清理事件监听
        /// </summary>
        private void OnDestroy()
        {
            // 清理事件监听
            MyGame.Events.GameEvents.OnLevelUnlocked -= HandleLevelUnlocked;
            MyGame.Events.GameEvents.OnLevelCompleted -= HandleLevelCompleted;
            MyGame.Events.GameEvents.OnLevelStatusUpdate -= HandleLevelStatusUpdate;
        }
        
        /// <summary>
        /// 处理关卡状态更新事件
        /// </summary>
        private void HandleLevelStatusUpdate()
        {
            UpdateAllEntranceStates();
            Log.Info(LOG_MODULE, "更新所有关卡状态");
        }
        
        /// <summary>
        /// 处理关卡解锁事件
        /// </summary>
        private void HandleLevelUnlocked(string levelId)
        {
            SetLevelUnlocked(levelId, true);
            Log.Info(LOG_MODULE, $"解锁关卡: {levelId}");
        }
        
        /// <summary>
        /// 处理关卡完成事件
        /// </summary>
        private void HandleLevelCompleted(string levelId)
        {
            SetLevelCompleted(levelId, true);
            Log.Info(LOG_MODULE, $"完成关卡: {levelId}");
        }
        
        /// <summary>
        /// 轮询式更新（保留用于兼容性，但主要使用事件驱动）
        /// </summary>
        private void Update()
        {
            if (enableAutoStateUpdate && Time.time - lastStateUpdateTime >= stateUpdateInterval)
            {
                UpdateAllEntranceStates();
                lastStateUpdateTime = Time.time;
            }
        }
        
        #endregion
        
        #region 关卡入口管理
        
        /// <summary>
        /// 注册关卡入口
        /// </summary>
        /// <param name="entrance">关卡入口实例</param>
        public void RegisterLevelEntrance(LevelEntrance entrance)
        {
            if (entrance == null)
            {
                Log.Warning(LOG_MODULE, "尝试注册空的关卡入口");
                return;
            }
            
            string levelId = entrance.LevelId;
            
            if (string.IsNullOrEmpty(levelId))
            {
                Log.Warning(LOG_MODULE, $"关卡入口的LevelId为空: {entrance.LevelName}");
                return;
            }
            
            if (!levelEntrances.ContainsKey(levelId))
            {
                levelEntrances[levelId] = entrance;
                Log.Info(LOG_MODULE, $"注册关卡入口: {entrance.LevelName} (ID: {levelId})");
                
                // 初始状态检查
                entrance.CheckUnlockStatus();
                entrance.UpdateVisualState();
            }
            else
            {
                Log.Warning(LOG_MODULE, $"重复的关卡入口ID: {levelId}");
            }
        }
        
        /// <summary>
        /// 注销关卡入口
        /// </summary>
        /// <param name="levelId">关卡ID</param>
        public void UnregisterLevelEntrance(string levelId)
        {
            if (levelEntrances.ContainsKey(levelId))
            {
                var entrance = levelEntrances[levelId];
                levelEntrances.Remove(levelId);
                Log.Info(LOG_MODULE, $"注销关卡入口: {entrance.LevelName} (ID: {levelId})");
                
                // 如果注销的是当前选中的入口，清空选中状态
                if (currentSelectedEntrance == entrance)
                {
                    currentSelectedEntrance = null;
                }
            }
        }
        
        /// <summary>
        /// 获取关卡入口
        /// </summary>
        /// <param name="levelId">关卡ID</param>
        /// <returns>关卡入口实例，如果不存在返回null</returns>
        public LevelEntrance GetLevelEntrance(string levelId)
        {
            levelEntrances.TryGetValue(levelId, out var entrance);
            return entrance;
        }
        
        /// <summary>
        /// 获取所有关卡入口
        /// </summary>
        /// <returns>所有关卡入口的列表</returns>
        public List<LevelEntrance> GetAllLevelEntrances()
        {
            return new List<LevelEntrance>(levelEntrances.Values);
        }
        
        /// <summary>
        /// 设置当前选中的关卡入口
        /// </summary>
        /// <param name="entrance">要选中的关卡入口</param>
        public void SetCurrentSelected(LevelEntrance entrance)
        {
            if (entrance != currentSelectedEntrance)
            {
                var previousEntrance = currentSelectedEntrance;
                currentSelectedEntrance = entrance;
                
                Log.Info(LOG_MODULE, $"设置当前选中关卡: {entrance?.LevelName ?? "无"}");
                
                // 可以在这里触发选中事件
                // OnCurrentSelectedChanged?.Invoke(previousEntrance, entrance);
            }
        }
        
        #endregion
        
        #region 状态管理
        
        /// <summary>
        /// 更新所有关卡入口状态
        /// </summary>
        public void UpdateAllEntranceStates()
        {
            foreach (var entrance in levelEntrances.Values)
            {
                entrance.CheckUnlockStatus();
                entrance.UpdateVisualState();
            }
            
            Log.Info(LOG_MODULE, $"更新了 {levelEntrances.Count} 个关卡入口的状态");
        }
        
        /// <summary>
        /// 设置关卡解锁状态
        /// </summary>
        /// <param name="levelId">关卡ID</param>
        /// <param name="unlocked">是否解锁</param>
        public void SetLevelUnlocked(string levelId, bool unlocked)
        {
            var entrance = GetLevelEntrance(levelId);
            if (entrance != null)
            {
                entrance.SetUnlocked(unlocked);
                Log.Info(LOG_MODULE, $"设置关卡解锁状态: {entrance.LevelName} -> {unlocked}");
            }
            else
            {
                Log.Warning(LOG_MODULE, $"找不到关卡入口: {levelId}");
            }
        }
        
        /// <summary>
        /// 设置关卡完成状态
        /// </summary>
        /// <param name="levelId">关卡ID</param>
        /// <param name="completed">是否完成</param>
        public void SetLevelCompleted(string levelId, bool completed)
        {
            var entrance = GetLevelEntrance(levelId);
            if (entrance != null)
            {
                entrance.SetCompleted(completed);
                Log.Info(LOG_MODULE, $"设置关卡完成状态: {entrance.LevelName} -> {completed}");
            }
            else
            {
                Log.Warning(LOG_MODULE, $"找不到关卡入口: {levelId}");
            }
        }
        
        /// <summary>
        /// 解锁所有关卡（用于测试）
        /// </summary>
        public void UnlockAllLevels()
        {
            foreach (var entrance in levelEntrances.Values)
            {
                entrance.SetUnlocked(true);
            }
            
            Log.Info(LOG_MODULE, $"解锁了所有 {levelEntrances.Count} 个关卡");
        }
        
        /// <summary>
        /// 获取已解锁的关卡数量
        /// </summary>
        /// <returns>已解锁关卡数量</returns>
        public int GetUnlockedLevelCount()
        {
            int count = 0;
            foreach (var entrance in levelEntrances.Values)
            {
                if (entrance.IsUnlocked)
                {
                    count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// 获取已完成的关卡数量
        /// </summary>
        /// <returns>已完成关卡数量</returns>
        public int GetCompletedLevelCount()
        {
            int count = 0;
            foreach (var entrance in levelEntrances.Values)
            {
                if (entrance.IsCompleted)
                {
                    count++;
                }
            }
            return count;
        }
        
        #endregion
        
        #region 调试工具
        
        /// <summary>
        /// 打印所有关卡入口信息（用于调试）
        /// </summary>
        public void PrintAllEntranceInfo()
        {
            Log.Info(LOG_MODULE, $"=== 关卡入口信息 (共{levelEntrances.Count}个) ===");
            
            foreach (var entrance in levelEntrances.Values)
            {
                string status = entrance.IsUnlocked ? 
                    (entrance.IsCompleted ? "已完成" : "已解锁") : 
                    "未解锁";
                
                Log.Info(LOG_MODULE, $"  {entrance.LevelName} (ID: {entrance.LevelId}) - {status}");
            }
            
            Log.Info(LOG_MODULE, "================================");
        }
        
        #endregion
    }
}