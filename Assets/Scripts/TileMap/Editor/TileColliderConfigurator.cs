using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 瓦片碰撞体配置器
/// 用于在Unity编辑器中为瓦片设置碰撞体形状
/// </summary>
[CustomEditor(typeof(SharedRuleTile))]
public class TileColliderConfigurator : Editor
{
    private SharedRuleTile targetTile;
    private bool showColliderSettings = true;
    private bool showUsageInstructions = true;

    /// <summary>
    /// 当选中瓦片时初始化
    /// </summary>
    private void OnEnable()
    {
        targetTile = (SharedRuleTile)target;
    }

    /// <summary>
    /// 自定义编辑器界面
    /// </summary>
    public override void OnInspectorGUI()
    {
        // 绘制默认的编辑器界面
        DrawDefaultInspector();

        // 添加碰撞体配置区域
        showColliderSettings = EditorGUILayout.Foldout(showColliderSettings, "碰撞体设置");
        if (showColliderSettings)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 为瓦片添加碰撞体按钮
            if (GUILayout.Button("为瓦片添加矩形碰撞体"))
            {
                AddBoxColliderToTile();
            }

            // 为瓦片添加自定义多边形碰撞体按钮
            if (GUILayout.Button("为瓦片添加多边形碰撞体"))
            {
                AddPolygonColliderToTile();
            }

            // 清除瓦片碰撞体按钮
            if (GUILayout.Button("清除瓦片碰撞体"))
            {
                ClearTileCollider();
            }

            EditorGUILayout.EndVertical();
        }

        // 添加使用说明
        showUsageInstructions = EditorGUILayout.Foldout(showUsageInstructions, "使用说明");
        if (showUsageInstructions)
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("1. 确保墙体Tilemap已设置到正确的图层（默认为'Wall'）");
            EditorGUILayout.LabelField("2. 在场景中添加WallColliderManager脚本");
            EditorGUILayout.LabelField("3. 点击上方按钮为瓦片添加碰撞体");
            EditorGUILayout.LabelField("4. 运行游戏后，WallColliderManager会自动配置所有墙体Tilemap的碰撞体组件");
            EditorGUILayout.EndVertical();
        }

        // 如果有修改，标记为需要保存
        if (GUI.changed)
        {
            EditorUtility.SetDirty(targetTile);
        }
    }

    /// <summary>
    /// 为瓦片添加矩形碰撞体
    /// </summary>
    private void AddBoxColliderToTile()
    {
        // 由于SharedRuleTile继承自RuleTile，我们需要使用反射来设置碰撞体类型
        // 或者使用UnityEditor的Tilemap API来处理
        SetColliderType(targetTile, Tile.ColliderType.Grid);
        Debug.Log("已为瓦片添加矩形碰撞体");
    }

    /// <summary>
    /// 为瓦片添加多边形碰撞体
    /// </summary>
    private void AddPolygonColliderToTile()
    {
        // 设置为Grid类型的碰撞体，这会根据瓦片的网格形状创建碰撞体
        SetColliderType(targetTile, Tile.ColliderType.Grid);
        Debug.Log("已为瓦片添加多边形碰撞体");
    }

    /// <summary>
    /// 清除瓦片的碰撞体
    /// </summary>
    private void ClearTileCollider()
    {
        SetColliderType(targetTile, Tile.ColliderType.None);
        Debug.Log("已清除瓦片碰撞体");
    }

    /// <summary>
    /// 使用反射设置瓦片的碰撞体类型
    /// </summary>
    /// <param name="tile">目标瓦片</param>
    /// <param name="colliderType">碰撞体类型</param>
    private void SetColliderType(TileBase tile, Tile.ColliderType colliderType)
    {
        // 获取Tile类的colliderType字段
        System.Reflection.FieldInfo colliderTypeField = typeof(Tile).GetField("m_ColliderType", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (colliderTypeField != null)
        {
            // 设置碰撞体类型
            colliderTypeField.SetValue(tile, colliderType);
            EditorUtility.SetDirty(tile);
        }
        else
        {
            Debug.LogWarning("无法设置瓦片的碰撞体类型，请检查Unity版本");
        }
    }
}