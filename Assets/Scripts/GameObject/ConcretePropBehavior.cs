using System.Collections;
using System.Collections.Generic;
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
    private List<GameObjectBase> _stickedObjects = new();
    private bool _hasSticked = false;

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
        _propObject = GetComponent<PropObject>();
    }

    public void Execute(ObjectType type)
    {
        switch (type)
        {
            case ObjectType.ROCK:
                StartCoroutine(FleeFromPlayer());
                break;
            case ObjectType.ICE:
                StartCoroutine(IceGrow());
                break;
            case ObjectType.MAILBOX:
                WaitForStory();
                Debug.Log("邮箱开始等待");
                break;
            case ObjectType.SLIME:
                StartCoroutine(FleeFromPlayer());
                
                Debug.Log("史莱姆已经苏醒");
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

                if (gridManager.CanMoveTo(targetPos) && 
                    (_config.MovementRestriction.useAreaRestriction == false ||
                     _config.MovementRestriction.allowedPositions.Contains(targetPos)))
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

    private IEnumerator IceGrow(){
        var levelManager = FindObjectOfType<LevelManager>();
        var gridManager = levelManager.GridManager;
        var player = levelManager.PlayerController;
        player.SetMoveCoolDown(0.5f); // 暂缓玩家输入

        // 初始位置和生长参数
        Vector2Int currentGrowPos = gridManager.WorldToGridPosition(transform.position);
        List<Vector2Int> iceOccupiedPositions = new List<Vector2Int> { currentGrowPos };
        float startTime = Time.time;
        Vector2Int growDirection = Vector2Int.right; // 横向生长方向（向右）

        while (Time.time - startTime < _config._maxGrowTime)
        {
            Vector2Int nextPos = currentGrowPos + growDirection;

            // 检查是否超出网格边界
            if (!gridManager.IsValidPosition(nextPos))
                break;

            // 获取目标位置的所有物体
            List<GameObjectBase> objectsAtNextPos = gridManager.GetAllObjectsAtPosition(nextPos);
            bool canGrow = false;

            if (objectsAtNextPos.Count == 0)
            {
                // 无障碍物，直接生长
                canGrow = true;
            }
            else if (objectsAtNextPos.Count == 1)
            {
                // 单个障碍物，尝试推走
                GameObjectBase obstacle = objectsAtNextPos[0];
                Vector2Int pushTargetPos = nextPos + growDirection;

                // 检查推走目标位置是否有效
                if (gridManager.IsValidPosition(pushTargetPos) && gridManager.CanMoveTo(pushTargetPos))
                {
                    // 推走障碍物
                    gridManager.MoveObject(obstacle, pushTargetPos);
                    canGrow = true;
                }
            }
            // else: 多个障碍物，无法生长

            if (canGrow)
            {
                // 占领新位置
                iceOccupiedPositions.Add(nextPos);
                gridManager.RegisterObject(nextPos, _propObject);
                currentGrowPos = nextPos;

                // 等待生长间隔
                yield return new WaitForSeconds(_config._growSpeed);
            }
            else
            {
                // 无法生长，尝试反向生长（如果是首次受阻）
                if (growDirection == Vector2Int.right)
                {
                    growDirection = Vector2Int.left;
                    currentGrowPos = iceOccupiedPositions[0]; // 回到初始位置
                    yield return new WaitForSeconds(_config._growSpeed);
                }
                else
                {
                    // 双向都受阻，停止生长
                    break;
                }
            }
        }

    }

    ///<summary>
    /// 计算逃跑方向
    /// <returns>逃跑方向</returns>
    /// <summary>
    private Vector2Int CalculateFleeDirection(Vector2Int playerPos, Vector2Int currentPos)
    {
        var gridManager = FindObjectOfType<LevelManager>().GridManager;
        
        // 生成方向候选列表（包含当前计算方向和其他可能方向）
        Vector2Int[] directions = {
            currentPos - playerPos, // 原计算方向
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
    
        // 过滤可移动方向并优先选择安全距离外的位置
        foreach (var dir in directions)
        {
            Vector2Int normalizedDir = new(
                Mathf.Clamp(dir.x, -1, 1),
                Mathf.Clamp(dir.y, -1, 1));
                
            Vector2Int targetPos = currentPos + normalizedDir;
            
            if (gridManager.CanMoveTo(targetPos) && 
                !IsPlayerTooClose(targetPos, playerPos) &&
                (!_config.MovementRestriction.useAreaRestriction || 
                 _config.MovementRestriction.allowedPositions.Contains(targetPos)))
            {
                return normalizedDir;
            }
        }
        return Vector2Int.zero; // 没有可行方向时保持原位
    }

    private bool IsPlayerTooClose(Vector2Int targetPos, Vector2Int playerPos)
    {
        return Mathf.Abs(targetPos.x - playerPos.x) + 
               Mathf.Abs(targetPos.y - playerPos.y) <= _config.SafeDistance;
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

    private IEnumerator StickCheck(Vector2Int targetPos)
    {
        var gridManager = FindObjectOfType<GridManager>();
        
        if (gridManager.CanMoveTo(targetPos))
        {
            var targetObj = gridManager.GetObjectAtPosition(targetPos);
            if (CanStickTo(targetObj))
            {
                StickObject(targetObj);
                yield return StartCoroutine(MoveWithStickedObjects(targetPos));
            }
        }
    }

    private bool CanStickTo(GameObjectBase target)
    {
        return target != null && 
            _config._isSticky &&
            target.Type != ObjectType.SLIME &&
            target.Type != ObjectType.PLAYER;
    }



    private void StickObject(GameObjectBase obj)
    {
        _stickedObjects.Add(obj);
        obj.transform.SetParent(transform);
        _hasSticked = true; // 标记已粘合状态
        _config._isSticky = false; // 暂时禁用粘性
    }

    private IEnumerator MoveWithStickedObjects(Vector2Int targetPos)
    {
        var gridManager = FindObjectOfType<GridManager>();
        if (gridManager.CanMoveTo(targetPos))
        {
            gridManager.MoveObject(_propObject, targetPos);
            foreach (var obj in _stickedObjects)
            {
                gridManager.MoveObject(obj, targetPos);
            }
        }
        yield return new WaitForSeconds(0.5f); // 移动完成后保持非粘性状态0.5秒
        
        // 重置状态
        _hasSticked = false; 
        _config._isSticky = true;
        _stickedObjects.Clear();
    }
}
