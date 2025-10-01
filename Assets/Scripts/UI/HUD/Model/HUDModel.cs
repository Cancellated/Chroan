using System;
using Logger;

namespace MyGame.UI.HUD.Model
{
    /// <summary>
    /// HUD模型，负责管理HUD的数据和业务逻辑
    /// </summary>
    public class HUDModel : BaseModel
    {
        private const string LOG_MODULE = LogModules.HUD;

        // 可以在这里添加HUD相关的数据字段

        /// <summary>
        /// 初始化模型
        /// </summary>
        public override void Initialize()
        {
            if (!IsInitialized)
            {
                Log.Info(LOG_MODULE, "初始化HUD模型");
                
                // 初始化数据字段
                InitializeData();
                
                // 调用基类初始化
                base.Initialize();
            }
        }

        /// <summary>
        /// 初始化数据字段
        /// </summary>
        private void InitializeData()
        {
            // 在这里初始化模型的各种数据字段
        }

        /// <summary>
        /// 清理模型资源
        /// </summary>
        public override void Cleanup()
        {
            if (IsInitialized)
            {
                Log.Info(LOG_MODULE, "清理HUD模型资源");
                
                // 清理数据资源
                CleanupData();
                
                // 调用基类清理
                base.Cleanup();
            }
        }

        /// <summary>
        /// 清理数据资源
        /// </summary>
        private void CleanupData()
        {
            // 在这里清理模型的各种数据资源
        }

    }
}