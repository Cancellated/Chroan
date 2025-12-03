using System.Collections.Generic;
using UnityEngine;
using Logger;

namespace MyGame.Core.Utils
{
    /// <summary>
    /// 单个对象池实现
    /// 管理特定预制体的对象池
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        #region 字段

        [Header("对象池设置")]
        [Tooltip("要管理的预制体")]
        [SerializeField] private GameObject prefab;

        [Tooltip("初始池容量")]
        [SerializeField] private int initialPoolSize = 10;

        [Tooltip("是否自动扩展池容量")]
        [SerializeField] private bool allowPoolExpansion = true;

        [Tooltip("最大池容量（-1表示无限制）")]
        [SerializeField] private int maxPoolSize = -1;

        /// <summary>
        /// 日志模块标识
        /// </summary>
        private const string LOG_MODULE = LogModules.UTILS;

        /// <summary>
        /// 要池化的预制体
        /// </summary>
        private GameObject prefab;
        
        /// <summary>
        /// 初始池容量
        /// </summary>
        private int initialPoolSize = 10;
        
        /// <summary>
        /// 是否允许池自动扩展
        /// </summary>
        private bool allowPoolExpansion = true;
        
        /// <summary>
        /// 最大池容量（-1表示无限制）
        /// </summary>
        private int maxPoolSize = -1;

        /// <summary>
        /// 可用对象队列
        /// </summary>
        private Queue<GameObject> availableObjects = new Queue<GameObject>();

        /// <summary>
        /// 所有池化对象列表
        /// </summary>
        private List<GameObject> allPooledObjects = new List<GameObject>();

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool isInitialized = false;

        #endregion

        #region 属性

        /// <summary>
        /// 获取当前池中可用对象数量
        /// </summary>
        public int AvailableCount => availableObjects.Count;

        /// <summary>
        /// 获取池中所有对象总数
        /// </summary>
        public int TotalCount => allPooledObjects.Count;

        /// <summary>
        /// 获取池化对象的预制体
        /// </summary>
        public GameObject Prefab => prefab;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化对象池
        /// </summary>
        /// <param name="prefab">要池化的预制体</param>
        /// <param name="initialSize">初始池容量</param>
        /// <param name="allowExpansion">是否允许池自动扩展</param>
        /// <param name="maxSize">最大池容量</param>
        public void InitializePool(GameObject prefab, int initialSize = 10, bool allowExpansion = true, int maxSize = -1)
        {   
            if (isInitialized) return;
            
            this.prefab = prefab;
            this.initialPoolSize = initialSize;
            this.allowPoolExpansion = allowExpansion;
            this.maxPoolSize = maxSize;

            if (prefab == null)
            {   
                Log.Error(LOG_MODULE, "对象池预制体未设置！", this);
                return;
            }

            // 创建初始对象
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePooledObject();
            }
            
            isInitialized = true;
            
            Log.Info(LOG_MODULE, $"对象池 '{prefab.name}' 初始化完成，初始容量: {initialPoolSize}", this);
        }

        /// <summary>
        /// 创建单个池化对象
        /// </summary>
        /// <returns>创建的池化对象</returns>
        private GameObject CreatePooledObject()
        {   
            if (prefab == null) return null;
            
            // 检查池大小限制
            if (maxPoolSize > 0 && allPooledObjects.Count >= maxPoolSize)
            {   
                Log.Warning(LOG_MODULE, $"对象池 '{prefab.name}' 已达到最大容量 {maxPoolSize}", this);
                return null;
            }
            
            GameObject obj = Instantiate(prefab, transform);
            obj.name = $"{prefab.name}_Pooled_{allPooledObjects.Count}";
            
            // 添加池化对象组件
            PooledObject pooledObj = obj.GetComponent<PooledObject>();
            if (pooledObj == null)
            {   
                pooledObj = obj.AddComponent<PooledObject>();
            }
            pooledObj.Pool = this;
            
            obj.SetActive(false);
            availableObjects.Enqueue(obj);
            allPooledObjects.Add(obj);
            
            return obj;
        }

        #endregion

        #region 对象获取

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        /// <returns>获取的游戏对象</returns>
        public GameObject GetObject()
        {   
            return GetObject(null, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// 从池中获取对象，并设置父对象和位置
        /// </summary>
        /// <param name="parent">父对象transform</param>
        /// <param name="position">对象位置</param>
        /// <returns>获取的游戏对象</returns>
        public GameObject GetObject(Transform parent, Vector3 position)
        {   
            return GetObject(parent, position, Quaternion.identity);
        }

        /// <summary>
        /// 从池中获取对象，并设置父对象、位置和旋转
        /// </summary>
        /// <param name="parent">父对象transform</param>
        /// <param name="position">对象位置</param>
        /// <param name="rotation">对象旋转</param>
        /// <returns>获取的游戏对象</returns>
        public GameObject GetObject(Transform parent, Vector3 position, Quaternion rotation)
        {   
            if (!isInitialized)
            {   
                Log.Warning(LOG_MODULE, "对象池未初始化，正在自动初始化", this);
                InitializePool(prefab, initialPoolSize, allowPoolExpansion, maxPoolSize);
            }
            
            GameObject obj = null;
            
            // 尝试从可用队列中获取对象
            if (availableObjects.Count > 0)
            {   
                obj = availableObjects.Dequeue();
            }
            // 如果允许扩展且池未满，则创建新对象
            else if (allowPoolExpansion && (maxPoolSize <= 0 || allPooledObjects.Count < maxPoolSize))
            {   
                obj = CreatePooledObject();
                if (obj != null)
                {   
                    Log.Info(LOG_MODULE, $"对象池 '{prefab.name}' 自动扩展，当前容量: {allPooledObjects.Count}", this);
                }
            }
            
            if (obj == null)
            {   
                Log.Warning(LOG_MODULE, $"无法从对象池 '{prefab.name}' 获取对象，池已满", this);
                return null;
            }
            
            // 设置对象属性
            if (parent != null)
            {   
                obj.transform.SetParent(parent);
            }
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            
            // 触发对象激活事件
            PooledObject pooledObj = obj.GetComponent<PooledObject>();
            if (pooledObj != null)
            {   
                pooledObj.OnGetFromPool();
            }
            
            return obj;
        }

        #endregion

        #region 对象回收

        /// <summary>
        /// 回收对象到池中
        /// </summary>
        /// <param name="obj">要回收的对象</param>
        public void ReturnObject(GameObject obj)
        {   
            if (obj == null) return;
            
            // 确保对象是从此池中获取的
            PooledObject pooledObject = obj.GetComponent<PooledObject>();
            if (pooledObject == null || pooledObject.Pool != this)
            {
                Log.Warning(LOG_MODULE, $"尝试回收不属于此池的对象: {obj.name}", this);
                return;
            }
            
            // 重置对象状态
            obj.transform.SetParent(transform);
            obj.SetActive(false);
            
            // 触发对象回收事件
            pooledObj.OnReturnToPool();
            
            // 添加到可用队列
            availableObjects.Enqueue(obj);
        }

        #endregion

        #region 池管理

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void ClearPool()
        {   
            // 销毁所有可用对象
            foreach (var obj in availableObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            availableObjects.Clear();

            // 销毁所有子对象（包括正在使用的）
            foreach (Transform child in transform)
            {
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }

            totalObjectsCount = 0;
            isInitialized = false;
            Log.Info(LOG_MODULE, $"对象池 '{prefab?.name}' 已清空", this);
        }

        /// <summary>
        /// 预加载指定数量的对象
        /// </summary>
        /// <param name="count">预加载数量</param>
        public void PreloadObjects(int count)
        {   
            if (!isInitialized)
            {   
                InitializePool(prefab, initialPoolSize, allowPoolExpansion, maxPoolSize);
            }
            
            int currentCount = allPooledObjects.Count;
            int targetCount = currentCount + count;
            
            // 考虑最大池大小限制
            if (maxPoolSize > 0 && targetCount > maxPoolSize)
            {   
                targetCount = maxPoolSize;
                count = targetCount - currentCount;
            }
            
            if (count <= 0) return;
            
            for (int i = 0; i < count; i++)
            {   
                CreatePooledObject();
            }
            
            Log.Info(LOG_MODULE, $"对象池 '{prefab.name}' 预加载完成，新增 {count} 个对象，当前容量: {allPooledObjects.Count}", this);
        }

        #endregion
    }
}