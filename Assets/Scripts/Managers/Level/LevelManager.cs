using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.System;
using Level.Grid;

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
    private void Awake()
    {
        GridManager = this.gameObject.GetComponent<GridManager>();
        RuleManager = this.gameObject.GetComponent<RuleManager>();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void init(int level)
    {
        // // 加载关卡数据
        // GameObject levelData = Resources.Load<GameObject>("Levels/Level" + level);
        // if (levelData == null)
        // {
        //     Debug.LogError("Level " + level + " not found!");
        //     return;
        // }
        // // 实例化关卡数据
        // GameObject levelInstance = Instantiate(levelData);
        // 解析关卡数据
        // ...
        // 解析每个对象
        Transform interactableObjects = this.transform.Find("InteractableObjectsParent");//此处通过父物体，找到所有可交互物体，并储存在字典中
        foreach (Transform child in interactableObjects)
        {
            // // 检查是否有ObjectType组件
            // ObjectType objectType = child.GetComponent<ObjectType>();
            // if (objectType != null)
            // {
            //     // 检查字典中是否已有该类型的列表
            //     if (!levelDataDict.ContainsKey(objectType.Type))
            //     {
            //         levelDataDict[objectType.Type] = new List<GameObjectBase>();
            //     }
            //     // 实例化对象并添加到字典
            //     GameObjectBase interactableObject = Instantiate(child.gameObject).GetComponent<GameObjectBase>();
            //     levelDataDict[objectType.Type].Add(interactableObject);
            // }
            GameObjectBase interactableObject = child.GetComponent<GameObjectBase>();
            //interactableObject.GridPosition = child.position;
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