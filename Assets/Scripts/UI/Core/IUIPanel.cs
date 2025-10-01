using System.Collections;
using UnityEngine;

namespace MyGame.UI
{
    /// <summary>
    /// UI面板接口，定义了所有UI面板需要实现的基础方法
    /// </summary>
    public interface IUIPanel
    {
        /// <summary>
        /// 显示面板
        /// </summary>
        void Show();
        
        /// <summary>
        /// 隐藏面板
        /// </summary>
        void Hide();
        
        /// <summary>
        /// 是否显示面板
        /// </summary>
        bool IsVisible { get; }
        
        /// <summary>
        /// 面板类型，用于UIManager进行状态管理
        /// </summary>
        UIType PanelType { get; }
        
        /// <summary>
        /// 初始化面板
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 清理面板资源
        /// </summary>
        void Cleanup();
    }
}
