using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 墙体碰撞体管理器
/// 自动为墙体Tilemap配置碰撞体组件
/// </summary>
public class WallColliderManager : MonoBehaviour
{
    [Header("墙体层配置")]
    [Tooltip("墙体Tilemap所在的图层名称")]
    public string wallLayerName = "Wall";

    [Tooltip("是否使用复合碰撞体")]
    public bool useCompositeCollider = true;

    [Tooltip("碰撞体的物理材质")]
    public PhysicsMaterial2D physicsMaterial;

    [Tooltip("是否在运行时自动刷新碰撞体")]
    public bool autoRefreshColliders = false;

    [Tooltip("自动刷新的时间间隔（秒）")]
    [Range(0.1f, 5f)]
    public float refreshInterval = 1f;

    [Tooltip("是否启用瓦片碰撞体覆盖")]
    public bool overrideTileColliders = false;

    [Tooltip("当覆盖瓦片碰撞体时使用的碰撞体类型")]
    public Tile.ColliderType overrideColliderType = Tile.ColliderType.Grid;

    // 存储找到的墙体Tilemap
    private List<Tilemap> wallTilemaps = new List<Tilemap>();
    private Coroutine refreshCoroutine;

    /// <summary>
    /// 初始化时查找并配置墙体Tilemap的碰撞体
    /// </summary>
    private void Awake()
    {
        FindWallTilemaps();
        ConfigureColliders();
    }

    /// <summary>
    /// 启用时启动自动刷新（如果启用）
    /// </summary>
    private void OnEnable()
    {
        if (autoRefreshColliders && refreshCoroutine == null)
        {
            refreshCoroutine = StartCoroutine(AutoRefreshColliders());
        }
    }

    /// <summary>
    /// 禁用时停止自动刷新
    /// </summary>
    private void OnDisable()
    {
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
            refreshCoroutine = null;
        }
    }

    /// <summary>
    /// 查找场景中所有墙体Tilemap
    /// </summary>
    private void FindWallTilemaps()
    {
        // 清空之前的列表
        wallTilemaps.Clear();

        // 查找所有Tilemap组件
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();

        foreach (Tilemap tilemap in allTilemaps)
        {
            // 检查Tilemap所在的图层名称是否匹配
            if (tilemap.gameObject.layer == LayerMask.NameToLayer(wallLayerName))
            {
                wallTilemaps.Add(tilemap);
                Debug.Log("找到墙体Tilemap: " + tilemap.name);
            }
        }

        if (wallTilemaps.Count == 0)
        {
            Debug.LogWarning("未找到名为'" + wallLayerName + "'的图层上的Tilemap。请确保图层名称正确，并已将墙体Tilemap设置到该图层。");
        }
    }

    /// <summary>
    /// 为墙体Tilemap配置碰撞体组件
    /// </summary>
    private void ConfigureColliders()
    {
        foreach (Tilemap tilemap in wallTilemaps)
        {
            // 添加或获取TilemapCollider2D组件
            TilemapCollider2D collider = tilemap.GetComponent<TilemapCollider2D>();
            if (collider == null)
            {
                collider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
            }

            // 设置碰撞体属性
            collider.usedByComposite = useCompositeCollider;

            if (physicsMaterial != null)
            {
                collider.sharedMaterial = physicsMaterial;
            }

            // 如果需要复合碰撞体，添加CompositeCollider2D组件
            if (useCompositeCollider)
            {
                CompositeCollider2D compositeCollider = tilemap.GetComponent<CompositeCollider2D>();
                if (compositeCollider == null)
                {
                    compositeCollider = tilemap.gameObject.AddComponent<CompositeCollider2D>();
                }

                // 添加Rigidbody2D组件（复合碰撞体需要）
                Rigidbody2D rb = tilemap.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = tilemap.gameObject.AddComponent<Rigidbody2D>();
                }
                rb.bodyType = RigidbodyType2D.Static;
            }
            else
            {
                // 如果不需要复合碰撞体，移除相关组件
                CompositeCollider2D compositeCollider = tilemap.GetComponent<CompositeCollider2D>();
                if (compositeCollider != null)
                {
                    Destroy(compositeCollider);
                }
            }

            Debug.Log("已为Tilemap '" + tilemap.name + "' 配置碰撞体");
        }
    }

    /// <summary>
    /// 手动刷新碰撞体配置（可从编辑器或其他脚本调用）
    /// </summary>
    public void RefreshColliders()
    {
        FindWallTilemaps();
        ConfigureColliders();
    }

    /// <summary>
    /// 自动刷新碰撞体的协程
    /// </summary>
    private IEnumerator AutoRefreshColliders()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            ConfigureColliders(); // 注意：这里只刷新碰撞体配置，不重新查找Tilemap
        }
    }

    /// <summary>
    /// 覆盖瓦片的碰撞体类型（仅在编辑器模式下有效）
    /// </summary>
    [ContextMenu("覆盖瓦片碰撞体")]
    public void OverrideTilesColliderType()
    {
#if UNITY_EDITOR
        if (!overrideTileColliders)
        {
            Debug.LogWarning("瓦片碰撞体覆盖功能未启用");
            return;
        }

        FindWallTilemaps();

        foreach (Tilemap tilemap in wallTilemaps)
        {
            // 获取瓦片地图中的所有瓦片位置
            BoundsInt bounds = tilemap.cellBounds;
            TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    TileBase tile = allTiles[x + y * bounds.size.x];
                    if (tile != null)
                    {
                        // 使用反射设置瓦片的碰撞体类型
                        SetTileColliderType(tile, overrideColliderType);
                    }
                }
            }

            Debug.Log("已覆盖Tilemap '" + tilemap.name + "' 中所有瓦片的碰撞体类型");
        }
#endif
    }

    /// <summary>
    /// 使用反射设置瓦片的碰撞体类型
    /// </summary>
    /// <param name="tile">目标瓦片</param>
    /// <param name="colliderType">碰撞体类型</param>
    private void SetTileColliderType(TileBase tile, Tile.ColliderType colliderType)
    {
#if UNITY_EDITOR
        // 获取Tile类的colliderType字段
        System.Reflection.FieldInfo colliderTypeField = typeof(Tile).GetField("m_ColliderType",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (colliderTypeField != null)
        {
            // 设置碰撞体类型
            colliderTypeField.SetValue(tile, colliderType);
            UnityEditor.EditorUtility.SetDirty(tile);
        }
#endif
    }
}