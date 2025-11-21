using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AI.Behavior.Perception
{
    /// <summary>
    /// 过滤组件 - 最小单元组件，负责对检测到的对象进行过滤和分类
    /// </summary>
    public class FilterComponent : MonoBehaviour, IActionComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName => "FilterComponent";

        /// <summary>
        /// 允许的标签列表
        /// </summary>
        [SerializeField] private List<string> _allowedTags = new();
        public List<string> AllowedTags
        {
            get => _allowedTags;
            set => _allowedTags = value ?? new List<string>();
        }

        /// <summary>
        /// 拒绝的标签列表
        /// </summary>
        [SerializeField] private List<string> _rejectedTags = new();
        public List<string> RejectedTags
        {
            get => _rejectedTags;
            set => _rejectedTags = value ?? new List<string>();
        }

        /// <summary>
        /// 允许的组件类型名称列表
        /// </summary>
        [SerializeField] private List<string> _requiredComponentNames = new();
        public List<string> RequiredComponentNames
        {
            get => _requiredComponentNames;
            set => _requiredComponentNames = value ?? new List<string>();
        }

        /// <summary>
        /// 过滤后的对象列表
        /// </summary>
        public List<GameObject> FilteredObjects { get; private set; }

        /// <summary>
        /// 初始化组件
        /// </summary>
        /// <param name="gameObject">游戏对象引用</param>
        public void Initialize(GameObject gameObject)
        {
            FilteredObjects = new List<GameObject>();
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
        /// <returns>执行结果，过滤组件始终返回true</returns>
        public bool Execute()
        {
            // 过滤组件本身不执行过滤，需要通过其他方法调用
            return true;
        }

        /// <summary>
        /// 更新组件状态
        /// </summary>
        public void Update()
        {
            // 过滤组件的核心逻辑由其他方法调用
        }

        /// <summary>
        /// 重置组件状态
        /// </summary>
        public void Reset()
        {
            FilteredObjects.Clear();
        }

        /// <summary>
        /// 过滤对象列表
        /// </summary>
        /// <param name="objectsToFilter">要过滤的对象列表</param>
        /// <returns>过滤后的对象列表</returns>
        public List<GameObject> FilterObjects(List<GameObject> objectsToFilter)
        {
            if (objectsToFilter == null)
                return new List<GameObject>();

            FilteredObjects = objectsToFilter.Where(IsObjectAllowed).ToList();
            return FilteredObjects;
        }

        /// <summary>
        /// 检查对象是否被允许
        /// </summary>
        /// <param name="obj">要检查的对象</param>
        /// <returns>如果对象被允许则返回true</returns>
        private bool IsObjectAllowed(GameObject obj)
        {
            if (obj == null || !obj.activeInHierarchy)
                return false;

            // 检查标签过滤
            if (!CheckTagFilter(obj))
                return false;

            // 检查组件过滤
            if (!CheckComponentFilter(obj))
                return false;

            return true;
        }

        /// <summary>
        /// 检查标签过滤
        /// </summary>
        /// <param name="obj">要检查的对象</param>
        /// <returns>如果标签通过过滤则返回true</returns>
        private bool CheckTagFilter(GameObject obj)
        {
            // 如果有拒绝的标签列表，检查对象是否有拒绝的标签
            if (_rejectedTags.Count > 0 && _rejectedTags.Contains(obj.tag))
                return false;

            // 如果有允许的标签列表，检查对象是否有允许的标签
            if (_allowedTags.Count > 0 && !_allowedTags.Contains(obj.tag))
                return false;

            return true;
        }

        /// <summary>
        /// 检查组件过滤
        /// </summary>
        /// <param name="obj">要检查的对象</param>
        /// <returns>如果组件通过过滤则返回true</returns>
        private bool CheckComponentFilter(GameObject obj)
        {
            if (_requiredComponentNames.Count == 0)
                return true;

            // 检查对象是否包含所有必需的组件
            foreach (var componentName in _requiredComponentNames)
            {
                // 尝试按名称获取组件
                Component component = obj.GetComponent(componentName);
                if (component == null)
                {
                    // 尝试按类型获取组件（如果提供的是类型名称）
                    try
                    {
                        System.Type type = System.Type.GetType(componentName);
                        if (type != null)
                        {
                            component = obj.GetComponent(type);
                        }
                    }
                    catch (System.Exception)
                    {
                        // 忽略类型获取错误
                    }

                    if (component == null)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 添加允许的标签
        /// </summary>
        /// <param name="tag">要添加的标签</param>
        public void AddAllowedTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag) && !_allowedTags.Contains(tag))
            {
                _allowedTags.Add(tag);
            }
        }

        /// <summary>
        /// 添加拒绝的标签
        /// </summary>
        /// <param name="tag">要添加的标签</param>
        public void AddRejectedTag(string tag)
        {
            if (!string.IsNullOrEmpty(tag) && !_rejectedTags.Contains(tag))
            {
                _rejectedTags.Add(tag);
            }
        }

        /// <summary>
        /// 添加必需的组件名称
        /// </summary>
        /// <param name="componentName">组件名称</param>
        public void AddRequiredComponent(string componentName)
        {
            if (!string.IsNullOrEmpty(componentName) && !_requiredComponentNames.Contains(componentName))
            {
                _requiredComponentNames.Add(componentName);
            }
        }

        /// <summary>
        /// 按标签过滤对象
        /// </summary>
        /// <param name="objects">对象列表</param>
        /// <param name="tag">目标标签</param>
        /// <returns>具有指定标签的对象列表</returns>
        public List<GameObject> FilterByTag(List<GameObject> objects, string tag)
        {
            if (objects == null || string.IsNullOrEmpty(tag))
                return new List<GameObject>();

            return objects.Where(obj => obj != null && obj.CompareTag(tag)).ToList();
        }

        /// <summary>
        /// 按组件过滤对象
        /// </summary>
        /// <param name="objects">对象列表</param>
        /// <param name="componentType">组件类型</param>
        /// <returns>具有指定组件的对象列表</returns>
        public List<GameObject> FilterByComponent<T>(List<GameObject> objects)
        {
            if (objects == null)
                return new List<GameObject>();

            return objects.Where(obj => obj != null && obj.GetComponent<T>() != null).ToList();
        }
    }
}