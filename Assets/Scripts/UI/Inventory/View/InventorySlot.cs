using Inventory.controller;
using Inventory.data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 表示背包系统中的单个物品槽位，负责显示物品图标和数量，并处理交互逻辑
/// </summary>
namespace Inventory.view
{
    public class InventorySlot : MonoBehaviour
    {
        #region 字段
        [SerializeField] private Image icon; // 物品图标显示组件
        [SerializeField] private TMP_Text quantityText; // 物品数量文本组件
        [SerializeField] private Button slotButton; // 槽位点击交互组件
    
        private int slotIndex; // 当前槽位在背包中的索引位置
        private InventoryView view; // 所属的背包视图控制器
        private ItemData currentItem; // 当前槽位存放的物品数据
        #endregion

        #region 方法
        /// <summary>
        /// 初始化槽位，设置索引和所属视图
        /// </summary>
        /// <param name="index">槽位索引</param>
        /// <param name="inventoryView">背包视图控制器</param>
        public void Initialize(int index, InventoryView inventoryView)
        {
            slotIndex = index;
            view = inventoryView;
            slotButton.onClick.AddListener(OnSlotClick);
            
            ClearSlot();
        }
        
        /// <summary>
        /// 设置当前槽位显示的物品和数量
        /// </summary>
        /// <param name="item">物品数据</param>
        /// <param name="quantity">物品数量</param>
        public void SetItem(ItemData item, int quantity)
        {
            currentItem = item;
            
            if (item != null)
            {
                icon.sprite = item.Icon;
                icon.gameObject.SetActive(true);
                quantityText.text = quantity > 1 ? quantity.ToString() : "";
            }
            else
            {
                ClearSlot();
            }
        }
        
        /// <summary>
        /// 清空当前槽位，隐藏物品图标和数量
        /// </summary>

        public void ClearSlot()
        {
            icon.gameObject.SetActive(false);
            quantityText.text = "";
            currentItem = null;
        }
        /// <summary>
        /// 处理槽位点击事件，显示物品详情或执行交互逻辑
        /// </summary>

        private void OnSlotClick()
        {
            if (currentItem != null)
            {
                // 通过InventoryController单例显示当前物品的详细信息
                InventoryController.Instance?.UseItem(currentItem);
            }
        }
        
        // UI事件处理
        public void OnBeginDrag()
        {
            if (currentItem != null)
            {
                view.StartDrag(slotIndex, currentItem.Icon);
            }
        }
        
        public void OnEndDrag()
        {
            // 寻找目标槽位
            int targetSlotIndex = -1;
            
            // 实际项目中这里需要实现射线检测找到目标槽位
            // 简化版：直接使用最后悬停的槽位
            
            view.EndDrag(targetSlotIndex);
        }
    }
    #endregion
}