using System.Collections;
using Level;
using Level.Grid;
using MyGame.Control;
using MyGame.Managers;
using UnityEngine;

public class ConcretePropBehavior : MonoBehaviour, IPropBehavior
{
    private Transform _transform;
    private PropBehaviorSO _config;
    private PropObject _propObject;//其父物体

    public Vector2Int GridPosition { get; set; }

    public void Initialize(PropBehaviorSO config)
    {
        _config = config;
        GetComponent<SpriteRenderer>().sprite = _config.DisplaySprite;
    }
    
    void Awake()
    {
        _transform = GetComponent<Transform>();
        _propObject = GetComponentInParent<PropObject>();
    }

    public void Execute(ObjectType type)
    {
        switch (type)
        {
            case ObjectType.ROCK:
                StartCoroutine(FleeFromPlayer());
                break;
        }
    }

    public void Cancel(ObjectType type)
    {
        if (type == ObjectType.ROCK)
        {
            StopAllCoroutines();
        }
    }

    /// <summary>
    /// 岩石从玩家位置逃跑（3格）
    /// </summary>
    /// <returns></returns>
    private IEnumerator FleeFromPlayer()
    {
        var levelManager = FindObjectOfType<LevelManager>();
        var gridManager = levelManager.GridManager;
        var player = levelManager.PlayerController;
    
        while (true)
        {
            Vector2Int playerGridPos = gridManager.WorldToGridPosition(player.transform.position);
            Vector2Int currentGridPos = gridManager.WorldToGridPosition(transform.position);
    
            // 计算与玩家的曼哈顿距离
            int distance = Mathf.Abs(playerGridPos.x - currentGridPos.x) + 
                          Mathf.Abs(playerGridPos.y - currentGridPos.y);
    
            if (distance <= _config.SafeDistance)
            {
                Vector2Int direction = CalculateFleeDirection(playerGridPos, currentGridPos);
                Vector2Int targetPos = currentGridPos + direction;
    
                if (gridManager.CanMoveTo(targetPos))
                {
                    LevelEvent.TriggerMoveRequest(new ObjectMovedEventData {
                        Target = _propObject,
                        OldPos = currentGridPos,
                        NewPos = targetPos
                    });
                }
            }
            yield return new WaitForSeconds(_config.MoveInterval);
        }
    }
    private IEnumerator IceGrow(){
        playerController.SetMoveCoolDown(0.5f);//暂缓玩家输入
        yield return new WaitForSeconds(_config._maxGrowTime);

    }

    private Vector2Int CalculateFleeDirection(Vector2Int playerPos, Vector2Int currentPos)
    {
        // 优先选择与玩家位置相反的方向
        Vector2Int delta = currentPos - playerPos;
        return new Vector2Int(
            delta.x != 0 ? Mathf.Clamp(delta.x, -1, 1) : 0,
            delta.y != 0 ? Mathf.Clamp(delta.y, -1, 1) : 0
        );
    }

}