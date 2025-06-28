using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Level;

public class BreakableGlass : InteractiveObject
{
    [SerializeField] private GameObject brokenEffectPrefab;

    public override void Activate()
    {
        Instantiate(brokenEffectPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public override void Silence()
    {
        // 玻璃沉默状态无需特殊处理
    }


    public void Break()
    {
        if (!isActiveAndEnabled) return;
        
        // 触发破碎事件
        LevelEvent.TriggerObjectBroken(this);
        
        // 执行破碎逻辑
        Activate();
    }

    // 修改现有碰撞检测
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("DestructiveRock"))
        {
            Break(); // 改为调用新方法
        }
    }
}
