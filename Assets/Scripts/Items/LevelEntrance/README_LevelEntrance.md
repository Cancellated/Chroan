# 关卡入口系统使用指南

## 概述

关卡入口系统提供了一个基于Tile大小的交互式关卡选择机制。玩家可以走到关卡入口Tile上，系统会自动检测并弹出确认界面，让玩家选择是否进入对应关卡。

## 系统组件

### 1. LevelEntrance (关卡入口组件)
- **位置**: `Assets/Scripts/Items/LevelEntrance/LevelEntrance.cs`
- **功能**: 关卡入口的核心交互组件
- **特性**: Tile大小、自动网格对齐、状态管理

### 2. LevelEntranceManager (关卡入口管理器)
- **位置**: `Assets/Scripts/Items/LevelEntrance/LevelEntranceManager.cs`
- **功能**: 统一管理所有关卡入口
- **特性**: 自动状态更新、调试工具

### 3. LevelConfirmationView (关卡确认界面)
- **位置**: `Assets/Scripts/UI/LevelConfirmation/View/LevelConfirmationView.cs`
- **功能**: 玩家确认进入关卡的界面
- **特性**: 淡入淡出动画、键盘支持

## 快速开始

### 步骤1: 创建关卡入口GameObject

1. 在Unity编辑器中创建一个空的GameObject
2. 添加 `LevelEntrance` 组件
3. 配置以下参数：

```csharp
// 必需配置
levelId = "level_001"        // 关卡唯一标识
levelName = "冰雪关卡"        // 关卡显示名称
scenePath = "IceLevelScene"   // 目标场景路径

// 贴图配置
unlockedSprite = [解锁贴图]   // 解锁状态贴图
lockedSprite = [锁定贴图]     // 未解锁状态贴图

// 交互配置
activationDelay = 0.3f        // 激活延迟时间
interactionHintPrefab = [提示UI预设] // 交互提示UI
```

### 步骤2: 设置关卡入口管理器

1. 在场景中创建一个空的GameObject
2. 添加 `LevelEntranceManager` 组件
3. 管理器会自动检测场景中的所有关卡入口

### 步骤3: 创建关卡确认界面（可选）

1. 创建UI Canvas
2. 添加 `LevelConfirmationView` 组件
3. 配置UI元素引用

## 详细配置说明

### LevelEntrance 配置参数

#### 关卡配置
- `levelId`: 关卡唯一标识，用于存档系统集成
- `levelName`: 关卡显示名称，会在提示界面显示
- `scenePath`: 目标场景路径，使用场景切换系统加载

#### Tile设置
- `tileSize`: Tile尺寸，默认(1,1)与游戏网格对齐
- `interactionOffset`: 碰撞体偏移，防止边缘检测问题

#### 贴图设置
- `unlockedSprite`: 解锁状态下的贴图
- `lockedSprite`: 未解锁状态下的贴图（灰暗显示）

#### 交互设置
- `activationDelay`: 激活延迟时间，防止误触
- `interactionHintPrefab`: 交互提示UI预设
- `hintVerticalOffset`: 提示UI的垂直偏移

### LevelEntranceManager 配置参数

- `enableAutoStateUpdate`: 是否启用自动状态更新
- `stateUpdateInterval`: 状态更新间隔（秒）

## 交互流程

### 正常流程
1. 玩家走到关卡入口Tile上
2. 系统检测玩家位置（中心检测）
3. 显示交互提示（"冰雪关卡\n进入"）
4. 玩家停留0.3秒后激活
5. 弹出关卡确认界面
6. 玩家选择"进入关卡"或"取消"
7. 根据选择执行相应操作

### 未解锁流程
1. 玩家走到未解锁的关卡入口
2. 显示交互提示（"冰雪关卡\n未解锁"）
3. 确认按钮显示为"未解锁"且不可点击

## 代码集成

### 手动设置关卡状态

```csharp
// 获取关卡入口管理器
var manager = LevelEntranceManager.Instance;

// 设置关卡解锁状态
manager.SetLevelUnlocked("level_001", true);

// 设置关卡完成状态
manager.SetLevelCompleted("level_001", false);

// 解锁所有关卡（测试用）
manager.UnlockAllLevels();
```

### 获取关卡信息

```csharp
// 获取特定关卡入口
var entrance = manager.GetLevelEntrance("level_001");

// 获取所有关卡入口
var allEntrances = manager.GetAllLevelEntrances();

// 获取统计信息
int unlockedCount = manager.GetUnlockedLevelCount();
int completedCount = manager.GetCompletedLevelCount();
```

### 调试工具

```csharp
// 打印所有关卡信息
manager.PrintAllEntranceInfo();
```

## 与存档系统集成

### 检查解锁状态

在 `LevelEntrance.CheckUnlockStatus()` 方法中集成存档系统：

```csharp
private void CheckUnlockStatus()
{
    // 集成存档系统
    if (SaveManager.Instance != null)
    {
        var saveData = SaveManager.Instance.GetCurrentSaveData();
        isUnlocked = saveData.gameProgress.unlockedLevels.Contains(levelId);
        isCompleted = saveData.gameProgress.completedLevels.Contains(levelId);
    }
}
```

### 关卡完成后的处理

在关卡完成后，更新关卡状态：

```csharp
// 关卡完成后调用
LevelEntranceManager.Instance.SetLevelCompleted(completedLevelId, true);

// 解锁下一个关卡
LevelEntranceManager.Instance.SetLevelUnlocked(nextLevelId, true);
```

## 最佳实践

### 1. 关卡ID命名规范
- 使用有意义的ID，如："level_001", "boss_level", "secret_level"
- 保持ID唯一性
- 避免使用特殊字符

### 2. 场景路径管理
- 使用场景名称而不是完整路径
- 确保场景在Build Settings中已添加

### 3. 贴图资源管理
- 为不同主题的关卡使用不同的贴图风格
- 保持贴图尺寸一致（推荐64x64或128x128）

### 4. 布局建议
- 将关卡入口布置在逻辑路径上
- 使用不同的区域表示不同的难度或主题
- 考虑玩家的移动路径和视觉引导

## 故障排除

### 常见问题

#### 1. 关卡入口不响应
- 检查碰撞体是否正确设置
- 确认玩家Tag为"Player"
- 检查LevelEntranceManager是否存在

#### 2. 贴图显示异常
- 确认Sprite导入设置正确
- 检查Sprite Renderer组件
- 验证贴图尺寸是否合适

#### 3. 确认界面不显示
- 检查LevelConfirmationView组件是否存在
- 确认Canvas渲染顺序
- 检查UI元素的Active状态

#### 4. 场景切换失败
- 确认场景路径正确
- 检查场景是否在Build Settings中
- 验证场景切换系统是否正常工作

## 扩展功能

### 自定义视觉效果
可以扩展 `PlayTileActivationEffect()` 方法添加更多视觉效果：

```csharp
private void PlayTileActivationEffect()
{
    // 现有的脉冲效果
    StartCoroutine(TilePulseEffect());
    
    // 添加粒子效果
    if (activationParticleSystem != null)
    {
        activationParticleSystem.Play();
    }
    
    // 添加音效
    AudioManager.PlaySFX("LevelEntranceActivate");
}
```

### 高级交互逻辑
可以重写交互检测逻辑实现更复杂的行为：

```csharp
protected virtual bool CanActivate()
{
    // 添加自定义条件
    return isUnlocked && 
           !isActivated && 
           HasRequiredItems() && 
           MeetsLevelRequirement();
}
```

## 版本历史

- v1.0.0: 初始版本，基础关卡入口功能
- 特性: Tile大小设计、状态管理、确认界面

## 技术支持

如遇到问题，请检查：
1. 组件配置是否正确
2. 依赖系统是否正常工作
3. 日志输出中的错误信息

如需进一步帮助，请参考项目的事件系统和UI框架文档。