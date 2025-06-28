using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogData
{
    // 对话ID
    public string DialogID;
    // 对话正文内容，支持富文本标签
    public string Content;
    // 说话角色立绘资源路径
    public string Character;
    //角色名称
    public string Name;
    //关联的背景音乐资源路径
    public string BGM;
    //关联的背景图片资源路径
    public string Background;
    public DialogData(string[] fields, Dictionary<string, int> headerMap)
    {
        DialogID = GetFieldSafe(fields, "DialogID", headerMap);
        Content = GetFieldSafe(fields, "Content", headerMap);
        Character = GetFieldSafe(fields, "Character", headerMap);
        Name = GetFieldSafe(fields, "Name", headerMap);
        BGM = GetFieldSafe(fields, "BGM", headerMap);
        Background = GetFieldSafe(fields, "Background", headerMap);
    }

    private string GetFieldSafe(string[] fields, string header, Dictionary<string, int> headerMap)
    {
        if (headerMap.TryGetValue(header, out int index) && index < fields.Length)
        {
            return fields[index];
        }
        return string.Empty;
    }
}