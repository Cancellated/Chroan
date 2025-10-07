using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// 空间分割系统
/// 负责将瓦片地图划分为网格状的空间单元格，实现高效的区域管理和查询
/// </summary>
public class SpatialPartitionSystem : MonoBehaviour
{
    [Tooltip("空间单元格的大小（单位：瓦片）")]
    [Range(5, 50)]
    public int cellSize = 10;

    // 空间单元格字典，键为单元格坐标，值为该单元格内的瓦片地图列表
    private Dictionary<Vector2Int, List<Tilemap>> spatialCells = new();

    /// <summary>
    /// 将瓦片地图添加到空间分割系统
    /// </summary>
    /// <param name="tilemap">要添加的瓦片地图</param>
    public void AddTilemap(Tilemap tilemap)
    {
        if (tilemap == null) return;

        // 清除该瓦片地图之前可能存在的空间信息
        RemoveTilemap(tilemap);

        // 获取瓦片地图的边界
        BoundsInt bounds = tilemap.cellBounds;
        Vector3Int min = bounds.min;
        Vector3Int max = bounds.max;

        // 计算瓦片地图覆盖的所有单元格
        Vector2Int minCell = WorldToCell(tilemap.CellToWorld(min));
        Vector2Int maxCell = WorldToCell(tilemap.CellToWorld(max));

        // 将瓦片地图添加到所有覆盖的单元格中
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector2Int cellPos = new Vector2Int(x, y);
                if (!spatialCells.ContainsKey(cellPos))
                {
                    spatialCells[cellPos] = new List<Tilemap>();
                }
                if (!spatialCells[cellPos].Contains(tilemap))
                {
                    spatialCells[cellPos].Add(tilemap);
                }
            }
        }
    }

    /// <summary>
    /// 从空间分割系统中移除瓦片地图
    /// </summary>
    /// <param name="tilemap">要移除的瓦片地图</param>
    public void RemoveTilemap(Tilemap tilemap)
    {
        // 遍历所有单元格，移除包含该瓦片地图的引用
        foreach (var cell in spatialCells)
        {
            cell.Value.Remove(tilemap);
        }
        
        // 清理空单元格
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        foreach (var cell in spatialCells)
        {
            if (cell.Value.Count == 0)
            {
                emptyCells.Add(cell.Key);
            }
        }
        
        foreach (var cellPos in emptyCells)
        {
            spatialCells.Remove(cellPos);
        }
    }

    /// <summary>
    /// 获取指定位置周围一定半径内的所有瓦片地图
    /// </summary>
    /// <param name="worldPosition">中心点世界位置</param>
    /// <param name="radius">查询半径（单位：单元格）</param>
    /// <returns>该区域内的瓦片地图列表</returns>
    public List<Tilemap> GetTilemapsInRadius(Vector3 worldPosition, int radius)
    {
        List<Tilemap> result = new List<Tilemap>();
        HashSet<Tilemap> uniqueTilemaps = new HashSet<Tilemap>();
        
        Vector2Int centerCell = WorldToCell(worldPosition);
        
        // 遍历中心单元格周围指定半径内的所有单元格
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int cellPos = new Vector2Int(centerCell.x + x, centerCell.y + y);
                
                // 如果单元格存在，则将其中的瓦片地图添加到结果中
                if (spatialCells.ContainsKey(cellPos))
                {
                    foreach (var tilemap in spatialCells[cellPos])
                    {
                        if (!uniqueTilemaps.Contains(tilemap))
                        {
                            uniqueTilemaps.Add(tilemap);
                            result.Add(tilemap);
                        }
                    }
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// 将世界坐标转换为单元格坐标
    /// </summary>
    /// <param name="worldPosition">世界坐标</param>
    /// <returns>对应的单元格坐标</returns>
    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        float cellWorldSize = cellSize * GetTileSize();
        return new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / cellWorldSize),
            Mathf.FloorToInt(worldPosition.y / cellWorldSize)
        );
    }

    /// <summary>
    /// 估算瓦片大小（假设所有瓦片大小相同）
    /// </summary>
    /// <returns>瓦片大小</returns>
    private float GetTileSize()
    {
        // 这里简化处理，假设所有瓦片地图使用相同的瓦片大小
        // 实际项目中可能需要根据具体的瓦片地图配置来获取准确的瓦片大小
        return 1.0f;
    }

    /// <summary>
    /// 清空所有空间数据
    /// </summary>
    public void Clear()
    {
        spatialCells.Clear();
    }

    /// <summary>
    /// 可视化空间单元格（仅在编辑器模式下显示）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!Application.isEditor || Application.isPlaying)
            return;

        Gizmos.color = new Color(0, 1, 0, 0.2f);
        float cellWorldSize = cellSize * GetTileSize();
        
        foreach (var cell in spatialCells.Keys)
        {
            Vector3 cellCenter = new Vector3(
                cell.x * cellWorldSize + cellWorldSize / 2, 
                cell.y * cellWorldSize + cellWorldSize / 2, 
                0
            );
            Gizmos.DrawWireCube(cellCenter, new Vector3(cellWorldSize, cellWorldSize, 0));
        }
    }
}