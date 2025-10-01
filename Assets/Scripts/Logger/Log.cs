
using UnityEngine;

namespace Logger
{
    public static class Log
    {
        // 日志级别控制
        public enum LogLevel { None, Error, Warning, Info, Debug }
        public static LogLevel currentLogLevel = LogLevel.Info;

        // 基础日志方法
        public static void Info(string module, string message, UnityEngine.Object context = null)
        {
            if (currentLogLevel < LogLevel.Info) return;

            string formatted = $"[{module}] {message}";

            if (context != null)
                Debug.Log(formatted, context);
            else
                Debug.Log(formatted);
        }

        // 警告日志
        public static void Warning(string module, string message, UnityEngine.Object context = null)

        {
            if (currentLogLevel < LogLevel.Warning) return;

            string formatted = $"[{module}] ⚠️ {message}";

            if (context != null)
                Debug.LogWarning(formatted, context);
            else
                Debug.LogWarning(formatted);
        }

        // 错误日志
        public static void Error(string module, string message, UnityEngine.Object context = null)

        {
            if (currentLogLevel < LogLevel.Error) return;

            string formatted = $"[{module}] ❌ {message}";

            if (context != null)
                Debug.LogError(formatted, context);
            else
                Debug.LogError(formatted);
        }

        // 带颜色的日志
        public static void LogColor(string module, string message, Color color, UnityEngine.Object context = null)

        {
            if (currentLogLevel < LogLevel.Info) return;

            string hexColor = ColorUtility.ToHtmlStringRGBA(color);
            string formatted = $"<color=#{hexColor}>[{module}] {message}</color>";

            if (context != null)
                Debug.Log(formatted, context);
            else
                Debug.Log(formatted);
        }

        // 调试专用日志（只在开发版本显示）
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        public static void DebugLog(string module, string message, UnityEngine.Object context = null)

        {
            string formatted = $"[{module}] 🐞 {message}";

            if (context != null)
                Debug.Log(formatted, context);
            else
                Debug.Log(formatted);
        }
    }
}