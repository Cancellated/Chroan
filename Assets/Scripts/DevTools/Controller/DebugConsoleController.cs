using UnityEngine;
using Logger;

namespace MyGame.DevTool
{
    /// <summary>
    /// 调试控制台控制器，连接模型和视图，处理用户输入和命令执行
    /// </summary>
    public class DebugConsoleController : MonoBehaviour
    {
        private const string LOG_MODULE = LogModules.DEBUGCONSOLE;
        
        // 注入的依赖
        public DebugConsole view;
        private DebugCommandModel model;
        
        /// <summary>
        /// 设置控制器的视图引用
        /// </summary>
        /// <param name="consoleView">调试控制台视图实例</param>
        public void SetView(DebugConsole consoleView)
        {
            view = consoleView;
        }
        
        // 初始化
        private void Awake()
        {
            // 创建命令模型实例
            model = new DebugCommandModel();
            model.InitializeCommands();
            
            // 绑定视图事件
            if (view != null)
            {
                // 在现有系统中，需要手动处理命令提交
                // 这里通过监听OnCommandEntered的调用来实现控制器的功能
            }
            else
            {
                Log.Error(LOG_MODULE, "未找到DebugConsoleView组件");
            }
        }
        
        /// <summary>
        /// 处理用户输入的命令
        /// </summary>
        /// <param name="commandText">命令文本</param>
        public void HandleCommand(string commandText)
        {
            if (model.ExecuteCommand(commandText))
            {
                // 命令执行成功
                Log.Info(LOG_MODULE, "执行命令: " + commandText);
            }
            else if (!string.IsNullOrEmpty(commandText))
            {
                // 命令不存在
                if (view != null)
                {
                    view.Print("未知命令，输入 help 查看可用命令。");
                }
                Log.Warning(LOG_MODULE, "未知命令: " + commandText);
            }
        }
        
        /// <summary>
        /// 输出信息到控制台
        /// </summary>
        /// <param name="message">消息内容</param>
        public void PrintToConsole(string message)
        {
            if (view != null)
            {
                view.Print(message);
            }
        }
    }
}