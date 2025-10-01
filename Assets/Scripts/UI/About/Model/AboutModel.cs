using UnityEngine;

namespace MyGame.UI.About.Model
{
    /// <summary>
    /// 关于面板数据模型
    /// 存储关于面板需要显示的基本数据
    /// </summary>
    public class AboutModel : BaseModel
    {
        #region 字段

        /// <summary>
        /// 游戏标题
        /// </summary>
        private string m_gameTitle = "Game Jam Tool Pack";

        /// <summary>
        /// 游戏版本号
        /// </summary>
        private string m_version = "1.0.0";

        /// <summary>
        /// 版权信息
        /// </summary>
        private string m_copyright = "© 2023 Game Jam Team. All rights reserved.";

        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置游戏标题
        /// </summary>
        public string GameTitle
        {
            get => m_gameTitle;
            set => m_gameTitle = value;
        }

        /// <summary>
        /// 获取或设置游戏版本号
        /// </summary>
        public string Version
        {
            get => m_version;
            set => m_version = value;
        }

        /// <summary>
        /// 获取或设置版权信息
        /// </summary>
        public string Copyright
        {
            get => m_copyright;
            set => m_copyright = value;
        }

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化模型
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// 清理模型资源
        /// </summary>
        public override void Cleanup()
        {
            base.Cleanup();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 更新关于信息
        /// </summary>
        /// <param name="gameTitle">游戏标题</param>
        /// <param name="version">版本号</param>
        /// <param name="copyright">版权信息</param>
        public void UpdateAboutInfo(
            string gameTitle = null,
            string version = null,
            string copyright = null
        )
        {
            // 更新非空字段
            if (!string.IsNullOrEmpty(gameTitle))
                m_gameTitle = gameTitle;

            if (!string.IsNullOrEmpty(version))
                m_version = version;

            if (!string.IsNullOrEmpty(copyright))
                m_copyright = copyright;
        }

        #endregion
    }
}