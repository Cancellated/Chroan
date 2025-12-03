using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Logger;
using MyGame.UI.Dialogue.Model;

namespace MyGame.UI.Dialogue.Editor
{
    /// <summary>
    /// 对话CSV导入工具
    /// 用于将CSV格式的对话数据转换为DialogueDatabase ScriptableObject
    /// </summary>
    public class DialogueCSVImporter : EditorWindow
    {
        private const string LOG_MODULE = LogModules.DIALOGUE;
        
        private TextAsset m_csvFile;
        private string m_outputPath = "Assets/Resources/Dialogues";
        private string m_outputFileName = "NewDialogueDatabase";
        
        // 绘制编辑器窗口
        private void OnGUI()
        {
            GUILayout.Label("对话CSV导入工具", EditorStyles.boldLabel);
            
            m_csvFile = EditorGUILayout.ObjectField("CSV文件:", m_csvFile, typeof(TextAsset), false) as TextAsset;
            
            EditorGUILayout.Space();
            
            GUILayout.Label("输出设置", EditorStyles.boldLabel);
            m_outputPath = EditorGUILayout.TextField("输出路径:", m_outputPath);
            m_outputFileName = EditorGUILayout.TextField("文件名:", m_outputFileName);
            
            EditorGUILayout.Space();
            
            using (new EditorGUI.DisabledGroupScope(m_csvFile == null))
            {
                if (GUILayout.Button("导入CSV并创建DialogueDatabase"))
                {
                    ImportCSV();
                }
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("CSV格式说明:\n" +
                                   "1. 第一行必须是标题行\n" +
                                   "2. 必需列: ID, SpeakerName, DialogueText\n" +
                                   "3. 可选列: SpeakerIcon, NextDialogueID, TriggerEvents\n" +
                                   "4. 选项格式: 列名格式为 Choice_1_Text, Choice_1_NextID, Choice_2_Text, Choice_2_NextID...\n" +
                                   "5. 角色状态格式: 列名格式为 Character_1_ID, Character_1_Name, Character_1_Emotion...", 
                                   MessageType.Info);
        }
        
        /// <summary>
        /// 导入CSV文件并创建DialogueDatabase
        /// </summary>
        private void ImportCSV()
        {
            if (m_csvFile == null)
            {
                Log.Error(LOG_MODULE, "请选择CSV文件");
                return;
            }
            
            // 确保输出目录存在
            if (!Directory.Exists(m_outputPath))
            {
                Directory.CreateDirectory(m_outputPath);
            }
            
            try
            {
                // 解析CSV数据
                List<DialogueEntry> dialogueEntries = ParseCSV(m_csvFile.text);
                
                if (dialogueEntries.Count == 0)
                {
                    Log.Warning(LOG_MODULE, "未解析到任何对话数据");
                    return;
                }
                
                // 创建新的DialogueDatabase
                string assetPath = $"{m_outputPath}/{m_outputFileName}.asset";
                DialogueDatabase database = CreateInstance<DialogueDatabase>();
                
                // 使用序列化对象编辑ScriptableObject
                SerializedObject serializedDatabase = new SerializedObject(database);
                SerializedProperty entriesProperty = serializedDatabase.FindProperty("dialogueEntries");
                
                entriesProperty.ClearArray();
                
                // 添加解析后的对话条目
                foreach (DialogueEntry entry in dialogueEntries)
                {
                    entriesProperty.InsertArrayElementAtIndex(entriesProperty.arraySize);
                    SerializedProperty entryProperty = entriesProperty.GetArrayElementAtIndex(entriesProperty.arraySize - 1);
                    
                    // 设置对话条目属性
                    entryProperty.FindPropertyRelative("ID").stringValue = entry.ID;
                    entryProperty.FindPropertyRelative("SpeakerName").stringValue = entry.SpeakerName;
                    entryProperty.FindPropertyRelative("SpeakerIcon").stringValue = entry.SpeakerIcon;
                    entryProperty.FindPropertyRelative("DialogueText").stringValue = entry.DialogueText;
                    entryProperty.FindPropertyRelative("NextDialogueID").stringValue = entry.NextDialogueID;
                    
                    // 设置触发事件
                    SerializedProperty eventsProperty = entryProperty.FindPropertyRelative("TriggerEvents");
                    eventsProperty.ClearArray();
                    foreach (string evt in entry.TriggerEvents)
                    {
                        eventsProperty.InsertArrayElementAtIndex(eventsProperty.arraySize);
                        eventsProperty.GetArrayElementAtIndex(eventsProperty.arraySize - 1).stringValue = evt;
                    }
                    
                    // 设置选项
                    SerializedProperty choicesProperty = entryProperty.FindPropertyRelative("Choices");
                    choicesProperty.ClearArray();
                    foreach (DialogueChoice choice in entry.Choices)
                    {
                        choicesProperty.InsertArrayElementAtIndex(choicesProperty.arraySize);
                        SerializedProperty choiceProperty = choicesProperty.GetArrayElementAtIndex(choicesProperty.arraySize - 1);
                        
                        choiceProperty.FindPropertyRelative("ChoiceText").stringValue = choice.ChoiceText;
                        choiceProperty.FindPropertyRelative("NextDialogueID").stringValue = choice.NextDialogueID;
                        choiceProperty.FindPropertyRelative("IsAvailable").boolValue = choice.IsAvailable;
                    }
                    
                    // 设置角色状态
                    SerializedProperty characterStatesProperty = entryProperty.FindPropertyRelative("CharacterStates");
                    characterStatesProperty.ClearArray();
                    foreach (CharacterState state in entry.CharacterStates)
                    {
                        characterStatesProperty.InsertArrayElementAtIndex(characterStatesProperty.arraySize);
                        SerializedProperty stateProperty = characterStatesProperty.GetArrayElementAtIndex(characterStatesProperty.arraySize - 1);
                        
                        stateProperty.FindPropertyRelative("CharacterID").stringValue = state.CharacterID;
                        stateProperty.FindPropertyRelative("CharacterName").stringValue = state.CharacterName;
                        stateProperty.FindPropertyRelative("Emotion").stringValue = state.Emotion;
                        stateProperty.FindPropertyRelative("AnimationTrigger").stringValue = state.AnimationTrigger;
                        stateProperty.FindPropertyRelative("PositionOffset").vector2Value = state.PositionOffset;
                        stateProperty.FindPropertyRelative("IsSpeaking").boolValue = state.IsSpeaking;
                    }
                }
                
                // 应用修改
                serializedDatabase.ApplyModifiedProperties();
                
                // 保存资产
                AssetDatabase.CreateAsset(database, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                Log.Info(LOG_MODULE, $"成功创建DialogueDatabase: {assetPath}");
                EditorUtility.DisplayDialog("导入成功", $"成功导入{dialogueEntries.Count}条对话数据并创建DialogueDatabase", "确定");
                
                // 选中创建的资产
                Selection.activeObject = database;
            }
            catch (System.Exception e)
            {
                Log.Error(LOG_MODULE, $"导入CSV时出错: {e.Message}");
                EditorUtility.DisplayDialog("导入失败", $"导入CSV时出错: {e.Message}", "确定");
            }
        }
        
        /// <summary>
        /// 解析CSV文本为对话条目列表
        /// </summary>
        /// <param name="csvText">CSV文本内容</param>
        /// <returns>对话条目列表</returns>
        private List<DialogueEntry> ParseCSV(string csvText)
        {
            List<DialogueEntry> entries = new List<DialogueEntry>();
            
            // 使用正则表达式分割CSV行（考虑引号中的逗号）
            string[] lines = Regex.Split(csvText, "\r\n|,|\r");
            
            if (lines.Length < 2) // 至少需要标题行和一行数据
                return entries;
            
            // 获取标题行
            string[] headers = ParseCSVLine(lines[0]);
            
            // 处理数据行
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                
                string[] values = ParseCSVLine(line);
                
                // 确保值数组长度足够
                if (values.Length < headers.Length)
                {
                    string[] paddedValues = new string[headers.Length];
                    System.Array.Copy(values, paddedValues, values.Length);
                    for (int j = values.Length; j < headers.Length; j++)
                    {
                        paddedValues[j] = "";
                    }
                    values = paddedValues;
                }
                
                // 创建对话条目
                DialogueEntry entry = new DialogueEntry();
                entry.Choices = new List<DialogueChoice>();
                entry.TriggerEvents = new List<string>();
                entry.CharacterStates = new List<CharacterState>();
                
                // 解析基本字段和选项
                Dictionary<int, DialogueChoice> choices = new Dictionary<int, DialogueChoice>();
                Dictionary<int, CharacterState> characterStates = new Dictionary<int, CharacterState>();
                
                for (int j = 0; j < headers.Length; j++)
                {
                    string header = headers[j].Trim();
                    string value = j < values.Length ? values[j].Trim() : "";
                    
                    switch (header)
                    {
                        case "ID":
                            entry.ID = value;
                            break;
                        case "SpeakerName":
                            entry.SpeakerName = value;
                            break;
                        case "SpeakerIcon":
                            entry.SpeakerIcon = value;
                            break;
                        case "DialogueText":
                            entry.DialogueText = value;
                            break;
                        case "NextDialogueID":
                            entry.NextDialogueID = value;
                            break;
                        case "TriggerEvents":
                            if (!string.IsNullOrEmpty(value))
                            {
                                // 使用分号分隔多个事件
                                string[] events = value.Split(';');
                                foreach (string evt in events)
                                {
                                    if (!string.IsNullOrEmpty(evt.Trim()))
                                        entry.TriggerEvents.Add(evt.Trim());
                                }
                            }
                            break;
                    }
                    
                    // 解析选项（格式：Choice_1_Text, Choice_1_NextID）
                    Match choiceMatch = Regex.Match(header, @"Choice_(\d+)_(\w+)");
                    if (choiceMatch.Success)
                    {
                        int choiceIndex = int.Parse(choiceMatch.Groups[1].Value);
                        string propertyName = choiceMatch.Groups[2].Value;
                        
                        if (!choices.ContainsKey(choiceIndex))
                        {
                            choices[choiceIndex] = new DialogueChoice { IsAvailable = true };
                        }
                        
                        DialogueChoice choice = choices[choiceIndex];
                        switch (propertyName)
                        {
                            case "Text":
                                choice.ChoiceText = value;
                                break;
                            case "NextID":
                                choice.NextDialogueID = value;
                                break;
                            case "Available":
                                if (!string.IsNullOrEmpty(value))
                                {
                                    bool.TryParse(value, out choice.IsAvailable);
                                }
                                break;
                        }
                    }
                    
                    // 解析角色状态（格式：Character_1_ID, Character_1_Name 等）
                    Match characterMatch = Regex.Match(header, @"Character_(\d+)_(\w+)");
                    if (characterMatch.Success)
                    {
                        int charIndex = int.Parse(characterMatch.Groups[1].Value);
                        string propertyName = characterMatch.Groups[2].Value;
                        
                        if (!characterStates.ContainsKey(charIndex))
                        {
                            characterStates[charIndex] = new CharacterState();
                        }
                        
                        CharacterState state = characterStates[charIndex];
                        switch (propertyName)
                        {
                            case "ID":
                                state.CharacterID = value;
                                break;
                            case "Name":
                                state.CharacterName = value;
                                break;
                            case "Emotion":
                                state.Emotion = value;
                                break;
                            case "AnimationTrigger":
                                state.AnimationTrigger = value;
                                break;
                            case "PositionX":
                                if (!string.IsNullOrEmpty(value))
                                {
                                    float x;
                                    if (float.TryParse(value, out x))
                                    {
                                        Vector2 pos = state.PositionOffset;
                                        pos.x = x;
                                        state.PositionOffset = pos;
                                    }
                                }
                                break;
                            case "PositionY":
                                if (!string.IsNullOrEmpty(value))
                                {
                                    float y;
                                    if (float.TryParse(value, out y))
                                    {
                                        Vector2 pos = state.PositionOffset;
                                        pos.y = y;
                                        state.PositionOffset = pos;
                                    }
                                }
                                break;
                            case "IsSpeaking":
                                if (!string.IsNullOrEmpty(value))
                                {
                                    bool.TryParse(value, out state.IsSpeaking);
                                }
                                break;
                        }
                    }
                }
                
                // 添加选项到对话条目
                foreach (int key in choices.Keys)
                {
                    entry.Choices.Add(choices[key]);
                }
                
                // 添加角色状态到对话条目
                foreach (int key in characterStates.Keys)
                {
                    entry.CharacterStates.Add(characterStates[key]);
                }
                
                entries.Add(entry);
            }
            
            return entries;
        }
        
        /// <summary>
        /// 解析CSV单行，考虑引号中的逗号
        /// </summary>
        /// <param name="line">CSV行</param>
        /// <returns>分割后的值数组</returns>
        private string[] ParseCSVLine(string line)
        {
            List<string> values = new List<string>();
            bool inQuotes = false;
            System.Text.StringBuilder currentValue = new System.Text.StringBuilder();
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    // 检查是否是转义的引号（两个连续的引号）
                    if (i < line.Length - 1 && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++; // 跳过下一个引号
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // 当遇到逗号且不在引号中时，完成当前值
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }
            
            // 添加最后一个值
            values.Add(currentValue.ToString());
            
            return values.ToArray();
        }
        
        /// <summary>
        /// 在Unity编辑器菜单中添加入口
        /// </summary>
        [MenuItem("工具/对话系统/CSV导入工具")]
        public static void ShowWindow()
        {
            GetWindow<DialogueCSVImporter>("对话CSV导入器");
        }
    }
}