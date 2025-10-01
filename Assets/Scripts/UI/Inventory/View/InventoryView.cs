using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventory.data;
using Inventory.controller;
using MyGame.UI;
using System.Collections.Generic;

namespace Inventory.view
{
    /// <summary>
    /// 背包视图类，负责背包UI的显示和交互
    /// </summary>
    public class InventoryView : BaseView<InventoryController>
    {
        [Header("UI References")]
        [Tooltip("背包面板")]
        [SerializeField] private GameObject inventoryPanel;
        [Tooltip("物品槽容器")]
        [SerializeField] private Transform itemsContainer;
        [Tooltip("物品槽预制体")]
        [SerializeField] private InventorySlot slotPrefab;
        [Tooltip("容量显示文本")]
        [SerializeField] private TMP_Text capacityText;
        [Tooltip("关闭按钮")]
        [SerializeField] private Button closeButton;
        
        [Header("Drag & Drop")]
        [Tooltip("拖拽Canvas")]
        [SerializeField] private Canvas dragCanvas;
        [Tooltip("拖拽图标")]
        [SerializeField] private Image dragIcon;
        
        private InventorySlot[] slots;
        private bool isDragging;
        private int draggedSlotIndex = -1;
        private int capacity;
        
        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Awake()
        {
            // 设置面板类型
            m_panelType = UIType.Inventory;
            
            // 调用基类初始化
            base.Awake();
            
            // 设置关闭按钮事件
            closeButton.onClick.AddListener(Hide);
            dragIcon.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 初始化背包UI
        /// </summary>
        public void InitializeInventory(int newCapacity)
        {
            capacity = newCapacity;
            
            // 清空现有槽位
            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 创建新槽位
            slots = new InventorySlot[capacity];
            for (int i = 0; i < capacity; i++)
            {
                slots[i] = Instantiate(slotPrefab, itemsContainer);
                slots[i].Initialize(i, this);
            }
            
            capacityText.text = $"0/{capacity}";
        }
        
        /// <summary>
        /// 更新槽位显示
        /// </summary>
        public void UpdateSlot(int index, ItemData item, int quantity)
        {
            if (index < 0 || index >= slots.Length) return;
            
            slots[index].SetItem(item, quantity);
        }
        
        /// <summary>
        /// 更新容量显示
        /// </summary>
        public void UpdateCapacity(int current, int max)
        {
            capacityText.text = $"{current}/{max}";
        }
        
        /// <summary>
        /// 显示面板（重写基类方法）
        /// </summary>
        public override void Show()
        {
            base.Show();
            Debug.Log("InventoryView: 背包已显示");
        }
        
        /// <summary>
        /// 隐藏面板（重写基类方法）
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            Debug.Log("InventoryView: 背包已隐藏");
        }
        
        /// <summary>
        /// 开始拖拽物品
        /// </summary>
        public void StartDrag(int slotIndex, Sprite icon)
        {
            isDragging = true;
            draggedSlotIndex = slotIndex;
            dragIcon.sprite = icon;
            dragIcon.gameObject.SetActive(true);
            dragIcon.transform.position = Input.mousePosition;
        }
        
        /// <summary>
        /// 更新拖拽位置
        /// </summary>
        private void Update()
        {
            if (isDragging)
            {
                dragIcon.transform.position = Input.mousePosition;
            }
        }
        
        /// <summary>
        /// 结束拖拽
        /// </summary>
        public void EndDrag(int targetSlotIndex)
        {
            if (!isDragging) return;
            
            dragIcon.gameObject.SetActive(false);
            isDragging = false;
            
            if (targetSlotIndex >= 0 && targetSlotIndex < slots.Length)
            {
                if(m_controller != null)
                {
                    m_controller.MoveItem(draggedSlotIndex, targetSlotIndex);
                }
            }
        }
        
        /// <summary>
        /// 使用物品
        /// </summary>
        /// <param name="item">要使用的物品数据</param>
        public void UseItem(ItemData item)
        {
            if (m_controller != null)
            {
                m_controller.UseItem(item);
            }
        }
    }
}