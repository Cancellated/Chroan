using UnityEngine;
using Inventory.data;
using Inventory.view;
using MyGame.Events;
using System;
using MyGame.UI;

namespace Inventory.controller
{
    /// <summary>
    /// 背包控制器类，负责处理背包的逻辑和数据管理
    /// 作为MVC架构中的控制器层
    /// </summary>
    public class InventoryController : MonoBehaviour
    {    
        #region 字段
        [Tooltip("背包数据模型")]
        [SerializeField] private InventoryModel model;
        [Tooltip("背包视图")]
        [SerializeField] private InventoryView view;
        [Tooltip("物品数据库")]
        [SerializeField] private ItemDatabase itemDatabase;
        private GameControl _inputActions;
        #endregion
        
        #region 生命周期
        /// <summary>
        /// 初始化控制器
        /// </summary>
        private void Awake()
        {
            _inputActions = new GameControl();
            Initialize();
        }
        
        /// <summary>
        /// 初始化模型和视图
        /// </summary>
        private void Initialize()
        {
            // 初始化模型
            model = new InventoryModel();
            
            // 绑定控制器到视图
            if (view != null)
            {
                view.BindController(this);
                view.InitializeInventory(model.Capacity);
            }
            else
            {
                Debug.LogError("InventoryController: 视图未找到");
            }
            
            // 注册事件
            model.OnInventoryChanged += UpdateInventoryView;
            
            // 测试添加物品
            AddTestItems();
        }
        
        /// <summary>
        /// 启用控制器
        /// </summary>
        private void OnEnable()
        {
            _inputActions.Enable();
        }
        
        /// <summary>
        /// 禁用控制器
        /// </summary>
        private void OnDisable()
        {
            _inputActions.Disable();
        }
        
        /// <summary>
        /// 添加测试物品
        /// </summary>
        private void AddTestItems()
        {
            // 添加测试物品
            AddItem("health_potion", 5);
            AddItem("sword", 1);
            AddItem("gold_coin", 42);
        }
        
        /// <summary>
        /// 更新函数，处理输入检测
        /// </summary>
        private void Update()
        {
            // 使用InputSystem检测快捷键
            if (_inputActions.GamePlay.Inventory.triggered)
            {
                GameEvents.TriggerMenuShow(UIType.Inventory, true);
            }
        }
        #endregion

        #region 方法    
        // 添加物品
        public bool AddItem(string itemID, int quantity = 1)
        {
            ItemData item = itemDatabase.GetItem(itemID);
            if (item == null) return false;
            
            return model.AddItem(item, quantity);
        }
        
        // 移除物品
        public bool RemoveItem(string itemID, int quantity = 1)
        {
            return model.RemoveItem(itemID, quantity);
        }
        
        // 移动物品
        public void MoveItem(int fromIndex, int toIndex)
        {
            model.MoveItem(fromIndex, toIndex);
        }
        
        // 使用物品
        public void UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= model.Items.Count) return;
            
            var item = itemDatabase.GetItem(model.Items[slotIndex].ItemID);
            if (item != null && item.Type == ItemData.ItemType.Consumable)
            {
                // 实际使用逻辑
                Debug.Log($"使用物品: {item.Name}");
                
                // 移除一个物品
                RemoveItem(item.ID, 1);
            }
        }
        
        // 显示物品详情
        public void ShowItemDetails(ItemData item)
        {
            // 实际项目中这里会显示一个详情面板
            Debug.Log($"显示物品详情: {item.Name}\n{item.Description}");
        }
        
        // 更新视图
        private void UpdateInventoryView()
        {
            // 更新所有槽位
            for (int i = 0; i < model.Capacity; i++)
            {
                if (i < model.Items.Count)
                {
                    var item = itemDatabase.GetItem(model.Items[i].ItemID);
                    view.UpdateSlot(i, item, model.Items[i].Quantity);
                }
                else
                {
                    view.UpdateSlot(i, null, 0);
                }
            }
            
            // 更新容量显示
            view.UpdateCapacity(model.Items.Count, model.Capacity);
        }

        /// <summary>
        /// 使用物品
        /// </summary>
        /// <param name="currentItem">要使用的物品数据</param>
        internal void UseItem(ItemData currentItem)
        {
            if (currentItem != null && currentItem.Type == ItemData.ItemType.Consumable)
            {
                // 实际使用逻辑
                Debug.Log($"使用物品: {currentItem.Name}");
                
                // 移除一个物品
                RemoveItem(currentItem.ID, 1);
            }
        
        #endregion
    }
}
}