using Logger;
using System;
using UnityEngine;

namespace MyGame.UI.Loading.Model
{
    /// <summary>
    /// 加载界面模型
    /// 负责管理加载界面相关的数据和业务逻辑
    /// </summary>
    public class LoadingScreenModel : BaseModel
    {
        private const string LOG_MODULE = LogModules.LOADING;
        
        private string m_currentLoadingScene = string.Empty;
        private float m_loadingProgress = 0f;
        private bool m_isLoading = false;
        
        /// <summary>
        /// 当前加载的场景名称
        /// </summary>
        public string CurrentLoadingScene
        {
            get { return m_currentLoadingScene; }
            set
            {
                if (m_currentLoadingScene != value)
                {
                    m_currentLoadingScene = value;
                    // 这里可以添加属性变化通知逻辑
                }
            }
        }
        
        /// <summary>
        /// 当前加载进度 (0-1)
        /// </summary>
        public float LoadingProgress
        {
            get { return m_loadingProgress; }
            set
            {
                float newValue = Mathf.Clamp01(value);
                if (m_loadingProgress != newValue)
                {
                    m_loadingProgress = newValue;
                    // 这里可以添加属性变化通知逻辑
                }
            }
        }
        
        /// <summary>
        /// 是否正在加载中
        /// </summary>
        public bool IsLoading
        {
            get { return m_isLoading; }
            set
            {
                if (m_isLoading != value)
                {
                    m_isLoading = value;
                    // 这里可以添加属性变化通知逻辑
                }
            }
        }
        
        
        /// <summary>
        /// 重置模型状态
        /// </summary>
        public void Reset()
        {
            m_currentLoadingScene = string.Empty;
            m_loadingProgress = 0f;
            m_isLoading = false;
        }
        
        /// <summary>
        /// 更新加载进度
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="progress">进度值 (0-1)</param>
        public void UpdateLoadingProgress(string sceneName, float progress)
        {
            CurrentLoadingScene = sceneName;
            LoadingProgress = progress;
        }
        
        /// <summary>
        /// 开始加载
        /// </summary>
        /// <param name="sceneName">要加载的场景名称</param>
        public void StartLoading(string sceneName)
        {
            CurrentLoadingScene = sceneName;
            LoadingProgress = 0f;
            IsLoading = true;
        }
        
        /// <summary>
        /// 完成加载
        /// </summary>
        public void CompleteLoading()
        {
            LoadingProgress = 1f;
            IsLoading = false;
        }
    }
}