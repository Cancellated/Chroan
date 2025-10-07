using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using MyGame.Data;
using MyGame.UI.SaveLoad.Events;
using MyGame.UI.SaveLoad.Controller;
using MyGame.UI;
using Logger;

namespace MyGame.UI.SaveLoad.View
{
    /// <summary>
    /// 存档菜单视图接口
    /// 定义存档槽UI的基本操作
    /// </summary>
    public interface ISaveSlotUI
    {
        void Initialize(SaveSlotInfo slotInfo, SaveLoadMenuView view);
        void UpdateDisplay();
        void SetSelected(bool selected);
        string SlotName { get; }
    }
    
    /// <summary>
    /// 存档槽UI的抽象基类
    /// 提供存档槽UI的通用功能
    /// </summary>
    public abstract class SaveSlotUI : MonoBehaviour, ISaveSlotUI
    {
        [Header("Save Slot Components")]
        [SerializeField] protected Text slotNameText;
        [SerializeField] protected Text timestampText;
        [SerializeField] protected Text progressText;
        [SerializeField] protected Button slotButton;
        
        protected SaveSlotInfo _slotInfo;
        protected SaveLoadMenuView _view;
        protected bool _isSelected = false;
        
        /// <summary>
        /// 初始化存档槽UI
        /// </summary>
        /// <param name="slotInfo">存档槽信息</param>
        /// <param name="view">视图引用</param>
        public virtual void Initialize(SaveSlotInfo slotInfo, SaveLoadMenuView view)
        {
            _slotInfo = slotInfo;
            _view = view;
            
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(HandleSlotButtonClick);
            }
            
            UpdateDisplay();
        }
        
        /// <summary>
        /// 更新存档槽显示
        /// </summary>
        public virtual void UpdateDisplay()
        {
            if (slotNameText != null)
            {
                slotNameText.text = _slotInfo.DisplayName;
            }
            
            if (timestampText != null)
            {
                timestampText.text = _slotInfo.HasSave ? _slotInfo.LastModified : "空存档槽";
            }
            
            if (progressText != null)
            {
                progressText.text = _slotInfo.HasSave ? _slotInfo.ProgressText : string.Empty;
            }
        }
        
        /// <summary>
        /// 设置存档槽选中状态
        /// </summary>
        /// <param name="selected">是否选中</param>
        public virtual void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateHighlight();
        }
        
        /// <summary>
        /// 获取存档槽名称
        /// </summary>
        public string SlotName
        {
            get { return _slotInfo != null ? _slotInfo.SlotName : string.Empty; }
        }
        
        /// <summary>
        /// 处理存档槽按钮点击事件
        /// </summary>
        protected virtual void HandleSlotButtonClick()
        {
            if (_view != null)
            {
                _view.OnSaveSlotClick(_slotInfo.SlotName, _slotInfo.SaveData);
            }
        }
        
        /// <summary>
        /// 更新存档槽高亮状态
        /// </summary>
        protected abstract void UpdateHighlight();
        
        /// <summary>
        /// 清理资源
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
            }
        }
    }
    
    /// <summary>
    /// 存档菜单视图基类
    /// 负责显示存档菜单的UI元素并处理用户交互
    /// 继承自BaseView以保持架构一致性
    /// </summary>
    public class SaveLoadMenuView : BaseView<SaveLoadMenuController>
    {
        [Header("Save Slots")]
        [SerializeField] protected Transform saveSlotsContainer;
        

        [Header("Save Options")]
        [SerializeField] protected GameObject saveOptionsMenu;
        [SerializeField] protected Button saveButton;
        [SerializeField] protected Button loadButton;
        [SerializeField] protected Button deleteButton;
        [SerializeField] protected Button cancelButton;
        
        [Header("Main Options")]
        [SerializeField] protected Button newGameButton;
        [SerializeField] protected Button backButton;

        protected SaveLoadMenuModel _model;
        protected List<ISaveSlotUI> _saveSlotUIs = new();
        
        /// <summary>
        /// 控制器组件
        /// 重写基类Controller属性以提供对具体类型的访问
        /// </summary>
        public SaveLoadMenuController Controller
        {
            get { return m_controller; }
            set { BindController(value); }
        }
        
        /// <summary>
        /// 模型组件
        /// </summary>
        public SaveLoadMenuModel Model
        {
            get { return _model; }
            set { SetModel(value); }
        }
        
        /// <summary>
        /// 初始化视图
        /// 重写基类Initialize方法
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            BindButtonEvents();
            HideSaveOptionsMenu();
        }
        
        /// <summary>
        /// 尝试自动绑定控制器
        /// </summary>
        protected override void TryBindController()
        {
            // 尝试查找控制器组件
            var controller = GetComponentInParent<SaveLoadMenuController>();
            if (controller != null)
            {
                BindController(controller);
            }
        }
        
        /// <summary>
        /// 控制器绑定后的回调
        /// </summary>
        protected override void OnControllerBound()
        {
            // 当控制器绑定时，设置模型引用
            if (m_controller != null && m_controller.Model != null)
            {
                Model = m_controller.Model;
            }
        }
        
        /// <summary>
        /// 控制器解绑后的回调
        /// </summary>
        protected override void OnControllerUnbound()
        {
            // 当控制器解绑时，清除模型引用
            if (_model != null)
            {
                UnsubscribeFromModelEvents();
                _model = null;
            }
        }
        
        /// <summary>
        /// 设置模型组件
        /// </summary>
        /// <param name="model">模型实例</param>
        public virtual void SetModel(SaveLoadMenuModel model)
        {
            if (_model != null)
            {
                UnsubscribeFromModelEvents();
            }
            
            _model = model;
            
            if (_model != null)
            {
                SubscribeToModelEvents();
            }
            
            UpdateView();
        }
        
        /// <summary>
        /// 更新视图显示
        /// </summary>
        public virtual void UpdateView()
        {
            CreateSaveSlotUIs();
            UpdateSaveOptionsButtonStates();
        }
        
        // 用于跟踪异步加载操作
        private List<AsyncOperationHandle<GameObject>> _prefabLoadHandles = new();
        
        /// <summary>
        /// 创建存档槽UI
        /// 使用Addressable Assets异步加载预制件
        /// </summary>
        protected virtual async void CreateSaveSlotUIs()
        {
            if (_model == null || saveSlotsContainer == null)
                return;
            
            // 获取SaveLoadMenuConfig
            SaveLoadMenuConfig config = null;
            if (m_controller != null)
            {
                config = m_controller.Config;
            }
            
            if (config == null)
            {
                Log.Error(LogModules.SAVE, "SaveLoadMenuConfig is not set in controller", this);
                return;
            }
            
            // 清理现有存档槽UI和加载句柄
            ClearSaveSlotUIs();
            ClearLoadHandles();
            
            try
            {
                // 异步加载存档槽预制件
                AsyncOperationHandle<GameObject> prefabLoadHandle = Addressables.LoadAssetAsync<GameObject>(config.SaveSlotPrefabAddress);
                _prefabLoadHandles.Add(prefabLoadHandle);
                
                // 等待加载完成
                await prefabLoadHandle.Task;
                
                if (prefabLoadHandle.Status == AsyncOperationStatus.Succeeded && prefabLoadHandle.Result != null)
                {
                    GameObject saveSlotPrefab = prefabLoadHandle.Result;
                    
                    // 创建新的存档槽UI
                    foreach (var slotInfo in _model.SaveSlots)
                    {
                        GameObject slotGO = Instantiate(saveSlotPrefab, saveSlotsContainer);
                        
                        if (slotGO.TryGetComponent<ISaveSlotUI>(out var slotUI))
                        {
                            slotUI.Initialize(slotInfo, this);
                            slotUI.SetSelected(_model.SelectedSaveSlotName == slotInfo.SlotName);
                            _saveSlotUIs.Add(slotUI);
                        }
                    }
                }
                else
                {
                    Log.Error(LogModules.SAVE, "Failed to load save slot prefab with Addressable: " + config.SaveSlotPrefabAddress, this);
                }
            }
            catch (System.Exception e)
            {
                Log.Error(LogModules.SAVE, "Error loading save slot prefab: " + e.Message, this);
            }
        }
        
        /// <summary>
        /// 清理异步加载句柄
        /// </summary>
        protected virtual void ClearLoadHandles()
        {
            foreach (var handle in _prefabLoadHandles)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _prefabLoadHandles.Clear();
        }
        
        /// <summary>
        /// 清理存档槽UI
        /// </summary>
        protected virtual void ClearSaveSlotUIs()
        {
            foreach (var slotUI in _saveSlotUIs)
            {
                if (slotUI is not null and MonoBehaviour)
                {
                    Destroy(((MonoBehaviour)slotUI).gameObject);
                }
            }
            
            _saveSlotUIs.Clear();
        }
        
        /// <summary>
        /// 更新存档选项按钮状态
        /// </summary>
        protected virtual void UpdateSaveOptionsButtonStates()
        {
            if (_model == null)
                return;
            
            bool hasSelectedSlot = !string.IsNullOrEmpty(_model.SelectedSaveSlotName);
            bool hasSaveData = _model.SelectedSaveData != null;
            
            if (saveButton != null)
            {
                saveButton.interactable = hasSelectedSlot;
            }
            
            if (loadButton != null)
            {
                loadButton.interactable = hasSaveData;
            }
            
            if (deleteButton != null)
            {
                deleteButton.interactable = hasSaveData;
            }
        }
        
        /// <summary>
        /// 绑定按钮事件
        /// </summary>
        protected virtual void BindButtonEvents()
        {
            if (saveButton != null)
            {
                saveButton.onClick.AddListener(HandleSaveButtonClick);
            }
            
            if (loadButton != null)
            {
                loadButton.onClick.AddListener(HandleLoadButtonClick);
            }
            
            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(HandleDeleteButtonClick);
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(HandleCancelButtonClick);
            }
            
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(HandleNewGameButtonClick);
            }
            
            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackButtonClick);
            }
        }
        
        /// <summary>
        /// 解绑按钮事件
        /// </summary>
        protected virtual void UnbindButtonEvents()
        {
            if (saveButton != null)
            {
                saveButton.onClick.RemoveListener(HandleSaveButtonClick);
            }
            
            if (loadButton != null)
            {
                loadButton.onClick.RemoveListener(HandleLoadButtonClick);
            }
            
            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveListener(HandleDeleteButtonClick);
            }
            
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(HandleCancelButtonClick);
            }
            
            if (newGameButton != null)
            {
                newGameButton.onClick.RemoveListener(HandleNewGameButtonClick);
            }
            
            if (backButton != null)
            {
                backButton.onClick.RemoveListener(HandleBackButtonClick);
            }
        }
        
        /// <summary>
        /// 订阅模型事件
        /// </summary>
        protected virtual void SubscribeToModelEvents()
        {
            if (_model != null)
            {
                _model.OnSaveSlotsUpdated += OnSaveSlotsUpdated;
                _model.OnSelectedSaveSlotChanged += OnSelectedSaveSlotChanged;
            }
        }
        
        /// <summary>
        /// 取消订阅模型事件
        /// </summary>
        protected virtual void UnsubscribeFromModelEvents()
        {
            if (_model != null)
            {
                _model.OnSaveSlotsUpdated -= OnSaveSlotsUpdated;
                _model.OnSelectedSaveSlotChanged -= OnSelectedSaveSlotChanged;
            }
        }
        
        /// <summary>
        /// 处理存档槽点击事件
        /// </summary>
        /// <param name="slotName">存档槽名称</param>
        /// <param name="saveData">存档数据</param>
        public virtual void OnSaveSlotClick(string slotName, SaveData saveData = null)
        {
            // 触发存档槽选中事件
            SaveLoadMenuEvents.TriggerSaveSlotSelected(slotName, saveData);
            
            if (saveData == null)
            {
                // 空槽位直接存档
                SaveLoadMenuEvents.TriggerSaveGame(slotName);
            }
            else
            {
                // 有存档时显示操作菜单
                ShowSaveOptionsMenu();
            }
        }
        
        /// <summary>
        /// 处理存档按钮点击事件
        /// </summary>
        protected virtual void HandleSaveButtonClick()
        {
            if (_model != null && !string.IsNullOrEmpty(_model.SelectedSaveSlotName))
            {
                SaveLoadMenuEvents.TriggerSaveGame(_model.SelectedSaveSlotName);
                HideSaveOptionsMenu();
            }
        }
        
        /// <summary>
        /// 处理加载按钮点击事件
        /// </summary>
        protected virtual void HandleLoadButtonClick()
        {
            if (_model != null && !string.IsNullOrEmpty(_model.SelectedSaveSlotName))
            {
                SaveLoadMenuEvents.TriggerLoadGame(_model.SelectedSaveSlotName);
                HideSaveOptionsMenu();
            }
        }
        
        /// <summary>
        /// 处理删除按钮点击事件
        /// </summary>
        protected virtual void HandleDeleteButtonClick()
        {
            if (_model != null && !string.IsNullOrEmpty(_model.SelectedSaveSlotName))
            {
                SaveLoadMenuEvents.TriggerDeleteSave(_model.SelectedSaveSlotName);
                HideSaveOptionsMenu();
            }
        }
        
        /// <summary>
        /// 处理取消按钮点击事件
        /// </summary>
        protected virtual void HandleCancelButtonClick()
        {
            HideSaveOptionsMenu();
        }
        
        /// <summary>
        /// 处理新游戏按钮点击事件
        /// </summary>
        protected virtual void HandleNewGameButtonClick()
        {
            SaveLoadMenuEvents.TriggerCreateNewGame();
        }
        
        /// <summary>
        /// 处理返回按钮点击事件
        /// </summary>
        protected virtual void HandleBackButtonClick()
        {
            SaveLoadMenuEvents.TriggerBackToMainMenu();
        }
        
        /// <summary>
        /// 显示存档选项菜单
        /// </summary>
        public virtual void ShowSaveOptionsMenu()
        {
            if (saveOptionsMenu != null)
            {
                saveOptionsMenu.SetActive(true);
            }
        }
        
        /// <summary>
        /// 隐藏存档选项菜单
        /// </summary>
        public virtual void HideSaveOptionsMenu()
        {
            if (saveOptionsMenu != null)
            {
                saveOptionsMenu.SetActive(false);
            }
        }
        

        
        
        /// <summary>
        /// 处理存档槽更新事件
        /// </summary>
        protected virtual void OnSaveSlotsUpdated()
        {
            UpdateView();
        }
        
        /// <summary>
        /// 处理选中存档槽变更事件
        /// </summary>
        protected virtual void OnSelectedSaveSlotChanged()
        {
            UpdateSaveOptionsButtonStates();
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        protected override void OnDestroy()
        {
            UnbindButtonEvents();
            UnsubscribeFromModelEvents();
            ClearSaveSlotUIs();
            ClearLoadHandles(); // 清理Addressable资源
        }
    }
}