using UnityEngine;
using UnityEngine.Tilemaps;
using Logger;

namespace AI.Behavior
{
    /// <summary>
    /// Tilemap辅助类，提供共享的Tilemap操作功能
    /// </summary>
    public static class TilemapHelper
    {
        /// <summary>
        /// 将世界坐标转换为网格坐标
        /// </summary>
        /// <param name="worldPosition">世界坐标</param>
        /// <param name="tilemap">目标Tilemap</param>
        /// <returns>网格坐标</returns>
        public static Vector3Int WorldToGridPosition(Vector3 worldPosition, Tilemap tilemap)
        {
            if (tilemap == null)
                return Vector3Int.zero;
            
            return tilemap.WorldToCell(worldPosition);
        }
        
        /// <summary>
        /// 将网格坐标转换为世界坐标并居中
        /// </summary>
        /// <param name="gridPosition">网格坐标</param>
        /// <param name="tilemap">目标Tilemap</param>
        /// <returns>居中的世界坐标</returns>
        public static Vector3 GridToWorldPosition(Vector3Int gridPosition, Tilemap tilemap)
        {
            if (tilemap == null)
                return Vector3.zero;
            
            return tilemap.GetCellCenterWorld(gridPosition);
        }
        
        /// <summary>
        /// 将世界坐标对齐到网格中心
        /// </summary>
        /// <param name="worldPosition">世界坐标</param>
        /// <param name="tilemap">目标Tilemap</param>
        /// <returns>对齐到网格中心的世界坐标</returns>
        public static Vector3 AlignToGridCenter(Vector3 worldPosition, Tilemap tilemap)
        {
            if (tilemap == null)
                return worldPosition;
                
            Vector3Int gridPosition = WorldToGridPosition(worldPosition, tilemap);
            return GridToWorldPosition(gridPosition, tilemap);
        }
        
        /// <summary>
        /// 检查指定位置是否在地面上
        /// </summary>
        /// <param name="position">要检查的位置</param>
        /// <param name="groundTilemap">地面Tilemap</param>
        /// <param name="context">日志上下文对象</param>
        /// <returns>如果位置在地面上则返回true，否则返回false</returns>
        public static bool IsPositionOnGround(Vector2 position, Tilemap groundTilemap, Object context = null)
        {
            if (groundTilemap != null)
            {
                Vector3Int gridPosition = WorldToGridPosition(position, groundTilemap);
                bool hasGroundTile = groundTilemap.HasTile(gridPosition);
                return hasGroundTile;
            }
            
            Log.Debug(LogModules.AI, $"位置({position})地面检测 - 没有Tilemap引用，无法检测地面", context);
            return false;
        }
        
        /// <summary>
        /// 检查网格位置是否可通行
        /// </summary>
        /// <param name="gridPosition">要检查的网格位置</param>
        /// <param name="groundTilemap">地面Tilemap</param>
        /// <param name="wallTilemap">墙体Tilemap</param>
        /// <param name="cellSize">网格单元格大小</param>
        /// <returns>如果可通行则返回true，否则返回false</returns>
        public static bool IsCellWalkable(Vector3Int gridPosition, Tilemap groundTilemap, Tilemap wallTilemap, Vector2 cellSize)
        {
            // 1. 检查地板是否存在
            if (groundTilemap == null || !groundTilemap.HasTile(gridPosition))
            {
                return false;
            }
            
            // 2. 检查是否有墙体
            if (wallTilemap != null && wallTilemap.HasTile(gridPosition))
            {
                return false;
            }
            
            // 3. 使用Physics2D进行碰撞检测
            Vector3 worldPosition = GridToWorldPosition(gridPosition, groundTilemap);
            Collider2D[] colliders = Physics2D.OverlapBoxAll(worldPosition, 
                                                          new Vector2(cellSize.x - 0.1f, cellSize.y - 0.1f), 
                                                          0f);
            
            foreach (Collider2D collider in colliders)
            {
                // 检查是否有不可通行的碰撞体（Obstacle标签）
                if (collider.CompareTag("Obstacle") || collider.CompareTag("Player"))
                {
                    return false;
                }
            }
            
            // 位置可通行
            return true;
        }
        
        /// <summary>
        /// 检查特定方向是否被障碍物阻挡
        /// </summary>
        /// <param name="originPosition">起始位置</param>
        /// <param name="direction">要检查的方向</param>
        /// <param name="checkDistance">检查距离</param>
        /// <param name="ignoreObject">需要忽略的对象（通常是自身）</param>
        /// <param name="context">日志上下文对象</param>
        /// <returns>如果方向被阻挡则返回true，否则返回false</returns>
        public static bool IsDirectionBlocked(Vector2 originPosition, Vector2 direction, float checkDistance, GameObject ignoreObject = null, Object context = null)
        {
            // 创建一个LayerMask，包含Wall层
            LayerMask wallLayer = LayerMask.GetMask("Wall");
            
            // 使用射线检测检查方向是否有碰撞体
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                originPosition,
                direction,
                checkDistance
            );
            
            // 检查是否击中了带有Obstacle标签的对象或者Wall层的对象
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    // 跳过需要忽略的对象
                    if (hit.collider.gameObject == ignoreObject)
                    {
                        continue;
                    }
                    
                    // 检查是否是Obstacle标签或Wall层
                    bool isObstacleTag = hit.collider.CompareTag("Obstacle");
                    bool isWallLayer = (wallLayer.value & (1 << hit.collider.gameObject.layer)) > 0;
                    
                    if (isObstacleTag || isWallLayer)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 查找并设置地面Tilemap引用
        /// </summary>
        /// <returns>找到的地面Tilemap或null</returns>
        public static Tilemap FindGroundTilemap()
        {
            Log.Debug(LogModules.AI, "开始查找地面Tilemap", null);
            return FindSpecificTilemap("ground");
        }
        
        /// <summary>
        /// 查找并设置墙体Tilemap引用
        /// </summary>
        /// <returns>找到的墙体Tilemap或null</returns>
        public static Tilemap FindWallTilemap()
        {
            Log.Debug(LogModules.AI, "开始查找墙体Tilemap", null);
            return FindSpecificTilemap("walls");
        }
        
        /// <summary>
        /// 通用Tilemap查找方法
        /// </summary>
        private static Tilemap FindSpecificTilemap(string tilemapType)
        {
            Tilemap[] allTilemaps = Object.FindObjectsOfType<Tilemap>();
            
            // 记录所有找到的Tilemap名称，用于调试
            string allTilemapNames = "";
            foreach (var tm in allTilemaps)
            {
                allTilemapNames += tm.name + ", ";
            }
            Log.Debug(LogModules.AI, $"找到的Tilemap名称: {allTilemapNames}", null);
            
            // 根据类型确定关键词数组
            string[] keywords = GetTilemapKeywords(tilemapType);
            
            // 尝试精确匹配
            foreach (var tm in allTilemaps)
            {
                foreach (var keyword in keywords)
                {
                    if (tm.name.Equals(keyword, System.StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Debug(LogModules.AI, $"找到精确匹配的{tilemapType} Tilemap: {tm.name}", null);
                        return tm;
                    }
                }
            }
            
            // 尝试包含匹配
            foreach (var tm in allTilemaps)
            {
                foreach (var keyword in keywords)
                {
                    if (tm.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Log.Debug(LogModules.AI, $"找到包含匹配的{tilemapType} Tilemap: {tm.name}", null);
                        return tm;
                    }
                }
            }
            
            // 如果找不到，尝试直接使用场景中的第一个Tilemap作为地面，第二个作为墙体
            if (allTilemaps.Length > 0 && tilemapType.ToLower() == "ground")
            {
                Log.Warning(LogModules.AI, $"未找到指定的地面Tilemap，使用场景中的第一个Tilemap: {allTilemaps[0].name}", null);
                return allTilemaps[0];
            }
            
            if (allTilemaps.Length > 1 && tilemapType.ToLower() == "walls")
            {
                Log.Warning(LogModules.AI, $"未找到指定的墙体Tilemap，使用场景中的第二个Tilemap: {allTilemaps[1].name}", null);
                return allTilemaps[1];
            }
            
            Log.Warning(LogModules.AI, $"未找到{tilemapType}类型的Tilemap，关键词: {string.Join(", ", keywords)}", null);
            return null;
        }
        
        /// <summary>
        /// 根据Tilemap类型获取搜索关键词
        /// </summary>
        private static string[] GetTilemapKeywords(string tilemapType)
        {
            // 转换为小写进行比较
            string lowerType = tilemapType.ToLower();

            return lowerType switch
            {
                "ground" or "floor" => new string[] { "ground", "floor", "地面", "地板", "Ground", "Floor" },
                "wall" or "walls" => new string[] { "wall", "walls", "墙", "墙壁", "Wall", "Walls" },
                _ => new string[] { lowerType, tilemapType },
            };
        }
    }
}