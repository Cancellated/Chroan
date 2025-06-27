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
        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="unloadCurrent">是否卸载当前场景</param>
        public void LoadSceneAsync(string sceneName, bool unloadCurrent = true)
        {
            StartCoroutine(LoadSceneAsyncCoroutine(sceneName, unloadCurrent));
        }

        private IEnumerator LoadSceneAsyncCoroutine(string sceneName, bool unloadCurrent)
        {
            // 触发场景加载开始事件
            GameEvents.TriggerSceneLoadStart(sceneName);

            if (unloadCurrent)
            {
                // 卸载当前场景
                var currentScene = SceneManager.GetActiveScene();
                GameEvents.TriggerSceneUnload(currentScene.name);
                yield return SceneManager.UnloadSceneAsync(currentScene);
            }

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
        /// 直接加载场景（同步）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void LoadScene(string sceneName)
        {
            GameEvents.TriggerSceneLoadStart(sceneName);
            SceneManager.LoadScene(sceneName);
            GameEvents.TriggerSceneLoadComplete(sceneName);
        }
    }
}
