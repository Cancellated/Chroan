using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Logger;

namespace MyGame.DevTool
{
    /// <summary>
    /// 调试命令模型，负责管理所有调试命令的注册、查找和执行
    /// </summary>
    public class DebugCommandModel
    {
        private const string LOG_MODULE = LogModules.DEBUGCONSOLE;
        // 命令字典：键为命令名称，值为(执行方法, 描述)
        private readonly Dictionary<string, (Action action, string description)> _commands = new();
        
        /// <summary>
        /// 初始化所有命令
        /// </summary>
        public void InitializeCommands()
        {
            // 使用反射获取DebugCommands静态类中所有标记了DebugCommand特性的方法
            foreach (var method in typeof(DebugCommands).GetMethods(
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attr = method.GetCustomAttribute<DebugCommand>();
                if (attr != null)
                {
                    // 将方法添加到命令字典
                    _commands[attr.CommandName] = (
                        () => method.Invoke(null, null),
                        attr.Description
                    );
                }
            }
            
            Log.Info(LOG_MODULE, "调试命令初始化完成，共加载了" + _commands.Count + "个命令");
        }
        
        /// <summary>
        /// 执行指定的命令
        /// </summary>
        /// <param name="commandName">命令名称</param>
        /// <returns>是否执行成功</returns>
        public bool ExecuteCommand(string commandName)
        {
            if (_commands.TryGetValue(commandName.ToLower().Trim(), out var command))
            {
                try
                {
                    command.action();
                    return true;
                }
                catch (Exception e)
                {
                    Log.Error(LOG_MODULE, "执行命令" + commandName + "时出错: " + e.Message);
                    return false;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 获取所有命令及其描述
        /// </summary>
        /// <returns>命令名称和描述的字典</returns>
        public Dictionary<string, string> GetAllCommands()
        {
            Dictionary<string, string> commandDescriptions = new();
            foreach (var cmd in _commands)
            {
                commandDescriptions[cmd.Key] = cmd.Value.description;
            }
            return commandDescriptions;
        }
        
        /// <summary>
        /// 检查命令是否存在
        /// </summary>
        /// <param name="commandName">命令名称</param>
        /// <returns>命令是否存在</returns>
        public bool CommandExists(string commandName)
        {
            return _commands.ContainsKey(commandName.ToLower().Trim());
        }
    }
}