using UnityEngine;
using Logger;
using MyGame.UI.Dialogue.Model;
using MyGame.UI.Dialogue.View;
using MyGame.Events;
using MyGame.UI;
using MyGame.Managers;

namespace MyGame.UI.Dialogue.Controller
{
    /// <summary>
    /// 对话系统控制器
    /// 负责处理对话逻辑和事件响应
    /// </summary>
    public class DialogueController : BaseController<DialogueView, DialogueModel>
    {
        private const string LOG_MODULE = LogModules.DIALOGUE;
        
        #region 初始化和清理
        /// <summary>
        /// 初始化控制器
        /// </summary>
        public override void Initialize()
        {
            if (!IsInitialized)
            {
                Log.Info(LOG_MODULE, "初始化对话系统控制器");
                
                // 创建并初始化模型
                CreateAndInitializeModel();
                
                // 调用基类初始化
                base.Initialize();
            }
        }
        
        /// <summary>
        /// 初始化逻辑
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            // 注册事件监听
            RegisterEvents();
        }
        
        /// <summary>
        /// 注册事件监听
        /// </summary>
        private void RegisterEvents()
        {
            // 注册游戏事件
            GameEvents.OnDialogueStart += HandleDialogueStart;
            GameEvents.OnDialogueEnd += HandleDialogueEnd;
            GameEvents.OnDialogueChoiceSelected += HandleDialogueChoiceSelected;
            GameEvents.OnDialogueContinue += HandleDialogueContinue;
            
            // 注册UI状态切换事件
            GameEvents.OnMenuShow += HandleMenuShow;
        }
        
        /// <summary>
        /// 取消注册事件监听
        /// </summary>
        private void UnregisterEvents()
        {
            // 取消注册游戏事件
            GameEvents.OnDialogueStart -= HandleDialogueStart;
            GameEvents.OnDialogueEnd -= HandleDialogueEnd;
            GameEvents.OnDialogueChoiceSelected -= HandleDialogueChoiceSelected;
            GameEvents.OnDialogueContinue -= HandleDialogueContinue;
            
            // 取消注册UI状态切换事件
            GameEvents.OnMenuShow -= HandleMenuShow;
        }
        
        /// <summary>
        /// 清理控制器资源
        /// </summary>
        public override void Cleanup()
        {
            if (IsInitialized)
            {
                Log.Info(LOG_MODULE, "清理对话系统控制器资源");
                
                // 取消注册事件
                UnregisterEvents();
                
                // 清理模型资源
                if (m_model != null)
                {
                    m_model.Cleanup();
                    m_model = null;
                }
                
                // 调用基类清理
                base.Cleanup();
            }
        }
        
        /// <summary>
        /// 清理逻辑
        /// </summary>
        protected override void OnCleanup()
        {
            base.OnCleanup();
        }
        
        #endregion
        
        #region 事件处理
        
        /// <summary>
        /// 处理对话开始事件
        /// </summary>
        /// <param name="dialogueId">对话ID</param>
        private void HandleDialogueStart(string dialogueId)
        {
            Log.Info(LOG_MODULE, $"处理对话开始事件: {dialogueId}");
            
            // 通过模型开始对话
            if (m_model.StartDialogue(dialogueId))
            {
                // 更新视图
                UpdateDialogueView();
                
                // 显示对话UI
                if (m_view != null)
                {
                    m_view.Show();
                }
                
                // 切换到UI输入模式
                if (InputManager.Instance != null)
                {
                    InputManager.Instance.SwitchToUIMode();
                }
            }
        }
        
        /// <summary>
        /// 处理对话结束事件
        /// </summary>
        private void HandleDialogueEnd()
        {
            Log.Info(LOG_MODULE, "处理对话结束事件");
            
            // 结束对话
            m_model.EndDialogue();
            
            // 隐藏对话UI
            if (m_view != null)
            {
                m_view.Hide();
            }
            
            // 切换回游戏输入模式
            if (InputManager.Instance != null)
            {
                InputManager.Instance.SwitchToGamePlayMode();
            }
        }
        
        /// <summary>
        /// 处理对话选项选择事件
        /// </summary>
        /// <param name="choiceIndex">选项索引</param>
        private void HandleDialogueChoiceSelected(int choiceIndex)
        {
            Log.Info(LOG_MODULE, $"处理对话选项选择事件: {choiceIndex}");
            
            // 通过模型处理选项选择
            if (m_model.SelectChoice(choiceIndex))
            {
                // 更新视图
                UpdateDialogueView();
            }
        }
        
        /// <summary>
        /// 处理对话继续事件
        /// </summary>
        private void HandleDialogueContinue()
        {
            Log.Info(LOG_MODULE, "处理对话继续事件");
            
            // 通过模型继续对话
            if (m_model.ContinueToNextDialogue())
            {
                // 更新视图
                UpdateDialogueView();
            }
            else if (!m_model.IsDialogueActive)
            {
                // 如果对话不再激活，触发对话结束事件
                GameEvents.TriggerDialogueEnd();
            }
        }
        
        /// <summary>
        /// 处理UI状态切换事件
        /// </summary>
        /// <param name="menuType">UI类型</param>
        /// <param name="show">是否显示</param>
        private void HandleMenuShow(UIType menuType, bool show)
        {
            // 处理UI互斥关系
            if (show && menuType != UIType.Dialogue && m_view != null && m_view.IsVisible)
            {
                // 如果显示其他UI，则暂停对话
                PauseDialogue();
            }
            else if (!show && menuType != UIType.Dialogue && m_model.IsDialogueActive)
            {
                // 如果隐藏其他UI，且对话处于活动状态，则恢复对话
                ResumeDialogue();
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 更新对话视图
        /// </summary>
        private void UpdateDialogueView()
        {
            if (m_view == null || !m_model.IsDialogueActive)
                return;
            
            // 更新选项可用性
            m_model.UpdateChoiceAvailability();
            
            // 更新视图显示
            m_view.UpdateDialogueDisplay(m_model.CurrentDialogue);
        }
        
        /// <summary>
        /// 暂停对话
        /// </summary>
        private void PauseDialogue()
        {
            Log.Info(LOG_MODULE, "暂停对话");
            
            if (m_view != null && m_view.IsVisible)
            {
                m_view.Hide();
            }
        }
        
        /// <summary>
        /// 恢复对话
        /// </summary>
        private void ResumeDialogue()
        {
            Log.Info(LOG_MODULE, "恢复对话");
            
            if (m_view != null)
            {
                m_view.Show();
                UpdateDialogueView();
            }
        }
        
        #endregion
    }
}