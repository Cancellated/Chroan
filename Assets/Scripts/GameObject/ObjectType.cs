using System.Collections.Generic;
using UnityEngine;
public enum ObjectType//可交互对象的具体种类
{
    PLAYER,//玩家
    ROCK,//岩石
    GLASS,//玻璃
    GOAL,//通关物品
    NOUN,//名词，用于区别“物品”和“文字”
    IS,
    ALIVE,
    PUSH//（拓展）
}

// 新增运算符扩展方法
// public static class VerbExtensions
// {
//     public static int GetPriority(this Verb verb) => verb switch
//     {
//         Verb.IS => 2,
//         Verb.AND => 1,
//         Verb.OR => 0,
//         _ => 0
//     };

//     public static bool Combine(this Verb verb, bool a, bool b) => verb switch
//     {
//         Verb.AND => a && b,
//         Verb.OR => a || b,
//         _ => false
//     };
// }
public class Property
{
    public ObjectType Type { get; }

    public Property(ObjectType type)
    {
        Type = type;
    }
}