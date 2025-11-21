# AI行为组件系统文档

## 概述
本文档介绍了AI行为组件系统，该系统允许将AI行为拆分为可复用的组件，实现高度模块化和可扩展性。

## 组件结构

### 1. IActionComponent 接口
所有行为组件的基础接口，定义了组件的基本方法。

**主要方法：**
- `string ComponentName` - 获取组件名称
- `void Initialize()` - 初始化组件
- `bool CanExecute()` - 判断组件是否可以执行
- `void Execute()` - 执行组件行为
- `void Update()` - 更新组件状态
- `void Reset()` - 重置组件状态

**使用方法：**
创建新的行为组件时，需要实现此接口并提供上述方法的具体实现。

### 2. MovementComponent 组件
提供基础移动功能的组件。

**主要属性：**
- `MovementSpeed` - 移动速度
- `StoppingDistance` - 停止距离
- `IsMoving` - 是否正在移动

**主要方法：**
- `SetTargetPosition(Vector2 target)` - 设置目标位置
- `StopMovement()` - 停止移动
- `MoveTowardsTarget()` - 向目标位置移动

### 3. EscapeComponent 组件
实现逃离威胁源的行为组件。

**主要属性：**
- `SafetyDistance` - 逃离安全距离
- `EscapeSpeedMultiplier` - 逃离速度倍数
- `DetectionRange` - 威胁检测范围

**主要方法：**
- `SetThreatSource(Transform threat)` - 设置威胁源
- `CalculateEscapeDirection()` - 计算逃离方向
- `StopEscaping()` - 停止逃离行为

### 4. BehaviorComponentManager 管理器
管理多个行为组件的协调执行。

**主要方法：**
- `RegisterComponent(IActionComponent component)` - 注册行为组件
- `UnregisterComponent(string componentName)` - 注销行为组件
- `GetComponent<T>(string componentName)` - 获取指定类型的组件
- `ExecuteComponent(string componentName)` - 执行指定组件
- `SetComponentPriorities(Dictionary<string, int> componentPriorities)` - 设置组件执行优先级
- `ResetAllComponents()` - 重置所有组件

## 使用示例

### 示例1：基本使用方法

```csharp
using UnityEngine;
using AI.Behavior;

public class AIController : MonoBehaviour
{
    private BehaviorComponentManager _behaviorManager;
    private MovementComponent _movementComponent;
    private EscapeComponent _escapeComponent;
    
    private Transform _playerTransform;
    
    private void Awake()
    {
        // 获取或添加行为管理器
        _behaviorManager = GetComponent<BehaviorComponentManager>();
        if (_behaviorManager == null)
        {
            _behaviorManager = gameObject.AddComponent<BehaviorComponentManager>();
        }
        
        // 获取移动组件
        _movementComponent = GetComponent<MovementComponent>();
        if (_movementComponent == null)
        {
            _movementComponent = gameObject.AddComponent<MovementComponent>();
        }
        
        // 获取逃离组件
        _escapeComponent = GetComponent<EscapeComponent>();
        if (_escapeComponent == null)
        {
            _escapeComponent = gameObject.AddComponent<EscapeComponent>();
        }
        
        // 初始化管理器
        _behaviorManager.Initialize();
        
        // 查找玩家
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    private void Update()
    {
        // 检测玩家接近
        if (_playerTransform != null)
        {
            float distance = Vector2.Distance(transform.position, _playerTransform.position);
            
            // 如果玩家在威胁范围内，设置威胁源并执行逃离
            if (distance < 5f)
            {
                _escapeComponent.SetThreatSource(_playerTransform);
                _behaviorManager.ExecuteComponent("EscapeComponent");
            }
            // 否则可以执行其他行为
            else
            {
                // 例如随机移动
                if (!_movementComponent.IsMoving)
                {
                    Vector2 randomTarget = new Vector2(
                        transform.position.x + Random.Range(-3f, 3f),
                        transform.position.y + Random.Range(-3f, 3f)
                    );
                    _movementComponent.SetTargetPosition(randomTarget);
                    _behaviorManager.ExecuteComponent("MovementComponent");
                }
            }
        }
    }
}
```

### 示例2：设置组件优先级

```csharp
private void SetComponentPriorities()
{
    // 设置组件执行优先级（数字越小优先级越高）
    Dictionary<string, int> priorities = new Dictionary<string, int>()
    {
        { "EscapeComponent", 1 },      // 最高优先级：逃离行为最重要
        { "MovementComponent", 2 },    // 中等优先级：基本移动
        { "AttackComponent", 3 }       // 较低优先级：攻击行为
    };
    
    _behaviorManager.SetComponentPriorities(priorities);
}
```

### 示例3：自定义行为组件

```csharp
using UnityEngine;
using AI.Behavior;

public class PatrolComponent : MonoBehaviour, IActionComponent
{
    public string ComponentName { get { return "PatrolComponent"; } }
    
    private MovementComponent _movementComponent;
    private Vector2[] _patrolPoints;
    private int _currentPointIndex = 0;
    [SerializeField] private float _waitTimeAtPoint = 1f;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;
    
    public void Initialize()
    {
        _movementComponent = GetComponent<MovementComponent>();
        if (_movementComponent == null)
        {
            _movementComponent = gameObject.AddComponent<MovementComponent>();
        }
        
        // 初始化巡逻点
        InitializePatrolPoints();
    }
    
    public bool CanExecute()
    {
        return _movementComponent != null && _patrolPoints != null && _patrolPoints.Length > 0;
    }
    
    public void Execute()
    {
        if (!CanExecute())
            return;
        
        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0)
            {
                _isWaiting = false;
                MoveToNextPoint();
            }
        }
        else
        {
            // 检查是否到达当前巡逻点
            if (Vector2.Distance(transform.position, _patrolPoints[_currentPointIndex]) <= _movementComponent.StoppingDistance)
            {
                // 到达巡逻点，等待一段时间
                _isWaiting = true;
                _waitTimer = _waitTimeAtPoint;
            }
            else
            {
                // 继续移动到当前巡逻点
                _movementComponent.SetTargetPosition(_patrolPoints[_currentPointIndex]);
                _movementComponent.Execute();
            }
        }
    }
    
    public void Update()
    {
        if (CanExecute())
        {
            Execute();
        }
    }
    
    public void Reset()
    {
        _currentPointIndex = 0;
        _isWaiting = false;
        _waitTimer = 0f;
        
        if (_movementComponent != null)
        {
            _movementComponent.StopMovement();
        }
    }
    
    private void InitializePatrolPoints()
    {
        // 示例：创建围绕当前位置的巡逻点
        Vector2 center = transform.position;
        _patrolPoints = new Vector2[]
        {
            center + new Vector2(3, 0),
            center + new Vector2(0, 3),
            center + new Vector2(-3, 0),
            center + new Vector2(0, -3)
        };
    }
    
    private void MoveToNextPoint()
    {
        // 移动到下一个巡逻点
        _currentPointIndex = (_currentPointIndex + 1) % _patrolPoints.Length;
        _movementComponent.SetTargetPosition(_patrolPoints[_currentPointIndex]);
        _movementComponent.Execute();
    }
}
```

## 最佳实践

1. **组件分离**：每个组件应该只负责一种特定的行为，遵循单一职责原则。

2. **依赖注入**：组件之间的依赖应该通过引用传递，避免硬编码依赖。

3. **优先级管理**：合理设置组件执行优先级，确保重要行为（如逃离）优先执行。

4. **状态检查**：在执行组件前，始终检查`CanExecute()`以确保行为可以安全执行。

5. **事件驱动**：考虑使用事件系统来触发行为，而不是在Update中持续检查条件。

## 注意事项

- 确保所有组件都正确初始化后再使用
- 避免组件之间的循环依赖
- 对于复杂行为，可以组合多个基础组件来实现
- 在不需要时及时停止或重置组件，避免不必要的计算
