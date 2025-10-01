using UnityEngine;
using Logger;
using MyGame.UI.PauseMenu.Controller;

namespace MyGame.UI.PauseMenu.View
{
    /// <summary>
    /// 暂停菜单视图，负责显示暂停界面和处理基本的UI显隐
    /// </summary>
    public class PauseMenuView : BaseView<PauseMenuController>
    {
        private const string LOG_MODULE = LogModules.PAUSEMENU;

        /// <summary>
        /// 初始化暂停菜单
        /// </summary>
        protected override void Awake()
        {
            // 设置面板类型为暂停菜单
            m_panelType = UIType.PauseMenu;

            // 调用基类的Awake方法，完成基础初始化
            base.Awake();
        }

        /// <summary>
        /// 尝试自动绑定控制器
        /// </summary>
        protected override void TryBindController()
        {
            // 查找当前GameObject上的控制器
            // 如果没有找到控制器，则创建一个新的
            if (!gameObject.TryGetComponent<PauseMenuController>(out var controller))
            {
                controller = gameObject.AddComponent<PauseMenuController>();
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
            Log.Info(LOG_MODULE, "暂停菜单控制器已绑定");
        }

        /// <summary>
        /// 控制器解绑后的回调
        /// </summary>
        protected override void OnControllerUnbound()
        {
            base.OnControllerUnbound();
            Log.Info(LOG_MODULE, "暂停菜单控制器已解绑");
        }

        /// <summary>
        /// 初始化面板
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            Log.Info(LOG_MODULE, "暂停菜单已初始化");
        }

        /// <summary>
        /// 清理面板资源
        /// </summary>
        public override void Cleanup()
        {
            base.Cleanup();
            Log.Info(LOG_MODULE, "暂停菜单资源已清理");
        }

        /// <summary>
        /// 显示暂停菜单
        /// </summary>
        public override void Show()
        {
            Log.Info(LOG_MODULE, "显示暂停菜单");
            base.Show();
        }

        /// <summary>
        /// 隐藏暂停菜单
        /// </summary>
        public override void Hide()
        {
            Log.Info(LOG_MODULE, "隐藏暂停菜单");
            base.Hide();
        }
    }
}