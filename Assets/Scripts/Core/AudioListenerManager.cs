using UnityEngine;
using Logger;
using MyGame.Events;

namespace MyGame.DevTools
{
    /// <summary>
    /// 音频监听器管理器
    /// 负责自动检测并处理场景中存在的多个AudioListener组件
    /// 确保场景中始终只有一个活跃的AudioListener
    /// </summary>
    public class AudioListenerManager : Singleton<AudioListenerManager>
    {
        private const string LOG_MODULE = LogModules.DEVTOOLS;
        
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            // 注册场景加载完成事件
            GameEvents.OnSceneLoadComplete += OnSceneLoadComplete;
            
            // 初始检查
            CheckAndFixAudioListeners();
        }
        
        private void OnDisable()
        {
            // 注销事件监听
            GameEvents.OnSceneLoadComplete -= OnSceneLoadComplete;
        }
        #endregion
        
        #region 事件处理
        /// <summary>
        /// 场景加载完成时检查并修复AudioListener
        /// </summary>
        /// <param name="sceneName">加载完成的场景名称</param>
        private void OnSceneLoadComplete(string sceneName)
        {
            CheckAndFixAudioListeners();
        }
        #endregion
        
        #region 核心功能
        /// <summary>
        /// 检查并修复场景中的AudioListener组件
        /// 确保场景中始终只有一个活跃的AudioListener
        /// </summary>
        public void CheckAndFixAudioListeners()
        {
            // 获取场景中所有的AudioListener组件
            AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();
            
            // 如果有多个AudioListener
            if (audioListeners.Length > 1)
            {
                Log.Warning(LOG_MODULE, $"检测到{audioListeners.Length}个AudioListener组件，仅保留一个。");
                
                // 保留第一个AudioListener，禁用其余的
                for (int i = 1; i < audioListeners.Length; i++)
                {
                    audioListeners[i].enabled = false;
                    Log.Info(LOG_MODULE, $"已禁用GameObject '{audioListeners[i].gameObject.name}'上的AudioListener组件");
                }
            }
        }
        
        /// <summary>
        /// 查找并返回当前活跃的AudioListener
        /// </summary>
        /// <returns>当前活跃的AudioListener，如果没有则返回null</returns>
        public AudioListener GetActiveAudioListener()
        {
            AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();
            
            foreach (AudioListener listener in audioListeners)
            {
                if (listener.enabled)
                {
                    return listener;
                }
            }
            
            return null;
        }
        #endregion
        
        #region 公共接口
        /// <summary>
        /// 静态方法，用于手动触发检查
        /// </summary>
        public static void CheckAudioListeners()
        {
            if (Instance != null)
            {
                Instance.CheckAndFixAudioListeners();
            }
        }
        #endregion
    }
}