using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Logger;
using System.Linq;

namespace AI.Behavior
{
    /// <summary>
    /// 方向选择器工具类
    /// 提供通用的方向评估和选择功能，可用于逃离、追击等多种行为
    /// </summary>
    public static class DirectionSelector
    {
        // 四个基本方向向量
        private static readonly Vector2[] BasicDirections = new Vector2[]
        {
            Vector2.up,    // 上
            Vector2.down,  // 下
            Vector2.left,  // 左
            Vector2.right  // 右
        };

        /// <summary>
        /// 选择最佳方向
        /// 基于目标方向和可通行性评估，选择最佳移动方向
        /// </summary>
        /// <param name="currentPosition">当前世界坐标位置</param>
        /// <param name="targetDirection">目标方向向量（如远离威胁或朝向目标）</param>
        /// <param name="groundTilemap">地面Tilemap</param>
        /// <param name="walkableCheckFunc">可通行性检查函数</param>
        /// <param name="sortByHighestWeight">是否按最高权重排序（true用于逃离，false用于追击）</param>
        /// <returns>最佳移动方向向量，如果没有有效方向则返回Vector2.zero</returns>
        public static Vector2 SelectBestDirection(Vector3 currentPosition, Vector2 targetDirection, 
                                                 Tilemap groundTilemap, System.Func<Vector2Int, bool> walkableCheckFunc,
                                                 bool sortByHighestWeight = true)
        {
            // 获取当前位置的网格坐标
            Vector3Int currentGridPos = TilemapHelper.WorldToGridPosition(currentPosition, groundTilemap);
            
            // 存储可通行方向及其权重
            Dictionary<Vector2, float> validDirections = new();
            
            // 检查每个方向的通行性并计算权重
            for (int i = 0; i < BasicDirections.Length; i++)
            {
                // 计算目标网格位置
                Vector2Int targetGridPos = new(
                    currentGridPos.x + Mathf.RoundToInt(BasicDirections[i].x),
                    currentGridPos.y + Mathf.RoundToInt(BasicDirections[i].y)
                );
                
                // 检查该位置是否可通行
                if (walkableCheckFunc(targetGridPos))
                {
                    // 计算该方向的权重
                    float weight = CalculateDirectionWeight(BasicDirections[i], targetDirection);
                    validDirections.Add(BasicDirections[i], weight);
                }
            }
            
            // 如果没有可通行方向，返回零向量
            if (validDirections.Count == 0)
            {
                return Vector2.zero;
            }
            
            // 找出最佳方向
            KeyValuePair<Vector2, float> bestDirection = validDirections.First();
            
            foreach (var dir in validDirections)
            {
                if (sortByHighestWeight)
                {
                    // 逃离逻辑：选择权重最大的方向
                    if (dir.Value > bestDirection.Value)
                    {
                        bestDirection = dir;
                    }
                }
                else
                {
                    // 追击逻辑：选择权重最小的方向
                    if (dir.Value < bestDirection.Value)
                    {
                        bestDirection = dir;
                    }
                }
            }
            
            return bestDirection.Key;
        }

        /// <summary>
        /// 基于方向与目标方向的点积来确定权重
        /// </summary>
        /// <param name="direction">待评估的移动方向</param>
        /// <param name="targetDirection">目标方向向量</param>
        /// <returns>方向权重，范围-1到1</returns>
        public static float CalculateDirectionWeight(Vector2 direction, Vector2 targetDirection)
        {
            // 标准化方向向量以确保点积计算的一致性
            Vector2 normalizedDirection = direction.normalized;
            Vector2 normalizedTargetDirection = targetDirection.normalized;
            
            // 计算点积：点积值范围为-1到1
            // 值为1表示方向完全一致，值为-1表示方向完全相反
            float dotProduct = Vector2.Dot(normalizedDirection, normalizedTargetDirection);
            
            return dotProduct;
        }
        
        /// <summary>
        /// 获取所有有效的移动方向
        /// </summary>
        /// <param name="currentPosition">当前世界坐标位置</param>
        /// <param name="groundTilemap">地面Tilemap</param>
        /// <param name="walkableCheckFunc">可通行性检查函数</param>
        /// <returns>有效的移动方向列表</returns>
        public static List<Vector2> GetValidDirections(Vector3 currentPosition, Tilemap groundTilemap, 
                                                      System.Func<Vector2Int, bool> walkableCheckFunc)
        {
            List<Vector2> validDirections = new();
            
            // 获取当前位置的网格坐标
            Vector3Int currentGridPos = TilemapHelper.WorldToGridPosition(currentPosition, groundTilemap);
            
            // 检查每个方向的通行性
            for (int i = 0; i < BasicDirections.Length; i++)
            {
                Vector2Int targetGridPos = new(
                    currentGridPos.x + Mathf.RoundToInt(BasicDirections[i].x),
                    currentGridPos.y + Mathf.RoundToInt(BasicDirections[i].y)
                );
                
                if (walkableCheckFunc(targetGridPos))
                {
                    validDirections.Add(BasicDirections[i]);
                }
            }
            
            return validDirections;
        }
    }
}
