using UnityEngine;
using Inventory.data;
using Inventory.view;
using MyGame.System;
using System;

namespace Inventory.controller
{
    public class InventoryController : Singleton<InventoryController>
    {    
        #region 字段
        [SerializeField] private InventoryModel model;
        [SerializeField] private InventoryView view;
        [SerializeField] private ItemDatabase itemDatabase;
        private GameControl _inputActions;
        #endregion
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _inputActions = new GameControl();
            Initialize();
        }
        
        private void Initialize()
        {
            // 初始化模型和视图
            model = new InventoryModel();
            view.Initialize(model.Capacity);
            
            // 注册事件
            model.OnInventoryChanged += UpdateInventoryView;
            
            // 测试添加物品
            AddTestItems();
        }
        
        private void AddTestItems()
        {
            // 添加测试物品
            AddItem("health_potion", 5);
            AddItem("sword", 1);
            AddItem("gold_coin", 42);
        }
        
        private void Update()
        {
            // 使用InputSystem检测快捷键
            if (_inputActions.GamePlay.Inventory.triggered)
            {
                GameEvents.TriggerInventoryShow(true);
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

        internal void UseItem(ItemData currentItem)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}