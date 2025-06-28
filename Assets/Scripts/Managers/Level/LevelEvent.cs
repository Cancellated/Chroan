using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System;
using UnityEngine;


namespace Level
{
    /// <summary>
    /// 关卡内事件
    /// </summary>
    public static class LevelEvent
    {
        public static event Action<ObjectMovedEventData> OnObjectMoved;
        public static void TriggerObjectMoved(ObjectMovedEventData eventData)
        {
            OnObjectMoved?.Invoke(eventData);
        }

        public static event Action<ObjectMovedEventData> OnMoveRequest;
        public static void TriggerMoveRequest(ObjectMovedEventData eventData)
        {
            OnMoveRequest?.Invoke(eventData);
            Debug.Log("触发移动请求");
        }

        #region 激活与沉默
        public static event Action<Rule> OnRuleActivated;
        public static void TriggerRuleDeactivated(Rule rule)
        {
            OnRuleDeactivated?.Invoke(rule);
        }

        public static event Action<Rule> OnRuleDeactivated;
        public static void TriggerRuleActivated(Rule rule)
        {
            OnRuleActivated?.Invoke(rule);
        }

        public static event Action<PropSilencedEventData> OnPropSilenced;
        public static void TriggerPropSilenced(PropSilencedEventData eventData)
        {
            OnPropSilenced?.Invoke(eventData);
        }

        public static event Action<PropActivatedEventData> OnPropActivated;
        public static void TriggerPropActivated(PropActivatedEventData eventData)
        {
            OnPropActivated?.Invoke(eventData);
        }
        #endregion

        #region 道具交互
        //破碎（比如玻璃）
        public static event Action<InteractiveObject> OnObjectBroken;
        public static void TriggerObjectBroken(InteractiveObject obj)
        {
            OnObjectBroken?.Invoke(obj);
        }
        #endregion
    }

    /// <summary>
    /// 道具移动
    /// </summary>
    public class ObjectMovedEventData
    {
        public GameObjectBase Target;
        public Vector2Int OldPos;
        public Vector2Int NewPos;
    }

    /// <summary>
    /// 道具激活
    /// </summary>
    public class PropActivatedEventData
    {
        public PropObject Prop { get; set; }
        public Rule Rule { get; set; }
        public float ActivationTime { get; set; }
    }
    /// <summary>
    /// 道具沉默（不被激活）
    /// </summary>
    public class PropSilencedEventData
    {
        public PropObject Prop { get; set; }
        public Rule Rule { get; set; }
    }
}


