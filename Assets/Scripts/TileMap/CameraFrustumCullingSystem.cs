using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// 摄像机视野剔除系统
/// 负责管理摄像机视野内外的瓦片碰撞体刷新，优化性能
/// 支持死区设置，提升玩家体验
/// </summary>
public class CameraFrustumCullingSystem : MonoBehaviour
{
    [Header("摄像机配置")]
    [Tooltip("目标摄像机，如果未指定则使用主摄像机")]
    public Camera targetCamera;
    
    [Header("死区设置")]
    [Tooltip("视野外缓冲区域大小（单位：瓦片）- 标准死区配置")]
    [Range(0, 20)]
    public int frustumBuffer = 5;
    
    [Tooltip("水平死区大小(tile)")]
    [Range(0, 10)]
    public int horizontalDeadZone = 4;
    
    [Tooltip("垂直死区大小(tile)")]
    [Range(0, 6)]
    public int verticalDeadZone = 2;
    
    [Header("高级设置")]
    [Tooltip("是否启用死区功能")]
    public bool enableDeadZone = true;
    
    [Tooltip("摄像机移动平滑度（数值越大移动越快）")]
    [Range(0.1f, 10f)]
    public float cameraSmoothTime = 5f;
    
    // 上一帧视野内的瓦片位置范围
    private BoundsInt lastVisibleBounds;
    
    // 表示上一帧是否有有效的视野数据
    private bool hasLastVisibleBounds = false;
    
    // 死区相关的内部状态
    private Vector3 lastCameraTargetPosition;
    private Vector3 cameraVelocity = Vector3.zero;
    private bool hasLastTargetPosition = false;
    
    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        
        // 初始化目标位置
        if (targetCamera != null)
        {
            lastCameraTargetPosition = targetCamera.transform.position;
            hasLastTargetPosition = true;
        }
    }
    
    void LateUpdate()
    {
        if (enableDeadZone && targetCamera != null)
        {
            UpdateCameraWithDeadZone();
        }
    }
    
    /// <summary>
    /// 使用死区机制更新摄像机位置
    /// </summary>
    private void UpdateCameraWithDeadZone()
    {
        // 这里需要与玩家位置系统集成
        // 暂时保持当前摄像机位置，等待集成玩家跟随系统
        Vector3 currentCameraPos = targetCamera.transform.position;
        
        if (hasLastTargetPosition)
        {
            Vector3 deadZoneCenter = lastCameraTargetPosition;
            Vector3 deadZoneSize = new(
                horizontalDeadZone * targetCamera.orthographicSize * 2f * ((float)Screen.width / Screen.height) / 18f,
                verticalDeadZone * 2f * targetCamera.orthographicSize / 10f,
                0
            );
            
            Vector3 deadZoneMin = deadZoneCenter - deadZoneSize * 0.5f;
            Vector3 deadZoneMax = deadZoneCenter + deadZoneSize * 0.5f;
            
            // 检查玩家是否超出死区范围
            if (IsPlayerOutsideDeadZone(currentCameraPos, deadZoneMin, deadZoneMax))
            {
                // 更新摄像机目标位置
                lastCameraTargetPosition = currentCameraPos;
                hasLastTargetPosition = true;
            }
        }
        else
        {
            lastCameraTargetPosition = currentCameraPos;
            hasLastTargetPosition = true;
        }
    }
    
    /// <summary>
    /// 检查玩家是否在死区外（需要集成实际玩家位置）
    /// </summary>
    private bool IsPlayerOutsideDeadZone(Vector3 playerPosition, Vector3 deadZoneMin, Vector3 deadZoneMax)
    {
        // TODO: 这里需要传入实际玩家位置
        // 目前使用摄像机位置作为占位符
        return playerPosition.x < deadZoneMin.x || playerPosition.x > deadZoneMax.x ||
               playerPosition.y < deadZoneMin.y || playerPosition.y > deadZoneMax.y;
    }
    
    /// <summary>
    /// 获取当前摄像机视野内的瓦片边界
    /// </summary>
    /// <param name="tilemap">目标瓦片地图</param>
    /// <returns>视野内的瓦片边界</returns>
    public BoundsInt GetVisibleTileBounds(Tilemap tilemap)
    {
        if (targetCamera == null || tilemap == null)
        {
            return new BoundsInt();
        }
        
        // 获取摄像机视锥在世界空间中的边界
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = 2.0f * targetCamera.orthographicSize;
        float cameraWidth = cameraHeight * screenAspect;
        
        Vector3 cameraPosition = targetCamera.transform.position;
        Vector3 frustumCenter = new(
            cameraPosition.x, 
            cameraPosition.y, 
            0
        );
        
        // 计算包含整个视野和缓冲区的世界空间边界
        float extendedWidth = cameraWidth / 2 + frustumBuffer * tilemap.cellSize.x;
        float extendedHeight = cameraHeight / 2 + frustumBuffer * tilemap.cellSize.y;
        
        Vector3 frustumMin = new(
            frustumCenter.x - extendedWidth, 
            frustumCenter.y - extendedHeight, 
            0
        );
        Vector3 frustumMax = new(
            frustumCenter.x + extendedWidth, 
            frustumCenter.y + extendedHeight, 
            0
        );
        
        // 转换为瓦片坐标
        Vector3Int minTile = tilemap.WorldToCell(frustumMin);
        Vector3Int maxTile = tilemap.WorldToCell(frustumMax);
        
        // 确保最小坐标小于最大坐标
        if (minTile.x > maxTile.x) {
            (maxTile.x, minTile.x) = (minTile.x, maxTile.x);
        }
        if (minTile.y > maxTile.y) {
            (maxTile.y, minTile.y) = (minTile.y, maxTile.y);
        }
        
        // 创建并返回边界
        BoundsInt visibleBounds = new(minTile, maxTile - minTile + Vector3Int.one);
        
        // 保存当前边界用于下次比较
        lastVisibleBounds = visibleBounds;
        hasLastVisibleBounds = true;
        
        return visibleBounds;
    }
    
    /// <summary>
    /// 检查指定位置是否在视野（包括缓冲区）内
    /// </summary>
    /// <param name="tilemap">目标瓦片地图</param>
    /// <param name="tilePosition">瓦片位置</param>
    /// <returns>是否在视野内</returns>
    public bool IsTileInVisibleArea(Tilemap tilemap, Vector3Int tilePosition)
    {
        if (targetCamera == null || tilemap == null)
        {
            return true; // 如果没有摄像机或瓦片地图，默认返回true
        }
        
        // 如果没有上次的视野数据，获取当前视野
        if (!hasLastVisibleBounds)
        {
            GetVisibleTileBounds(tilemap);
        }
        
        // 检查瓦片位置是否在视野边界内
        return lastVisibleBounds.Contains(tilePosition);
    }
    
    /// <summary>
    /// 获取视野（包括缓冲区）内的所有瓦片位置
    /// </summary>
    /// <param name="tilemap">目标瓦片地图</param>
    /// <returns>视野内的瓦片位置列表</returns>
    public List<Vector3Int> GetVisibleTilePositions(Tilemap tilemap)
    {
        List<Vector3Int> positions = new();
        
        BoundsInt visibleBounds = GetVisibleTileBounds(tilemap);
        
        // 遍历边界内的所有位置
        for (int x = visibleBounds.xMin; x <= visibleBounds.xMax; x++)
        {
            for (int y = visibleBounds.yMin; y <= visibleBounds.yMax; y++)
            {
                positions.Add(new Vector3Int(x, y, 0));
            }
        }
        
        return positions;
    }
    
    /// <summary>
    /// 刷新视野内的瓦片碰撞体
    /// </summary>
    /// <param name="tilemap">目标瓦片地图</param>
    /// <param name="refreshAction">刷新动作委托</param>
    public void RefreshVisibleAreaColliders(Tilemap tilemap, System.Action<Tilemap, Vector3Int> refreshAction)
    {
        if (tilemap == null || refreshAction == null)
        {
            return;
        }
        
        BoundsInt visibleBounds = GetVisibleTileBounds(tilemap);
        
        // 遍历边界内的所有位置
        for (int x = visibleBounds.xMin; x <= visibleBounds.xMax; x++)
        {
            for (int y = visibleBounds.yMin; y <= visibleBounds.yMax; y++)
            {
                Vector3Int position = new(x, y, 0);
                
                // 检查瓦片是否存在
                if (tilemap.HasTile(position))
                {
                    refreshAction(tilemap, position);
                }
            }
        }
    }
    
    /// <summary>
    /// 可视化当前视野区域（仅在编辑器模式下显示）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!Application.isEditor || Application.isPlaying || targetCamera == null)
            return;
        
        // 绘制视野区域（绿色）
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = 2.0f * targetCamera.orthographicSize;
        float cameraWidth = cameraHeight * screenAspect;
        
        Vector3 cameraPosition = targetCamera.transform.position;
        
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(cameraPosition, new Vector3(cameraWidth, cameraHeight, 0.1f));
        
        // 绘制视野缓冲区（黄色）
        float extendedWidth = cameraWidth / 2 + frustumBuffer;
        float extendedHeight = cameraHeight / 2 + frustumBuffer;
        
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawWireCube(cameraPosition, new Vector3(extendedWidth * 2, extendedHeight * 2, 0.1f));
    }
}