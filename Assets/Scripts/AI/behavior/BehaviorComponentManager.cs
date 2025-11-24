using UnityEngine;
using System.Collections.Generic;
using Logger;
using AI.Behavior;

/// <summary>
/// 行为组件管理器
/// 负责管理和协调多个行为组件的执行
/// </summary>
public class BehaviorComponentManager : MonoBehaviour
{
    /// <summary>
    /// 组件字典，用于存储和快速访问已注册的行为组件
    /// </summary>
    private Dictionary<string, IActionComponent> _actionComponents = new();

    /// <summary>
    /// 组件执行优先级队列
    /// </summary>
    private List<IActionComponent> _prioritizedComponents = new();

    /// <summary>
    /// 初始化管理器
    /// </summary>
    private void Awake()
    {
        Initialize();
    }

    /// <summary>
    /// 初始化所有已注册的组件
    /// </summary>
    public void Initialize()
    {
        // 获取并初始化所有挂载在同一游戏对象上的IActionComponent组件
        IActionComponent[] components = GetComponents<IActionComponent>();
        foreach (var component in components)
        {
            RegisterComponent(component);
        }
    }

    /// <summary>
    /// 注册行为组件
    /// </summary>
    /// <param name="component">要注册的行为组件</param>
    public void RegisterComponent(IActionComponent component)
    {
        if (component == null)
        {
            Log.Warning(LogModules.AI, "BehaviorComponentManager: 尝试注册空组件", this);
            return;
        }

        string componentName = component.ComponentName;
        if (string.IsNullOrEmpty(componentName))
        {
            Log.Warning(LogModules.AI, "BehaviorComponentManager: 组件名称为空", this);
            return;
        }

        // 如果组件已存在，先移除旧组件
        if (_actionComponents.ContainsKey(componentName))
        {
            _actionComponents.Remove(componentName);
            _prioritizedComponents.Remove(component);
        }

        // 注册新组件
        _actionComponents.Add(componentName, component);
        _prioritizedComponents.Add(component);

        // 初始化组件，传入当前GameObject作为owner
        component.Initialize(gameObject);

        Log.Info(LogModules.AI, $"BehaviorComponentManager: 成功注册组件 '{componentName}'", this);
    }

    /// <summary>
    /// 注销行为组件
    /// </summary>
    /// <param name="componentName">要注销的组件名称</param>
    public void UnregisterComponent(string componentName)
    {
        if (string.IsNullOrEmpty(componentName))
            return;

        if (_actionComponents.TryGetValue(componentName, out IActionComponent component))
        {
            _actionComponents.Remove(componentName);
            _prioritizedComponents.Remove(component);
            
            Log.Info(LogModules.AI, $"BehaviorComponentManager: 成功注销组件 '{componentName}'", this);
        }
    }

    /// <summary>
    /// 获取行为组件
    /// </summary>
    /// <param name="componentName">组件名称</param>
    /// <returns>找到的组件，未找到则返回null</returns>
    public T GetComponent<T>(string componentName) where T : class, IActionComponent
    {
        if (_actionComponents.TryGetValue(componentName, out IActionComponent component))
        {
            return component as T;
        }
        return null;
    }

    /// <summary>
    /// 获取行为组件（通用版本）
    /// </summary>
    /// <param name="componentName">组件名称</param>
    /// <returns>找到的组件，未找到则返回null</returns>
    public new IActionComponent GetComponent(string componentName)
    {
        _actionComponents.TryGetValue(componentName, out IActionComponent component);
        return component;
    }

    /// <summary>
    /// 执行指定组件
    /// </summary>
    /// <param name="componentName">要执行的组件名称</param>
    /// <returns>执行是否成功</returns>
    public bool ExecuteComponent(string componentName)
    {
        IActionComponent component = GetComponent(componentName);
        if (component != null && component.CanExecute())
        {
            component.Execute();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 更新所有组件
    /// </summary>
    private void Update()
    {
        UpdateAllComponents();
    }

    /// <summary>
    /// 更新所有组件
    /// 实现串行执行流程：感知->决策->移动
    /// </summary>
    public void UpdateAllComponents()
    {
        // 第一阶段：更新感知组件
        foreach (var component in _prioritizedComponents)
        {
            if (IsPerceptionComponent(component))
            {
                component.Update();
            }
        }
        
        // 第二阶段：执行行为树（决策）
        // 行为树的执行由外部的BehaviorTreeExecutor处理
        // 这里只更新行为组件中的决策逻辑
        foreach (var component in _prioritizedComponents)
        {
            if (IsDecisionComponent(component) && !(component is PerceptionComponent) && !(component is MovementComponent))
            {
                component.Update();
            }
        }
        
        // 第三阶段：执行移动组件
        foreach (var component in _prioritizedComponents)
        {
            if (IsMovementComponent(component))
            {
                component.Update();
            }
        }
    }
    
    /// <summary>
    /// 判断组件是否为感知组件
    /// </summary>
    /// <param name="component">要判断的组件</param>
    /// <returns>如果是感知组件则返回true</returns>
    private bool IsPerceptionComponent(IActionComponent component)
    {
        return component is PerceptionComponent || 
               component.ComponentName.Contains("Perception");
    }
    
    /// <summary>
    /// 判断组件是否为决策组件
    /// </summary>
    /// <param name="component">要判断的组件</param>
    /// <returns>如果是决策组件则返回true</returns>
    private bool IsDecisionComponent(IActionComponent component)
    {
        return component.ComponentName.Contains("Decision") || 
               component.ComponentName.Contains("Escape") || 
               component.ComponentName.Contains("Behavior");
    }
    
    /// <summary>
    /// 判断组件是否为移动组件
    /// </summary>
    /// <param name="component">要判断的组件</param>
    /// <returns>如果是移动组件则返回true</returns>
    private bool IsMovementComponent(IActionComponent component)
    {
        return component is MovementComponent || 
               component.ComponentName.Contains("Movement") || 
               component.ComponentName.Contains("PositionControl") || 
               component.ComponentName.Contains("SpeedControl") || 
               component.ComponentName.Contains("ObstacleDetection");
    }

    /// <summary>
    /// 重置所有组件
    /// </summary>
    public void ResetAllComponents()
    {
        foreach (var component in _prioritizedComponents)
        {
            component.Reset();
        }
    }

    /// <summary>
    /// 设置组件执行优先级
    /// </summary>
    /// <param name="componentPriorities">组件名称和优先级的字典，优先级数字越小优先级越高</param>
    public void SetComponentPriorities(Dictionary<string, int> componentPriorities)
    {
        // 创建优先级排序的组件列表
        var prioritizedList = new List<KeyValuePair<int, IActionComponent>>();

        foreach (var component in _actionComponents.Values)
        {
            int priority = int.MaxValue; // 默认最低优先级
            if (componentPriorities.TryGetValue(component.ComponentName, out int p))
            {
                priority = p;
            }
            prioritizedList.Add(new KeyValuePair<int, IActionComponent>(priority, component));
        }

        // 按优先级排序
        prioritizedList.Sort((a, b) => a.Key.CompareTo(b.Key));

        // 更新优先级队列
        _prioritizedComponents.Clear();
        foreach (var item in prioritizedList)
        {
            _prioritizedComponents.Add(item.Value);
        }
    }

    /// <summary>
    /// 获取已注册组件的数量
    /// </summary>
    /// <returns>组件数量</returns>
    public int GetComponentCount()
    {
        return _actionComponents.Count;
    }
}
