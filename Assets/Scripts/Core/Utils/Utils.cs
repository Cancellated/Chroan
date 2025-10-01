using System;
using System.Collections;
using System.Collections.Generic;
using MyGame.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyGame.Utils
{
    /// <summary>
    /// 系统工具类，提供常用的系统级工具方法
    /// </summary>
    public static class Utils
    {
        #region 数学工具

        /// <summary>
        /// 平滑插值
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="target">目标值</param>
        /// <param name="smoothTime">平滑时间</param>
        /// <returns>插值结果</returns>
        public static float SmoothDamp(float current, float target, float smoothTime)
        {
            return Mathf.Lerp(current, target, 1f - Mathf.Exp(-Time.deltaTime / smoothTime));
        }

        /// <summary>
        /// 角度标准化到0-360度
        /// </summary>
        /// <param name="angle">角度值</param>
        /// <returns>标准化后的角度</returns>
        public static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0) angle += 360f;
            return angle;
        }

        /// <summary>
        /// 角度差计算（考虑最短路径）
        /// </summary>
        /// <param name="from">起始角度</param>
        /// <param name="to">目标角度</param>
        /// <returns>角度差</returns>
        public static float AngleDifference(float from, float to)
        {
            float diff = (to - from + 180f) % 360f - 180f;
            return diff < -180f ? diff + 360f : diff;
        }

        #endregion

        #region 集合工具

        /// <summary>
        /// 安全地获取列表元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="list">列表</param>
        /// <param name="index">索引</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>元素或默认值</returns>
        public static T SafeGet<T>(this IList<T> list, int index, T defaultValue = default)
        {
            return list != null && index >= 0 && index < list.Count ? list[index] : defaultValue;
        }

        /// <summary>
        /// 随机获取列表元素
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="list">列表</param>
        /// <returns>随机元素</returns>
        public static T RandomElement<T>(this IList<T> list)
        {
            return list != null && list.Count > 0 ? list[UnityEngine.Random.Range(0, list.Count)] : default;
        }

        /// <summary>
        /// 打乱列表顺序
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="list">列表</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            if (list == null || list.Count <= 1) return;
            
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        #endregion

        #region 字符串工具

        /// <summary>
        /// 格式化时间显示
        /// </summary>
        /// <param name="seconds">秒数</param>
        /// <returns>格式化时间字符串</returns>
        public static string FormatTime(float seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.Hours > 0 
                ? string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds)
                : string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
        }

        /// <summary>
        /// 截断字符串
        /// </summary>
        /// <param name="text">原始字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <param name="suffix">后缀</param>
        /// <returns>截断后的字符串</returns>
        public static string Truncate(this string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
            
            return text[..(maxLength - suffix.Length)] + suffix;
        }

        #endregion

        #region 调试工具

        /// <summary>
        /// 绘制调试边界框
        /// </summary>
        /// <param name="bounds">边界框</param>
        /// <param name="color">颜色</param>
        /// <param name="duration">持续时间</param>
        public static void DrawBounds(Bounds bounds, Color color, float duration = 0.1f)
        {
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;
            
            // 绘制边界框的8个顶点
            Vector3[] corners = new Vector3[8];
            corners[0] = center + new Vector3(-size.x, -size.y, -size.z) * 0.5f;
            corners[1] = center + new Vector3(size.x, -size.y, -size.z) * 0.5f;
            corners[2] = center + new Vector3(size.x, -size.y, size.z) * 0.5f;
            corners[3] = center + new Vector3(-size.x, -size.y, size.z) * 0.5f;
            corners[4] = center + new Vector3(-size.x, size.y, -size.z) * 0.5f;
            corners[5] = center + new Vector3(size.x, size.y, -size.z) * 0.5f;
            corners[6] = center + new Vector3(size.x, size.y, size.z) * 0.5f;
            corners[7] = center + new Vector3(-size.x, size.y, size.z) * 0.5f;
            
            // 绘制12条边
            Debug.DrawLine(corners[0], corners[1], color, duration);
            Debug.DrawLine(corners[1], corners[2], color, duration);
            Debug.DrawLine(corners[2], corners[3], color, duration);
            Debug.DrawLine(corners[3], corners[0], color, duration);
            
            Debug.DrawLine(corners[4], corners[5], color, duration);
            Debug.DrawLine(corners[5], corners[6], color, duration);
            Debug.DrawLine(corners[6], corners[7], color, duration);
            Debug.DrawLine(corners[7], corners[4], color, duration);
            
            Debug.DrawLine(corners[0], corners[4], color, duration);
            Debug.DrawLine(corners[1], corners[5], color, duration);
            Debug.DrawLine(corners[2], corners[6], color, duration);
            Debug.DrawLine(corners[3], corners[7], color, duration);
        }

        #endregion
    }
}