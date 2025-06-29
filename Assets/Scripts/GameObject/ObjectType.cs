using System.Collections.Generic;
using UnityEngine;
public enum ObjectType//可交互对象的具体种类
{
    PLAYER,//玩家
    ROCK,//岩石
    ICE,//玻璃
    MAILBOX,//邮箱
    GOAL,//通关物品
    SLIME,//粘液

    NOUN,//名词，用于区别“物品”和“文字”
    IS,
    ALIVE,
    PUSH//（拓展）
}

public class Property
{
    public ObjectType Type { get; }

    public Property(ObjectType type)
    {
        Type = type;
    }
}