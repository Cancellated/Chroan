using System.Collections.Generic;
using UnityEngine;

namespace MyGame.UI.Dialogue.Model
{
    [System.Serializable]
    public class DialogueEntry
    {
        public string ID; // 对话ID
        public string SpeakerName; // 说话者名称
        public string SpeakerIcon; // 说话者头像引用
        public string DialogueText; // 对话文本
        public List<DialogueChoice> Choices = new(); // 对话选项
        public string NextDialogueID; // 下一个对话ID（如果没有选项）
        public List<string> TriggerEvents = new(); // 触发的事件列表
        
        public List<CharacterState> CharacterStates = new(); // 当前对话中所有角色的状态
    }
    
    [System.Serializable]
    public class DialogueChoice
    {
        public string ChoiceText; // 选项文本
        public string NextDialogueID; // 选择该选项后跳转的对话ID
        public bool IsAvailable; // 该选项是否可用（可能基于游戏状态）
    }
    
    [System.Serializable]
    public class CharacterState
    {
        public string CharacterID; // 角色唯一标识
        public string CharacterName; // 角色名称
        public string Emotion; // 表情差分
        public string AnimationTrigger; // 要触发的动画触发器
        public Vector2 PositionOffset; // 在对话UI中的位置偏移
        public bool IsSpeaking; // 是否是当前说话者
    }
}