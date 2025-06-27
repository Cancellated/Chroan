using UnityEngine;
using Inventory.data;

namespace Inventory.data
{
/// <summary>
/// 物品数据库，负责存储所有可使用的物品数据
/// </summary>
public class ItemDatabase : MonoBehaviour
    {
        [SerializeField] private ItemData[] items;
        
        public ItemData GetItem(string id)
        {
            foreach (var item in items)
            {
                if (item.ID == id) return item;
            }
            return null;
        }
    }
}