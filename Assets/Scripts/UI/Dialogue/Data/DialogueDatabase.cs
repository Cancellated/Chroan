using System.Collections.Generic;
using UnityEngine;

namespace MyGame.UI.Dialogue.Model
{
    [CreateAssetMenu(fileName = "DialogueDatabase", menuName = "Dialogue/Dialogue Database")]
    public class DialogueDatabase : ScriptableObject
    {
        [SerializeField] private List<DialogueEntry> dialogueEntries = new();
        
        // 对话条目字典，用于快速查找
        private Dictionary<string, DialogueEntry> dialogueDict;
        
        /// <summary>
        /// 初始化对话字典
        /// </summary>
        public void Initialize()
        {
            dialogueDict = new Dictionary<string, DialogueEntry>();
            foreach (var entry in dialogueEntries)
            {
                if (!dialogueDict.ContainsKey(entry.ID))
                {
                    dialogueDict.Add(entry.ID, entry);
                }
            }
        }
        
        /// <summary>
        /// 根据ID获取对话条目
        /// </summary>
        /// <param name="id">对话ID</param>
        /// <returns>对话条目</returns>
        public DialogueEntry GetDialogueById(string id)
        {
            if (dialogueDict == null)
            {
                Initialize();
            }
            
            dialogueDict.TryGetValue(id, out DialogueEntry entry);
            return entry;
        }
    }
}