using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Data;
using MyGame.UI;

namespace MyGame.UI.SaveLoad
{
    /// <summary>
    /// 存档菜单的数据模型类，继承自ObservableModel
    /// 用于管理和提供存档菜单所需的数据
    /// </summary>
    public class SaveLoadMenuModel : ObservableModel
    {
        private SaveData _selectedSaveData;
        private string _selectedSaveSlotName;
        private bool _isAutoSaveSlot;
        private List<SaveSlotInfo> _saveSlots = new List<SaveSlotInfo>();
        
        /// <summary>
        /// 存档槽信息更新事件
        /// </summary>
        public event System.Action OnSaveSlotsUpdated;
        
        /// <summary>
        /// 选中存档槽变更事件
        /// </summary>
        public event System.Action OnSelectedSaveSlotChanged;
        
        /// <summary>
        /// 选中的存档数据
        /// </summary>
        public SaveData SelectedSaveData
        {
            get { return _selectedSaveData; }
            set 
            {
                SetProperty(ref _selectedSaveData, value, nameof(SelectedSaveData));
                OnSelectedSaveSlotChanged?.Invoke();
            }
        }
        
        /// <summary>
        /// 选中的存档槽名称
        /// </summary>
        public string SelectedSaveSlotName
        {
            get { return _selectedSaveSlotName; }
            set 
            {
                SetProperty(ref _selectedSaveSlotName, value, nameof(SelectedSaveSlotName));
                OnSelectedSaveSlotChanged?.Invoke();
            }
        }
        
        /// <summary>
        /// 是否为自动存档槽
        /// </summary>
        public bool IsAutoSaveSlot
        {
            get { return _isAutoSaveSlot; }
            set { SetProperty(ref _isAutoSaveSlot, value, nameof(IsAutoSaveSlot)); }
        }
        
        /// <summary>
        /// 存档槽信息列表
        /// </summary>
        public List<SaveSlotInfo> SaveSlots
        {
            get { return _saveSlots; }
            set 
            {
                SetProperty(ref _saveSlots, value, nameof(SaveSlots));
                OnSaveSlotsUpdated?.Invoke();
            }
        }
        
        /// <summary>
        /// 初始化模型
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            // 初始化数据
            _saveSlots = new List<SaveSlotInfo>();
        }
        
        /// <summary>
        /// 清理模型资源
        /// </summary>
        public override void Cleanup()
        {
            base.Cleanup();
            // 清理资源
            _saveSlots.Clear();
            _selectedSaveData = null;
            _selectedSaveSlotName = null;
        }
        
        /// <summary>
        /// 设置选中的存档槽
        /// </summary>
        /// <param name="slotName">存档槽名称</param>
        /// <param name="saveData">存档数据</param>
        public void SetSelectedSaveSlot(string slotName, SaveData saveData = null)
        {
            SelectedSaveSlotName = slotName;
            SelectedSaveData = saveData;
            IsAutoSaveSlot = slotName == SaveLoadMenuConstants.AUTO_SAVE_SLOT;
        }
        
        /// <summary>
        /// 更新存档槽列表
        /// </summary>
        /// <param name="slots">新的存档槽列表</param>
        public void UpdateSaveSlots(List<SaveSlotInfo> slots)
        {
            SaveSlots = new List<SaveSlotInfo>(slots);
        }
    }
    
    /// <summary>
    /// 存档槽信息结构
    /// 存储单个存档槽的元数据
    /// </summary>
    public class SaveSlotInfo
    {
        public string SlotName { get; set; }
        public string DisplayName { get; set; }
        public bool HasSave { get; set; }
        public string LastModified { get; set; }
        public string Version { get; set; }
        public string ProgressText { get; set; }
        public SaveData SaveData { get; set; }
        public bool IsAutoSave { get; set; }
    }
}