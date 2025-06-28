using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class PropObject : InteractiveObject
{
    [SerializeField] private PropBehaviorSO behaviorConfig;
    private IPropBehavior currentBehavior;

    protected override void Awake()
    {
        base.Awake();
        currentBehavior = behaviorConfig.CreateBehavior();
    }
    public void init(ObjectType type)
    {
        this.Type = behaviorConfig.Type;
    }

    public override void Activate() => currentBehavior?.Execute(this.Type);
    public override void Silence() => currentBehavior?.Cancel(this.Type);
}