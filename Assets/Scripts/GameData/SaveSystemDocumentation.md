# 游戏存档系统文档

## 1. 系统概述

本存档系统提供了一个完整的游戏数据保存和加载解决方案，支持游戏进度、设置和元数据的统一管理。系统采用模块化设计，包含数据结构定义、存档接口、实现类和管理器，能够轻松集成到Unity游戏项目中。

## 2. 系统架构

存档系统由以下几个核心组件组成：

- **数据结构**：定义了游戏需要保存的各类数据
- **存档接口**：定义了存档系统的通用功能
- **存档实现**：提供了具体的存储机制
- **存档管理器**：作为游戏代码与存档系统之间的桥梁
- **事件系统**：用于存档操作的通知与响应


## 3. 核心组件详解

### 3.1 数据结构

数据结构定义了游戏中需要保存的数据内容，主要包括：

#### SaveData.cs
```csharp
[Serializable]
public class SaveData
{
    public GameProgress Progress; // 游戏进度数据
    public GameSettings Settings; // 游戏设置数据
    public string Timestamp;      // 存档时间戳
    public string Version;        // 游戏版本号
}
```

#### GameProgress.cs
包含玩家位置、当前关卡、任务状态、经验值等游戏进度数据。

#### GameSettings.cs
包含音量、画质、分辨率等游戏设置数据。

### 3.2 存档接口

```csharp
public interface ISaveSystem
{
    bool SaveGame(SaveData data, string saveName);
    Task<bool> SaveGameAsync(SaveData data, string saveName);
    SaveData LoadGame(string saveName);
    Task<SaveData> LoadGameAsync(string saveName);
    bool DeleteSave(string saveName);
    Task<bool> DeleteSaveAsync(string saveName);
    bool DoesSaveExist(string saveName);
    List<string> GetAvailableSaves();
}
```

### 3.3 存档实现

系统提供了基于JSON文件的存档实现：

#### JsonSaveSystem.cs
- 使用Unity的持久化数据路径进行存储
- 通过JsonUtility进行数据序列化和反序列化
- 支持同步和异步操作
- 包含错误处理和日志记录

### 3.4 存档管理器

```csharp
public class SaveManager : Singleton<SaveManager>
```

SaveManager是一个单例类，作为游戏代码与存档系统之间的桥梁，提供了便捷的API来管理游戏存档。

### 3.5 事件系统

#### SaveEvents.cs
定义了存档相关的事件，如保存完成、加载完成、删除完成等，用于游戏各系统间的通信。

## 4. 使用指南

### 4.1 初始化

存档系统在首次访问时会自动初始化，无需额外操作：

```csharp
// 访问SaveManager的实例会自动初始化存档系统
SaveManager.Instance; // 这会创建SaveManager的实例并初始化内部的JsonSaveSystem
```

### 4.2 创建新游戏

```csharp
// 创建一个新的游戏存档数据
SaveManager.Instance.NewGame();
```

### 4.3 保存游戏

```csharp
// 同步保存游戏
bool success = SaveManager.Instance.SaveCurrentGame("SaveSlot1");

// 异步保存游戏
await SaveManager.Instance.SaveCurrentGameAsync("SaveSlot1");
```

### 4.4 加载游戏

```csharp
// 同步加载游戏
bool success = SaveManager.Instance.LoadGame("SaveSlot1");

// 异步加载游戏
await SaveManager.Instance.LoadGameAsync("SaveSlot1");
```

### 4.5 删除游戏

```csharp
// 同步删除游戏
bool success = SaveManager.Instance.DeleteSave("SaveSlot1");

// 异步删除游戏
await SaveManager.Instance.DeleteSaveAsync("SaveSlot1");
```

### 4.6 检查存档存在性

```csharp
// 检查指定存档是否存在
bool exists = SaveManager.Instance.DoesSaveExist("SaveSlot1");
```

### 4.7 获取可用存档列表

```csharp
// 获取所有可用的存档
List<string> saves = SaveManager.Instance.AvailableSaves;
```

### 4.8 修改当前游戏数据

```csharp
// 获取当前游戏数据
SaveData currentData = SaveManager.Instance.CurrentSaveData;

// 修改玩家位置
currentData.Progress.PlayerPosition = new Vector3(10, 0, 10);

// 修改当前关卡
currentData.Progress.CurrentLevel = "Level2";

// 修改音量设置
currentData.Settings.MasterVolume = 0.8f;

// 保存修改
SaveManager.Instance.SaveCurrentGame("SaveSlot1");
```

## 5. 事件监听

您可以通过SaveEvents监听存档相关的事件：

```csharp
private void OnEnable()
{
    // 监听存档保存完成事件
    SaveEvents.SaveCompleted += OnSaveCompleted;
    
    // 监听存档加载完成事件
    SaveEvents.LoadCompleted += OnLoadCompleted;
    
    // 监听存档删除完成事件
    SaveEvents.DeleteCompleted += OnDeleteCompleted;
    
    // 监听存档操作失败事件
    SaveEvents.OperationFailed += OnOperationFailed;
}

private void OnDisable()
{
    // 取消所有事件监听
    SaveEvents.SaveCompleted -= OnSaveCompleted;
    SaveEvents.LoadCompleted -= OnLoadCompleted;
    SaveEvents.DeleteCompleted -= OnDeleteCompleted;
    SaveEvents.OperationFailed -= OnOperationFailed;
}

private void OnSaveCompleted(string saveName)
{
    Debug.Log("游戏保存成功: " + saveName);
}

private void OnLoadCompleted(string saveName)
{
    Debug.Log("游戏加载成功: " + saveName);
    // 在这里可以处理加载完成后的逻辑，如更新UI、场景等
}

private void OnDeleteCompleted(string saveName)
{
    Debug.Log("游戏删除成功: " + saveName);
}

private void OnOperationFailed(string operation, string message)
{
    Debug.LogError("存档操作失败 (" + operation + "): " + message);
}
```

## 6. 测试工具

系统提供了一个测试脚本`SaveSystemTester.cs`，可以直接添加到游戏对象上进行存档系统功能测试。

测试工具提供以下功能：
- 新游戏测试
- 保存游戏测试
- 加载游戏测试
- 删除游戏测试
- 检查存档存在性
- 获取所有存档
- 完整测试流程

## 7. 扩展指南

### 7.1 添加新的存档数据

如果需要在存档中添加新的数据类型，可以按照以下步骤操作：

1. 在适当的地方创建新的可序列化数据结构
2. 在SaveData类中添加对应的字段
3. 在GameProgress或GameSettings类中添加相关字段和方法

### 7.2 实现新的存档系统

如果需要使用不同的存储方式（如加密存储、云存储等），可以创建新的ISaveSystem实现：

```csharp
public class EncryptedSaveSystem : ISaveSystem
{
    // 实现ISaveSystem接口中的所有方法
    // ...
}
```

然后在SaveManager中切换实现：

```csharp
// 在SaveManager类中修改
private ISaveSystem m_saveSystem = new EncryptedSaveSystem();
```

## 8. 最佳实践

1. **定期自动保存**：可以在游戏关键节点或定时自动保存游戏进度

   ```csharp
   // 每5分钟自动保存
   private float m_autoSaveTimer = 0f;
   private const float AUTO_SAVE_INTERVAL = 300f; // 5分钟
   
   private void Update()
   {
       m_autoSaveTimer += Time.deltaTime;
       if (m_autoSaveTimer >= AUTO_SAVE_INTERVAL)
       {
           m_autoSaveTimer = 0f;
           if (SaveManager.Instance.CurrentSaveData != null)
           {
               SaveManager.Instance.SaveCurrentGame("AutoSave");
           }
       }
   }
   ```

2. **保存前确认**：在覆盖重要存档前，最好先询问玩家

3. **错误处理**：存档操作可能会失败，确保进行适当的错误处理

4. **避免频繁保存**：过于频繁的保存可能会影响游戏性能

5. **版本控制**：在SaveData中保留版本号，以便未来数据结构变更时进行迁移

## 9. 常见问题解答

**Q: 存档文件保存在哪里？**
A: 在Unity中，存档文件默认保存在`Application.persistentDataPath`目录下的`Saves`文件夹中。

**Q: 如何修改存档文件的存储位置？**
A: 可以修改JsonSaveSystem.cs中的`m_saveDirectory`字段来更改存储位置。

**Q: 如何加密存档文件？**
A: 可以实现一个新的ISaveSystem接口，在保存和加载时对数据进行加密和解密处理。

**Q: 存档系统支持跨平台吗？**
A: 是的，存档系统使用Unity的`Application.persistentDataPath`，该路径在不同平台上会指向适合的持久化存储位置。

## 10. 版本历史

**版本 1.0**
- 实现了基本的存档系统架构
- 提供了JSON文件存储的实现
- 支持游戏进度和设置的保存和加载
- 包含事件系统和测试工具