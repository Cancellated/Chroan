public abstract class InteractiveObject : GameObjectBase
{
    // 实现 IObjectBehavior 接口
    public abstract void Activate();
    public abstract void Silence();

    // 事件订阅（通用逻辑）
    protected override void Awake()
    {
        base.Awake();
        LevelEvent.OnRuleActivated += HandleActivation;
        LevelEvent.OnRuleDeactivated += HandleSilencing;
    }

    // 事件处理（调用行为方法）
    private void HandleActivation(Rule rule)
    {
        if (rule.Noun == this.Type) Activate();  // 触发行为
    }

    private void HandleSilencing(Rule rule)
    {
        if (rule.Noun == this.Type) Silence();  // 触发行为
    }
}