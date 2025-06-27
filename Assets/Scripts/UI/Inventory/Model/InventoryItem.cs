using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.data
{
    /// <summary>
    /// 背包中的单个物品项
    /// </summary>
    [System.Serializable]
    public class InventoryItem
    {
        /// <summary>
        /// 物品的唯一标识符
        /// </summary>
        public string ItemID;
        
        /// <summary>
        /// 当前堆叠数量
        /// </summary>
        public int Quantity;
        
        public InventoryItem(string id, int quantity)
        {
            ItemID = id;
            Quantity = quantity;
        }
        
        public void AddQuantity(int amount) => Quantity += amount;
        public void RemoveQuantity(int amount) => Quantity = Mathf.Max(0, Quantity - amount);
    }
}
