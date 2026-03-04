using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// 瓦片地图事件系统
/// 负责监听和分发瓦片地图的变化事件
/// </summary>
public class TilemapEventSystem : MonoBehaviour
{
    private readonly List<TilemapChangeListener> changeListeners = new();
    private static TilemapEventSystem instance;
    
    public static TilemapEventSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TilemapEventSystem>();
                if (instance == null)
                {
                    GameObject obj = new("TilemapEventSystem");
                    instance = obj.AddComponent<TilemapEventSystem>();
                    // 标记为不跨场景销毁，确保在场景切换时正确管理
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// 注册瓦片地图变化监听器
    /// </summary>
    /// <param name="listener">要注册的监听器</param>
    public void RegisterListener(TilemapChangeListener listener)
    {
        if (!changeListeners.Contains(listener))
        {
            changeListeners.Add(listener);
        }
    }

    /// <summary>
    /// 取消注册瓦片地图变化监听器
    /// </summary>
    /// <param name="listener">要取消注册的监听器</param>
    public void UnregisterListener(TilemapChangeListener listener)
    {
        changeListeners.Remove(listener);
    }

    /// <summary>
    /// 触发瓦片地图变化事件
    /// </summary>
    /// <param name="tilemap">变化的瓦片地图</param>
    /// <param name="position">变化的位置</param>
    public void TriggerTilemapChanged(Tilemap tilemap, Vector3Int position)
    {
        foreach (var listener in changeListeners)
        {
            if (listener.IsListeningToTilemap(tilemap))
            {
                listener.OnTilemapChanged(tilemap, position);
            }
        }
    }

    /// <summary>
    /// 触发瓦片地图批量变化事件
    /// </summary>
    /// <param name="tilemap">变化的瓦片地图</param>
    public void TriggerTilemapBatchChanged(Tilemap tilemap)
    {
        foreach (var listener in changeListeners)
        {
            if (listener.IsListeningToTilemap(tilemap))
            {
                listener.OnTilemapBatchChanged(tilemap);
            }
        }
    }
}

/// <summary>
/// 瓦片地图变化监听器接口
/// </summary>
public abstract class TilemapChangeListener : MonoBehaviour
{
    protected readonly List<Tilemap> listeningTilemaps = new();

    /// <summary>
    /// 开始监听指定的瓦片地图
    /// </summary>
    /// <param name="tilemap">要监听的瓦片地图</param>
    public void StartListeningTo(Tilemap tilemap)
    {
        if (!listeningTilemaps.Contains(tilemap))
        {
            listeningTilemaps.Add(tilemap);
            TilemapEventSystem.Instance.RegisterListener(this);
        }
    }

    /// <summary>
    /// 停止监听指定的瓦片地图
    /// </summary>
    /// <param name="tilemap">要停止监听的瓦片地图</param>
    public void StopListeningTo(Tilemap tilemap)
    {
        listeningTilemaps.Remove(tilemap);
        if (listeningTilemaps.Count == 0)
        {
            TilemapEventSystem.Instance.UnregisterListener(this);
        }
    }

    /// <summary>
    /// 检查是否正在监听指定的瓦片地图
    /// </summary>
    /// <param name="tilemap">要检查的瓦片地图</param>
    /// <returns>是否正在监听</returns>
    public bool IsListeningToTilemap(Tilemap tilemap)
    {
        return listeningTilemaps.Contains(tilemap);
    }

    /// <summary>
    /// 当瓦片地图的单个瓦片变化时调用
    /// </summary>
    /// <param name="tilemap">变化的瓦片地图</param>
    /// <param name="position">变化的位置</param>
    public abstract void OnTilemapChanged(Tilemap tilemap, Vector3Int position);

    /// <summary>
    /// 当瓦片地图批量变化时调用
    /// </summary>
    /// <param name="tilemap">变化的瓦片地图</param>
    public abstract void OnTilemapBatchChanged(Tilemap tilemap);

    /// <summary>
    /// 清理监听
    /// </summary>
    protected virtual void OnDestroy()
    {
        TilemapEventSystem.Instance.UnregisterListener(this);
    }
}