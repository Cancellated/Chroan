using System.Collections;
//using System.Diagnostics;
using Level;
using Level.Grid;
using MyGame.Control;
using MyGame.Managers;
using MyGame.System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConcretePropBehavior : MonoBehaviour, IPropBehavior
{
    private Transform _transform;
    private PropBehaviorSO _config;
    private PropObject _propObject;//其父物体

    private GameControl _inputActions;

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
                Debug.Log("岩石开始逃跑");
                break;
            case ObjectType.MAILBOX:
                WaitForStory();
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
                    LevelEvent.TriggerMoveRequest(new ObjectMovedEventData
                    {
                        Target = _propObject,
                        OldPos = currentGridPos,
                        NewPos = targetPos
                    });
                }
            }
            yield return new WaitForSeconds(_config.MoveInterval);
        }
    }

    // private IEnumerator IceGrow(){
    //     var levelManager = FindObjectOfType<LevelManager>();
    //     var gridManager = levelManager.GridManager;
    //     var player = levelManager.PlayerController;
    //     player.SetMoveCoolDown(0.5f); // 暂缓玩家输入

    //     // 初始位置和生长参数
    //     Vector2Int currentGrowPos = gridManager.WorldToGridPosition(transform.position);
    //     List<Vector2Int> iceOccupiedPositions = new List<Vector2Int> { currentGrowPos };
    //     float startTime = Time.time;
    //     Vector2Int growDirection = Vector2Int.right; // 横向生长方向（向右）

    //     while (Time.time - startTime < _config._maxGrowTime)
    //     {
    //         Vector2Int nextPos = currentGrowPos + growDirection;

    //         // 检查是否超出网格边界
    //         if (!gridManager.IsValidPosition(nextPos))
    //             break;

    //         // 获取目标位置的所有物体
    //         List<GameObjectBase> objectsAtNextPos = gridManager.GetAllObjectsAtPosition(nextPos);
    //         bool canGrow = false;

    //         if (objectsAtNextPos.Count == 0)
    //         {
    //             // 无障碍物，直接生长
    //             canGrow = true;
    //         }
    //         else if (objectsAtNextPos.Count == 1)
    //         {
    //             // 单个障碍物，尝试推走
    //             GameObjectBase obstacle = objectsAtNextPos[0];
    //             Vector2Int pushTargetPos = nextPos + growDirection;

    //             // 检查推走目标位置是否有效
    //             if (gridManager.IsValidPosition(pushTargetPos) && gridManager.CanMoveTo(pushTargetPos))
    //             {
    //                 // 推走障碍物
    //                 gridManager.MoveObject(obstacle, pushTargetPos);
    //                 canGrow = true;
    //             }
    //         }
    //         // else: 多个障碍物，无法生长

    //         if (canGrow)
    //         {
    //             // 占领新位置
    //             iceOccupiedPositions.Add(nextPos);
    //             gridManager.RegisterObject(nextPos, _propObject);
    //             currentGrowPos = nextPos;

    //             // 等待生长间隔
    //             yield return new WaitForSeconds(_config._growSpeed);
    //         }
    //         else
    //         {
    //             // 无法生长，尝试反向生长（如果是首次受阻）
    //             if (growDirection == Vector2Int.right)
    //             {
    //                 growDirection = Vector2Int.left;
    //                 currentGrowPos = iceOccupiedPositions[0]; // 回到初始位置
    //                 yield return new WaitForSeconds(_config._growSpeed);
    //             }
    //             else
    //             {
    //                 // 双向都受阻，停止生长
    //                 break;
    //             }
    //         }
    //     }

    // }

    ///<summary>
    /// 计算逃跑方向
    /// <returns>逃跑方向</returns>
    /// <summary>
    private Vector2Int CalculateFleeDirection(Vector2Int playerPos, Vector2Int currentPos)
    {
        // 优先选择与玩家位置相反的方向
        Vector2Int delta = currentPos - playerPos;
        return new Vector2Int(
            delta.x != 0 ? Mathf.Clamp(delta.x, -1, 1) : 0,
            delta.y != 0 ? Mathf.Clamp(delta.y, -1, 1) : 0
        );
    }


    ///<summary>
    /// 触发对话
    /// <summary>
    public void WaitForStory()
    {
        if (_inputActions.GamePlay.Interact.IsPressed())
        {
            // 获取玩家当前位置
            var playerPos = FindObjectOfType<LevelManager>().PlayerController.CurrentGridPos;

            // 计算曼哈顿距离
            int distance = Mathf.Abs(playerPos.x - GridPosition.x) +
                          Mathf.Abs(playerPos.y - GridPosition.y);

            if (distance == 1) // 相邻网格判断
            {
                GameEvents.TriggerStoryEnter(_config._storyId);
            }
        }
    }

}