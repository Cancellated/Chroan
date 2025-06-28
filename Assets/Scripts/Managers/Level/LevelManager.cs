using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.System;
using Level.Grid;
using MyGame.Control;
using MyGame.Managers;


namespace Level
{
/// <summary>
/// 管理每个关卡本身逻辑
/// </summary>
public class LevelManager : MonoBehaviour
{
    Dictionary<ObjectType, List<GameObjectBase>> levelDataDict = new Dictionary<ObjectType, List<GameObjectBase>>();
    public GridManager GridManager { get; private set; }
    public RuleManager RuleManager { get; private set; }
    public PlayerController PlayerController { get; private set; }
    private int currentLevel = GameManager.Instance.GetGameProgress().currentLevelIndex;

    private void Awake()
    {
        GridManager = GetComponent<GridManager>();
        RuleManager = GetComponent<RuleManager>();
        PlayerController = FindObjectOfType<PlayerController>();
        // 添加初始化调用
        init(currentLevel);
    }
    void init(int level = 0)
    {

        Transform interactableObjects = this.transform.Find("InteractableObjectsParent");//此处通过父物体，找到所有可交互物体，并储存在字典中
        foreach (Transform child in interactableObjects)
        {
            
            GameObjectBase interactableObject = child.GetComponent<GameObjectBase>();
            
            if (interactableObject != null)
            {
                if (!levelDataDict.ContainsKey(interactableObject.Type))
                {
                    levelDataDict[interactableObject.Type] = new List<GameObjectBase>();

                }
                levelDataDict[interactableObject.Type].Add(interactableObject);
            }
        }
    }

    //找到所有WordObject
    public WordObject[] GetWordObjects()//独立出这个方法，因为rule检测只与wordObject有关
    {
        List<WordObject> wordObjects = new List<WordObject>();
        foreach (var pair in levelDataDict)
        {
            foreach (var obj in pair.Value)
            {
                if (obj is WordObject)
                {
                    wordObjects.Add(obj as WordObject);
                }
            }
        }
        return wordObjects.ToArray();
    }

    //获取指定类型（ObjectType）的所有游戏对象
    public GameObjectBase[] GetGameObjectsOfType(ObjectType type)
    {
        return levelDataDict[type].ToArray();
    }

    public void LevelComplete()
    {
        GameEvents.TriggerGameOver(true);
    }
}
}