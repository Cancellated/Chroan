using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可推物体接口
/// 用于实现类似推箱子游戏中的箱子等可被玩家推动的物体
/// </summary>
public interface IPushable
{
    /// <summary>
    /// 尝试推动物体
    /// </summary>
    /// <param name="pushDirection">推动方向</param>
    /// <param name="pushDistance">推动距离（单位：网格）</param>
    /// <returns>如果推动成功返回true，否则返回false</returns>
    bool TryPush(Vector2 pushDirection, float pushDistance = 1f);
    
    /// <summary>
    /// 检查物体是否可以被推动到目标位置
    /// </summary>
    /// <param name="targetGridPosition">目标网格位置</param>
    /// <returns>如果目标位置可推送到返回true，否则返回false</returns>
    bool CanBePushedTo(Vector3Int targetGridPosition);
    
    /// <summary>
    /// 获取物体当前的网格位置
    /// </summary>
    /// <returns>物体的网格位置</returns>
    Vector3Int GetGridPosition();
}
