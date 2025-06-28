using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System;
using UnityEngine;

public class ObjectMovedEventData
{
    public GameObjectBase Target;
    public Vector2Int OldPos;
    public Vector2Int NewPos;
}
public class PropActivatedEventData
{
    public PropObject Prop { get; set; }
    public Rule Rule { get; set; }
    public float ActivationTime { get; set; }
}
public class PropSilencedEventData
{
    public PropObject Prop { get; set; }
    public Rule Rule { get; set; }
}

public static class LevelEvent
{
    public static event Action<ObjectMovedEventData> OnObjectMoved;

    public static event Action<Rule> OnRuleActivated;
    public static event Action<Rule> OnRuleDeactivated;
    public static event Action<PropActivatedEventData> OnPropActivated;
    public static event Action<PropSilencedEventData> OnPropSilenced;

    public static void TriggerPropActivated(PropActivatedEventData eventData)
    {
        OnPropActivated?.Invoke(eventData);
    }
    public static void TriggerPropSilenced(PropSilencedEventData eventData)
    {
        OnPropSilenced?.Invoke(eventData);
    }

    public static void TriggerRuleActivated(Rule rule)
    {
        OnRuleActivated?.Invoke(rule);
    }
    public static void TriggerRuleDeactivated(Rule rule)
    {
        OnRuleDeactivated?.Invoke(rule);
    }

    public static void TriggerObjectMoved(ObjectMovedEventData eventData)
    {
        OnObjectMoved?.Invoke(eventData);
    }
}

