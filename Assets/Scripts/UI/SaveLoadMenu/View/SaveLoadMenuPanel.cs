using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyGame.Data;
using MyGame.UI.SaveLoad.View;

namespace MyGame.UI.SaveLoad.View
{
    /// <summary>
    /// 存档菜单面板类，继承自SaveLoadMenuView
    /// 实现具体的存档菜单UI显示逻辑
    /// </summary>
    public class SaveLoadMenuPanel : SaveLoadMenuView
    {
        [Header("自定义UI组件")]
        [SerializeField]
        private TextMeshProUGUI _menuTitleText;
        
        [SerializeField]
        private TextMeshProUGUI _selectedSlotInfoText;
        
        /// <summary>
        /// 初始化面板
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            
            // 设置初始标题
            if (_menuTitleText != null)
            {
                _menuTitleText.text = "存档/读档菜单";
            }
            
            // 初始化选中信息文本
            if (_selectedSlotInfoText != null)
            {
                _selectedSlotInfoText.text = "请选择一个存档";
            }
        }
        
        /// <summary>
        /// 更新视图显示
        /// </summary>
        public override void UpdateView()
        {
            base.UpdateView();
            
            // 更新选中存档信息显示
            UpdateSelectedSlotInfo();
        }
        
        /// <summary>
        /// 更新选中存档信息显示
        /// </summary>
        private void UpdateSelectedSlotInfo()
        {
            if (_selectedSlotInfoText == null || _model == null)
                return;
            
            if (string.IsNullOrEmpty(_model.SelectedSaveSlotName))
            {
                _selectedSlotInfoText.text = "请选择一个存档";
                return;
            }
            
            // 查找选中的存档信息
            SaveSlotInfo selectedSlotInfo = null;
            if (_model.SaveSlots != null)
            {
                foreach (var slotInfo in _model.SaveSlots)
                {
                    if (slotInfo.SlotName == _model.SelectedSaveSlotName)
                    {
                        selectedSlotInfo = slotInfo;
                        break;
                    }
                }
            }
            
            // 显示选中的存档信息
            if (selectedSlotInfo != null && selectedSlotInfo.SaveData != null)
            {
                string info = $"选中存档：{selectedSlotInfo.SlotName}\n";
                info += $"存档时间：{selectedSlotInfo.SaveData.saveTime}\n";
                info += $"游戏版本：{selectedSlotInfo.SaveData.version}\n";
                
                // 添加游戏进度信息
                if (selectedSlotInfo.SaveData.gameProgress != null)
                {
                    info += $"当前关卡：{selectedSlotInfo.SaveData.gameProgress.currentLevel}\n";
                    info += $"已完成关卡：{selectedSlotInfo.SaveData.gameProgress.completedLevels.Count}\n";
                    info += $"活跃任务：{selectedSlotInfo.SaveData.gameProgress.activeQuests.Count}\n";
                }
                
                _selectedSlotInfoText.text = info;
            }
            else
            {
                _selectedSlotInfoText.text = $"选中存档：{_model.SelectedSaveSlotName}\n(空存档槽)";
            }
        }
        
        /// <summary>
        /// 格式化日期时间（已直接在SaveData中格式化）
        /// </summary>
        /// <param name="timestamp">格式化的时间字符串</param>
        /// <returns>格式化后的日期时间字符串</returns>
        private string FormatDateTime(string timestamp)
        {
            return timestamp; // 已在SaveData中格式化
        }
        
        /// <summary>
        /// 显示面板
        /// </summary>
        public override void Show()
        {
            base.Show();
            
            // 隐藏存档选项菜单
            HideSaveOptionsMenu();
        }
        
        /// <summary>
        /// 隐藏面板
        /// </summary>
        public override void Hide()
        {
            base.Hide();
            
            // 隐藏存档选项菜单
            HideSaveOptionsMenu();
        }
    }
    
    /// <summary>
    /// 具体的存档槽UI实现
    /// </summary>
    public class SaveSlotUIImplementation : SaveSlotUI
    {
        [SerializeField]
        private TextMeshProUGUI _slotNameText;
        
        [SerializeField]
        private TextMeshProUGUI _saveTimeText;
        
        [SerializeField]
        private TextMeshProUGUI _gameProgressText;
        
        [SerializeField]
        private Image _highlightImage;
        
        [SerializeField]
        private Button _slotButton;
        
        
        /// <summary>
        /// 初始化存档槽UI
        /// </summary>
        /// <param name="slotInfo">存档槽信息</param>
        /// <param name="view">视图引用</param>
        public override void Initialize(SaveSlotInfo slotInfo, SaveLoadMenuView view)
        {
            base.Initialize(slotInfo, view);
            
            // 设置存档槽名称
            if (_slotNameText != null)
            {
                _slotNameText.text = slotInfo.SlotName;
            }
            
            // 注册点击事件
            if (_slotButton != null)
            {
                _slotButton.onClick.RemoveAllListeners();
                _slotButton.onClick.AddListener(HandleSlotButtonClick);
            }
            
            // 更新高亮状态
            UpdateHighlight();
        }
        
        /// <summary>
        /// 更新显示内容
        /// </summary>
        public override void UpdateDisplay()
        {
            // 设置存档时间
            if (_saveTimeText != null)
            {
                if (_slotInfo.SaveData != null)
                {
                    _saveTimeText.text = _slotInfo.SaveData.saveTime;
                }
                else
                {
                    _saveTimeText.text = "空存档槽";
                }
            }
            
            // 设置游戏进度
            if (_gameProgressText != null)
            {
                if (_slotInfo.SaveData != null && _slotInfo.SaveData.gameProgress != null)
                {
                    _gameProgressText.text = $"关卡：{_slotInfo.SaveData.gameProgress.currentLevel}";
                }
                else
                {
                    _gameProgressText.text = "";
                }
            }
        }
        
        /// <summary>
        /// 设置存档槽是否被选中
        /// </summary>
        /// <param name="selected">是否选中</param>
        public override void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateHighlight();
        }
        
        /// <summary>
        /// 更新高亮显示
        /// </summary>
        protected override void UpdateHighlight()
        {
            if (_highlightImage != null)
            {
                _highlightImage.enabled = _isSelected;
            }
        }
    }
}