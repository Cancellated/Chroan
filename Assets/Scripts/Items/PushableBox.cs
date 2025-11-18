using UnityEngine;
using UnityEngine.Tilemaps;
using MyGame.Control;
using System.Collections.Generic;
using Logger;
using static Logger.LogModules;

/// <summary>
/// 可推箱子类
/// 实现IPushable接口，提供箱子被推动的具体逻辑
/// </summary>
public class PushableBox : MonoBehaviour, IPushable
{
    [Header("推箱子设置")]
    [Tooltip("推箱子的移动速度")]
    [SerializeField] private float _pushSpeed = 3f;
    [Tooltip("箱子的碰撞体标签")]
    [SerializeField] private string _colliderTag = "Obstacle";
    [Tooltip("是否启用连锁推动功能（箱子推箱子）")]
    [SerializeField] private bool _enableChainPush = false;
    
    [Header("Tilemap设置")]
    [Tooltip("是否自动查找Tilemap（推荐）")]
    [SerializeField] private bool _autoFindTilemaps = true;
    [Tooltip("地板瓦片地图（可选，自动查找时会覆盖此设置）")]
    [SerializeField] private Tilemap _groundTilemap;
    [Tooltip("墙体瓦片地图（可选，自动查找时会覆盖此设置）")]
    [SerializeField] private Tilemap _wallTilemap;
    
    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private static readonly List<PushableBox> allBoxes = new(); // 所有可推箱子实例的列表
    private bool _isMoving = false;
    private Animator _animator;

    private const string LOG_MODULE = BOX;
    
    /// <summary>
    /// 初始化组件和设置
    /// </summary>
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        
        // 自动注册到全局列表
        if (!allBoxes.Contains(this))
            allBoxes.Add(this);
            
        // 如果启用自动查找Tilemap，则自动查找对应的Tilemap
        if (_autoFindTilemaps)
        {
            AutoFindTilemaps();
        }
        
        // 确保组件存在并正确配置
        EnsureComponents();
    }
    
    private void OnDestroy()
    {
        // 从全局列表中移除
        allBoxes.Remove(this);
    }    
    
    #region 自动查找Tilemap
    /// <summary>
    /// 自动查找Tilemap方法
    /// 修复：优化Tilemap识别逻辑，精确匹配Ground和Walls
    /// </summary>
    private void AutoFindTilemaps()
    {
        Log.Info(TILEMAP, $"[自动查找] 开始自动查找Tilemap", gameObject);
        
        // 获取当前Tilemap（如果当前对象在Tilemap上）
        Tilemap currentTilemap = GetComponentInParent<Tilemap>();
        if (currentTilemap != null)
        {
            string tilemapName = currentTilemap.name.ToLower();
            Log.Info(TILEMAP, $"[自动查找] 找到当前Tilemap: {tilemapName}", gameObject);
            
            // 精确匹配Ground和Walls
            if (tilemapName == "ground")
            {
                _groundTilemap = currentTilemap;
                Log.Info(TILEMAP, $"[自动查找] 当前Tilemap识别为地板: {currentTilemap.name}", gameObject);
            }
            else if (tilemapName == "walls")
            {
                _wallTilemap = currentTilemap;
                Log.Info(TILEMAP, $"[自动查找] 当前Tilemap识别为墙体: {currentTilemap.name}", gameObject);
            }
        }
        
        // 查找所有Tilemap
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        Log.Info(TILEMAP, $"[自动查找] 场景中共有 {allTilemaps.Length} 个Tilemap", gameObject);
        
        foreach (var tilemap in allTilemaps)
        {
            if (tilemap == currentTilemap) continue; // 跳过当前Tilemap（已经处理过了）
            
            string tilemapName = tilemap.name.ToLower();
            Log.DebugLog(BOX, $"[自动查找] 检查Tilemap: {tilemap.name}", gameObject);
            
            // 精确匹配Ground和Walls
            if (_groundTilemap == null && tilemapName == "ground")
            {
                _groundTilemap = tilemap;
                Log.Info(TILEMAP, $"[自动查找] 识别为地板Tilemap: {tilemap.name}", gameObject);
            }
            
            if (_wallTilemap == null && tilemapName == "walls")
            {
                _wallTilemap = tilemap;
                Log.Info(TILEMAP, $"[自动查找] 识别为墙体Tilemap: {tilemap.name}", gameObject);
            }
        }
        
        // 如果自动查找失败，输出警告
        if (_groundTilemap == null)
        {
            Log.Warning(TILEMAP, $"[自动查找] 警告: 未找到地板Tilemap（Ground）！请手动设置 _groundTilemap", gameObject);
        }
        
        if (_wallTilemap == null)
        {
            Log.Warning(TILEMAP, $"[自动查找] 警告: 未找到墙体Tilemap（Walls）！请手动设置 _wallTilemap", gameObject);
        }
    }
    
    /// <summary>
    /// 检查是否为地板Tilemap
    /// </summary>
    private bool IsGroundTilemap(Tilemap tilemap)
    {
        // 检查Tilemap的碰撞设置
        if (tilemap.TryGetComponent<TilemapCollider2D>(out var collider))
        {
            // 地板通常设置为Trigger或无碰撞
            return collider.isTrigger || !tilemap.gameObject.layer.Equals(7); // 7是Ground层
        }
        
        // 检查Tilemap下的Tile
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new(x, y, 0);
                TileBase tile = tilemap.GetTile(cell);
                if (tile != null)
                {
                    // 如果有瓦片，根据瓦片名称或类型判断
                    string tileName = tile.name.ToLower();
                    if (tileName.Contains("floor") || tileName.Contains("ground") || 
                        tileName.Contains("grass") || tileName.Contains("dirt"))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 检查是否为墙体Tilemap
    /// </summary>
    private bool IsWallTilemap(Tilemap tilemap)
    {
        // 检查Tilemap的碰撞设置
        TilemapCollider2D collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider != null && !collider.isTrigger)
        {
            // 墙体通常不是Trigger
            return true;
        }
        
        // 检查Tilemap下的Tile
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new(x, y, 0);
                TileBase tile = tilemap.GetTile(cell);
                if (tile != null)
                {
                    // 如果有瓦片，根据瓦片名称或类型判断
                    string tileName = tile.name.ToLower();
                    if (tileName.Contains("wall") || tileName.Contains("rock") || 
                        tileName.Contains("stone") || tileName.Contains("brick"))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    #endregion


    /// <summary>
    /// 获取指定位置的Tilemap对象
    /// </summary>
    public static PushableBox GetBoxAtPosition(Vector3Int cellPosition)
    {
        foreach (var box in allBoxes)
        {
            if (box != null && box._groundTilemap != null)
            {
                Vector3Int boxCell = box._groundTilemap.WorldToCell(box.transform.position);
                if (boxCell == cellPosition)
                {
                    return box;
                }
            }
        }
        return null;
    }
    
    /// <summary>
    /// 确保所有必要组件都已添加并正确配置
    /// </summary>
    private void EnsureComponents()
    {
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        
        if (_collider == null)
        {
            _collider = gameObject.AddComponent<BoxCollider2D>();
        }
        
        // 设置标签为障碍物
        gameObject.tag = _colliderTag;
    }
    
    #region 推动物体
    /// <summary>
    /// 尝试推动可推物体
    /// 添加日志输出用于调试
    /// </summary>
    /// <param name="pushDirection">推动方向</param>
    /// <param name="pushDistance">推动距离（默认为1个网格）</param>
    /// <returns>推动成功返回true，失败返回false</returns>
    public bool TryPush(Vector2 pushDirection, float pushDistance = 1f)
    {
        if (_isMoving)
        {
            Log.DebugLog("箱子", $"[箱子推动] 物体正在移动中，无法推动", gameObject);
            return false;
        }
        
        Log.DebugLog("箱子", $"[箱子推动] 尝试推动: 方向={pushDirection}, 距离={pushDistance}", gameObject);
        
        // 计算目标网格位置
        Vector3Int currentGridPosition = GetGridPosition();
        int horizontalSteps = Mathf.RoundToInt(pushDirection.x * pushDistance);
        int verticalSteps = Mathf.RoundToInt(pushDirection.y * pushDistance);
        Vector3Int targetGridPosition = new(
            currentGridPosition.x + horizontalSteps,
            currentGridPosition.y + verticalSteps,
            currentGridPosition.z
        );
        
        Log.DebugLog("箱子", $"[箱子推动] 当前位置: {currentGridPosition}, 目标位置: {targetGridPosition}", gameObject);
        
        // 检查是否可以推动到目标位置
        if (CanBePushedTo(targetGridPosition))
        {
            Log.DebugLog("箱子", $"[箱子推动] 检查通过，开始移动到 {targetGridPosition}", gameObject);
            StartCoroutine(MoveToGridPosition(targetGridPosition));
            return true;
        }
        else
        {
            Log.DebugLog("箱子", $"[箱子推动] 检查失败，无法推动到 {targetGridPosition}", gameObject);
            return false;
        }
    }
    
    /// <summary>
    /// 检查物体是否可以被推动到目标位置
    /// 简化：仅保留必要日志
    /// </summary>
    /// <param name="targetGridPosition">目标网格位置</param>
    /// <returns>如果目标位置可推送到返回true，否则返回false</returns>
    public bool CanBePushedTo(Vector3Int targetGridPosition)
    {
        // 1. 检查地板是否存在（必须有地板才能推送）
        if (_groundTilemap == null || !_groundTilemap.HasTile(targetGridPosition))
        {
            Log.Warning(TILEMAP, $"箱子 {gameObject.name} 推动失败: 目标位置没有地板", gameObject);
            return false;
        }
        
        // 2. 检查是否有墙体（如果有墙体则不可推送）
        if (_wallTilemap != null && _wallTilemap.HasTile(targetGridPosition))
        {
            Log.Warning(TILEMAP, $"箱子 {gameObject.name} 推动失败: 目标位置有墙体", gameObject);
            return false;
        }
        
        // 3. 检查目标位置是否有其他障碍物或可推物体
        Vector3 worldPosition = GridToWorldPosition(targetGridPosition);
        
        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            worldPosition,
            new Vector2(0.8f, 0.8f), // 稍微小于网格大小，避免边缘检测问题
            0f
        );
        
        foreach (Collider2D collider in colliders)
        {
            // 跳过自身
            if (collider == _collider)
            {
                continue;
            }
            
            // 检查是否有障碍物或其他可推物体
            if (collider.CompareTag(_colliderTag))
            {
                // 检查是否是另一个可推物体
                
                if (collider.TryGetComponent<IPushable>(out var otherPushable))
                {
                    // 如果是可推物体且启用了连锁推动功能
                    if (_enableChainPush)
                    {
                        // 计算另一个可推物体的目标位置
                        Vector3Int currentPos = otherPushable.GetGridPosition();
                        Vector3Int otherTargetPos = currentPos + (targetGridPosition - GetGridPosition());
                        
                        // 检查另一个可推物体是否可以被推动
                        if (!otherPushable.CanBePushedTo(otherTargetPos))
                        {
                            Log.Warning(TILEMAP, $"箱子 {gameObject.name} 连锁推动失败: {collider.gameObject.name} 无法推动到 {otherTargetPos}", gameObject);
                            return false;
                        }
                    }
                    else
                    {
                        // 如果是不可推的障碍物，或者禁用了连锁推动但遇到了可推物体，返回false
                        Log.Warning(TILEMAP, $"箱子 {gameObject.name} 推动失败: 遇到可推物体但连锁推动已禁用", gameObject);
                        return false;
                    }
                }
                else
                {
                    // 不可推的障碍物
                    Log.Warning(TILEMAP, $"箱子 {gameObject.name} 推动失败: 遇到不可推障碍物 {collider.gameObject.name}", gameObject);
                    return false;
                }
            }
        }
        
        return true;
    }

    /// <summary>
    /// 将世界位置转换为网格位置
    /// 简化：移除调试输出，仅保留必要的坐标转换功能
    /// </summary>
    /// <param name="worldPosition">世界位置</param>
    /// <returns>网格位置</returns>
    private Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        if (_groundTilemap == null)
        {
            return Vector3Int.zero;
        }
        
        return _groundTilemap.WorldToCell(worldPosition);
    }
    
    /// <summary>
    /// 获取网格位置
    /// 简化：移除调试输出
    /// </summary>
    public Vector3Int GetGridPosition()
    {
        return WorldToGridPosition(transform.position);
    }

    /// <summary>
    /// 获取当前网格位置（用于调试）
    /// 简化：移除所有调试输出
    /// </summary>
    public Vector3Int GetCurrentGridPosition()
    {
        return WorldToGridPosition(transform.position);
    }
    
    /// <summary>
    /// 将网格位置转换为世界位置
    /// 简化：移除调试输出
    /// </summary>
    /// <param name="gridPosition">网格位置</param>
    /// <returns>世界位置</returns>
    private Vector3 GridToWorldPosition(Vector3Int gridPosition)
    {
        if (_groundTilemap == null)
        {
            return Vector3.zero;
        }
        
        return _groundTilemap.GetCellCenterWorld(gridPosition);
    }
    
    /// <summary>
    /// 平滑移动到目标网格位置
    /// 简化：移除冗余日志输出
    /// </summary>
    /// <param name="targetGridPos">目标网格位置</param>
    private System.Collections.IEnumerator MoveToGridPosition(Vector3Int targetGridPos)
    {
        _isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 targetPos = GridToWorldPosition(targetGridPos);
        float journeyLength = Vector3.Distance(startPos, targetPos);
        float startTime = Time.time;
        
        // 设置移动状态的动画参数（如果有）
        if (_animator != null)
        {
            _animator.SetBool("IsMoving", true);
        }
        
        int loopCount = 0;
        
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            loopCount++;
            float distCovered = (Time.time - startTime) * _pushSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            
            // 简化：只移动根对象（纯GameObject方案）
            transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
            
            yield return null;
        }
        
        // 简化：精确对齐到网格中心（只处理根对象）
        transform.position = targetPos;
        
        _isMoving = false;
        
        // 重置移动状态的动画参数（如果有）
        if (_animator != null)
        {
            _animator.SetBool("IsMoving", false);
        }
    }
    
    #endregion
}