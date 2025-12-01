using System.Collections.Generic;
using UnityEngine;

namespace AI.behavior.EventDriven
{
    /// <summary>
    /// AI事件类型常量定义
    /// 用于定义AI行为系统中使用的各类事件名称
    /// </summary>
    public static class AIEventTypes
    {
        #region 感知相关事件

        /// <summary>
        /// 感知到威胁事件
        /// 参数: [GameObject self, GameObject threatSource]
        /// </summary>
        public const string PerceptionThreatDetected = "Perception.ThreatDetected";

        /// <summary>
        /// 感知到噪音事件
        /// 参数: [GameObject self, Vector3 noisePosition, float noiseIntensity]
        /// </summary>
        public const string PerceptionNoiseHeard = "Perception.NoiseHeard";

        /// <summary>
        /// 感知到视觉刺激事件
        /// 参数: [GameObject self, GameObject visualStimulus, Vector3 stimulusPosition]
        /// </summary>
        public const string PerceptionVisualStimulus = "Perception.VisualStimulus";

        #endregion

        #region 行为相关事件

        /// <summary>
        /// 行为开始事件
        /// 参数: [GameObject self, string behaviorName]
        /// </summary>
        public const string BehaviorStarted = "Behavior.Started";

        /// <summary>
        /// 行为完成事件
        /// 参数: [GameObject self, string behaviorName, bool success]
        /// </summary>
        public const string BehaviorCompleted = "Behavior.Completed";

        /// <summary>
        /// 行为中断事件
        /// 参数: [GameObject self, string behaviorName, string reason]
        /// </summary>
        public const string BehaviorInterrupted = "Behavior.Interrupted";

        #endregion

        #region 行为树节点事件

        /// <summary>
        /// 节点执行开始事件
        /// 参数: [BTNode node, string nodeName]
        /// </summary>
        public const string NodeExecutionStarted = "Node.ExecutionStarted";

        /// <summary>
        /// 节点执行完成事件
        /// 参数: [BTNode node, string nodeName, BTNodeState result]
        /// </summary>
        public const string NodeExecutionCompleted = "Node.ExecutionCompleted";

        /// <summary>
        /// 节点状态改变事件
        /// 参数: [BTNode node, string nodeName, BTNodeState oldState, BTNodeState newState]
        /// </summary>
        public const string NodeStateChanged = "Node.StateChanged";

        #endregion

        #region 移动相关事件

        /// <summary>
        /// 开始移动事件
        /// 参数: [GameObject self, Vector3 targetPosition]
        /// </summary>
        public const string MovementStarted = "Movement.Started";

        /// <summary>
        /// 移动完成事件
        /// 参数: [GameObject self, Vector3 targetPosition, bool reached]
        /// </summary>
        public const string MovementCompleted = "Movement.Completed";

        /// <summary>
        /// 移动被阻挡事件
        /// 参数: [GameObject self, GameObject obstacle]
        /// </summary>
        public const string MovementBlocked = "Movement.Blocked";

        #endregion

        #region 实用方法

        /// <summary>
        /// 获取所有已定义的事件类型
        /// 用于调试和枚举所有可能的事件
        /// </summary>
        /// <returns>事件类型名称列表</returns>
        public static List<string> GetAllEventTypes()
        {
            return new List<string>
            {
                // 感知相关
                PerceptionThreatDetected,
                PerceptionNoiseHeard,
                PerceptionVisualStimulus,
                
                // 行为相关
                BehaviorStarted,
                BehaviorCompleted,
                BehaviorInterrupted,
                
                // 节点相关
                NodeExecutionStarted,
                NodeExecutionCompleted,
                NodeStateChanged,
                
                // 移动相关
                MovementStarted,
                MovementCompleted,
                MovementBlocked,
            };
        }

        /// <summary>
        /// 检查事件名称是否有效
        /// </summary>
        /// <param name="eventName">要检查的事件名称</param>
        /// <returns>是否为有效事件名称</returns>
        public static bool IsValidEventType(string eventName)
        {
            return GetAllEventTypes().Contains(eventName);
        }

        #endregion
    }
}
