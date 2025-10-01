using UnityEngine;
using Logger;
using MyGame.UI.HUD.Controller;

namespace MyGame.UI.HUD.View
{
    /// <summary>
    /// 平视显示器视图，负责显示游戏中的核心信息UI
    /// </summary>
    public class HUDView : BaseView<HUDController>
    {
        private const string LOG_MODULE = LogModules.HUD;

        /// <summary>
        /// 初始化HUD
        /// </summary>
        protected override void Awake()
        {
            // 设置面板类型为HUD
            m_panelType = UIType.HUD;

            // 调用基类的Awake方法，完成基础初始化
            base.Awake();
        }

        /// <summary>
        /// 尝试自动绑定控制器
        /// </summary>
        protected override void TryBindController()
        {
            // 查找当前GameObject上的控制器
            if (TryGetComponent<HUDController>(out var controller))
            {
                // 找到控制器，直接绑定
                BindController(controller);
                return;
            }
            // 如果没有找到控制器，则创建一个新的
            else if (!gameObject.TryGetComponent(out controller))
            {
                controller = gameObject.AddComponent<HUDController>();
                controller.Initialize();
            }
            
            // 设置控制器的视图引用
            controller.SetView(this);
            
            // 绑定控制器到视图
            BindController(controller);
        }

        /// <summary>
        /// 控制器绑定后的回调
        /// </summary>
        protected override void OnControllerBound()
        {
            base.OnControllerBound();
            Log.Info(LOG_MODULE, "HUD控制器已绑定");
        }

        /// <summary>
        /// 控制器解绑后的回调
        /// </summary>
        protected override void OnControllerUnbound()
        {
            base.OnControllerUnbound();
            Log.Info(LOG_MODULE, "HUD控制器已解绑");
        }

        /// <summary>
        /// 初始化面板
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            Log.Info(LOG_MODULE, "HUD已初始化");
        }

        /// <summary>
        /// 清理面板资源
        /// </summary>
        public override void Cleanup()
        {
            base.Cleanup();
            Log.Info(LOG_MODULE, "HUD资源已清理");
        }

        /// <summary>
        /// 显示HUD
        /// </summary>
        public override void Show()
        {
            Log.Info(LOG_MODULE, "显示HUD");
            base.Show();
        }

        /// <summary>
        /// 隐藏HUD
        /// </summary>
        public override void Hide()
        {
            Log.Info(LOG_MODULE, "隐藏HUD");
            base.Hide();
        }
    }
}