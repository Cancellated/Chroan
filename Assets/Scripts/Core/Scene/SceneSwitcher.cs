using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MyGame.Events;
using Logger;

namespace MyGame.Managers
{
    /// <summary>
    /// 场景切换管理器，负责处理场景加载和卸载
    /// 实现了基于事件的统一场景切换系统
    /// </summary>
    public class SceneSwitcher : Singleton<SceneSwitcher>
    {
        private const string module = LogModules.SCENE;

        #region 生命周期
        private void OnEnable()
        {
            // 注册场景加载请求事件监听
            GameEvents.OnSceneLoadStart += OnSceneLoadStartHandler;
            // 注册加载界面准备就绪事件监听
            GameEvents.OnLoadingScreenReady += OnLoadingScreenReadyHandler;
        }

        private void OnDisable()
        {
            // 注销场景加载请求事件监听
            GameEvents.OnSceneLoadStart -= OnSceneLoadStartHandler;
            // 注销加载界面准备就绪事件监听
            GameEvents.OnLoadingScreenReady -= OnLoadingScreenReadyHandler;
        }
        #endregion
        
        #region 统一入口
        /// <summary>
        /// 请求加载场景（静态方法，外部系统可以直接调用）
        /// 这是统一的场景加载入口，通过事件机制实现
        /// </summary>
        /// <param name="sceneName">要加载的场景名称</param>
        public static void RequestLoadScene(string sceneName)
        {
            Log.Info(module, $"发起场景加载请求: {sceneName}");
            GameEvents.TriggerSceneLoadStart(sceneName);
        }
        #endregion

        #region 事件处理方法
        /// <summary>
        /// 处理场景加载开始事件
        /// 此方法仅记录日志，实际加载逻辑已移至OnLoadingScreenReadyHandler
        /// </summary>
        /// <param name="sceneName">要加载的场景名称</param>
        private void OnSceneLoadStartHandler(string sceneName)
        {
            Log.Info(module, $"接收到场景加载请求: {sceneName}");
            // 不再直接加载场景，等待加载界面准备就绪后由OnLoadingScreenReadyHandler处理
        }
        
        /// <summary>
        /// 处理加载界面准备就绪事件
        /// 当加载界面完全显示后，开始实际的场景加载
        /// </summary>
        /// <param name="sceneName">要加载的场景名称</param>
        private void OnLoadingScreenReadyHandler(string sceneName)
        {
            Log.Info(module, $"加载界面已准备就绪，开始实际加载场景: {sceneName}");
            LoadSceneAsync(sceneName);
        }
        #endregion

        #region 场景加载方法
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
            Log.Info(module, $"开始异步加载场景: {sceneName}");

            // 根据unloadCurrent参数决定加载模式
            LoadSceneMode loadMode = unloadCurrent ? LoadSceneMode.Single : LoadSceneMode.Additive;

            if (unloadCurrent)
            {
                // 如果是Single模式，不需要单独卸载当前场景，Unity会自动处理
            }
            else
            {
                // 记录当前活动场景名称，用于Additive模式下的日志记录
                var currentScene = SceneManager.GetActiveScene();
                Log.Info(module, $"将使用Additive模式加载，当前场景 '{currentScene.name}' 会被保留");
            }

            // 异步加载新场景
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName, loadMode);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // 如果是Additive模式，需要手动设置新场景为活动场景
            if (!unloadCurrent)
            {
                var newScene = SceneManager.GetSceneByName(sceneName);
                SceneManager.SetActiveScene(newScene);
            }

            // 触发场景加载完成事件
            GameEvents.TriggerSceneLoadComplete(sceneName);
            Log.Info(module, $"场景加载完成: {sceneName}");
        }

        /// <summary>
        /// 直接加载场景（同步）
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        public void LoadScene(string sceneName)
        {
            Log.Info(module, $"开始同步加载场景: {sceneName}");
            SceneManager.LoadScene(sceneName);
            // 注意：同步加载后可能无法立即触发完成事件，因为场景加载是阻塞的
            // 如果需要确保完成事件被触发，请使用异步加载方法
        }

        /// <summary>
        /// 请求卸载场景
        /// </summary>
        /// <param name="sceneName">要卸载的场景名称</param>
        public static void RequestUnloadScene(string sceneName)
        {
            Log.Info(module, $"发起场景卸载请求: {sceneName}");
            GameEvents.TriggerSceneUnload(sceneName);
        }
        #endregion
    }
}