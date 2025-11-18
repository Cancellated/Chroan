using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 共享规则瓦片类，允许不同的Rule Tile互相识别连接
/// 解决多种墙体贴图无法正确连接的问题
/// </summary>
[CreateAssetMenu(fileName = "New SharedRuleTile", menuName = "2D/Tiles/Shared Rule Tile")]
public class SharedRuleTile : RuleTile<SharedRuleTile.Neighbor> {
    /// <summary>
    /// 可识别的兼容Rule Tile列表
    /// </summary>
    [Tooltip("可识别的兼容Rule Tile列表")]
    public List<RuleTile> compatibleTiles = new();
    
    /// <summary>
    /// 此瓦片使用的物理材质
    /// 当WallColliderManager启用时会被应用到对应碰撞体
    /// </summary>
    [Tooltip("此瓦片使用的物理材质")]
    public PhysicsMaterial2D physicsMaterial;
    
    /// <summary>
    /// 是否使用瓦片自身的物理材质覆盖瓦片地图的物理材质
    /// </summary>
    [Tooltip("是否使用瓦片自身的物理材质覆盖瓦片地图的物理材质")]
    public bool overrideMapPhysicsMaterial = false;

    /// <summary>
    /// 自定义邻居类型枚举
    /// </summary>
    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int Null = 3;
        public const int NotNull = 4;
        public const int Compatible = 5; // 兼容的Rule Tile
    }

    /// <summary>
    /// 重写规则匹配方法，增加对兼容Rule Tile的识别
    /// </summary>
    /// <param name="neighbor">邻居类型</param>
    /// <param name="tile">瓦片对象</param>
    /// <returns>是否匹配规则</returns>
    public override bool RuleMatch(int neighbor, TileBase tile) {
        // 检查是否是兼容的Rule Tile（包括自身）
        bool isCompatible = false;
        if (tile == this) {
            isCompatible = true;
        } else if (tile is RuleTile otherRuleTile) {
            isCompatible = compatibleTiles.Contains(otherRuleTile);
        }
        
        // 1. 处理This规则：判断有此类型或者兼容类型的tile
        if (neighbor == Neighbor.This) {
            // 当规则要求This时，接受自身或兼容的tile
            return isCompatible;
        }
        
        // 2. 处理NotThis规则：判断不存在此类型或者兼容类型的tile
        if (neighbor == Neighbor.NotThis) {
            // 当规则要求NotThis时，拒绝自身或兼容的tile（接受空或非兼容的tile）
            return !isCompatible;
        }
        
        // 3. 处理自定义邻居类型
        switch (neighbor) {
            case Neighbor.Null:
                // 自定义的Null类型，表示该位置必须为空
                return tile == null;
            case Neighbor.NotNull:
                // 自定义的NotNull类型，表示该位置必须有任意tile
                return tile != null;
            case Neighbor.Compatible:
                // 自定义的Compatible类型，检查是否是兼容的tile
                return isCompatible;
        }
        
        // 4. 处理基础规则：
        // neighbor=0 表示不关心该方向的tile，始终返回true
        // neighbor=1 表示该方向必须有匹配的tile（必须是自身或兼容tile）
        // neighbor=2 表示该方向必须不是自身或兼容tile
        switch (neighbor) {
            case 0:
                // Don't Care - 不关心该方向的tile
                return true;
            case 1:
                // This - 该方向必须是自身或兼容tile
                return isCompatible;
            case 2:
                // Not This - 该方向必须不是自身或兼容tile
                return !isCompatible;
            default:
                // 如果遇到未处理的neighbor值，调用基类方法
                return base.RuleMatch(neighbor, tile);
        }
    }
}