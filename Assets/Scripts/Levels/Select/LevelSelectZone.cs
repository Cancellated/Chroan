using System.Collections;
using System.Collections.Generic;
using MyGame.Managers;
using UnityEngine;

public class LevelSelectZone : MonoBehaviour
{
   [SerializeField] private LevelData _targetLevel;
   private void Start()
    {
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError("缺少Collider2D组件", this);
        }
        if (_targetLevel == null)
        {
            Debug.LogWarning("未分配LevelData资源", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            Debug.Log("玩家进入关卡选择区域");
            LevelSelectManager.Instance.LoadSelectedLevel(_targetLevel);
        }
    }
}
