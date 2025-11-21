在实现日志输出方法时，使用项目中logger命名空间下的方法输出日志，它包含Info、DebugLog、Warning、Error方法，用于输出不同级别的日志信息，使用时需要的参数：
Info(string module, string message, Object context = null)
其中：
module：日志所属的模块，使用LogModules脚本中定义的枚举值。
message：日志的具体信息，描述发生的事件或问题。
context：可选参数，用于提供额外的上下文信息，例如对象引用、变量值等。

LogModules脚本定义了各个模块的枚举值，用于在日志输出时指定日志所属的模块，在需要定义新的大类模块时，需要在LogModules脚本中添加新的枚举值并使用它。

本项目使用的是unity中的input system，所有的输入事件都通过该系统处理,编写输入事件处理方法时，需要使用InputSystem类的方法来获取输入值而不是直接监听键位。
项目中多数视觉效果通过动画状态机实现，在编写脚本时不必通过脚本实现动画效果，而是通过动画状态机来控制。
游戏本体是基于tilemap的2D游戏，计算坐标时需要注意tilemap的坐标系统与世界坐标系统的差异。