using System;
using System.Collections.Generic;
using UnityEngine;
using Inventory.data;


namespace Inventory{
/// <summary>
/// 可序列化的背包数据模型，负责管理物品的添加、移除和位置交换
/// </summary>
[System.Serializable]
    public class InventoryModel
    {
        #region 字段
        /// <summary>
        /// 背包的最大容量
        /// </summary>
        public int Capacity { get; private set; } = 20;
        
        /// <summary>
        /// 当前背包中的物品列表
        /// </summary>
        private List<InventoryItem> items = new();
        
        /// <summary>
        /// 只读的物品列表访问接口
        /// </summary>
        public IReadOnlyList<InventoryItem> Items => items;
        
        /// <summary>
        /// 当背包内容发生变化时触发的事件
        /// </summary>
        public event Action OnInventoryChanged;

        #endregion

        #region 方法    
        /// <summary>
        /// 向背包中添加物品
        /// </summary>
        /// <param name="item">要添加的物品数据</param>
        /// <param name="quantity">要添加的数量，默认为1</param>
        /// <returns>是否成功添加所有请求数量的物品</returns>
        public bool AddItem(ItemData item, int quantity = 1)
        {
            // 优先尝试堆叠到已有物品
            foreach (var invItem in items)
            {
                if (invItem.ItemID == item.ID && invItem.Quantity < item.MaxStack)
                {
                    int canAdd = item.MaxStack - invItem.Quantity;
                    int addAmount = Mathf.Min(canAdd, quantity);
                    invItem.AddQuantity(addAmount);
                    quantity -= addAmount;
                    
                    if (quantity <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
            
            // 添加新物品直到背包满或数量耗尽
            while (quantity > 0 && items.Count < Capacity)
            {
                int addAmount = Mathf.Min(quantity, item.MaxStack);
                items.Add(new InventoryItem(item.ID, addAmount));
                quantity -= addAmount;
            }
            
            OnInventoryChanged?.Invoke();
            return quantity == 0;
        }
        
        /// <summary>
        /// 从背包中移除指定物品
        /// </summary>
        /// <param name="itemID">要移除的物品ID</param>
        /// <param name="quantity">要移除的数量，默认为1</param>
        /// <returns>是否成功移除指定数量的物品</returns>
        public bool RemoveItem(string itemID, int quantity = 1)
        {
            // 从后向前遍历以避免索引问题
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i].ItemID == itemID)
                {
                    if (items[i].Quantity > quantity)
                    {
                        // 只移除部分数量
                        items[i].RemoveQuantity(quantity);
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                    else
                    {
                        // 移除整个物品
                        quantity -= items[i].Quantity;
                        items.RemoveAt(i);
                        
                        if (quantity <= 0)
                        {
                            OnInventoryChanged?.Invoke();
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        /// <summary>
        /// 交换背包中两个物品的位置
        /// </summary>
        /// <param name="fromIndex">源物品索引</param>
        /// <param name="toIndex">目标物品索引</param>
        public void MoveItem(int fromIndex, int toIndex)
        {
            // 检查索引有效性
            if (fromIndex < 0 || fromIndex >= items.Count || 
                toIndex < 0 || toIndex >= items.Count)
                return;
            
            // 交换物品位置
            var temp = items[fromIndex];
            items[fromIndex] = items[toIndex];
            items[toIndex] = temp;
            
            OnInventoryChanged?.Invoke();
        }
    }
    #endregion
}