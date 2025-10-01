using UnityEngine;

namespace MyGame
{
    /// <summary>
    /// 通用MonoBehaviour单例基类。
    /// 继承此类可快速实现全局唯一的管理器或工具类。
    /// </summary>
    /// <typeparam name="T">单例类型，需继承自MonoBehaviour</typeparam>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region 字段

        /// <summary>
        /// 单例实例。
        /// </summary>
        private static T _instance;

        /// <summary>
        /// 线程锁，确保多线程环境下的安全（虽然Unity主线程为主，但加锁更稳妥）。
        /// </summary>
        private static readonly object _lock = new();

        #endregion

        #region 属性

        /// <summary>
        /// 获取单例实例，若不存在则自动查找或创建。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            // 优先查找场景中已存在的实例
                            _instance = Object.FindFirstObjectByType<T>();
                            if (_instance == null)
                            {
                                // 若不存在则自动创建
                                var singletonObject = new GameObject(typeof(T).Name);
                                _instance = singletonObject.AddComponent<T>();
                                DontDestroyOnLoad(singletonObject);
                            }
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region 生命周期

        /// <summary>
        /// 保证单例唯一性，重复实例自动销毁。
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }   
        }

        #endregion
    }
}
