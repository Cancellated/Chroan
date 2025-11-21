# Unity 行为树系统 (Unity Behavior Tree System)

## 概述 (Overview)

这是一个完整的Unity行为树实现，提供了灵活的AI行为建模和执行框架。该系统采用模块化设计，支持复杂的AI决策和行为逻辑。

## 核心特性 (Features)

- **模块化设计**：节点、组合节点、装饰器节点和叶子节点相互独立，易于扩展
- **状态管理**：完整的节点状态系统（Success、Failure、Running）
- **运行时执行**：高性能的行为树执行器和管理器
- **事件系统**：支持行为树状态变化事件
- **灵活组合**：支持Selector、Sequence、Parallel等多种组合策略
- **装饰器模式**：通过装饰器节点修改子节点行为
- **丰富示例**：提供完整的AI行为示例代码

## 目录结构 (Directory Structure)

```
Assets/Scripts/AI/BehaviorTree/
├── Node.cs                    # 基础节点和状态定义
├── CompositeNodes.cs          # 组合节点（Selector、Sequence、Parallel）
├── DecoratorNodes.cs          # 装饰器节点（Inverter、Succeeder等）
├── LeafNodes.cs              # 叶子节点（Action、Condition等）
├── BehaviorTree.cs           # 行为树执行器和管理器
└── BehaviorTreeExamples.cs   # 使用示例
```

## 核心类说明 (Core Classes)

### 1. 节点状态 (Node State)

```csharp
public enum BTNodeState
{
    Success,    // 执行成功
    Failure,    // 执行失败
    Running     // 执行中
}
```

### 2. 基础节点类 (Base Node Class)

```csharp
public abstract class BTNode
{
    public string NodeName { get; set; }
    public abstract BTNodeState Execute();
    public virtual void Reset() { }
    public virtual void AddChild(BTNode child) { }
}
```

### 3. 组合节点 (Composite Nodes)

#### 选择器节点 (BTSelector)
- 从左到右依次执行子节点
- 任何一个子节点返回Success即停止执行
- 所有子节点都返回Failure才返回Failure

#### 序列节点 (BTSequence)
- 从左到右依次执行子节点
- 任何一个子节点返回Failure即停止执行
- 所有子节点都返回Success才返回Success

#### 并行节点 (BTParallel)
- 同时执行所有子节点
- 可配置成功和失败条件
- 支持最小成功数量、最小失败数量等条件

### 4. 装饰器节点 (Decorator Nodes)

#### 反转器 (BTInverter)
- 将子节点的结果反转（Success → Failure，Failure → Success）

#### 成功器 (BTSucceeder)
- 无论子节点结果如何，都返回Success

#### 失败器 (BTFailer)
- 无论子节点结果如何，都返回Failure

#### 重复器 (BTRepeater)
- 重复执行子节点指定次数
- 支持无限重复

#### 延迟器 (BTDelay)
- 在执行子节点前等待指定时间

### 5. 叶子节点 (Leaf Nodes)

#### 动作节点 (BTActionNode)
- 执行具体的业务逻辑
- 返回执行结果

#### 条件节点 (BTConditionNode)
- 检查特定条件
- 只返回Success或Failure

#### 组件操作节点 (BTComponentActionNode)
- 封装常见的组件操作
- 支持Transform、Rigidbody等组件

#### 等待节点 (BTWaitNode)
- 等待指定时间

#### 日志节点 (BTLogNode)
- 记录调试信息

#### 随机选择器 (BTRandomSelector)
- 随机执行子节点之一

## 使用指南 (Usage Guide)

### 1. 创建行为树

```csharp
// 创建根节点
var root = new BTSelector("Root");

// 创建序列节点
var sequence = new BTSequence("ActionSequence");

// 创建条件节点
var condition = new BTConditionNode(
    () => Vector3.Distance(transform.position, target.position) < 5f,
    "DistanceCheck");

// 创建动作节点
var action = new BTActionNode(
    () => {
        Log.Info("执行攻击！", LogModules.AI);
        return BTNodeState.Success;
    },
    "Attack");

// 构建行为树
sequence.AddChild(condition);
sequence.AddChild(action);
root.AddChild(sequence);
```

### 2. 注册行为树

```csharp
// 获取行为树管理器
var btManager = FindObjectOfType<BehaviorTreeManager>();

// 注册行为树
var executor = btManager.RegisterTree(
    "EnemyAI_001",  // 名称
    root,           // 根节点
    0.1f,          // 更新间隔
    true           // 自动开始
);
```

### 3. 在MonoBehaviour中使用

```csharp
public class EnemyController : MonoBehaviour
{
    private BehaviorTreeExecutor behaviorTree;

    void Start()
    {
        // 创建行为树
        var root = CreateBehaviorTree();
        
        // 注册行为树
        var btManager = FindObjectOfType<BehaviorTreeManager>();
        behaviorTree = btManager.RegisterTree(
            $"EnemyAI_{gameObject.GetInstanceID()}", 
            root, 0.1f, true);
    }

    BTNode CreateBehaviorTree()
    {
        // 在这里创建你的行为树结构
        // 详见示例代码
    }

    void OnDestroy()
    {
        // 清理资源
        if (behaviorTree != null)
        {
            behaviorTree.Stop();
        }
    }
}
```

## 高级用法 (Advanced Usage)

### 1. 使用装饰器修改行为

```csharp
// 创建重复攻击动作
var attackAction = new BTActionNode(/* ... */);
var repeatDecorator = new BTRepeater(3); // 重复3次
repeatDecorator.SetChild(attackAction);

// 创建条件反转
var conditionNode = new BTConditionNode(/* ... */);
var inverterDecorator = new BTInverter();
inverterDecorator.SetChild(conditionNode);
```

### 2. 复杂组合逻辑

```csharp
// 并行执行多个任务
var parallelNode = new BTParallel(ParallelPolicy.RequireOne);
parallelNode.AddChild(moveAction);
parallelNode.AddChild(attackCondition);
parallelNode.AddChild(healthCheck);

// 嵌套选择器和序列
var complexSequence = new BTSequence("ComplexSequence");
complexSequence.AddChild(parallelNode);
complexSequence.AddChild(selectionNode);
```

### 3. 事件监听

```csharp
var executor = btManager.RegisterTree(/* ... */);
executor.OnTreeStateChanged += (sender, args) =>
{
    Log.Info($"行为树状态变化: {args.OldState} -> {args.NewState}", LogModules.AI);
};

executor.OnNodeStateChanged += (sender, args) =>
{
    Log.Info($"节点 {args.NodeName} 状态变化: {args.OldState} -> {args.NewState}", LogModules.AI);
};
```

## 最佳实践 (Best Practices)

### 1. 节点命名规范
- 使用描述性的节点名称，便于调试
- 例如：`"PlayerDetected"`, `"MoveToTarget"`, `"AttackCooldown"`

### 2. 性能优化
- 避免在每帧执行的节点中进行复杂计算
- 使用条件节点进行预检查
- 合理设置行为树的更新间隔

### 3. 调试技巧
- 使用BTLogNode记录关键状态
- 利用事件系统监听行为树执行状态
- 在Unity编辑器中可视化行为树结构

### 4. 错误处理
- 始终检查返回的节点状态
- 在动作节点中处理异常
- 使用Try-Catch包装可能失败的操作

## 示例场景 (Example Scenarios)

### 1. 敌人AI
- 巡逻：随机移动到指定点
- 发现玩家：转向并追击
- 攻击范围：执行攻击动作
- 状态管理：HP管理、冷却控制

### 2. 游戏角色
- 状态机：待机、移动、攻击、受伤等状态
- 资源管理：体力、法力、道具使用
- 目标选择：自动选择最优目标

### 3. NPC行为
- 任务系统：接受、完成、交付任务
- 社交行为：与玩家交互、对话选择
- 经济系统：购买、销售、交易

## 扩展指南 (Extension Guide)

### 1. 自定义节点
继承BTNode基类创建自定义节点：

```csharp
public class CustomNode : BTNode
{
    public CustomNode(string name) : base(name) { }

    public override BTNodeState Execute()
    {
        // 实现你的逻辑
        return BTNodeState.Success;
    }
}
```

### 2. 自定义装饰器
继承BTDecorator创建自定义装饰器：

```csharp
public class CustomDecorator : BTDecorator
{
    public CustomDecorator(string name) : base(name) { }

    public override BTNodeState Execute()
    {
        // 在子节点执行前后的逻辑
        BTNodeState result = Child.Execute();
        
        // 修改或处理结果
        return ModifyResult(result);
    }
}
```

### 3. 自定义叶子节点
继承BTLeafNode创建自定义叶子节点：

```csharp
public class CustomLeafNode : BTLeafNode
{
    public CustomLeafNode(string name) : base(name) { }

    public override BTNodeState Execute()
    {
        // 实现叶子节点的逻辑
        return BTNodeState.Success;
    }
}
```

## 常见问题 (FAQ)

### Q: 如何处理行为树循环？
A: 使用BTRepeater装饰器控制重复次数，或在条件节点中检查循环条件。

### Q: 行为树性能如何？
A: 系统采用高效的执行模式，可以处理数千个并发行为树实例。

### Q: 如何调试行为树？
A: 使用事件系统监听状态变化，添加日志节点记录关键执行步骤。

### Q: 能否在运行时修改行为树？
A: 是的，可以动态添加或移除节点，但需要注意线程安全。

### Q: 如何保存行为树状态？
A: 行为树状态存储在执行器中，可以序列化保存和恢复。

## 更新日志 (Changelog)

### v1.0.0 (当前版本)
- 完整的行为树核心系统
- 基础节点类型实现
- 执行器和管理器
- 丰富的示例代码

## 贡献指南 (Contributing)

欢迎提交Issue和Pull Request来改进这个项目！

## 许可证 (License)

本项目采用MIT许可证，详见LICENSE文件。