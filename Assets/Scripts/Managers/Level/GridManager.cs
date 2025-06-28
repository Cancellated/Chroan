using System;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Grid
{
    /// <summary>
/// 2D网格管理器（单例模式）
/// 功能：
/// 1. 管理游戏对象在网格中的注册/注销
/// 2. 处理网格坐标与世界坐标的转换
/// 3. 提供对象查询功能
/// 4. 规则系统的基础支持
/// </summary>
public class GridManager : MonoBehaviour
{
    public LevelManager LevelManager { get; private set; }
    // 网格参数
    [SerializeField] private int gridWidth = 16;
    [SerializeField] private int gridHeight = 16;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector2 originPosition = Vector2.zero; // 网格原点位置

    // 网格数据结构
    private GameObjectBase[,] grid; // 存储网格中的游戏对象
    private Dictionary<Vector2Int, List<GameObjectBase>> gridObjectMap; // 优化查询的字典

    // 编辑器预览

    [SerializeField] private Color gridColor = Color.gray;

    /// <summary>
    /// 单例初始化（双重检查锁定模式）
    /// </summary>
    private void Awake()
    {
        LevelManager = this.gameObject.GetComponent<LevelManager>();
    }

    private void Start()
    {
        OnDrawGizmos();
    }

    /// <summary>
    /// 注册游戏对象到网格
    /// </summary>
    public void RegisterObject(Vector2Int gridPos, GameObjectBase obj)
    {
        // 检查坐标是否在网格范围内
        if (!IsValidPosition(gridPos))
        {
            Debug.LogWarning($"坐标 {gridPos} 超出网格范围");
            return;
        }

        // 添加到二维数组
        grid[gridPos.x, gridPos.y] = obj;

        // 添加到字典（支持同一位置多个对象，如重叠物体）
        if (!gridObjectMap.ContainsKey(gridPos))
        {
            gridObjectMap[gridPos] = new List<GameObjectBase>();
        }
        gridObjectMap[gridPos].Add(obj);
    }

    /// <summary>
    /// 移动网格中的对象
    /// </summary>
    public void MoveObject(GameObjectBase obj, Vector2Int newGridPos)
    {
        // 先移除旧位置
        Vector2Int oldGridPos = obj.GridPosition;
        if (IsValidPosition(oldGridPos))
        {
            grid[oldGridPos.x, oldGridPos.y] = null;
            if (gridObjectMap.ContainsKey(oldGridPos))
            {
                gridObjectMap[oldGridPos].Remove(obj);
                if (gridObjectMap[oldGridPos].Count == 0)
                {
                    gridObjectMap.Remove(oldGridPos);
                }
            }
        }

        // 注册到新位置
        obj.SetGridPosition(newGridPos);
        RegisterObject(newGridPos, obj);

        LevelEvent.TriggerObjectMoved(new ObjectMovedEventData
        {
            Target = obj,
            OldPos = oldGridPos,
            NewPos = newGridPos
        });
    }

    #region 获取对象方法

    /// <summary>
    /// 获取网格位置的游戏对象
    /// </summary>
    public GameObjectBase GetObjectAtPosition(Vector2Int gridPos)
    {
        if (!IsValidPosition(gridPos)) return null;
        return grid[gridPos.x, gridPos.y];
    }

    /// <summary>
    /// 获取网格位置的所有游戏对象（支持重叠物体）////////////////////////////////////////////////////////////
    /// </summary>
    public List<GameObjectBase> GetAllObjectsAtPosition(Vector2Int gridPos)
    {
        if (gridObjectMap.TryGetValue(gridPos, out var objects))
        {
            return new List<GameObjectBase>(objects);
        }
        return new List<GameObjectBase>();
    }

    /// <summary>
    /// 获取指定类型的所有游戏对象
    /// </summary>
    public List<GameObjectBase> GetObjectsByType(ObjectType type)
    {
        List<GameObjectBase> result = new List<GameObjectBase>();
        foreach (var objList in gridObjectMap.Values)
        {
            foreach (var obj in objList)
            {
                if (obj.Type == type)
                {
                    result.Add(obj);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 获取所有文字对象（用于规则检测）
    /// </summary>
    public List<WordObject> GetAllWordObjects()
    {
        List<WordObject> result = new List<WordObject>();
        foreach (var objList in gridObjectMap.Values)
        {
            foreach (var obj in objList)
            {
                if (obj is WordObject wordObj)
                {
                    result.Add(wordObj);
                }
            }
        }
        return result;
    }
    #endregion


    /// <summary>
    /// 在Scene视图中绘制网格线
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;

        // 绘制垂直线
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 startPos = new Vector3(originPosition.x + x * cellSize, originPosition.y, 0);
            Vector3 endPos = new Vector3(originPosition.x + x * cellSize, originPosition.y + gridHeight * cellSize, 0);
            Gizmos.DrawLine(startPos, endPos);
        }

        // 绘制水平线
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 startPos = new Vector3(originPosition.x, originPosition.y + y * cellSize, 0);
            Vector3 endPos = new Vector3(originPosition.x + gridWidth * cellSize, originPosition.y + y * cellSize, 0);
            Gizmos.DrawLine(startPos, endPos);
        }
    }

    /// <summary>
    /// 检测网格位置是否有效
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public bool IsValidPosition(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth && gridPos.y >= 0 && gridPos.y < gridHeight;
    }

    /// <summary>
    /// 网格坐标转世界坐标
    /// </summary>
    public Vector2 GridToWorldPosition(Vector2Int gridPos)
    {
        return originPosition + new Vector2(
            gridPos.x * cellSize,
            gridPos.y * cellSize
        );
    }

    /// <summary>
    /// 世界坐标→网格坐标转换
    /// 算法：(世界坐标 - 原点偏移) / 单元格尺寸
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        // 坐标标准化计算
        int x = Mathf.FloorToInt((worldPos.x - originPosition.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.y - originPosition.y) / cellSize);

        // 边界保护（0 ≤ x < gridWidth）
        return new Vector2Int(
            Mathf.Clamp(x, 0, gridWidth - 1),
            Mathf.Clamp(y, 0, gridHeight - 1)
        );
    }
}
}
