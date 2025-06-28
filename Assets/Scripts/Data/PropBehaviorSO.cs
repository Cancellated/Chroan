using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "PropBehavior", menuName = "Game/Prop Behavior")]
public class PropBehaviorSO : ScriptableObject
{
    public ObjectType Type;

    // 创建行为实例（工厂方法）
    public IPropBehavior CreateBehavior()
    {
        return new ConcretePropBehavior(this); // 将配置注入行为实例
    }
}