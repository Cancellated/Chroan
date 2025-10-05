using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 墙体碰撞体演示脚本
/// 用于展示如何在运行时动态添加和配置墙体碰撞体
/// </summary>
public class WallColliderDemo : MonoBehaviour
{
    [Header("演示设置")]
    [Tooltip("墙体Tilemap预制体")]
    public GameObject wallTilemapPrefab;

    [Tooltip("墙体管理器")]
    public WallColliderManager wallColliderManager;

    [Tooltip("用于放置墙体的网格布局")]
    public Grid grid;

    [Tooltip("演示墙体的瓦片")]
    public TileBase wallTile;

    /// <summary>
    /// 在编辑器中显示辅助信息
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (grid != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(grid.transform.position, new Vector3(5, 5, 0.1f));
            Gizmos.color = Color.white;
        }
    }

    /// <summary>
    /// 生成随机墙体演示
    /// 可以通过UI按钮或其他方式调用此方法
    /// </summary>
    public void GenerateRandomWalls()
    {
        if (grid == null || wallTilemapPrefab == null || wallTile == null)
        {
            Debug.LogWarning("请确保设置了所有必要的组件：grid、wallTilemapPrefab和wallTile");
            return;
        }

        // 创建新的墙体Tilemap
        GameObject wallGO = Instantiate(wallTilemapPrefab, grid.transform);
        wallGO.name = "DemoWall";
        wallGO.layer = LayerMask.NameToLayer("Wall"); // 确保设置到正确的图层

        Tilemap tilemap = wallGO.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogWarning("墙体预制体中没有找到Tilemap组件");
            return;
        }

        // 随机生成一些墙体瓦片
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                // 70%的概率放置墙体瓦片
                if (Random.value < 0.7f)
                {
                    Vector3Int cellPosition = new Vector3Int(x, y, 0);
                    tilemap.SetTile(cellPosition, wallTile);
                }
            }
        }

        // 刷新碰撞体配置
        if (wallColliderManager != null)
        {
            wallColliderManager.RefreshColliders();
        }

        Debug.Log("已生成随机墙体并配置碰撞体");
    }

    /// <summary>
    /// 清除所有演示墙体
    /// 可以通过UI按钮或其他方式调用此方法
    /// </summary>
    public void ClearAllWalls()
    {
        if (grid == null)
        {
            return;
        }

        // 查找所有子物体中的墙体Tilemap
        Tilemap[] childTilemaps = grid.GetComponentsInChildren<Tilemap>();
        foreach (Tilemap tilemap in childTilemaps)
        {
            if (tilemap.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                // 清除瓦片但保留GameObject
                tilemap.ClearAllTiles();
            }
        }

        // 刷新碰撞体配置
        if (wallColliderManager != null)
        {
            wallColliderManager.RefreshColliders();
        }

        Debug.Log("已清除所有墙体瓦片");
    }

    /// <summary>
    /// 切换自动刷新碰撞体功能
    /// </summary>
    public void ToggleAutoRefresh()
    {
        if (wallColliderManager != null)
        {
            wallColliderManager.autoRefreshColliders = !wallColliderManager.autoRefreshColliders;
            Debug.Log("自动刷新碰撞体功能已" + (wallColliderManager.autoRefreshColliders ? "启用" : "禁用"));
        }
    }
}