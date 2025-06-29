using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Level.Grid;
using Level;


/// <summary>
/// 规则管理器，负责游戏规则的检测、激活和应用
/// </summary>
public class RuleManager : MonoBehaviour
{
    //private List<Rule> activeRules = new();//已激活的rule
    //private WordObject[] wordObjects;//当前场景内的所有文字
    private Dictionary<WordObject, List<Vector2Int>> activeRules = new();
    private Dictionary<Vector2Int, List<GameObjectBase>> ruleAffectedPositions = new Dictionary<Vector2Int, List<GameObjectBase>>();
    public LevelManager LevelManager { get; private set; }
    private GridManager gridManager;
    private void Awake()
    {
        LevelManager = this.gameObject.GetComponent<LevelManager>();
        gridManager = this.gameObject.GetComponent<GridManager>();
    }

    private void OnEnable()
    {
        LevelEvent.OnObjectMoved += HandleObjectMoved;
        //LevelEvent.OnObjectRegistered += HandleObjectRegistered;
    }

    private void OnDisable()
    {
        LevelEvent.OnObjectMoved -= HandleObjectMoved;
        //LevelEvent.OnObjectRegistered -= HandleObjectRegistered;
    }

    private void HandleObjectMoved(ObjectMovedEventData eventdata)
    {
        // 如果是文字对象，检查周围规则
        if (eventdata.Target is WordObject)
        {
            CheckRulesAroundPosition(eventdata.NewPos);
            CheckRulesAroundPosition(eventdata.OldPos);
        }

        // // 如果对象受规则影响，更新其状态
        // if (ruleAffectedPositions.ContainsKey(eventdata.NewPos))
        // {
        //     UpdateAffectedObjectsAtPosition(eventdata.NewPos);
        // }
    }

    // private void HandleObjectRegistered(GameObjectBase obj)
    // {
    //     // 新对象注册时，检查其位置是否受现有规则影响
    //     Vector2Int pos = obj.GridPosition;
    //     if (activeRules.Count > 0)
    //     {
    //         ApplyExistingRulesToNewObject(obj, pos);
    //     }
    // }

    private void CheckRulesAroundPosition(Vector2Int center)
    {
        // 检测3x3区域内的所有可能规则组合
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int checkPos = center + new Vector2Int(x, y);

                // 检查水平和垂直规则
                CheckRuleFormation(checkPos, Vector2Int.right); // 水平
                CheckRuleFormation(checkPos, Vector2Int.up);    // 垂直
            }
        }
    }

    private void CheckRuleFormation(Vector2Int startPos, Vector2Int direction)
    {
        // 获取三个连续位置的文字对象
        List<WordObject> words = new List<WordObject>();

        for (int i = -1; i <= 1; i++)
        {
            Vector2Int pos = startPos + direction * i;
            var wordObjs = GetWordObjectsAtPosition(pos);
            words.Add(wordObjs.Count > 0 ? wordObjs[0] : null);
        }

        // 检查是否形成有效规则
        if (words[0] != null && words[1] != null && words[2] != null)
        {
            TryActivateRule(words[0], words[1], words[2]);
        }
        else
        {
            TryDeactivateRule();
        }
    }

    private void TryActivateRule(WordObject noun, WordObject verb, WordObject property)
    {
        // 验证规则语法：名词+动词+属性
        if (noun.WordType == WordType.NOUN && verb.WordType == WordType.VERB && property.WordType == WordType.ADJECTIVE)
        {
            Rule newRule = new Rule(noun.word, verb.Type, property.Type);

            // 如果规则未激活
            if (!activeRules.ContainsKey(noun))
            {
                // 激活规则
                activeRules[noun] = new List<Vector2Int>();
                ApplyRule(newRule);
                LevelEvent.TriggerRuleActivated(newRule);
                for (int i = -1; i <= 1; i++)
                {
                    Vector2Int pos = noun.GridPosition;
                    activeRules[noun].Add(pos);
                }
            }

        }
    }

    /// <summary>
    /// 应用规则效果
    /// </summary>
    private void ApplyRule(Rule rule)
    {
        // 获取所有目标对象
        var targetObjects = LevelManager.GetGameObjectsOfType(rule.Noun);
        if (targetObjects == null)
        {
            Debug.Log("RuleManager: 未找到指定类型的游戏对象");
            return;
        }

        foreach (var obj in targetObjects)
        {
            // 应用规则效果
            obj.OnRuleApplied(rule);

            // 新增：如果是道具对象，通过事件系统触发激活行为
            if (obj is PropObject propObject)
            {
                LevelEvent.TriggerPropActivated(new PropActivatedEventData
                {
                    Prop = propObject,
                    Rule = rule,
                    ActivationTime = Time.time
                });
            }

            // 记录规则影响的位置
            Vector2Int pos = obj.GridPosition;
            if (!ruleAffectedPositions.ContainsKey(pos))
            {
                ruleAffectedPositions[pos] = new List<GameObjectBase>();
            }
            ruleAffectedPositions[pos].Add(obj);
        }
    }

    private void TryDeactivateRule()
    {
        foreach (var noun in activeRules.Keys)
        {
            bool isRuleStillValid = true;
            foreach (var pos in activeRules[noun])
            {
                if (!gridManager.GetObjectAtPosition(pos) is WordObject)
                {
                    isRuleStillValid = false;
                    break;
                }
            }
            if (!isRuleStillValid)
            {
                Rule rule = new Rule(noun.word, ObjectType.IS, ObjectType.ALIVE);
                DeactivateRule(rule, noun);
            }
        }
    }
    
//销毁规则
    private void DeactivateRule(Rule rule, WordObject noun)

    {
        if(LevelManager.GetGameObjectsOfType(rule.Noun)!=null){
            foreach (var obj in LevelManager.GetGameObjectsOfType(rule.Noun))
        {
            LevelEvent.TriggerRuleDeactivated(rule);

            // // 新增：如果是道具对象，通过事件系统触发沉默行为
            // if (obj is PropObject propObject)
            // {
            //     LevelEvent.OnPropSilenced?.Invoke(new PropSilencedEventData
            //     {
            //         Prop = propObject,
            //         Rule = rule
            //     });
            // }
        }

        // 清理记录
        activeRules.Remove(noun);

        LevelEvent.TriggerRuleDeactivated(rule);

        // 清理受影响的规则位置
        foreach (var positionList in ruleAffectedPositions.Values)
        {
            positionList.RemoveAll(obj => obj.Type == rule.Noun);
        }
        }
        // 撤销规则效果
        
    }

    private List<WordObject> GetWordObjectsAtPosition(Vector2Int gridPos)
    {
        // 从GridManager获取位置上的文字对象
        return gridManager.GetAllObjectsAtPosition(gridPos)
            .OfType<WordObject>()
            .ToList();
    }
}