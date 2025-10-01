# ToggleSwitch 组件使用说明

## 概述

ToggleSwitch 是一个自定义的UI组件，继承自Unity的Toggle组件，实现了传统的左右型开关样式。这个组件可以替代现有的Toggle组件，为游戏提供更美观、更符合现代UI设计的开关控件。

## 功能特点

- 左右滑动的开关样式
- 平滑的切换动画效果
- 可自定义的开关颜色
- 支持开关状态文本显示
- 完全兼容Unity的Toggle组件API

## 如何在项目中使用

### 方法一：直接替换现有Toggle组件

1. 在Unity编辑器中，找到 `m_invertYAxisToggle` 对应的GameObject
2. 移除现有的`Toggle`组件
3. 添加`ToggleSwitch`组件
4. 按照下面的配置说明设置组件属性

### 方法二：通过代码替换

在`SettingsPanelView.cs`文件中，我们可以修改代码来使用新的ToggleSwitch组件。下面是具体步骤：

1. 在文件顶部添加命名空间引用：
```csharp
using MyGame.UI.Components;
```

2. 将`m_invertYAxisToggle`的类型从`Toggle`改为`ToggleSwitch`：
```csharp
[Tooltip("Y轴反转开关")]
[SerializeField] private ToggleSwitch m_invertYAxisToggle;
```

3. 其他代码逻辑无需更改，因为ToggleSwitch继承自Toggle组件，完全兼容现有API

## 组件属性配置

在Unity编辑器中，为ToggleSwitch组件设置以下属性：

| 属性名 | 类型 | 说明 |
|--------|------|------|
| **Knob Rect Transform** | RectTransform | 开关滑块的RectTransform组件 |
| **On Color** | Color | 开关打开时的背景颜色 |
| **Off Color** | Color | 开关关闭时的背景颜色 |
| **Animation Speed** | float | 开关滑块的移动速度 |
| **On Text** | string | 开关打开时显示的文本（可选） |
| **Off Text** | string | 开关关闭时显示的文本（可选） |
| **Status Text** | Text | 显示开关状态的Text组件（可选） |

## 注意事项

1. 确保开关对象有一个Image组件作为背景
2. 滑块对象应该是开关对象的子对象
3. 如果不需要文本显示功能，可以将Status Text留空
4. 动画速度设置为0可以禁用平滑过渡效果

## 最佳实践

1. 为了保持UI风格一致，建议为所有开关使用相同的颜色和动画设置
2. 在移动平台上，建议适当增加开关的尺寸和点击区域，以提高触摸体验
3. 如果游戏支持多语言，确保On Text和Off Text也支持本地化