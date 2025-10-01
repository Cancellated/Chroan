using UnityEngine;
using MyGame.UI.Settings.Controller;

namespace MyGame.UI.Settings.Components
{
    /// <summary>
    /// 设置项组件的抽象基类
    /// 提供统一的接口和生命周期管理，属于MVC架构中的View层
    /// </summary>
    public abstract class BaseSettingsComponent : MonoBehaviour
    {
        /// <summary>
        /// 设置面板控制器的引用
        /// </summary>
        protected SettingsPanelController m_controller;

        /// <summary>
        /// 初始化组件，绑定控制器并设置初始状态
        /// </summary>
        /// <param name="controller">设置面板控制器</param>
        public virtual void Initialize(SettingsPanelController controller)
        {
            m_controller = controller;
            
            // 只有在控制器不为空时才初始化组件和绑定事件
            if (m_controller != null)
            {
                InitializeComponent();
                BindEvents();
            }
        }

        /// <summary>
        /// 初始化组件的UI和数据
        /// 派生类需要实现此方法进行特定的初始化
        /// </summary>
        protected abstract void InitializeComponent();

        /// <summary>
        /// 绑定用户交互事件
        /// 派生类需要实现此方法绑定特定的事件处理器
        /// </summary>
        protected abstract void BindEvents();

        /// <summary>
        /// 更新组件的显示状态
        /// 当设置数据变化时调用此方法更新UI
        /// </summary>
        public abstract void UpdateView();

        /// <summary>
        /// 清理组件资源，解绑事件
        /// 在组件销毁时调用
        /// </summary>
        protected abstract void Cleanup();

        /// <summary>
        /// 在对象销毁时调用清理方法
        /// </summary>
        protected virtual void OnDestroy()
        {
            Cleanup();
        }
    }
}