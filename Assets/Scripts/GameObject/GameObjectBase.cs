using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;



public abstract class GameObjectBase : MonoBehaviour
{
    public Vector2Int GridPosition { get; protected set; } // 网格坐标
    public ObjectType Type { get; protected set; } // 对象类型（PLAYER、ROCK、GOAL等）
    public List<Property> Properties { get; protected set; } // 动态属性（ALIVE等）
    public List<Rule> AppliedRules { get; protected set; } // 当前应用的规则
    protected virtual void Awake() { }


    protected virtual void Start()
    {
        SetTag();
    }
    protected virtual void SetTag()
    {
        gameObject.tag = "Interactable";
    }
    public void SetGridPosition(Vector2Int newPosition)
    {
        GridPosition = newPosition;
        //transform.position = new Vector2(newPosition.x, newPosition.y);
    }

    // 规则应用时的回调
    public virtual void OnRuleApplied(Rule newRule)
    {

    }

    // 规则移除时的回调
    public virtual void OnRuleRemoved(Rule oldRule)
    {

    }
}
