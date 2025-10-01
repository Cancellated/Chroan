# ToggleSwitch组件结构图

## 组件层次结构

```
ToggleSwitchRoot (GameObject)
├── Image (背景组件) - 必须
│   └── [Sprite] - 背景精灵
├── ToggleSwitch (自定义组件) - 必须
│   ├── [Knob Rect Transform] - 滑块RectTransform引用
│   ├── [On Color] - 打开状态颜色
│   ├── [Off Color] - 关闭状态颜色
│   ├── [Animation Speed] - 动画速度
│   ├── [On Text] - 打开状态文本
│   ├── [Off Text] - 关闭状态文本
│   └── [Status Text] - 状态文本组件引用
├── Knob (GameObject) - 必须 (作为子对象)
│   └── Image (滑块图形) - 必须
│       └── [Sprite] - 滑块精灵
└── StatusText (GameObject) - 可选 (作为子对象)
    └── Text (状态文本) - 可选
        ├── [Text] - 显示"开"/"关"
        ├── [Font] - 字体
        └── [Color] - 文本颜色
```

## 组件关系说明

1. **ToggleSwitchRoot**
   - 作为开关的根GameObject，包含所有其他组件
   - 必须添加Image组件作为背景
   - 必须添加ToggleSwitch自定义组件

2. **ToggleSwitch组件**
   - 继承自Unity的Toggle组件
   - 通过`[RequireComponent(typeof(Image))]`强制要求背景Image组件
   - 需要引用滑块的RectTransform来控制其位置
   - 可选引用Text组件来显示开关状态文本

3. **滑块对象**
   - 必须是ToggleSwitchRoot的子对象
   - 必须添加Image组件来显示滑块图形
   - RectTransform需要被赋值给ToggleSwitch组件的Knob Rect Transform属性

4. **状态文本对象** (可选)
   - 是ToggleSwitchRoot的子对象
   - 包含Text组件来显示"开"/"关"等状态文本
   - Text组件需要被赋值给ToggleSwitch组件的Status Text属性

## 属性配置说明

| 属性名 | 类型 | 说明 | 是否必须 |
|--------|------|------|---------|
| **背景Image** | Image | 显示开关的背景，颜色会根据开关状态变化 | 必须 |
| **Knob Rect Transform** | RectTransform | 开关滑块的RectTransform组件，用于控制滑块位置 | 必须 |
| **On Color** | Color | 开关打开时的背景颜色 | 必须 |
| **Off Color** | Color | 开关关闭时的背景颜色 | 必须 |
| **Animation Speed** | float | 开关滑块的移动速度，设置为0可禁用动画 | 必须 |
| **On Text** | string | 开关打开时显示的文本内容 | 可选 |
| **Off Text** | string | 开关关闭时显示的文本内容 | 可选 |
| **Status Text** | Text | 显示开关状态的Text组件 | 可选 |

## 工作原理

1. **初始化流程**
   - ToggleSwitch组件在Start()方法中初始化
   - 获取背景Image组件
   - 计算并存储滑块的起始位置和结束位置
   - 根据初始状态更新视觉效果
   - 注册值变化事件监听器

2. **状态变化处理**
   - 当开关状态改变时，触发OnToggleValueChanged方法
   - UpdateSwitchVisuals方法更新背景颜色、文本内容和滑块位置
   - 如果启用了动画，Update()方法会处理滑块的平滑移动

通过以上结构和配置，ToggleSwitch组件可以实现一个美观、功能完整的左右型开关控件。