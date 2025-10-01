# Inventory模块MVC优化技术方案
# Inventory模块MVC优化技术方案

## 1. 概述

本文档提供对Inventory目录下MVC实现的详细分析和优化建议。目前的实现是一个基础但相对标准的MVC模式，但仍有一些改进空间，可以进一步提高代码质量、性能和可维护性。

## 2. 当前实现分析

### 2.1 整体架构

当前Inventory模块采用经典的MVC三层架构：
- **Model层**：`InventoryModel.cs` - 负责数据管理和业务逻辑
- **View层**：`InventoryView.cs` - 负责UI渲染和用户交互
- **Controller层**：`InventoryController.cs` - 连接Model和View，处理用户输入

### 2.2 各层具体分析

#### Model层 (InventoryModel.cs)
- **优点**：
  - 数据封装良好，通过私有字段和公共属性保护数据
  - 业务逻辑（添加、移除、移动物品）实现完整
  - 使用事件机制通知视图更新
  - 不依赖视图组件，符合MVC分离原则

- **待优化点**：
  - 物品移动逻辑可以更完善（如支持空位插入）
  - 缺乏对物品使用效果的支持
  - 没有实现物品分类或过滤功能

#### View层 (InventoryView.cs)
- **优点**：
  - UI组件引用清晰
  - 实现了拖拽功能
  - 提供了槽位更新和容量显示的方法

- **待优化点**：
  - 直接依赖Controller单例，耦合度较高
  - 拖拽结束处理逻辑简单
  - 没有实现物品详情显示UI

#### Controller层 (InventoryController.cs)
- **优点**：
  - 使用单例模式便于全局访问
  - 连接了Model和View
  - 实现了快捷键触发
  - 响应模型变化更新视图

- **待优化点**：
  - 存在未实现的方法（如`UseItem(ItemData)`）
  - 物品使用逻辑简单
  - 依赖项管理可以更灵活

## 3. 优化建议

### 3.1 降低View与Controller的直接依赖

**当前问题**：View层直接通过`InventoryController.Instance`访问Controller，导致耦合度高。

**优化方案**：
```csharp
// 原代码
public void EndDrag(int targetSlotIndex)
{
    if (!isDragging) return;
    
    dragIcon.gameObject.SetActive(false);
    isDragging = false;
    
    if (targetSlotIndex >= 0 && targetSlotIndex < slots.Length)
    {
        InventoryController.Instance?.MoveItem(draggedSlotIndex, targetSlotIndex);
    }
}

// 优化后代码
public class InventoryView : MonoBehaviour
{
    // 使用委托代替直接调用
    public event Action<int, int> OnItemMoveRequested;
    
    public void EndDrag(int targetSlotIndex)
    {
        if (!isDragging) return;
        
        dragIcon.gameObject.SetActive(false);
        isDragging = false;
        
        if (targetSlotIndex >= 0 && targetSlotIndex < slots.Length)
        {
            OnItemMoveRequested?.Invoke(draggedSlotIndex, targetSlotIndex);
        }
    }
    
    // 显示物品详情
    public void ShowItemDetails(ItemData item)
    {
        if (item == null) return;
        
        itemName.text = item.ItemName;
        itemIcon.sprite = item.Icon;
        
        // 根据物品类型显示不同的属性
        string statsText = string.Empty;
        if (item is WeaponItem weapon)
        {
            statsText = $"Damage: {weapon.Damage}\nAttack Speed: {weapon.AttackSpeed}";
        }
        else if (item is HealthPotionItem potion)
        {
            statsText = $"Heal Amount: {potion.HealAmount}";
        }
        
        itemStats.text = statsText;
        detailsPanel.SetActive(true);
    }
    
    // 隐藏物品详情
    public void HideItemDetails()
    {
        detailsPanel.SetActive(false);
    }
    
    // 在槽位点击事件中调用
    public void OnSlotClick(int slotIndex)
    {
        // 通知Controller处理点击事件
        OnSlotClickRequested?.Invoke(slotIndex);
    }
    
    // 新的委托事件
    public event Action<int> OnSlotClickRequested;
}

// 在Controller中注册事件
    public class InventoryController : Singleton<InventoryController>
    {
        private void Initialize()
        {
            // ... 其他初始化代码 ...
    view.OnItemMoveRequested += MoveItem;
        }
    }
}


### 3.2 增强Model层的数据操作能力

**当前问题**：物品移动逻辑可以更完善，缺乏对物品使用效果的支持，没有实现物品分类或过滤功能。

**优化方案**：
```csharp
public class InventoryModel
{
    // 扩展物品移动逻辑，支持空位插入
    public bool MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= items.Count || toIndex < 0 || toIndex >= capacity)
            return false;
        
        if (items[fromIndex] == null)
            return false;
        
        // 如果目标位置已有物品，则交换
        if (toIndex < items.Count && items[toIndex] != null)
        {
            ItemStack temp = items[toIndex];
            items[toIndex] = items[fromIndex];
            items[fromIndex] = temp;
        }
        else
        {
            // 处理空位情况
            items.Insert(toIndex, items[fromIndex]);
            items.RemoveAt(fromIndex >= toIndex ? fromIndex + 1 : fromIndex);
        }
        
        OnInventoryChanged?.Invoke();
        return true;
    }
    
    // 添加物品分类功能
    public List<ItemStack> GetItemsByCategory(ItemCategory category)
    {
        return items.Where(item => item != null && item.Item.Category == category).ToList();
    }
    
    // 添加物品过滤功能
    public List<ItemStack> GetFilteredItems(Predicate<ItemData> filter)
    {
        return items.Where(item => item != null && filter(item.Item)).ToList();
    }
}

// 物品使用接口
public interface IUsable
{
    bool Use(GameObject user);
}

// 在具体物品类中实现使用功能
public class HealthPotionItem : ConsumableItem, IUsable
{
    public int HealAmount { get; set; }
    
    public bool Use(GameObject user)
    {
        HealthComponent health = user.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.Heal(HealAmount);
            return true;
        }
        return false;
    }
}
```

### 3.3 引入依赖注入提高代码灵活性

**当前问题**：Controller使用单例模式，依赖项管理不够灵活。

**优化方案**：
```csharp
// 使用依赖注入替代单例模式
public class InventoryController : MonoBehaviour
{
    [Inject] private InventoryModel model;
    [Inject] private InventoryView view;
    [Inject] private ItemDatabase itemDatabase;
    
    private void Awake()
    {
        // 通过依赖注入框架注入依赖项
        DependencyContainer.Inject(this);
        Initialize();
    }
    
    private void Initialize()
    {
        // 注册事件监听器
        model.OnInventoryChanged += UpdateView;
        view.OnItemMoveRequested += MoveItem;
        view.OnSlotClickRequested += HandleSlotClick;
        
        // 初始化视图
        UpdateView();
    }
    
    // 实现物品使用逻辑
    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= model.Items.Count)
            return false;
        
        ItemStack itemStack = model.Items[slotIndex];
        if (itemStack == null)
            return false;
        
        if (itemStack.Item is IUsable usableItem)
        {
            if (usableItem.Use(PlayerController.Instance.gameObject))
            {
                model.RemoveItem(slotIndex, 1);
                return true;
            }
        }
        
        return false;
    }
}
```

### 3.4 完善错误处理和边界检查

**当前问题**：缺乏详细的错误日志和异常处理。

**优化方案**：

**当前问题**：缺乏详细的错误日志和异常处理。

**优化方案**：
```csharp
// 原代码
public bool AddItem(string itemID, int quantity = 1)
{
    ItemData item = itemDatabase.GetItem(itemID);
    if (item == null) return false;
    
    return model.AddItem(item, quantity);
}

// 优化后代码
public bool AddItem(string itemID, int quantity = 1)
{
    if (string.IsNullOrEmpty(itemID))
    {
        Debug.LogError("InventoryController: Cannot add item with empty ID");
        return false;
    }
    
    if (quantity <= 0)
    {
        Debug.LogError("InventoryController: Cannot add item with non-positive quantity");
        return false;
    }
    
    ItemData item = itemDatabase.GetItem(itemID);
    if (item == null)
    {
        Debug.LogError($"InventoryController: Item with ID '{itemID}' not found in database");
        return false;
    }
    
    bool result = model.AddItem(item, quantity);
    if (!result)
    {
        Debug.LogWarning($"InventoryController: Failed to add all requested quantity of item '{itemID}'");
    }
    
    return result;
}