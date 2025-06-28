public class ConcretePropBehavior : IPropBehavior
{
    private readonly PropBehaviorSO _config; // 道具配置（伤害值、特效等）
    //private Coroutine _activeCoroutine; // 用于管理持续效果的协程

    // 通过构造函数注入配置
    public ConcretePropBehavior(PropBehaviorSO config)
    {
        _config = config;
    }
    public void Execute(ObjectType Type)
    {
        switch (Type)
        {
            case ObjectType.PLAYER:
                // 执行玩家道具效果
                break;
            case ObjectType.ROCK:
                // 执行规则道具效果
                break;
                // 扩展其他对象类型
        }
    }
    public void Cancel(ObjectType Type)
    {
        switch (Type)
        {
            case ObjectType.PLAYER:
                // 取消玩家道具效果
                break;
            case ObjectType.ROCK:
                // 取消规则道具效果
                break;
                // 扩展其他对象类型
        }
    }

    // 实现接口方法...
}