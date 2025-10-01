# 🎮 Control 模块文档

## 📋 模块概述

Control模块负责处理玩家输入和游戏控制逻辑，基于Unity Input System实现跨平台输入处理。

## 🎯 核心功能

### 1. 玩家控制器 (PlayerController)
- **命名空间**: `MyGame.Control`
- **继承**: `Singleton<PlayerController>`（单例模式）
- **依赖**: Unity Input System (`GameControl`)

### 2. 输入系统架构
基于Unity Input System的新输入系统，支持：
- 键盘鼠标输入
- 游戏手柄输入
- 移动端触摸输入
- 可配置的输入绑定

## 🕹️ 输入映射

### 基础控制（默认键位）
| 按键/操作       | 功能描述       | 对应方法                    |
|----------------|----------------|---------------------------|
| WASD / 左摇杆  | 角色移动       | `PlayerMove()`            |
| 空格键 / A键   | 跳跃           | `PlayerJump()`            |
| F键 / X键      | 交互/拾取      | `PlayerInteract()`        |
| 鼠标左键       | 攻击           | `PlayerAttack()`          |

### 输入绑定配置
输入绑定可在以下位置修改：
- 文件路径: `Assets/Settings/Input System/GameControl.inputsettings`
- 编辑器路径: `Edit > Project Settings > Input System`

## 📝 使用示例

### 基础用法
```csharp
// 获取玩家控制器实例
PlayerController playerController = PlayerController.Instance;

// 读取移动输入
Vector2 moveInput = playerController.PlayerMove();

// 检测跳跃
if (playerController.PlayerJump())
{
    // 执行跳跃逻辑
    Jump();
}

// 检测交互
if (playerController.PlayerInteract())
{
    // 执行交互逻辑
    InteractWithObject();
}

// 检测攻击
if (playerController.PlayerAttack())
{
    // 执行攻击逻辑
    Attack();
}
```

### 事件驱动使用
```csharp
// 订阅输入事件
private void OnEnable()
{
    PlayerController.Instance.InputActions.GamePlay.Jump.performed += OnJumpPerformed;
}

private void OnJumpPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
{
    // 跳跃逻辑
}
```

## 🔧 API 参考

### PlayerController 类

#### 公共属性
- `GameControl InputActions` - 获取输入系统实例

#### 公共方法
- `Vector2 PlayerMove()` - 获取移动输入向量
- `bool PlayerJump()` - 检测跳跃输入触发
- `bool PlayerInteract()` - 检测交互输入触发
- `bool PlayerAttack()` - 检测攻击输入触发

## 🔄 集成指南

### 1. 添加到场景
1. 创建空GameObject命名为"PlayerController"
2. 添加`PlayerController`脚本组件
3. 确保场景中只有一个PlayerController实例

### 2. 自定义输入
1. 打开`GameControl.inputactions`文件
2. 添加新的Action Maps或Actions
3. 在PlayerController中添加对应的方法

### 3. 扩展功能
```csharp
// 添加新的输入检测
public bool PlayerDash()
{
    return _inputActions.GamePlay.Dash.triggered;
}
```

## 🎮 平台适配

### 支持平台
- ✅ PC (键盘鼠标)
- ✅ 游戏手柄 (Xbox/PlayStation/Switch)

### 平台特定配置
不同平台的输入绑定可以在Input System设置中单独配置。

## 📝 TODO列表
- [ ] 实现角色移动逻辑
- [ ] 实现跳跃系统
- [ ] 实现交互系统
- [ ] 实现攻击系统
- [ ] 添加移动设备触摸控制
