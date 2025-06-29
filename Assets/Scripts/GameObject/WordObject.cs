using System.Collections.Generic;
using UnityEngine;
//词性检测只用WordObjects检测
public enum WordType//文字的词性
{
    NOUN,
    VERB,
    ADJECTIVE,
}
public class WordObject : GameObjectBase
{
    public WordType WordType ; // 文字类型（名词、动词、属性）
    public string DisplayText { get; private set; } // 代表的文本内容（后期拓展选项）

    [SerializeField]
    public ObjectType word;


    protected override void Start()
    {
        Type = DetermineWordType(WordType);
        //WordType = DetermineWordType(); // 根据Type确定文字类型
        SetTag();
    }

    //添加文字标签
    protected override void SetTag()
    {
        gameObject.tag = "Word";
    }





    //触发规则就调用RuleManager的Rule激活Trigger
    override public void OnRuleApplied(Rule newRule)
    {
        //base.OnRuleApplied(newRule);
        //调用RuleManager的Rule激活Trigger
    }
    override public void OnRuleRemoved(Rule oldRule)
    {
        //base.OnRuleRemoved(oldRule);
    }


    //通过文字内容确定词性
    private ObjectType DetermineWordType(WordType wordType)
    {
        switch (wordType)
        {
            case WordType.NOUN:
                return ObjectType.NOUN;
            case WordType.VERB:
                return ObjectType.IS;
            case WordType.ADJECTIVE:
                return ObjectType.ALIVE;
            default:
                return ObjectType.NOUN;
        }
    }

}
