using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AI.Behavior.Perception
{
    /// <summary>
    /// 记忆组件 - 最小单元组件，负责跟踪和记忆AI之前感知到的对象
    /// </summary>
    public class MemoryComponent : MonoBehaviour, IActionComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName => "MemoryComponent";

        /// <summary>
        /// 记忆对象数据结构
        /// </summary>
        public class MemoryEntry
        {
            public GameObject Object;
            public Vector3 LastKnownPosition;
            public float LastSeenTime;
            public bool IsCurrentlyVisible;
            public int TimesEncountered;
        }

        /// <summary>
        /// 记忆对象列表
        /// </summary>
        private List<MemoryEntry> _memoryEntries = new List<MemoryEntry>();
        public List<MemoryEntry> MemoryEntries => _memoryEntries;

        /// <summary>
        /// 记忆保留时间（秒）
        /// </summary>
        [SerializeField] private float _memoryRetentionTime = 10f;
        public float MemoryRetentionTime
        {
            get => _memoryRetentionTime;
            set => _memoryRetentionTime = Mathf.Max(0, value);
        }

        /// <summary>
        /// 是否记住已销毁的对象
        /// </summary>
        [SerializeField] private bool _rememberDestroyedObjects = false;
        public bool RememberDestroyedObjects
        {
            get => _rememberDestroyedObjects;
            set => _rememberDestroyedObjects = value;
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        /// <param name="gameObject">游戏对象引用</param>
        public void Initialize(GameObject gameObject)
        {
            _memoryEntries = new List<MemoryEntry>();
        }

        /// <summary>
        /// 便捷初始化方法
        /// </summary>
        public void Initialize()
        {
            Initialize(gameObject);
        }

        /// <summary>
        /// 判断组件是否可以执行
        /// </summary>
        /// <returns>如果游戏对象处于激活状态则返回true</returns>
        public bool CanExecute()
        {
            return gameObject.activeInHierarchy;
        }

        /// <summary>
        /// 执行组件行为
        /// </summary>
        /// <returns>执行结果，记忆组件始终返回true</returns>
        public bool Execute()
        {
            // 清理过期记忆
            CleanExpiredMemories();
            // 更新所有对象的可见状态
            UpdateVisibilityStatus();
            return true;
        }

        /// <summary>
        /// 更新组件状态
        /// </summary>
        public void Update()
        {
            Execute();
        }

        /// <summary>
        /// 重置组件状态
        /// </summary>
        public void Reset()
        {
            _memoryEntries.Clear();
        }

        /// <summary>
        /// 记住一组对象
        /// </summary>
        /// <param name="visibleObjects">当前可见的对象列表</param>
        public void RememberObjects(List<GameObject> visibleObjects)
        {
            if (visibleObjects == null)
                return;

            // 标记所有当前记忆为不可见
            foreach (var entry in _memoryEntries)
            {
                entry.IsCurrentlyVisible = false;
            }

            // 处理新可见的对象
            foreach (var obj in visibleObjects)
            {
                if (obj == null)
                    continue;

                // 查找现有记忆
                MemoryEntry existingEntry = _memoryEntries.Find(e => object.ReferenceEquals(e.Object, obj));

                if (existingEntry != null)
                {
                    // 更新现有记忆
                    existingEntry.LastKnownPosition = obj.transform.position;
                    existingEntry.LastSeenTime = Time.time;
                    existingEntry.IsCurrentlyVisible = true;
                    existingEntry.TimesEncountered++;
                }
                else
                {
                    // 创建新记忆
                    _memoryEntries.Add(new MemoryEntry
                    {
                        Object = obj,
                        LastKnownPosition = obj.transform.position,
                        LastSeenTime = Time.time,
                        IsCurrentlyVisible = true,
                        TimesEncountered = 1
                    });
                }
            }
        }

        /// <summary>
        /// 记住单个对象
        /// </summary>
        /// <param name="obj">要记住的对象</param>
        public void RememberObject(GameObject obj)
        { if (obj == null) return;

            MemoryEntry existingEntry = _memoryEntries.Find(e => e.Object == obj);

            if (existingEntry != null)
            {
                existingEntry.LastKnownPosition = obj.transform.position;
                existingEntry.LastSeenTime = Time.time;
                existingEntry.IsCurrentlyVisible = true;
                existingEntry.TimesEncountered++;
            }
            else
            {
                _memoryEntries.Add(new MemoryEntry
                {
                    Object = obj,
                    LastKnownPosition = obj.transform.position,
                    LastSeenTime = Time.time,
                    IsCurrentlyVisible = true,
                    TimesEncountered = 1
                });
            }
        }

        /// <summary>
        /// 忘记指定对象
        /// </summary>
        /// <param name="obj">要忘记的对象</param>
        public void ForgetObject(GameObject obj)
        {
            if (obj == null)
                return;

            _memoryEntries.RemoveAll(e => e.Object == obj);
        }

        /// <summary>
        /// 获取特定对象的记忆
        /// </summary>
        /// <param name="obj">要查找的对象</param>
        /// <returns>对象的记忆条目，如果不存在则返回null</returns>
        public MemoryEntry GetMemoryOf(GameObject obj)
        {
            if (obj == null)
                return null;

            return _memoryEntries.Find(e => e.Object == obj);
        }

        /// <summary>
        /// 获取所有当前可见的对象记忆
        /// </summary>
        /// <returns>当前可见对象的记忆列表</returns>
        public List<MemoryEntry> GetVisibleMemories()
        {
            return _memoryEntries.Where(e => e.IsCurrentlyVisible && e.Object != null).ToList();
        }

        /// <summary>
        /// 获取所有已知但当前不可见的对象记忆
        /// </summary>
        /// <returns>已知但当前不可见对象的记忆列表</returns>
        public List<MemoryEntry> GetInvisibleMemories()
        {
            return _memoryEntries.Where(e => !e.IsCurrentlyVisible && e.Object != null).ToList();
        }

        /// <summary>
        /// 获取最近记忆的对象（按最后看到的时间排序）
        /// </summary>
        /// <param name="count">返回的记忆数量</param>
        /// <returns>最近记忆的对象列表</returns>
        public List<MemoryEntry> GetMostRecentMemories(int count = 5)
        {
            return _memoryEntries.Where(e => e.Object != null)
                .OrderByDescending(e => e.LastSeenTime)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// 获取最常遇到的对象（按遇到次数排序）
        /// </summary>
        /// <param name="count">返回的记忆数量</param>
        /// <returns>最常遇到的对象列表</returns>
        public List<MemoryEntry> GetMostEncounteredMemories(int count = 5)
        {
            return _memoryEntries.Where(e => e.Object != null)
                .OrderByDescending(e => e.TimesEncountered)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// 获取距离指定位置最近的记忆对象
        /// </summary>
        /// <param name="position">参考位置</param>
        /// <param name="maxDistance">最大距离</param>
        /// <returns>最近的记忆对象，如果没有则返回null</returns>
        public MemoryEntry GetClosestMemory(Vector3 position, float maxDistance = float.MaxValue)
        {
            return _memoryEntries
                .Where(e => e.Object != null && Vector3.Distance(e.LastKnownPosition, position) <= maxDistance)
                .OrderBy(e => Vector3.Distance(e.LastKnownPosition, position))
                .FirstOrDefault();
        }

        /// <summary>
        /// 检查对象是否在记忆中
        /// </summary>
        /// <param name="obj">要检查的对象</param>
        /// <returns>如果对象在记忆中则返回true</returns>
        public bool HasMemoryOf(GameObject obj)
        {
            if (obj == null)
                return false;

            return _memoryEntries.Any(e => e.Object == obj);
        }

        /// <summary>
        /// 清理过期记忆
        /// </summary>
        private void CleanExpiredMemories()
        {
            float currentTime = Time.time;

            for (int i = _memoryEntries.Count - 1; i >= 0; i--)
            {
                MemoryEntry entry = _memoryEntries[i];

                // 如果对象已销毁且不保留已销毁对象的记忆，则移除
                if (entry.Object == null && !_rememberDestroyedObjects)
                {
                    _memoryEntries.RemoveAt(i);
                }
                // 如果记忆已过期且对象当前不可见，则移除
                else if (currentTime - entry.LastSeenTime > _memoryRetentionTime && !entry.IsCurrentlyVisible)
                {
                    _memoryEntries.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 更新所有对象的可见状态
        /// </summary>
        private void UpdateVisibilityStatus()
        {
            // 检查记忆中的对象是否仍然有效
            foreach (var entry in _memoryEntries)
            {
                if (entry.Object == null)
                {
                    entry.IsCurrentlyVisible = false;
                }
                else if (!entry.Object.activeInHierarchy)
                {
                    entry.IsCurrentlyVisible = false;
                }
            }
        }

        /// <summary>
        /// 获取记忆中对象的总数
        /// </summary>
        public int MemoryCount => _memoryEntries.Count;
    }
}