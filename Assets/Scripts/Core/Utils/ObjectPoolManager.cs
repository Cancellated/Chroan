using System.Collections.Generic;
using UnityEngine;

namespace MyGame.Core.Utils
{
    /// <summary>
    /// 对象池配置结构
    /// </summary>
    [System.Serializable]
    public struct ObjectPoolSetup
    {   
        [Tooltip("要池化的预制体")]
        public GameObject prefab;
        
        [Tooltip("初始池容量")]
        public int initialPoolSize;
        
        [Tooltip("是否允许池自动扩展")]
        public bool allowPoolExpansion;
        
        [Tooltip("最大池容量（-1表示无限制）")]
        public int maxPoolSize;
    }

    /// <summary>
    /// 对象池管理器单例
    /// 直接管理所有对象池实例，提供全局访问点
    /// </summary>
    public class ObjectPoolManager : MyGame.Singleton<ObjectPoolManager>
    {
        #region 字段

        [Header("对象池配置")]
        [Tooltip("对象池配置列表")]
        [SerializeField] private List<ObjectPoolSetup> poolSetups = new List<ObjectPoolSetup>();

        /// <summary>
        /// 对象池字典，键为预制体，值为对应的对象池
        /// </summary>
        private Dictionary<GameObject, ObjectPool> pools = new Dictionary<GameObject, ObjectPool>();

        #endregion

        #region 初始化

        protected override void Awake()
        {   
            base.Awake();
            InitializeAllPools();
        }

        /// <summary>
        /// 初始化所有对象池
        /// </summary>
        private void InitializeAllPools()
        {   
            foreach (var setup in poolSetups)
            {   
                if (setup.prefab == null) continue;
                
                // 创建对象池游戏对象
                GameObject poolObj = new GameObject($"ObjectPool_{setup.prefab.name}");
                poolObj.transform.SetParent(transform);
                
                // 添加对象池组件并配置
                ObjectPool pool = poolObj.AddComponent<ObjectPool>();
                pool.InitializePool(setup.prefab, setup.initialPoolSize, setup.allowPoolExpansion, setup.maxPoolSize);
                
                // 添加到字典
                pools[setup.prefab] = pool;
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <param name="prefab">预制体引用</param>
        /// <returns>获取的游戏对象</returns>
        public static GameObject Get(GameObject prefab)
        {   
            return Instance.GetObject(prefab);
        }

        /// <summary>
        /// 从对象池中获取对象，并设置父对象和位置
        /// </summary>
        /// <param name="prefab">预制体引用</param>
        /// <param name="parent">父对象transform</param>
        /// <param name="position">对象位置</param>
        /// <returns>获取的游戏对象</returns>
        public static GameObject Get(GameObject prefab, Transform parent, Vector3 position)
        {   
            return Instance.GetObject(prefab, parent, position);
        }

        /// <summary>
        /// 从对象池中获取对象，并设置父对象、位置和旋转
        /// </summary>
        /// <param name="prefab">预制体引用</param>
        /// <param name="parent">父对象transform</param>
        /// <param name="position">对象位置</param>
        /// <param name="rotation">对象旋转</param>
        /// <returns>获取的游戏对象</returns>
        public static GameObject Get(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {   
            return Instance.GetObject(prefab, parent, position, rotation);
        }

        /// <summary>
        /// 回收对象到对象池
        /// </summary>
        /// <param name="obj">要回收的对象</param>
        public static void Return(GameObject obj)
        {   
            Instance.ReturnObject(obj);
        }

        /// <summary>
        /// 动态创建新的对象池
        /// </summary>
        /// <param name="prefab">要池化的预制体</param>
        /// <param name="initialSize">初始池容量</param>
        /// <param name="allowExpansion">是否允许池自动扩展</param>
        /// <param name="maxSize">最大池容量（-1表示无限制）</param>
        public static void CreatePool(GameObject prefab, int initialSize = 10, bool allowExpansion = true, int maxSize = -1)
        {   
            Instance.CreateNewPool(prefab, initialSize, allowExpansion, maxSize);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 从指定预制体的对象池中获取对象
        /// </summary>
        /// <param name="prefab">预制体引用</param>
        /// <returns>获取的游戏对象</returns>
        private GameObject GetObject(GameObject prefab)
        {   
            if (!pools.TryGetValue(prefab, out var pool))
            {   
                // 如果池不存在，动态创建
                pool = CreateNewPool(prefab);
            }
            
            return pool?.GetObject();
        }

        /// <summary>
        /// 从指定预制体的对象池中获取对象，并设置父对象和位置
        /// </summary>
        /// <param name="prefab">预制体引用</param>
        /// <param name="parent">父对象transform</param>
        /// <param name="position">对象位置</param>
        /// <returns>获取的游戏对象</returns>
        private GameObject GetObject(GameObject prefab, Transform parent, Vector3 position)
        {   
            if (!pools.TryGetValue(prefab, out var pool))
            {   
                // 如果池不存在，动态创建
                pool = CreateNewPool(prefab);
            }
            
            return pool?.GetObject(parent, position);
        }

        /// <summary>
        /// 从指定预制体的对象池中获取对象，并设置父对象、位置和旋转
        /// </summary>
        /// <param name="prefab">预制体引用</param>
        /// <param name="parent">父对象transform</param>
        /// <param name="position">对象位置</param>
        /// <param name="rotation">对象旋转</param>
        /// <returns>获取的游戏对象</returns>
        private GameObject GetObject(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {   
            if (!pools.TryGetValue(prefab, out var pool))
            {   
                // 如果池不存在，动态创建
                pool = CreateNewPool(prefab);
            }
            
            return pool?.GetObject(parent, position, rotation);
        }

        /// <summary>
        /// 回收对象到对应的池中
        /// </summary>
        /// <param name="obj">要回收的对象</param>
        private void ReturnObject(GameObject obj)
        {   
            if (obj == null)
                return;
                
            // 获取对象所属的池
            PooledObject pooledObject = obj.GetComponent<PooledObject>();
            if (pooledObject != null && pooledObject.Pool != null)
            {   
                pooledObject.Pool.ReturnObject(obj);
            }
            else
            {   
                Log.Warning(LOG_MODULE, "尝试回收的对象不是池化对象: " + obj.name, this);
            }
        }

        /// <summary>
        /// 动态创建新的对象池
        /// </summary>
        /// <param name="prefab">要池化的预制体</param>
        /// <param name="initialSize">初始池容量</param>
        /// <param name="allowExpansion">是否允许池自动扩展</param>
        /// <param name="maxSize">最大池容量（-1表示无限制）</param>
        /// <returns>创建的对象池</returns>
        private ObjectPool CreateNewPool(GameObject prefab, int initialSize = 10, bool allowExpansion = true, int maxSize = -1)
        {   
            if (prefab == null)
            {   
                Log.Error(LOG_MODULE, "无法创建对象池：预制体为null", this);
                return null;
            }
            
            // 创建对象池游戏对象
            GameObject poolObj = new GameObject($"DynamicObjectPool_{prefab.name}");
            poolObj.transform.SetParent(transform);
            
            // 添加对象池组件并配置
            ObjectPool pool = poolObj.AddComponent<ObjectPool>();
            pool.InitializePool(prefab, initialSize, allowExpansion, maxSize);
            
            // 添加到字典
            pools[prefab] = pool;
            
            Log.Info(LOG_MODULE, $"动态创建对象池: {prefab.name}", this);
            return pool;
        }

        #endregion
    }
}