# MVC架构与UIManager及事件系统协作时序图

下面是MVC架构、UIManager和事件系统之间的详细协作时序图，展示了从用户交互到UI更新的完整流程：

```mermaid
sequenceDiagram
    title MVC架构与UIManager及事件系统协作流程

    participant System as 系统启动
    participant GameManager
    participant GameEvents
    participant UIManager
    participant User
    participant UIController
    participant BaseView
    participant BaseController
    participant BaseModel
    participant OtherSystems

    %% 初始化阶段
    System ->> GameManager: 初始化游戏管理器
    GameManager ->> GameEvents: 注册游戏状态事件监听
    GameManager ->> UIManager: 初始化UI管理器
    UIManager ->> GameEvents: 注册UI事件监听
    UIManager ->> UIManager: 预加载常用UI面板

    %% UI显示流程
    User ->> UIController: 点击按钮/触发输入
    UIController ->> GameEvents: 触发UI显示事件
    GameEvents ->> UIManager: 通知显示UI面板
    UIManager ->> UIManager: 检查面板是否存在
    UIManager ->> BaseView: 实例化/获取UI面板
    BaseView ->> BaseView: Awake() - 初始化组件
    BaseView ->> BaseView: TryBindController() - 尝试绑定控制器
    UIManager ->> BaseController: 创建/获取控制器实例
    UIManager ->> BaseView: 调用BindController()
    BaseView ->> BaseView: OnControllerBound() - 控制器绑定回调
    BaseController ->> BaseModel: 创建/初始化模型实例
    BaseModel ->> BaseModel: OnInitialize() - 模型数据初始化
    BaseController ->> BaseView: 初始化视图显示
    BaseView ->> BaseView: Show() - 显示UI面板

    %% 用户交互处理
    User ->> BaseView: 与UI交互
    BaseView ->> BaseController: 调用控制器方法
    BaseController ->> BaseModel: 更新模型数据
    BaseModel ->> BaseModel: 处理数据变更
    BaseModel ->> GameEvents: 触发数据变更事件
    GameEvents ->> BaseController: 通知控制器数据已变更
    BaseController ->> BaseView: 调用UpdateView()
    BaseView ->> BaseView: 更新UI显示

    %% UI隐藏流程
    User ->> UIController: 触发隐藏UI操作
    UIController ->> GameEvents: 触发UI隐藏事件
    GameEvents ->> UIManager: 通知隐藏UI面板
    UIManager ->> BaseView: 调用Hide()
    BaseView ->> BaseView: 隐藏UI面板
    BaseView ->> BaseController: 通知控制器UI隐藏
    BaseController ->> BaseModel: 清理模型资源
    BaseModel ->> BaseModel: OnCleanup() - 模型资源清理
    UIManager ->> UIManager: 面板池化/销毁

    %% 全局事件流
    GameManager ->> GameEvents: 触发游戏状态事件
    GameEvents ->> BaseController: 控制器监听并响应
    BaseController ->> BaseView: 更新UI状态
    BaseModel ->> GameEvents: 触发业务数据事件
    GameEvents ->> GameManager: 游戏逻辑响应
    GameEvents ->> OtherSystems: 其他系统响应
```

## 时序图说明

这个时序图展示了以下几个核心流程：

### 1. 初始化阶段
- 游戏启动时，GameManager和UIManager依次初始化
- 各管理器向GameEvents注册事件监听器
- UIManager预加载常用UI面板以提高性能

### 2. UI显示流程
- 用户交互触发UIController调用GameEvents显示UI
- UIManager接收到事件后实例化或从缓存获取UI面板
- UI面板初始化并尝试绑定对应的控制器
- 控制器创建并初始化模型，完成后更新视图显示

### 3. 用户交互处理
- 用户与UI交互时，视图将操作转发给控制器
- 控制器处理业务逻辑并更新模型数据
- 模型数据变更后触发事件通知相关系统
- 控制器监听模型事件并更新视图显示

### 4. UI隐藏流程
- 用户触发隐藏UI操作，通过事件系统通知UIManager
- UIManager调用面板的Hide方法并进行资源管理
- 控制器和模型执行必要的清理工作

### 5. 全局事件流
- GameManager和其他系统通过GameEvents发送全局事件
- 控制器监听相关事件并更新UI状态
- 模型数据变更也通过事件系统通知全局

这种基于事件的MVC架构实现了各组件的松耦合协作，使代码更易于维护和扩展，特别适合复杂的游戏UI系统。