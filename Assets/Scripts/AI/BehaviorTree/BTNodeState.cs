using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.BehaviorTree
{
    /// <summary>
    /// 行为树节点执行状态
    /// </summary>
    public enum BTNodeState
    {
        /// <summary>
        /// 节点正在运行
        /// </summary>
        Running,
        
        /// <summary>
        /// 节点执行成功
        /// </summary>
        Success,
        
        /// <summary>
        /// 节点执行失败
        /// </summary>
        Failure
    }
}