using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

/// <summary>
/// 瓦片碰撞体类型编辑器
/// 提供一个简单的界面来设置瓦片的碰撞体类型，包括Sprite类型
/// </summary>
public class TileColliderTypeEditor : EditorWindow
{
    private SharedRuleTile selectedTile;
    private Tile.ColliderType colliderType = Tile.ColliderType.Sprite;
    private bool showInstructions = true;

    // 添加到Unity编辑器菜单
    [MenuItem("Tools/TileMap/瓦片碰撞体类型设置器")]
    public static void ShowWindow()
    {
        GetWindow<TileColliderTypeEditor>("瓦片碰撞体设置");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("瓦片碰撞体类型设置", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 选择瓦片
        selectedTile = (SharedRuleTile)EditorGUILayout.ObjectField(
            "选择瓦片", 
            selectedTile, 
            typeof(SharedRuleTile), 
            false);

        GUILayout.Space(10);

        // 选择碰撞体类型
        EditorGUILayout.LabelField("选择碰撞体类型:");
        colliderType = (Tile.ColliderType)EditorGUILayout.EnumPopup(colliderType);

        GUILayout.Space(10);

        // 设置碰撞体类型按钮
        if (selectedTile != null)
        {
            GUI.enabled = true;
            
            if (GUILayout.Button("应用碰撞体类型设置"))
            {
                SetTileColliderType(selectedTile, colliderType);
                Debug.LogFormat("已将瓦片 '{0}' 的碰撞体类型设置为: {1}", 
                    selectedTile.name, colliderType);
            }
        }
        else
        {
            GUI.enabled = false;
            GUILayout.Button("请先选择一个瓦片");
            GUI.enabled = true;
        }

        // 快速设置按钮
        GUILayout.Space(15);
        EditorGUILayout.LabelField("快速设置:", EditorStyles.boldLabel);
        
        GUILayout.BeginHorizontal();
        if (selectedTile != null)
        {
            if (GUILayout.Button("设置为Sprite"))
            {
                SetTileColliderType(selectedTile, Tile.ColliderType.Sprite);
                Debug.LogFormat("已将瓦片 '{0}' 的碰撞体类型设置为: Sprite", selectedTile.name);
            }
            
            if (GUILayout.Button("设置为Grid"))
            {
                SetTileColliderType(selectedTile, Tile.ColliderType.Grid);
                Debug.LogFormat("已将瓦片 '{0}' 的碰撞体类型设置为: Grid", selectedTile.name);
            }
            
            if (GUILayout.Button("设置为None"))
            {
                SetTileColliderType(selectedTile, Tile.ColliderType.None);
                Debug.LogFormat("已将瓦片 '{0}' 的碰撞体类型设置为: None", selectedTile.name);
            }
        }
        else
        {
            GUI.enabled = false;
            GUILayout.Button("设置为Sprite");
            GUILayout.Button("设置为Grid");
            GUILayout.Button("设置为None");
            GUI.enabled = true;
        }
        GUILayout.EndHorizontal();

        // 使用说明
        showInstructions = EditorGUILayout.Foldout(showInstructions, "使用说明");
        if (showInstructions)
        {
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("碰撞体类型说明:");
            EditorGUILayout.LabelField("- Sprite: 根据瓦片的精灵形状创建碰撞体");
            EditorGUILayout.LabelField("- Grid: 创建与瓦片网格大小相同的矩形碰撞体");
            EditorGUILayout.LabelField("- None: 不创建任何碰撞体");
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("使用步骤:");
            EditorGUILayout.LabelField("1. 在上方选择要设置的瓦片");
            EditorGUILayout.LabelField("2. 选择要应用的碰撞体类型");
            EditorGUILayout.LabelField("3. 点击'应用碰撞体类型设置'按钮");
            EditorGUILayout.LabelField("4. 或使用快速设置按钮直接应用常用类型");
            EditorGUILayout.EndVertical();
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
            EditorUtility.SetDirty(tile);
            AssetDatabase.SaveAssets();
        }
        else
        {
            Debug.LogWarning("无法设置瓦片的碰撞体类型，请检查Unity版本");
        }
    }

    // 当选择的对象改变时更新窗口
    private void OnSelectionChange()
    {
        if (Selection.activeObject is SharedRuleTile tile)
        {
            selectedTile = tile;
            // 尝试获取当前碰撞体类型
            GetCurrentColliderType(selectedTile);
            Repaint();
        }
    }

    /// <summary>
    /// 获取瓦片当前的碰撞体类型
    /// </summary>
    /// <param name="tile">目标瓦片</param>
    private void GetCurrentColliderType(TileBase tile)
    {
        if (tile == null)
            return;

        // 获取Tile类的colliderType字段
        System.Reflection.FieldInfo colliderTypeField = typeof(Tile).GetField("m_ColliderType", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (colliderTypeField != null)
        {
            // 获取当前碰撞体类型
            object value = colliderTypeField.GetValue(tile);
            if (value is Tile.ColliderType type)
            {
                colliderType = type;
            }
        }
    }
}