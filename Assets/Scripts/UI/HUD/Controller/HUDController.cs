using UnityEngine;
using Logger;
using MyGame.UI.HUD.Model;
using MyGame.UI.HUD.View;
using MyGame.Events;

namespace MyGame.UI.HUD.Controller
{
    /// <summary>
    /// HUD控制器，负责处理HUD的逻辑和事件响应
    /// </summary>
    public class HUDController : BaseController<HUDView, HUDModel>
    {
        private const string LOG_MODULE = LogModules.HUD;

        /// <summary>
        /// 初始化控制器
        /// </summary>
        public override void Initialize()
        {
            if (!IsInitialized)
            {
                Log.Info(LOG_MODULE, "初始化HUD控制器");
                
                // 创建并初始化模型
                CreateAndInitializeModel();
                
                // 调用基类初始化
                base.Initialize();
            }
        }

        /// <summary>
        /// 初始化逻辑
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // 注册事件监听
            RegisterEvents();
        }

        /// <summary>
        /// 注册事件监听
        /// </summary>
        private void RegisterEvents()
        {
            // 这里可以注册需要的游戏事件
            // GameEvents.OnSomeEvent += HandleSomeEvent;
        }

        /// <summary>
        /// 取消注册事件监听
        /// </summary>
        private void UnregisterEvents()
        {
            // 这里可以取消注册游戏事件
            // GameEvents.OnSomeEvent -= HandleSomeEvent;
        }

        /// <summary>
        /// 清理控制器资源
        /// </summary>
        public override void Cleanup()
        {
            if (IsInitialized)
            {
                Log.Info(LOG_MODULE, "清理HUD控制器资源");
                
                // 取消注册事件
                UnregisterEvents();
                
                // 清理模型资源
                if (m_model != null)
                {
                    m_model.Cleanup();
                    m_model = null;
                }
                
                // 调用基类清理
                base.Cleanup();
            }
        }

        /// <summary>
        /// 清理逻辑
        /// </summary>
        protected override void OnCleanup()
        {
            base.OnCleanup();
        }
    }
}