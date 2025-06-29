using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;    // 关卡名称
    public string sceneName;    // 场景名称
    public Sprite previewImage; // 预览图片
    public bool isUnlocked;     // 是否解锁
    public int levelIndex;      // 关卡索引（用于存档）
    public Sprite uncompletedPreview;  //未过关贴图
    public Sprite completedPreview;    //过关贴图
}
