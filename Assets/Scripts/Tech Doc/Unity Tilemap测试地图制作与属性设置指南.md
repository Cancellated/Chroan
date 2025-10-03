# Unity Tilemap测试地图制作与属性设置指南

## 1. 概述

本指南详细介绍如何在Unity中创建第一个测试地图，并通过Tilemap系统设置不同tile的物理性质，如墙体的不可通行性。这是实现玩家在网格上精确移动的基础步骤。

## 2. 准备工作

### 2.1 资源准备

1. 确保您已准备好所需的tile资源（精灵图集或单个精灵）
2. 建议将所有地图资源放入`Assets/Art Asset/Tile Map`目录下以便管理

### 2.2 创建基本场景结构

1. 在Unity中创建一个新场景或打开现有场景
2. 添加必要的组件：
   - 主相机（已默认添加）
   - 灯光（如果需要）
   - Tilemap组件（接下来将详细介绍）

## 3. 创建Tilemap并制作测试地图

### 3.1 添加Tilemap组件

1. 在Hierarchy面板中右键点击，选择`2D Object > Tilemap > Rectangular`
2. 这将创建一个Grid游戏对象，其中包含一个Tilemap子对象
3. 将Grid对象重命名为"MapGrid"，将Tilemap对象重命名为"Ground"（表示这是地面层）

### 3.2 创建Tile Palette

1. 打开Window > 2D > Tile Palette面板
2. 在Tile Palette面板中，点击"Create New Palette"
3. 设置Palette名称（如"TestMapPalette"），选择保存位置（建议保存在`Assets/Art Asset/Tile Map/Palettes`）
4. 点击"Create"

### 3.3 添加Tile到Palette

1. 将您的tile精灵从Project窗口拖放到Tile Palette面板中
2. 在弹出的对话框中，选择保存位置（建议保存在`Assets/Art Asset/Tile Map/Tiles`）
3. 点击"Save"
4. 现在您的tile应该显示在Palette中，可以随时使用

### 3.4 绘制测试地图

1. 在Tile Palette面板中，确保选择了"Paint"工具（刷漆图标）
2. 选择您想要绘制的tile
3. 在Scene或Game视图中，点击或拖动以在Tilemap上绘制tile
4. 如需擦除，选择"Erase"工具（橡皮擦图标）

## 4. 设置Tile的物理性质

### 4.1 使用Tilemap Collider 2D实现基本碰撞

1. 选择您的Tilemap对象（如"Ground"）
2. 添加`Tilemap Collider 2D`组件
3. 默认情况下，这个组件会为所有非空的tile单元格添加碰撞体
4. 如需优化，勾选"Used By Composite"选项
5. 添加`Composite Collider 2D`组件，系统会自动将Tilemap Collider 2D的"Used By Composite"选项勾上
6. 对于需要物理响应的Tilemap，您还可以添加`Rigidbody 2D`组件，并设置为"Static"

### 4.2 创建不同功能的Tilemap层

为了更好地管理不同类型的tile（如地面、墙体、水域等），建议创建多层Tilemap：

1. 右键点击Grid对象，选择`2D Object > Tilemap > Rectangular`添加新的Tilemap
2. 将新的Tilemap重命名为"Walls"（表示墙体层）
3. 对Walls层添加`Tilemap Collider 2D`和`Composite Collider 2D`组件
4. 根据需要创建更多层，如"Water"、"Interactive"等
5. 在Layers窗口中调整各层的顺序，确保视觉上的正确性

### 4.3 使用Rule Tile或Tilemap Editor设置特定tile的属性

若要使特定类型的tile（如墙体）具有不可通行性质，您有以下几种方法：

#### 方法1：使用Rule Tile（推荐）

1. 创建Rule Tile：右键点击Project窗口，选择`Create > 2D > Tiles > Rule Tile`
2. 将Rule Tile命名为"WallTile"
3. 点击打开Rule Tile，设置Default Sprite为您的墙体精灵
4. 在"Rules"区域，点击"Add Rule"
5. 设置规则以定义墙体的行为（例如，设置所有方向都不可通行）
6. 将Rule Tile添加到您的Tile Palette中并使用

#### 方法2：通过代码检测特定tile类型

您可以在移动控制系统中添加代码，检测玩家要移动到的格子是否包含不可通行的tile：

```csharp
/// <summary>
/// 检查网格位置是否可通行
/// </summary>
/// <param name="gridPosition">要检查的网格位置</param>
/// <returns>如果可通行则返回true，否则返回false</returns>
private bool IsCellWalkable(Vector3Int gridPosition)
{
    // 1. 检查是否在Tilemap边界内
    if (!groundTilemap.HasTile(gridPosition))
    {
        return false;
    }
    
    // 2. 检查是否有墙体tile
    if (wallsTilemap.HasTile(gridPosition))
    {
        return false;
    }
    
    // 3. 使用Physics2D进行精确碰撞检测
    Vector3 worldPosition = groundTilemap.GetCellCenterWorld(gridPosition);
    Collider2D[] colliders = Physics2D.OverlapBoxAll(
        worldPosition, 
        new Vector2(0.9f, 0.9f), // 略小于单元格大小，避免边缘检测问题
        0f
    );
    
    foreach (Collider2D collider in colliders)
    {
        // 检查是否有不可通行的碰撞体
        if (collider.CompareTag("Obstacle"))
        {
            return false;
        }
    }
    
    return true;
}
```

#### 方法3：使用Tilemap的标签或自定义属性

1. 选择您的墙体tile精灵
2. 在Inspector面板中，为其添加标签（如"Wall"）或设置自定义属性
3. 在代码中，通过检查tile的标签或属性来确定其是否可通行：

```csharp
/// <summary>
/// 检查tile是否可通行
/// </summary>
/// <param name="tilemap">要检查的Tilemap</param>
/// <param name="gridPosition">网格位置</param>
/// <returns>是否可通行</returns>
private bool IsTileWalkable(Tilemap tilemap, Vector3Int gridPosition)
{
    TileBase tile = tilemap.GetTile(gridPosition);
    if (tile == null)
        return false;
    
    // 检查tile的名称是否包含特定关键词
    if (tile.name.Contains("Wall"))
        return false;
    
    // 或者检查tile是否有特定的标签或属性
    // ...
    
    return true;
}
```

## 5. 完整的测试地图设置示例

### 5.1 场景结构

```
- MapGrid (Grid组件)
  - Ground (Tilemap组件，地面层)
    - Tilemap Renderer
    - Tilemap Collider 2D (Used By Composite勾选)
    - Composite Collider 2D
    - Rigidbody 2D (Body Type: Static)
  - Walls (Tilemap组件，墙体层)
    - Tilemap Renderer
    - Tilemap Collider 2D (Used By Composite勾选)
    - Composite Collider 2D
    - Rigidbody 2D (Body Type: Static)
  - Interactive (Tilemap组件，交互层)
    - Tilemap Renderer
- Player (玩家角色)
  - Sprite Renderer
  - Box Collider 2D
  - Rigidbody 2D (Body Type: Kinematic)
  - PlayerController (自定义脚本)
```

### 5.2 测试地图制作步骤

1. **规划地图布局**：确定地图的大小、形状和主要元素（如起点、障碍物、终点等）
2. **绘制地面层**：在Ground层上绘制基本地形
3. **添加障碍物**：在Walls层上绘制墙体等不可通行的元素
4. **设置交互元素**：在Interactive层上添加可交互的物体
5. **调整碰撞设置**：为各层添加适当的碰撞组件
6. **放置玩家角色**：将玩家角色放置在起始位置
7. **测试移动**：运行游戏，测试玩家是否能按预期在地图上移动

## 6. 优化与调试技巧

### 6.1 性能优化

1. **合并碰撞体**：使用Composite Collider 2D合并多个小碰撞体
2. **使用Layers**：为不同类型的tile使用不同的层，便于管理和优化
3. **减少碰撞检测范围**：在代码中优化碰撞检测逻辑，只检测必要的区域

### 6.2 调试技巧

1. **可视化网格**：添加Gizmos代码以在Scene视图中显示网格线
2. **碰撞调试**：使用Debug.DrawRay或类似方法可视化碰撞检测
3. **移动跟踪**：添加日志记录玩家的移动尝试和结果

```csharp
private void OnDrawGizmosSelected()
{
    // 绘制可通行区域
    Gizmos.color = Color.green;
    for (int x = -10; x < 10; x++)
    {
        for (int y = -10; y < 10; y++)
        {
            Vector3Int gridPos = new Vector3Int(x, y, 0);
            if (IsCellWalkable(gridPos))
            {
                Vector3 worldPos = groundTilemap.GetCellCenterWorld(gridPos);
                Gizmos.DrawWireCube(worldPos, new Vector3(0.9f, 0.9f, 0));
            }
        }
    }
}
```

## 7. 扩展功能

### 7.1 动态改变tile属性

在游戏过程中，您可能需要动态改变tile的属性，例如打开一扇门：

```csharp
/// <summary>
/// 移除指定位置的墙体tile
/// </summary>
/// <param name="gridPosition">要移除墙体的网格位置</param>
public void RemoveWall(Vector3Int gridPosition)
{
    wallsTilemap.SetTile(gridPosition, null);
    // 可以添加开门动画或音效
}
```

### 7.2 创建自定义Tile类

对于更复杂的需求，您可以创建自定义Tile类：

```csharp
[CreateAssetMenu(fileName = "New Custom Tile", menuName = "2D/Tiles/Custom Tile")]
public class CustomTile : TileBase
{
    public Sprite sprite;
    public bool isWalkable = true;
    public int movementCost = 1;
    public string tileType = "Ground";
    
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
        tileData.color = Color.white;
        tileData.transform = Matrix4x4.identity;
        tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
        tileData.colliderType = isWalkable ? Tile.ColliderType.None : Tile.ColliderType.Sprite;
    }
}
```

## 8. 注意事项

1. **像素对齐**：确保Tilemap的Cell Size与您的精灵大小匹配
2. **层级顺序**：调整不同Tilemap层的Order in Layer属性，确保正确的视觉层级
3. **性能考虑**：对于大型地图，考虑使用分割加载或动态加载技术
4. **版本控制**：Tilemap数据可能较大，确保您的版本控制系统正确处理Unity场景文件

通过以上步骤，您可以创建具有不同物理性质的测试地图，使palette中的tile在绘制时自动具有相应的属性（如墙体的不可通行性）。这将为您的游戏提供坚实的基础，实现精确的角色移动和交互。