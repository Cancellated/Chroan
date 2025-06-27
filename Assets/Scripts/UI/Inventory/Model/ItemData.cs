using System;
using UnityEngine;

namespace Inventory.data
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string ID;
        public string Name;
        public string Description;
        public Sprite Icon;
        public int MaxStack = 1;
        public GameObject Prefab; // 3D模型或2D精灵
        public ItemType Type;
        
        /// <summary>
        /// 物品类型
        /// </summary>
        public enum ItemType
        {
            Consumable, // 消耗品
            Weapon, // 武器
            Armor, // 防具
            Material, // 材料
            Quest // 任务
        }
    }
}