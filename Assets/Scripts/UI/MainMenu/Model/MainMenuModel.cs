using MyGame.UI;
using System;

namespace MyGame.UI.MainMenu.Model
{
    /// <summary>
    /// 主菜单的数据模型，管理主菜单相关的数据和业务逻辑
    /// </summary>
    public class MainMenuModel : ObservableModel
    {
        #region 字段与属性

        // 默认关卡场景名称
        private string m_defaultGameScene = "Level Select";
        private bool m_isSettingsVisible = false;
        private bool m_isAboutVisible = false;

        /// <summary>
        /// 默认启动的游戏场景名称
        /// </summary>
        public string DefaultGameScene
        {
            get { return m_defaultGameScene; }
            set { SetProperty(ref m_defaultGameScene, value, nameof(DefaultGameScene)); }
        }

        /// <summary>
        /// 设置面板是否可见
        /// </summary>
        public bool IsSettingsVisible
        {
            get { return m_isSettingsVisible; }
            set { SetProperty(ref m_isSettingsVisible, value, nameof(IsSettingsVisible)); }
        }

        /// <summary>
        /// 关于面板是否可见
        /// </summary>
        public bool IsAboutVisible
        {
            get { return m_isAboutVisible; }
            set { SetProperty(ref m_isAboutVisible, value, nameof(IsAboutVisible)); }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainMenuModel() { }

        #endregion
    }
}