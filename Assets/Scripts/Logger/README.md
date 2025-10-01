# Logger 模块使用文档

## 概述

Logger 是一个轻量级的日志系统，为 Unity 项目提供统一的日志记录功能。它支持不同级别的日志输出、模块分类和格式化输出，方便开发者在开发和调试过程中更好地追踪和记录应用程序的运行状态。

## 文件结构

```
Logger/
├── Log.cs              // 核心日志功能实现
├── LogModules.cs       // 日志模块常量定义
└── README.md           // 使用文档（当前文件）
```

## 日志级别

Logger 支持以下日志级别，按优先级从低到高排序：

| 级别 | 枚举值 | 描述 |
|------|-------|------|
| None | 0 | 不输出任何日志 |
| Error | 1 | 仅输出错误日志 |
| Warning | 2 | 输出警告和错误日志 |
| Info | 3 | 输出信息、警告和错误日志（默认） |
| Debug | 4 | 输出所有级别日志 |

### 设置日志级别

```csharp
// 设置为仅输出错误日志
Log.currentLogLevel = Log.LogLevel.Error;

// 设置为输出所有日志
Log.currentLogLevel = Log.LogLevel.Debug;
```

## 日志模块常量

`LogModules.cs` 定义了一系列常量，用于标识日志来源模块。这样可以方便地在日志中区分不同系统的输出。常用模块包括：

- 系统模块：SYSTEM, GAMEMANAGER, GAMEEVENTS, SCENE, UIMANAGER 等
- UI模块：UI
- 游戏数据模块：GAMEDATA, SAVE
- 调试模块：DEVTOOLS
- 游戏逻辑模块：PLAYER, AUDIO, INVENTORY

## 日志方法

### 信息日志 (Info)

用于记录一般信息，在日志级别为 Info 或更高时显示。

```csharp
Log.Info(LogModules.UI, "面板初始化完成");

// 带上下文对象的日志（可在 Unity 编辑器中点击日志跳转到对象）
Log.Info(LogModules.PLAYER, "玩家位置更新", gameObject);
```

输出格式：`[模块名] 消息内容`

### 警告日志 (Warning)

用于记录警告信息，在日志级别为 Warning 或更高时显示。警告日志会以黄色显示并带有警告图标。

```csharp
Log.Warning(LogModules.SAVE, "存档文件格式较旧，正在转换");

// 带上下文对象
Log.Warning(LogModules.INVENTORY, "物品数量超过上限", itemObject);
```

输出格式：`[模块名] ⚠️ 消息内容`

### 错误日志 (Error)

用于记录错误信息，在日志级别为 Error 或更高时显示。错误日志会以红色显示并带有错误图标。

```csharp
Log.Error(LogModules.SCENE, "无法加载场景：Level1");

// 带上下文对象
Log.Error(LogModules.AUDIO, "音频文件加载失败", audioSource);
```

输出格式：`[模块名] ❌ 消息内容`

### 彩色日志 (LogColor)

用于以自定义颜色显示日志，在日志级别为 Info 或更高时显示。

```csharp
// 使用 Unity 内置颜色
Log.LogColor(LogModules.DEVTOOLS, "调试信息：玩家状态正常", Color.green);

// 使用自定义颜色
Log.LogColor(LogModules.GAMEEVENTS, "事件触发：BossSpawned", new Color(1f, 0.5f, 0f));
```

### 调试日志 (DebugLog)

专门用于开发阶段的调试信息，仅在 Unity 编辑器或 Development Build 中显示，不影响发布版本的性能。

```csharp
// 调试日志只在编辑器和开发构建中显示
Log.DebugLog(LogModules.SYSTEM, "当前帧率：" + Time.deltaTime);

// 带上下文对象
Log.DebugLog(LogModules.PLAYER, "碰撞检测调试信息", collisionObject);
```

输出格式：`[模块名] 🐞 消息内容`

## 在代码中使用 Logger

### 1. 添加命名空间

```csharp
using Logger;
// 或者使用静态引用
using static Logger.LogModules;
```

### 2. 在类中定义日志模块

```csharp
public class PlayerController : MonoBehaviour
{
    private const string LOG_MODULE = LogModules.PLAYER;
    
    void Start()
    {
        Log.Info(module, "玩家控制器初始化完成");
    }
    
    void Update()
    {
        // 调试信息只在开发环境显示
        Log.DebugLog(module, "当前玩家位置：" + transform.position);
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Log.Warning(module, "玩家尝试退出游戏");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Log.Error(module, "玩家受到敌人攻击", gameObject);
        }
    }
}
```

## 最佳实践

1. **使用适当的日志级别**：根据信息的重要性选择合适的日志级别

2. **统一使用模块常量**：使用 `LogModules` 中定义的常量作为模块名，保持一致性

3. **添加足够的上下文**：尽可能提供详细的上下文信息，便于问题定位

4. **利用上下文对象**：在 Unity 中，传递 `UnityEngine.Object` 参数可以在编辑器中快速定位到相关对象

5. **调试日志分离**：使用 `DebugLog` 记录只在开发阶段需要的调试信息

6. **控制日志输出**：在发布版本中可以将日志级别设置为 `Error` 或 `Warning`，减少性能消耗

7. **彩色日志用于区分**：使用 `LogColor` 为不同类型的信息设置独特的颜色，便于快速识别

## 性能考虑

- `DebugLog` 方法使用了 `[System.Diagnostics.Conditional]` 特性，在非编辑器和非开发构建中会被编译器完全移除，不会产生任何性能开销
- 高频率调用日志方法可能会影响性能，尤其是在移动设备上
- 建议在发布版本中将日志级别设置为 `Error` 或 `Warning`

## 扩展 Logger

如果需要扩展 Logger 功能，可以考虑以下方式：

1. 在 `LogModules.cs` 中添加新的模块常量
2. 扩展 `Log.cs` 添加新的日志方法（如支持格式化字符串、时间戳等）
3. 实现自定义的日志处理器（如文件日志、网络日志等）