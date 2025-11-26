using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;
using Logger;

/// <summary>
/// 墙体碰撞体管理器
/// 自动配置墙体Tilemap的碰撞体组件，支持不同类型的碰撞体设置
/// </summary>
[ExecuteInEditMode]
public class WallColliderManager : TilemapChangeListener
{
    [Header("墙体设置")]
    [Tooltip("墙体Tilemap所在的图层名称")]
    public string wallLayerName = "Wall";

    [Header("碰撞体设置")]
    [Tooltip("是否启用复合碰撞体（用于合并相邻瓦片碰撞体）")]
    public bool useCompositeCollider = true;

    [Tooltip("是否自动刷新碰撞体（当Tilemap变化时自动更新）")]
    public bool autoRefreshColliders = false; // 小型地图默认禁用自动刷新

    [Tooltip("是否覆盖瓦片的碰撞体设置")]
    public bool overrideTileColliders = false;

    [Tooltip("当覆盖瓦片碰撞体设置时使用的碰撞体类型")]
    public Tile.ColliderType overrideColliderType = Tile.ColliderType.Sprite;

    [Tooltip("默认碰撞体的物理材质")]
    public PhysicsMaterial2D defaultPhysicsMaterial;

    [Header("高级物理材质配置")]
    [Tooltip("启用不同墙体类型使用不同物理材质")]
    public bool enableAdvancedPhysicsMaterials = false;

    [Tooltip("墙体物理材质配置列表")]
    public List<WallPhysicsMaterialConfig> wallPhysicsMaterialConfigs = new();
    
    [Tooltip("是否从Rule Tile获取物理材质")]
    public bool useRuleTilePhysicsMaterials = false;

    [Tooltip("自动刷新碰撞体间隔")]
    public float refreshCooldown = 5.0f; // 小型地图建议较长的刷新间隔

    private readonly List<Tilemap> wallTilemaps = new();
    private Coroutine refreshCoroutine;
    private float lastFindTime = -Mathf.Infinity;
    private const float findCooldown = 0.1f; // 防止短时间内重复查找的冷却时间
    
    [Header("优化设置")]
    [Tooltip("启用小型地图优化模式")]
    public bool smallMapOptimization = true;
    
    [Header("独立系统引用")]
    [Tooltip("空间分割系统")]
    public SpatialPartitionSystem spatialPartitionSystem;
    
    [Tooltip("摄像机视野剔除系统")]
    public CameraFrustumCullingSystem cameraFrustumCullingSystem;

    private void OnEnable()
    {
        // 查找所有墙体Tilemap
        FindWallTilemaps();

        // 配置所有墙体Tilemap的碰撞体
        ConfigureAllWallColliders();

        // 如果启用了自动刷新，则添加监听器
        if (autoRefreshColliders)
        {
            StartAutoRefresh();
        }
        
        // 自动获取或创建独立系统组件
        EnsureSystemsAreInitialized();
        
        // 注册到瓦片地图事件系统
        RegisterToEventSystem();
        
        // 将墙体Tilemap添加到空间分割系统
        AddTilemapsToSpatialSystem();
    }

    private void OnDisable()
    {
        // 停止自动刷新协程
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
            refreshCoroutine = null;
        }
    }
    
    protected override void OnDestroy()
    {
        // 清理创建的系统对象，避免场景关闭时未清理的对象警告
        // 停止自动刷新协程（额外保险）
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
            refreshCoroutine = null;
        }
        
        // 清理事件监听器
        foreach (var tilemap in wallTilemaps)
        {
            StopListeningTo(tilemap);
        }
        
        // 如果是编辑器模式，检查并清理TilemapEventSystem
        #if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // 查找并销毁临时创建的系统对象
            TilemapEventSystem[] eventSystems = FindObjectsOfType<TilemapEventSystem>();
            foreach (var system in eventSystems)
            {
                // 只销毁非场景中保存的对象（例如运行时创建的）
                if (!UnityEditor.EditorUtility.IsPersistent(system.gameObject))
                {
                    DestroyImmediate(system.gameObject);
                }
            }
        }
        #endif
    }

    private void Update()
    {
        // 在编辑模式下，如果启用了自动刷新，定期检查Tilemap变化
        if (Application.isEditor && !Application.isPlaying && autoRefreshColliders)
        {
            ConfigureAllWallColliders();
        }
    }

    /// <summary>
    /// 查找所有墙体Tilemap
    /// </summary>
    private void FindWallTilemaps()
    {
        // 防止短时间内重复执行查找操作
        if (Time.time - lastFindTime < findCooldown)
        {
            Log.Debug(LogModules.TILEMAP, "查找操作处于冷却中，跳过本次执行", this);
            return;
        }
        
        lastFindTime = Time.time;
        wallTilemaps.Clear();
        
        // 获取所有Tilemap组件
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        
        // 获取墙体图层ID
        int wallLayerId = LayerMask.NameToLayer(wallLayerName);
        
        // 检查图层是否存在
        if (wallLayerId == -1)
        {
            Log.Error(LogModules.TILEMAP, $"图层 '{wallLayerName}' 不存在，请检查拼写或在Tags & Layers设置中创建该图层。\n" +
                      "创建方法：在Unity编辑器顶部菜单栏选择'Edit > Project Settings > Tags & Layers'，" +
                      "在Layers列表中找到一个空槽位，输入'Walls'，然后点击空白处保存。", this);
            // 不立即返回，继续尝试通过名称查找作为备选方案
        }
        else
        {
            Log.Info(LogModules.TILEMAP, $"正在使用图层ID: {wallLayerId} (名称: {wallLayerName})", this);
            
            int initialCount = wallTilemaps.Count;
            
            // 尝试通过图层查找墙体Tilemap
            foreach (Tilemap tilemap in allTilemaps)
            {
                int tilemapLayerId = tilemap.gameObject.layer;
                string tilemapLayerName = LayerMask.LayerToName(tilemapLayerId);
                
                Log.Debug(LogModules.TILEMAP, $"Tilemap '{tilemap.name}' 在图层 '{tilemapLayerName}' (ID: {tilemapLayerId})", this);
                
                if (tilemapLayerId == wallLayerId)
                {
                    wallTilemaps.Add(tilemap);
                    Log.Debug(LogModules.TILEMAP, $"已添加墙体Tilemap: {tilemap.name}", this);
                }
            }
        }
        

        
        // 如果通过图层没有找到，尝试通过名称查找作为备选方案
        if (wallTilemaps.Count == 0)
        {
            Log.Warning(LogModules.TILEMAP, $"未找到图层为 '{wallLayerName}' 的墙体Tilemap，将尝试通过名称查找", this);
            
            foreach (Tilemap tilemap in allTilemaps)
            {
                // 检查Tilemap名称是否包含墙体相关关键词
                if (tilemap.name.Contains("Wall") || tilemap.name.Contains("wall") || tilemap.name.Contains("墙体") || 
                    tilemap.name.Contains("WallTilemap") || tilemap.name.Equals(wallLayerName))
                {
                    wallTilemaps.Add(tilemap);
                    Log.Info(LogModules.TILEMAP, $"通过名称匹配添加Tilemap: {tilemap.name}", this);
                }
            }
        }
        
        Log.Info(LogModules.TILEMAP, $"找到 {wallTilemaps.Count} 个墙体Tilemap", this);
    }

    /// <summary>
    /// 配置所有墙体Tilemap的碰撞体
    /// </summary>
    public void ConfigureAllWallColliders()
    {
        foreach (Tilemap tilemap in wallTilemaps)
        {
            ConfigureWallCollider(tilemap);
        }
    }

    /// <summary>
    /// 配置单个墙体Tilemap的碰撞体
    /// </summary>
    /// <param name="tilemap">目标Tilemap</param>
    private void ConfigureWallCollider(Tilemap tilemap)
    {
        if (tilemap == null)
            return;

        // 设置墙体游戏对象的标签为Obstacle，使其能被PlayerController的碰撞检测识别
        tilemap.gameObject.tag = "Obstacle";

        // 获取或添加TilemapCollider2D组件
        if (!tilemap.TryGetComponent<TilemapCollider2D>(out var tilemapCollider))
        {
            tilemapCollider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
        }

        // 设置碰撞体类型为Composite以支持复合碰撞体
        tilemapCollider.usedByComposite = useCompositeCollider;

        // 如果需要覆盖瓦片的碰撞体设置
        if (overrideTileColliders)
        {
            OverrideTileColliderTypes(tilemap);
        }

        // 配置复合碰撞体
        if (useCompositeCollider)
        {
            // 获取或添加CompositeCollider2D组件
            if (!tilemap.TryGetComponent<CompositeCollider2D>(out var compositeCollider))
            {
                compositeCollider = tilemap.gameObject.AddComponent<CompositeCollider2D>();
            }
            
            // 设置CompositeCollider2D参数，使碰撞检测更准确
            compositeCollider.generationType = CompositeCollider2D.GenerationType.Synchronous;

            // 配置物理材质
            PhysicsMaterial2D material = null;
            
            // 优先从Rule Tile获取物理材质（如果启用）
            if (useRuleTilePhysicsMaterials)
            {
                material = GetRuleTilePhysicsMaterial(tilemap);
            }
            
            // 如果没有从Rule Tile获取到材质或未启用该功能，则尝试其他方式
            if (material == null)
            {
                if (enableAdvancedPhysicsMaterials)
                {
                    // 尝试查找特定的物理材质配置
                    material = GetPhysicsMaterialForTilemap(tilemap);
                    if (material == null && defaultPhysicsMaterial != null)
                    {
                        material = defaultPhysicsMaterial;
                    }
                }
                else if (defaultPhysicsMaterial != null)
                {
                    material = defaultPhysicsMaterial;
                }
            }
            
            // 应用物理材质
            compositeCollider.sharedMaterial = material;

            // 确保有Rigidbody2D组件并正确配置
            if (!tilemap.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb = tilemap.gameObject.AddComponent<Rigidbody2D>();
            }
            // 设置刚体类型为静态
            rb.bodyType = RigidbodyType2D.Static;
        }
    }
    
    /// <summary>
    /// 从Rule Tile获取物理材质
    /// 检查瓦片地图中使用的Rule Tile是否定义了物理材质
    /// </summary>
    /// <param name="tilemap">目标瓦片地图</param>
    /// <returns>Rule Tile定义的物理材质，如果没有则返回null</returns>
    private PhysicsMaterial2D GetRuleTilePhysicsMaterial(Tilemap tilemap)
    {
        if (tilemap == null)
            return null;
        
        // 简化实现：检查瓦片地图中的第一个瓦片是否是SharedRuleTile
        // 在实际项目中可能需要根据具体需求进行更复杂的处理
        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
            {
                TileBase tile = tilemap.GetTile(position);
                if (tile is SharedRuleTile sharedRuleTile && sharedRuleTile.overrideMapPhysicsMaterial && sharedRuleTile.physicsMaterial != null)
                {
                    return sharedRuleTile.physicsMaterial;
                }
            }
        }
        
        return null;
    }

    /// <summary>
    /// 覆盖瓦片的碰撞体类型
    /// </summary>
    /// <param name="tilemap">目标Tilemap</param>
    private void OverrideTileColliderTypes(Tilemap tilemap)
    {
        // 获取Tilemap的所有瓦片位置
        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
            {
                TileBase tile = tilemap.GetTile(position);
                // 支持Tile和SharedRuleTile两种类型的瓦片
                if (tile is Tile)
                {
                    SetTileColliderType(tile, overrideColliderType);
                }
                // 对于SharedRuleTile，我们确保其碰撞体类型设置正确
                else if (tile is SharedRuleTile)
                {
                    // SharedRuleTile通常有自己的碰撞体配置，这里可以根据需要添加额外逻辑
                    // 由于SharedRuleTile是继承自RuleTile的自定义实现，它可能有自己的碰撞体设置方式
                }
            }
        }
    }

    /// <summary>
    /// 使用反射设置瓦片的碰撞体类型
    /// </summary>
    /// <param name="tile">目标瓦片</param>
    /// <param name="colliderType">碰撞体类型</param>
    private void SetTileColliderType(TileBase tile, Tile.ColliderType colliderType)
    {
        if (tile == null)
            return;

        // 获取Tile类的colliderType字段
        System.Reflection.FieldInfo colliderTypeField = typeof(Tile).GetField("m_ColliderType", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (colliderTypeField != null)
        {
            // 设置碰撞体类型
            colliderTypeField.SetValue(tile, colliderType);
        }
        else
        {
            Log.Warning(LogModules.GAMEMANAGER, "无法设置瓦片的碰撞体类型，请检查Unity版本", this);
        }
    }

    /// <summary>
    /// 开始自动刷新协程
    /// </summary>
    private void StartAutoRefresh()
    {
        refreshCoroutine ??= StartCoroutine(AutoRefreshCoroutine());
    }

    /// <summary>
    /// 自动刷新碰撞体的协程
    /// </summary>
    private IEnumerator AutoRefreshCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshCooldown);
            
            if (smallMapOptimization)
            {
                // 小型地图优化：只刷新一次（如果启用了自动刷新）
                if (Application.isPlaying)
                {
                    ConfigureAllWallColliders();
                    // 对于小型地图，运行时可以考虑禁用自动刷新以提高性能
                    ToggleAutoRefresh();
                }
                else
                {
                    ConfigureAllWallColliders();
                }
            }
            else
            {
                ConfigureAllWallColliders();
            }
        }
    }

    /// <summary>
    /// 切换自动刷新碰撞体功能
    /// </summary>
    public void ToggleAutoRefresh()
    {
        autoRefreshColliders = !autoRefreshColliders;
        
        if (autoRefreshColliders)
        {
            StartAutoRefresh();
            Log.Debug(LogModules.GAMEMANAGER, "已启用碰撞体自动刷新", this);
        }
        else
        {
            if (refreshCoroutine != null)
            {
                StopCoroutine(refreshCoroutine);
                refreshCoroutine = null;
            }
            Log.Debug(LogModules.GAMEMANAGER, "已禁用碰撞体自动刷新", this);
        }
    }

    /// <summary>
    /// 手动刷新所有碰撞体
    /// </summary>
    [ContextMenu("刷新碰撞体")]
    public void RefreshColliders()
    {
        FindWallTilemaps();
        ConfigureAllWallColliders();
        Log.Debug(LogModules.GAMEMANAGER, "已刷新所有碰撞体", this);
    }
    
    /// <summary>
    /// 获取指定瓦片地图应使用的物理材质
    /// </summary>
    /// <param name="tilemap">目标瓦片地图</param>
    /// <returns>应使用的物理材质，如果没有找到特定配置则返回null</returns>
    private PhysicsMaterial2D GetPhysicsMaterialForTilemap(Tilemap tilemap)
    {
        if (tilemap == null || !enableAdvancedPhysicsMaterials)
            return null;

        // 遍历配置列表，查找匹配的配置
        foreach (var config in wallPhysicsMaterialConfigs)
        {
            // 检查标签匹配
            if (!string.IsNullOrEmpty(config.tilemapTag) && tilemap.gameObject.CompareTag(config.tilemapTag))
            {
                return config.physicsMaterial;
            }

            // 检查名称匹配（可以使用通配符）
            if (!string.IsNullOrEmpty(config.tilemapNamePattern) && 
                System.Text.RegularExpressions.Regex.IsMatch(tilemap.name, config.tilemapNamePattern))
            {
                return config.physicsMaterial;
            }

            // 检查是否包含特定组件
            if (config.requiredComponent != null && tilemap.GetComponent(config.requiredComponent) != null)
            {
                return config.physicsMaterial;
            }
        }

        return null;
    }

    /// <summary>
    /// 确保所有独立系统组件已初始化
    /// </summary>
    private void EnsureSystemsAreInitialized()
    {
        // 瓦片地图事件系统已通过TilemapChangeListener自动初始化
        
        // 自动获取或创建空间分割系统
        if (spatialPartitionSystem == null)
        {
            spatialPartitionSystem = FindObjectOfType<SpatialPartitionSystem>();
            if (spatialPartitionSystem == null)
            {
                GameObject spatialSystemObj = new("SpatialPartitionSystem");
                spatialPartitionSystem = spatialSystemObj.AddComponent<SpatialPartitionSystem>();
            }
        }
        
        // 自动获取或创建摄像机视野剔除系统
        if (cameraFrustumCullingSystem == null)
        {
            cameraFrustumCullingSystem = FindObjectOfType<CameraFrustumCullingSystem>();
            if (cameraFrustumCullingSystem == null)
            {
                GameObject cameraSystemObj = new("CameraFrustumCullingSystem");
                cameraFrustumCullingSystem = cameraSystemObj.AddComponent<CameraFrustumCullingSystem>();
            }
        }
    }
    
    /// <summary>
    /// 注册到瓦片地图事件系统
    /// </summary>
    /// <summary>
    /// 注册到瓦片地图事件系统
    /// </summary>
    private void RegisterToEventSystem()
    {
        // 遍历所有墙体Tilemap，开始监听它们的变化
        foreach (var tilemap in wallTilemaps)
        {
            StartListeningTo(tilemap);
        }
    }
    
    /// <summary>
    /// 当瓦片地图的单个瓦片变化时调用
    /// </summary>
    /// <param name="tilemap">变化的瓦片地图</param>
    /// <param name="position">变化的位置</param>
    public override void OnTilemapChanged(Tilemap tilemap, Vector3Int position)
    {
        // 检查该瓦片地图是否是墙体瓦片地图
        if (wallTilemaps.Contains(tilemap))
        {
            // 重新配置该瓦片地图的碰撞体
            ConfigureWallCollider(tilemap);
        }
    }
    
    /// <summary>
    /// 当瓦片地图批量变化时调用
    /// </summary>
    /// <param name="tilemap">变化的瓦片地图</param>
    public override void OnTilemapBatchChanged(Tilemap tilemap)
    {
        // 批量变化时的处理逻辑与单个瓦片变化相同
        OnTilemapChanged(tilemap, Vector3Int.zero);
    }
    
    /// <summary>
    /// 将墙体Tilemap添加到空间分割系统
    /// </summary>
    private void AddTilemapsToSpatialSystem()
    {
        if (spatialPartitionSystem != null)
        {
            foreach (var tilemap in wallTilemaps)
            {
                spatialPartitionSystem.AddTilemap(tilemap);
            }
        }
    }
    

    
    /// <summary>
    /// 刷新指定区域内的碰撞体（通过空间分割系统）
    /// </summary>
    /// <param name="worldPosition">中心点世界位置</param>
    /// <param name="radius">刷新半径（单位：瓦片）</param>
    public void RefreshCollidersInArea(Vector3 worldPosition, int radius)
    {
        if (spatialPartitionSystem != null)
        {
            List<Tilemap> tilemapsInArea = spatialPartitionSystem.GetTilemapsInRadius(worldPosition, radius);
            
            foreach (var tilemap in tilemapsInArea)
            {
                if (wallTilemaps.Contains(tilemap))
                {
                    ConfigureWallCollider(tilemap);
                }
            }
            
            Log.Debug(LogModules.GAMEMANAGER, string.Format("刷新位置 {0} 周围半径 {1} 的碰撞体", worldPosition, radius), this);
        }
        else
        {
            // 如果没有空间分割系统，刷新所有碰撞体
            RefreshColliders();
        }
    }
    
    /// <summary>
    /// 刷新视野内的碰撞体（通过视野剔除系统）
    /// </summary>
    public void RefreshVisibleAreaColliders()
    {
        if (cameraFrustumCullingSystem != null)
        {
            foreach (var tilemap in wallTilemaps)
            {
                cameraFrustumCullingSystem.RefreshVisibleAreaColliders(tilemap, (t, pos) => 
                {
                    // 在这里可以实现对单个瓦片的碰撞体刷新
                    // 对于简化实现，我们直接重新配置整个瓦片地图
                    ConfigureWallCollider(t);
                });
            }
        }
        else
        {
            // 如果没有视野剔除系统，刷新所有碰撞体
            RefreshColliders();
        }
    }
    
    /// <summary>
    /// 切换小型地图优化模式
    /// </summary>
    [ContextMenu("切换小型地图优化模式")]
    public void ToggleSmallMapOptimization()
    {
        smallMapOptimization = !smallMapOptimization;
        Log.Debug(LogModules.GAMEMANAGER, string.Format("小型地图优化模式已{0}", smallMapOptimization ? "启用" : "禁用"), this);
        
        if (smallMapOptimization)
        {
            // 小型地图优化设置
            autoRefreshColliders = false;
            refreshCooldown = 5.0f;
        }
        else
        {
            // 恢复默认设置
            autoRefreshColliders = true;
            refreshCooldown = 2.0f;
        }
    }
    
    /// <summary>
    /// 重置所有系统组件
    /// </summary>
    [ContextMenu("重置系统组件")]
    public void ResetSystems()
    {
        EnsureSystemsAreInitialized();
        RegisterToEventSystem();
        AddTilemapsToSpatialSystem();
        Log.Debug(LogModules.GAMEMANAGER, "系统组件已重置", this);
    }
}

/// <summary>
/// 墙体物理材质配置类
/// 用于为不同类型的墙体设置不同的物理材质
/// </summary>
[System.Serializable]
public class WallPhysicsMaterialConfig
{
    [Tooltip("瓦片地图的标签")]
    public string tilemapTag = "";

    [Tooltip("瓦片地图名称模式（支持正则表达式）")]
    public string tilemapNamePattern = "";

    [Tooltip("瓦片地图必须包含的组件类型")]
    public System.Type requiredComponent = null;

    [Tooltip("此类型墙体使用的物理材质")]
    public PhysicsMaterial2D physicsMaterial = null;
}