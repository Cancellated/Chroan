using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Logger;
using MyGame.UI.Dialogue.Model;
using System.Collections.Generic;
using MyGame.UI.Dialogue.Controller;

namespace MyGame.UI.Dialogue.View
{
    /// <summary>
    /// 对话系统视图
    /// 负责显示对话内容和处理用户输入
    /// </summary>
    public class DialogueView : BaseView<DialogueController>
    {
        private const string LOG_MODULE = LogModules.DIALOGUE;
        
        [Header("对话UI组件")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private GameObject choicesContainer;
        [SerializeField] private GameObject choiceButtonPrefab;
        [SerializeField] private Animator dialogueAnimator;
        
        [Header("角色UI组件")]
        [SerializeField] private Transform charactersContainer;
        [SerializeField] private GameObject characterUIPrefab;
        
        private List<GameObject> activeCharacterUI = new();
        private List<Button> activeChoiceButtons = new();
        
        /// <summary>
        /// 显示对话UI
        /// </summary>
        public override void Show()
        {
            base.Show();
            Log.Info(LOG_MODULE, "显示对话UI");
            
            if (dialogueAnimator != null)
            {
                dialogueAnimator.SetTrigger("Show");
            }
        }
        
        /// <summary>
        /// 隐藏对话UI
        /// </summary>
        public override void Hide()
        {
            Log.Info(LOG_MODULE, "隐藏对话UI");
            
            if (dialogueAnimator != null)
            {
                dialogueAnimator.SetTrigger("Hide");
            }
            else
            {
                base.Hide();
            }
        }
        
        /// <summary>
        /// 当隐藏动画完成时调用
        /// </summary>
        public void OnHideAnimationComplete()
        {
            base.Hide();
        }
        
        /// <summary>
        /// 更新对话显示内容
        /// </summary>
        /// <param name="dialogueEntry">对话条目</param>
        public void UpdateDialogueDisplay(DialogueEntry dialogueEntry)
        {
            if (dialogueEntry == null)
                return;
            
            // 更新对话文本和说话者信息
            if (dialogueText != null)
                dialogueText.text = dialogueEntry.DialogueText;
            
            if (speakerNameText != null)
                speakerNameText.text = dialogueEntry.SpeakerName;
            
            // 这里可以添加头像显示逻辑
            
            // 显示或隐藏选项和继续按钮
            ShowChoices(dialogueEntry.Choices);
            
            // 显示角色状态
            ShowCharacterStates(dialogueEntry.CharacterStates);
            
        }
        
        /// <summary>
        /// 显示对话选项
        /// </summary>
        /// <param name="choices">选项列表</param>
        private void ShowChoices(List<DialogueChoice> choices)
        {
            // 清除现有选项
            ClearChoices();
            
            if (choicesContainer == null || choiceButtonPrefab == null)
                return;
            
            // 设置选项容器可见性
            choicesContainer.SetActive(choices.Count > 0);
            
            // 创建新选项按钮
            for (int i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                var buttonObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
                
                var button = buttonObj.GetComponent<Button>();
                var textComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                
                if (textComponent != null)
                    textComponent.text = choice.ChoiceText;
                
                if (button != null)
                {
                    button.interactable = choice.IsAvailable;
                    
                    // 绑定点击事件
                    int choiceIndex = i; // 捕获当前索引
                    button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
                    
                    activeChoiceButtons.Add(button);
                }
            }
        }
        
        /// <summary>
        /// 清除所有选项
        /// </summary>
        private void ClearChoices()
        {
            foreach (var button in activeChoiceButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            activeChoiceButtons.Clear();
        }
        
        /// <summary>
        /// 显示角色状态
        /// </summary>
        /// <param name="characterStates">角色状态列表</param>
        private void ShowCharacterStates(List<CharacterState> characterStates)
        {
            // 清除现有角色UI
            ClearCharacterUI();
            
            if (charactersContainer == null || characterUIPrefab == null)
                return;
            
            // 创建角色UI
            foreach (var state in characterStates)
            {
                var characterObj = Instantiate(characterUIPrefab, charactersContainer);
                
                // 设置角色UI位置
                if (characterObj.TryGetComponent<RectTransform>(out var rectTransform))
                {
                    rectTransform.anchoredPosition = state.PositionOffset;
                }
                
                // 设置角色名称、表情等
                var nameText = characterObj.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = state.CharacterName;
                
                // 触发角色动画
                var animator = characterObj.GetComponent<Animator>();
                if (animator != null && !string.IsNullOrEmpty(state.AnimationTrigger))
                {
                    animator.SetTrigger(state.AnimationTrigger);
                }
                
                activeCharacterUI.Add(characterObj);
            }
        }
        
        /// <summary>
        /// 清除所有角色UI
        /// </summary>
        private void ClearCharacterUI()
        {
            foreach (var obj in activeCharacterUI)
            {
                if (obj != null)
                    Destroy(obj);
            }
            activeCharacterUI.Clear();
        }
        
        /// <summary>
        /// 选项被选择时的回调
        /// </summary>
        /// <param name="choiceIndex">选项索引</param>
        private void OnChoiceSelected(int choiceIndex)
        {
            Log.Info(LOG_MODULE, $"选项被选择: {choiceIndex}");
            
            // 通过事件系统通知控制器
            MyGame.Events.GameEvents.TriggerDialogueChoiceSelected(choiceIndex);
        }
        
        /// <summary>
        /// 继续按钮点击回调
        /// </summary>
        public void OnContinueButtonClicked()
        {
            Log.Info(LOG_MODULE, "继续按钮被点击");
            
            // 通过事件系统通知控制器
            MyGame.Events.GameEvents.TriggerDialogueContinue();
        }
        
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public override void Cleanup()
        {
            Log.Info(LOG_MODULE, "清理对话视图资源");
            ClearChoices();
            ClearCharacterUI();
        }
    }
}