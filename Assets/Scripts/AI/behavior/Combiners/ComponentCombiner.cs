using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AI.Behavior.Combiners
{
    /// <summary>
    /// 基础组件组合器 - 用于组合多个基础组件并按顺序执行它们
    /// 实现IActionComponent接口，可作为行为树中的一个节点或独立组件使用
    /// </summary>
    public class ComponentCombiner : MonoBehaviour, IActionComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName => "ComponentCombiner";

        /// <summary>
        /// 子组件执行模式
        /// </summary>
        public enum ExecutionMode
        {
            Sequential,    // 顺序执行，一个失败则整体失败
            Parallel,      // 并行执行，返回所有组件的综合结果
            Selective,     // 选择执行，任一组件成功则整体成功
            AllSucceed     // 所有组件必须成功，一个失败则整体失败
        }

        /// <summary>
        /// 执行模式设置
        /// </summary>
        [SerializeField] private ExecutionMode _executionMode = ExecutionMode.Sequential;
        public ExecutionMode Mode
        {
            get => _executionMode;
            set => _executionMode = value;
        }

        /// <summary>
        /// 子组件列表
        /// </summary>
        [SerializeField] private List<IActionComponent> _childComponents = new();
        public List<IActionComponent> ChildComponents => _childComponents;

        /// <summary>
        /// 当前执行的组件索引
        /// </summary>
        private int _currentComponentIndex = 0;
        public int CurrentComponentIndex => _currentComponentIndex;

        /// <summary>
        /// 是否在执行中
        /// </summary>
        private bool _isExecuting = false;
        public bool IsExecuting => _isExecuting;

        /// <summary>
        /// 初始化组件组合器
        /// </summary>
        /// <param name="gameObject">游戏对象引用</param>
        public void Initialize(GameObject gameObject)
        {
            _childComponents.Clear();
            _currentComponentIndex = 0;
            _isExecuting = false;

            // 尝试从游戏对象获取已添加的IActionComponent组件
            IActionComponent[] components = gameObject.GetComponents<IActionComponent>();
            foreach (var component in components)
            {
                // 避免添加自己作为子组件
                if (!ReferenceEquals(component, this))
                {
                    AddComponent(component);
                }
            }
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
        /// <returns>如果有子组件且游戏对象处于激活状态则返回true</returns>
        public bool CanExecute()
        {
            return gameObject.activeInHierarchy && _childComponents.Count > 0;
        }

        /// <summary>
        /// 执行组件组合器，根据执行模式执行子组件
        /// </summary>
        /// <returns>执行结果，根据执行模式和子组件执行情况决定</returns>
        public bool Execute()
        {
            if (!CanExecute())
                return false;

            _isExecuting = true;
            bool result = false;

            switch (_executionMode)
            {
                case ExecutionMode.Sequential:
                    result = ExecuteSequential();
                    break;
                case ExecutionMode.Parallel:
                    result = ExecuteParallel();
                    break;
                case ExecutionMode.Selective:
                    result = ExecuteSelective();
                    break;
                case ExecutionMode.AllSucceed:
                    result = ExecuteAllSucceed();
                    break;
            }

            _isExecuting = false;
            return result;
        }

        /// <summary>
        /// 更新组件状态
        /// </summary>
        public void Update()
        {
            // 如果处于顺序执行模式，且正在执行中，则继续执行
            if (_executionMode == ExecutionMode.Sequential && _isExecuting && _currentComponentIndex < _childComponents.Count)
            {
                bool componentResult = ExecuteComponent(_childComponents[_currentComponentIndex]);
                if (componentResult)
                {
                    // 如果当前组件执行成功，移动到下一个组件
                    _currentComponentIndex++;
                    
                    // 如果已执行完所有组件，重置索引
                    if (_currentComponentIndex >= _childComponents.Count)
                    {
                        _currentComponentIndex = 0;
                        _isExecuting = false;
                    }
                }
                else
                {
                    // 如果当前组件执行失败，重置索引和执行状态
                    _currentComponentIndex = 0;
                    _isExecuting = false;
                }
            }
        }

        /// <summary>
        /// 重置组件组合器状态
        /// </summary>
        public void Reset()
        {
            _currentComponentIndex = 0;
            _isExecuting = false;

            // 重置所有子组件
            foreach (var component in _childComponents)
            {
                component.Reset();
            }
        }

        /// <summary>
        /// 顺序执行所有子组件
        /// </summary>
        /// <returns>如果所有组件按顺序执行成功则返回true</returns>
        private bool ExecuteSequential()
        {
            // 重置索引
            _currentComponentIndex = 0;

            // 按顺序执行每个组件
            foreach (var component in _childComponents)
            {
                if (!ExecuteComponent(component))
                {
                    return false; // 任一组件失败，则整体失败
                }
            }

            return true; // 所有组件执行成功
        }

        /// <summary>
        /// 并行执行所有子组件
        /// </summary>
        /// <returns>综合结果，如果超过半数组件执行成功则返回true</returns>
        private bool ExecuteParallel()
        {
            if (_childComponents.Count == 0)
                return false;

            int successCount = 0;

            // 并行执行所有组件
            foreach (var component in _childComponents)
            {
                if (ExecuteComponent(component))
                {
                    successCount++;
                }
            }

            // 如果超过半数组件执行成功，则返回true
            return successCount >= (_childComponents.Count / 2f);
        }

        /// <summary>
        /// 选择性执行子组件，任一组件成功则整体成功
        /// </summary>
        /// <returns>如果至少一个组件执行成功则返回true</returns>
        private bool ExecuteSelective()
        {
            foreach (var component in _childComponents)
            {
                if (ExecuteComponent(component))
                {
                    return true; // 任一组件成功，则整体成功
                }
            }

            return false; // 所有组件执行失败
        }

        /// <summary>
        /// 所有组件必须执行成功
        /// </summary>
        /// <returns>如果所有组件执行成功则返回true</returns>
        private bool ExecuteAllSucceed()
        {  return _childComponents.All(component => ExecuteComponent(component));
        }

        /// <summary>
        /// 执行单个组件
        /// </summary>
        /// <param name="component">要执行的组件</param>
        /// <returns>组件执行结果</returns>
        private bool ExecuteComponent(IActionComponent component)
        {  if (component == null)
                return false;

            if (component.CanExecute())
            {
                return component.Execute();
            }

            return false;
        }

        /// <summary>
        /// 添加子组件
        /// </summary>
        /// <param name="component">要添加的组件</param>
        /// <returns>是否添加成功</returns>
        public bool AddComponent(IActionComponent component)
        {
            if (component == null || _childComponents.Contains(component))
                return false;

            _childComponents.Add(component);
            return true;
        }

        /// <summary>
        /// 移除子组件
        /// </summary>
        /// <param name="component">要移除的组件</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveComponent(IActionComponent component)
        {  if (component == null)
                return false;

            bool removed = _childComponents.Remove(component);
            
            // 如果移除的是当前执行的组件，重置索引
            if (removed && _currentComponentIndex >= _childComponents.Count)
            {
                _currentComponentIndex = Mathf.Max(0, _childComponents.Count - 1);
            }
            
            return removed;
        }

        /// <summary>
        /// 按索引移除子组件
        /// </summary>
        /// <param name="index">组件索引</param>
        /// <returns>是否移除成功</returns>
        public bool RemoveComponentAt(int index)
        {
            if (index < 0 || index >= _childComponents.Count)
                return false;

            _childComponents.RemoveAt(index);
            
            // 调整当前执行索引
            if (_currentComponentIndex > index)
            {
                _currentComponentIndex--;
            }
            else if (_currentComponentIndex >= _childComponents.Count)
            {
                _currentComponentIndex = Mathf.Max(0, _childComponents.Count - 1);
            }
            
            return true;
        }

        /// <summary>
        /// 插入组件到指定位置
        /// </summary>
        /// <param name="index">插入位置</param>
        /// <param name="component">要插入的组件</param>
        /// <returns>是否插入成功</returns>
        public bool InsertComponent(int index, IActionComponent component)
        {
            if (component == null || _childComponents.Contains(component) || index < 0 || index > _childComponents.Count)
                return false;

            _childComponents.Insert(index, component);
            
            // 调整当前执行索引
            if (_currentComponentIndex >= index)
            {
                _currentComponentIndex++;
            }
            
            return true;
        }

        /// <summary>
        /// 获取指定组件索引
        /// </summary>
        /// <param name="component">要查找的组件</param>
        /// <returns>组件索引，如果不存在则返回-1</returns>
        public int GetComponentIndex(IActionComponent component)
        {  if (component == null)
                return -1;

            return _childComponents.IndexOf(component);
        }

        /// <summary>
        /// 查找特定类型的组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>找到的第一个组件，如果不存在则返回null</returns>
        public T FindComponent<T>() where T : class, IActionComponent
        {  foreach (var component in _childComponents)
            {
                if (component is T tComponent)
                {
                    return tComponent;
                }
            }
            return null;
        }

        /// <summary>
        /// 查找所有特定类型的组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>找到的所有组件列表</returns>
        public List<T> FindComponents<T>() where T : class, IActionComponent
        {  List<T> result = new();
            foreach (var component in _childComponents)
            {
                if (component is T tComponent)
                {
                    result.Add(tComponent);
                }
            }
            return result;
        }

        /// <summary>
        /// 清空所有子组件
        /// </summary>
        public void ClearComponents()
        {
            _childComponents.Clear();
            _currentComponentIndex = 0;
            _isExecuting = false;
        }
    }
}