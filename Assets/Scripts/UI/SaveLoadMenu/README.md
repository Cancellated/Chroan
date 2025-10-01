# 存档菜单系统 (SaveLoadMenu)

## 系统概述

存档菜单系统是基于MVC架构实现的UI组件，用于管理游戏的存档、读档、删除存档和创建新游戏功能。该系统与项目的事件系统(GameEvents)和存档系统(SaveManager)紧密集成，提供了完整的游戏存档管理功能。

## 目录结构

```
Assets/Scripts/UI/SaveLoadMenu/
├── SaveLoadMenuConstants.cs      # 常量定义
├── SaveLoadMenuModel.cs          # 数据模型
├── SaveLoadMenuController.cs     # 控制器
├── SaveLoadMenuView.cs           # 视图基类
├── SaveLoadMenuPanel.cs          # 面板实现
├── SaveLoadMenuConfig.cs         # 配置文件
└── README.md                     # 文档说明
```

## 如何使用

### 1. 创建配置文件

1. 在Unity编辑器中，右键点击`Assets`文件夹，选择`Create > GameJamToolPack > UI > SaveLoadMenuConfig`
2. 根据需要调整配置文件中的参数

### 2. 创建UI面板

1. 创建一个新的GameObject作为存档菜单面板
2. 添加`SaveLoadMenuPanel`组件
3. 分配必要的UI组件（存档槽容器、各种按钮等）
4. 确保面板实现了`IUIPanel`接口并在UIManager中注册

### 3. 打开存档菜单

通过事件系统打开存档菜单：

```csharp
GameEvents.TriggerMenuShow(UIType.SaveLoadMenu, true);
```

### 4. 使用存档菜单功能

- 点击存档槽选择存档
- 使用底部按钮进行保存、加载、删除操作
- 点击"创建新游戏"按钮开始新游戏
- 点击"返回"按钮关闭菜单

## 代码结构说明

### 1. 常量定义 (SaveLoadMenuConstants)

定义了存档菜单使用的各种常量，如自动存档槽名称、默认存档槽数量和日期格式等。

### 2. 数据模型 (SaveLoadMenuModel)

继承自`ObservableModel`，管理存档菜单的数据状态，包括选中的存档槽、存档信息列表等。

### 3. 控制器 (SaveLoadMenuController)

继承自`BaseController`，处理用户交互和业务逻辑，如初始化存档槽、保存/加载/删除存档等操作。

### 4. 视图 (SaveLoadMenuView & SaveLoadMenuPanel)

- `SaveLoadMenuView`：定义了视图的基本接口和功能
- `SaveLoadMenuPanel`：具体的面板实现，负责UI显示和用户交互

## 事件系统集成

存档菜单系统与GameEvents事件系统紧密集成：

- 通过`GameEvents.OnMenuShow`事件控制菜单的显示和隐藏
- 通过`GameEvents.OnSaveGame`、`GameEvents.OnLoadGame`等事件与SaveManager交互

## 配置选项说明

`SaveLoadMenuConfig`配置文件包含以下主要选项：

### 存档配置
- `EnableAutoSave`：是否启用自动存档
- `AutoSaveInterval`：自动存档间隔（秒）
- `MaxAutoSaveCount`：最大自动存档数量
- `MaxManualSaveCount`：最大手动存档数量

### UI配置
- `SaveSlotPrefabAddress`：存档槽预制件的Addressable资源地址
- `MenuAnimationDuration`：菜单动画时长
- `SlotAnimationDuration`：存档槽动画时长

### 文本配置
- `NewGameConfirmText`：新建游戏确认文本
- `DeleteSaveConfirmText`：删除存档确认文本
- `LoadSaveConfirmText`：加载存档确认文本

### 音效配置
各种操作的音效配置

### 视觉反馈配置
不同状态存档槽的颜色配置

## 扩展指南

### 添加新功能

1. 在`SaveLoadMenuModel`中添加新的数据字段
2. 在`SaveLoadMenuController`中实现业务逻辑
3. 在`SaveLoadMenuView`或`SaveLoadMenuPanel`中添加UI组件和显示逻辑

### 自定义UI样式

1. 修改`SaveLoadMenuPanel`中的UI显示逻辑
2. 调整`SaveLoadMenuConfig`中的颜色配置
3. 替换存档槽预制件

## 注意事项

1. 确保`SaveManager`已经正确初始化
2. 确保`UIManager`已经正确注册了存档菜单面板
3. 在删除存档时，建议添加确认对话框以防止误操作
4. 在生产环境中，考虑添加存档加密功能以保护游戏数据

## 已知问题

- 目前没有实现存档预览图功能
- 没有实现存档自动备份功能
- 没有实现存档文件大小限制功能

## 更新日志

### v1.0
- 初始版本
- 实现基本的存档、读档、删除存档和创建新游戏功能
- 实现存档槽UI显示
- 与事件系统和存档系统集成