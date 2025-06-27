using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventory.data;
using Inventory.controller;

namespace Inventory.view
{
    public class InventoryView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private InventorySlot slotPrefab;
        [SerializeField] private TMP_Text capacityText;
        [SerializeField] private Button closeButton;
        
        [Header("Drag & Drop")]
        [SerializeField] private Canvas dragCanvas;
        [SerializeField] private Image dragIcon;
        
        private InventorySlot[] slots;
        private bool isDragging;
        private int draggedSlotIndex = -1;
        
        private void Start()
        {
            closeButton.onClick.AddListener(() => SetVisible(false));
            dragIcon.gameObject.SetActive(false);
        }
        
        // 初始化背包UI
        public void Initialize(int capacity)
        {
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
        
        // 更新槽位显示
        public void UpdateSlot(int index, ItemData item, int quantity)
        {
            if (index < 0 || index >= slots.Length) return;
            
            slots[index].SetItem(item, quantity);
        }
        
        // 更新容量显示
        public void UpdateCapacity(int current, int max)
        {
            capacityText.text = $"{current}/{max}";
        }
        
        // 显示/隐藏背包
        public void SetVisible(bool visible)
        {
            inventoryPanel.SetActive(visible);
            if (visible) inventoryPanel.transform.SetAsLastSibling();
        }
        
        // 开始拖拽物品
        public void StartDrag(int slotIndex, Sprite icon)
        {
            isDragging = true;
            draggedSlotIndex = slotIndex;
            dragIcon.sprite = icon;
            dragIcon.gameObject.SetActive(true);
            dragIcon.transform.position = Input.mousePosition;
        }
        
        // 更新拖拽位置
        private void Update()
        {
            if (isDragging)
            {
                dragIcon.transform.position = Input.mousePosition;
            }
        }
        
        // 结束拖拽
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
    }
}