using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MyGame.System;

namespace MyGame.Managers
{
    /// <summary>
    /// 场景切换管理器，负责处理场景加载和卸载
    /// </summary>
    public class SceneSwitcher : Singleton<SceneSwitcher>
    {
        #region 字段
        [SerializeField] private string levelSelectScene = "LevelSelect";
        #endregion
        #region 生命周期
        void OnEnable()
        {
            GameEvents.OnSceneLoad += LoadScene;
            GameEvents.OnSceneLoadComplete += UnloadPreviousScene;
        }

        void OnDisable()
        {
            GameEvents.OnSceneLoad -= LoadScene;
            GameEvents.OnSceneLoadComplete -= UnloadPreviousScene;
        }
        #endregion

        
        #region 方法
        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="unloadCurrent">是否卸载当前场景</param>
        public void LoadSceneAsync(string sceneName)
        {
            StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
        }

        private IEnumerator LoadSceneAsyncCoroutine(string sceneName)
        {
            // 异步加载新场景
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // 设置新场景为活动场景
            var newScene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(newScene);

            // 触发场景加载完成事件
            GameEvents.TriggerSceneLoadComplete(sceneName);
        }

        /// <summary>
        /// 卸载上一个场景
        /// </summary>
        /// <param name="sceneName"></param>
        private void UnloadPreviousScene(string sceneName)
        {
            // 检查当前是否有活动场景
            if (SceneManager.GetActiveScene().name != levelSelectScene)
            {
                // 卸载当前活动场景
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            }
        }

        /// <summary>
        /// 直接加载场景（同步）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        #endregion
    }
}
