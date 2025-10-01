# 场景切换系统使用指南

## 概述

SceneSwitcher 实现了一个基于事件的统一场景切换系统，允许不同模块通过事件机制请求和响应场景加载/卸载操作，实现系统间的松耦合。

## 核心功能

1. **事件驱动架构**：通过 GameEvents 实现模块间解耦通信
2. **统一的场景加载入口**：提供静态方法 `RequestLoadScene` 作为统一的场景加载入口
3. **异步加载支持**：提供异步场景加载功能，避免游戏卡顿
4. **事件通知**：在场景加载过程中触发相关事件，让其他系统可以做出响应

## 如何使用

### 1. 加载场景

任何系统/组件都可以通过以下方式请求加载场景：

```csharp
// 方式1：使用静态方法直接请求加载场景（推荐）
SceneSwitcher.RequestLoadScene("GameScene");

// 方式2：直接触发GameEvents事件
GameEvents.TriggerSceneLoadStart("GameScene");
```

### 2. 响应场景切换事件

系统中的其他组件可以通过注册 GameEvents 事件来响应场景切换：

```csharp
private void OnEnable()
{
    // 注册场景加载开始事件
    GameEvents.OnSceneLoadStart += OnSceneLoadStart;
    
    // 注册场景加载完成事件
    GameEvents.OnSceneLoadComplete += OnSceneLoadComplete;
    
    // 注册场景卸载事件
    GameEvents.OnSceneUnload += OnSceneUnload;
}

private void OnDisable()
{
    // 注销所有事件监听
    GameEvents.OnSceneLoadStart -= OnSceneLoadStart;
    GameEvents.OnSceneLoadComplete -= OnSceneLoadComplete;
    GameEvents.OnSceneUnload -= OnSceneUnload;
}

private void OnSceneLoadStart(string sceneName)
{
    // 处理场景加载开始逻辑
    Debug.Log($"开始加载场景: {sceneName}");
}

private void OnSceneLoadComplete(string sceneName)
{
    // 处理场景加载完成逻辑
    Debug.Log($"场景加载完成: {sceneName}");
}

private void OnSceneUnload(string sceneName)
{
    // 处理场景卸载逻辑
    Debug.Log($"卸载场景: {sceneName}");
}
```

## 特别说明

1. 系统使用 `SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)` 进行异步加载，然后通过 `SceneManager.SetActiveScene` 设置活动场景
2. 默认情况下，加载新场景时会卸载当前活动场景，可通过参数控制此行为
3. 为了避免事件循环，异步加载过程中不再重复触发 `OnSceneLoadStart` 事件
4. 同步加载场景方法 `LoadScene` 会阻塞主线程，不建议在游戏运行时使用

## 最佳实践

1. 所有场景切换操作都应该通过 `SceneSwitcher.RequestLoadScene` 方法进行，而不是直接调用 SceneManager
2. 确保在适当的生命周期方法中（如 OnEnable/OnDisable）正确注册和注销事件监听
3. 在场景加载完成后进行必要的初始化工作
4. 在场景卸载前保存必要的数据和状态

## 与 MainMenuManager 集成

MainMenuManager 或其他 UI 管理器只需要调用 `SceneSwitcher.RequestLoadScene` 方法即可触发场景切换，不需要关心具体的加载实现。

```csharp
// 在 MainMenuManager 中
public void OnStartGameButtonClick()
{
    // 请求加载游戏场景
    SceneSwitcher.RequestLoadScene("GameScene");
}
```