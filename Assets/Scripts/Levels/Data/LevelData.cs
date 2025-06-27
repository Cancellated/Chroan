using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public string sceneName;
    public Sprite previewImage;
    public bool isUnlocked;
}