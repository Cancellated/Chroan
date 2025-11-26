
using UnityEngine;
using System.Collections.Generic;


namespace Logger
{
    public static class Log
    {
        // 日志级别控制
        public enum LogLevel { None, Error, Warning, Info, Debug }
        public static LogLevel currentLogLevel = LogLevel.Info;
        
        // 日志冷却控制相关
        private static readonly Dictionary<string, float> _logCooldowns = new();
        private const float DEFAULT_LOG_COOLDOWN = 1f;

        // 基础日志方法
        public static void Info(string module, string message, Object context = null)
        {
            if (currentLogLevel < LogLevel.Info) return;

            string formatted = $"[{module}] {message}";

            if (context != null)
                UnityEngine.Debug.Log(formatted, context);
            else
                UnityEngine.Debug.Log(formatted);
        }

        // 警告日志
        public static void Warning(string module, string message, Object context = null)

        {
            if (currentLogLevel < LogLevel.Warning) return;

            string formatted = $"[{module}] ⚠️ {message}";

            if (context != null)
                UnityEngine.Debug.LogWarning(formatted, context);
            else
                UnityEngine.Debug.LogWarning(formatted);
        }

        // 错误日志
        public static void Error(string module, string message, Object context = null)

        {
            if (currentLogLevel < LogLevel.Error) return;

            string formatted = $"[{module}] ❌ {message}";

            if (context != null)
                UnityEngine.Debug.LogError(formatted, context);
            else
                UnityEngine.Debug.LogError(formatted);
        }

        // 调试专用日志（只在开发版本显示）
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(string module, string message, Object context = null)

        {
            string formatted = $"[{module}] 🐞 {message}";

            if (context != null)
                UnityEngine.Debug.Log(formatted, context);
            else
                UnityEngine.Debug.Log(formatted);
        }
        
        /// <summary>
        /// 带冷却时间的日志输出方法，防止日志过于频繁
        /// </summary>
        /// <param name="logType">日志类型枚举</param>
        /// <param name="module">日志所属模块</param>
        /// <param name="message">日志内容</param>
        /// <param name="context">上下文对象</param>
        /// <param name="cooldownKey">冷却标识键，用于区分不同的日志冷却</param>
        /// <param name="cooldownTime">冷却时间（秒），默认使用DEFAULT_LOG_COOLDOWN</param>
        public static void LogWithCooldown(LogLevel logType, string module, string message, Object context = null, string cooldownKey = null, float cooldownTime = DEFAULT_LOG_COOLDOWN)
        {
            // 如果未指定冷却键，则使用模块+消息作为键
            if (string.IsNullOrEmpty(cooldownKey))
            {
                cooldownKey = $"{module}_{message}";
            }
            
            // 检查是否在冷却中
            if (IsOnCooldown(cooldownKey, cooldownTime))
            {
                return; // 在冷却中，不输出日志
            }
            
            // 根据日志类型输出相应级别的日志
            switch (logType)
            {
                case LogLevel.Debug:
                    Debug(module, message, context);
                    break;
                case LogLevel.Info:
                    Info(module, message, context);
                    break;
                case LogLevel.Warning:
                    Warning(module, message, context);
                    break;
                case LogLevel.Error:
                    Error(module, message, context);
                    break;
                default:
                    break;
            }
            
            // 更新冷却时间
            _logCooldowns[cooldownKey] = Time.realtimeSinceStartup;
        }
        
        /// <summary>
        /// 检查指定键是否处于冷却状态
        /// </summary>
        /// <param name="cooldownKey">冷却标识键</param>
        /// <param name="cooldownTime">冷却时间（秒）</param>
        /// <returns>如果在冷却中返回true，否则返回false</returns>
        private static bool IsOnCooldown(string cooldownKey, float cooldownTime)
        {
            if (_logCooldowns.TryGetValue(cooldownKey, out float lastLogTime))
            {
                // 检查是否超过冷却时间
                if (Time.realtimeSinceStartup - lastLogTime < cooldownTime)
                {
                    return true; // 仍在冷却中
                }
            }
            
            return false; // 不在冷却中或键不存在
        }
        
        /// <summary>
        /// 清除特定键的冷却状态
        /// </summary>
        /// <param name="cooldownKey">要清除的冷却键</param>
        public static void ClearCooldown(string cooldownKey)
        {
            if (_logCooldowns.ContainsKey(cooldownKey))
            {
                _logCooldowns.Remove(cooldownKey);
            }
        }
        
        /// <summary>
        /// 清除所有日志冷却状态
        /// </summary>
        public static void ClearAllCooldowns()
        {
            _logCooldowns.Clear();
        }
    }
}