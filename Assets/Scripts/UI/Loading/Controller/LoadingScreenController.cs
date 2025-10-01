using MyGame.Managers;
using Logger;
using UnityEngine;
using MyGame.UI.Loading.Model;
using MyGame.Events;
using MyGame.UI.Loading.View;

namespace MyGame.UI.Loading.Controller
{
    /// <summary>
    /// 加载界面控制器
    /// 负责处理加载界面的逻辑、事件响应和与视图的交互
    /// </summary>
    public class LoadingScreenController : BaseController<LoadingScreenView, LoadingScreenModel>
    {
        private const string LOG_MODULE = LogModules.LOADING;

        #region 初始化和清理
        /// <summary>
        /// 初始化控制器
        /// 调用基类Initialize并执行初始化逻辑
        /// </summary>
        public override void Initialize()
        {
            // 创建并初始化模型
            if (m_model == null)
            {
                m_model = new LoadingScreenModel();
                m_model.Initialize();
                SetModel(m_model);
            }
            
            base.Initialize();
        }

        /// <summary>
        /// 初始化控制器逻辑
        /// </summary>
        protected override void OnInitialize()
        {
        }

        /// <summary>
        /// 清理控制器资源
        /// </summary>
        protected override void OnCleanup()
        {
            // 清理模型资源
            m_model?.Cleanup();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 当加载界面隐藏动画播放完成时调用
        /// 负责隐藏界面但不销毁对象（加载界面已挂载到全局canvas下持久化）
        /// </summary>
        public void OnHideAnimationComplete()
        {
            Log.Info(LOG_MODULE, "加载界面隐藏动画播放完成");
            
            if (m_view != null && m_view.gameObject != null)
            {
                m_view.gameObject.SetActive(false);
            }
        }

        #endregion
    }
}