using System.Collections.Generic;
using UnityEngine;
using Logger;
using MyGame.Events;

namespace MyGame.UI.Dialogue.Model
{
    /// <summary>
    /// 对话系统数据模型
    /// 管理当前对话状态和数据
    /// </summary>
    public class DialogueModel : BaseModel
    {
        private const string LOG_MODULE = LogModules.DIALOGUE;
        
        [SerializeField] private DialogueDatabase dialogueDatabase;
        
        // 当前对话状态
        private DialogueEntry currentDialogue;
        private string currentDialogueId;
        private bool isDialogueActive = false;
        private int currentSpeakerIndex = 0;
        
        /// <summary>
        /// 当前对话条目
        /// </summary>
        public DialogueEntry CurrentDialogue => currentDialogue;
        
        /// <summary>
        /// 对话是否处于激活状态
        /// </summary>
        public bool IsDialogueActive => isDialogueActive;
        
        /// <summary>
        /// 当前说话者索引
        /// </summary>
        public int CurrentSpeakerIndex => currentSpeakerIndex;
        
        /// <summary>
        /// 初始化模型
        /// </summary>
        public override void Initialize()
        {
            if (!IsInitialized)
            {
                Log.Info(LOG_MODULE, "初始化对话系统模型");
                
                if (dialogueDatabase != null)
                {
                    dialogueDatabase.Initialize();
                }
                else
                {
                    Log.Error(LOG_MODULE, "对话数据库未赋值！");
                }
                
                base.Initialize();
            }
        }
        
        /// <summary>
        /// 开始对话
        /// </summary>
        /// <param name="dialogueId">对话ID</param>
        /// <returns>是否成功开始对话</returns>
        public bool StartDialogue(string dialogueId)
        {
            if (dialogueDatabase == null)
            {
                Log.Error(LOG_MODULE, "对话数据库未初始化！");
                return false;
            }
            
            var dialogue = dialogueDatabase.GetDialogueById(dialogueId);
            if (dialogue == null)
            {
                Log.Error(LOG_MODULE, $"找不到对话ID: {dialogueId}");
                return false;
            }
            
            currentDialogueId = dialogueId;
            currentDialogue = dialogue;
            isDialogueActive = true;
            currentSpeakerIndex = 0;
            
            Log.Info(LOG_MODULE, $"开始对话: {dialogueId} - {dialogue.SpeakerName}");
            return true;
        }
        
        /// <summary>
        /// 继续到下一段对话
        /// </summary>
        /// <returns>是否成功继续对话</returns>
        public bool ContinueToNextDialogue()
        {
            if (!isDialogueActive || currentDialogue == null)
                return false;
            
            // 如果有选项，让玩家选择，这里不自动继续
            if (currentDialogue.Choices.Count > 0)
                return false;
            
            // 如果有下一段对话ID，继续对话
            if (!string.IsNullOrEmpty(currentDialogue.NextDialogueID))
            {
                return StartDialogue(currentDialogue.NextDialogueID);
            }
            
            // 如果没有下一段对话，结束对话
            EndDialogue();
            return false;
        }
        
        /// <summary>
        /// 选择对话选项
        /// </summary>
        /// <param name="choiceIndex">选项索引</param>
        /// <returns>是否成功选择选项</returns>
        public bool SelectChoice(int choiceIndex)
        {
            if (!isDialogueActive || currentDialogue == null)
                return false;
            
            if (choiceIndex < 0 || choiceIndex >= currentDialogue.Choices.Count)
            {
                Log.Error(LOG_MODULE, $"无效的选项索引: {choiceIndex}");
                return false;
            }
            
            var choice = currentDialogue.Choices[choiceIndex];
            if (!choice.IsAvailable)
            {
                Log.Warning(LOG_MODULE, $"选项当前不可用: {choiceIndex}");
                return false;
            }
            
            // 处理选项触发的事件
            if (!string.IsNullOrEmpty(choice.NextDialogueID))
            {
                return StartDialogue(choice.NextDialogueID);
            }
            
            // 如果选项没有指定下一段对话，结束对话
            EndDialogue();
            return true;
        }
        
        /// <summary>
        /// 结束对话
        /// </summary>
        public void EndDialogue()
        {
            Log.Info(LOG_MODULE, $"结束对话: {currentDialogueId}");
            
            currentDialogue = null;
            currentDialogueId = string.Empty;
            isDialogueActive = false;
            currentSpeakerIndex = 0;
        }
        
        /// <summary>
        /// 更新选项可用性
        /// 可以根据游戏状态动态控制选项是否可用
        /// </summary>
        public void UpdateChoiceAvailability()
        {
            if (currentDialogue == null || currentDialogue.Choices.Count == 0)
                return;
            
            // 这里可以根据游戏状态更新选项可用性
            // 例如：检查玩家是否有特定物品、是否完成特定任务等
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public override void Cleanup()
        {
            if (IsInitialized)
            {
                Log.Info(LOG_MODULE, "清理对话系统模型资源");
                EndDialogue();
                base.Cleanup();
            }
        }
    }
}