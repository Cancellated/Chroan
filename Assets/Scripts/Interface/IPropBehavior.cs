using UnityEngine;

public interface IPropBehavior
{
    /// <summary>
    /// 执行道具激活逻辑
    /// </summary>
    /// <param name="prop">道具实例</param>
    void Execute(ObjectType Type)
    {
        switch (Type)
        {
            case ObjectType.ROCK:
                //
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 取消道具效果（沉默）
    /// </summary>
    /// <param name="prop">道具实例</param>
    void Cancel(ObjectType Type);

    /// <summary>
    /// 清理资源（可选，用于释放定时器、事件订阅等）
    /// </summary>
    //void Cleanup();
}