using System.Collections;
using System.Collections.Generic;
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
        //_propObject = GetComponent<PropObject>();
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
            Debug.Log("结束协程");
        }
    }

    /// <summary>
    /// 岩石从玩家位置逃跑（3格）
    /// </summary>
    /// <returns></returns>
    private IEnumerator FleeFromPlayer()
    {
        Debug.Log("岩石协程开始");
        var levelManager = FindObjectOfType<LevelManager>();
        var gridManager = levelManager.GridManager;
        var player = levelManager.PlayerController;

        while (true)
        {
            Vector2Int playerGridPos = gridManager.WorldToGridPosition(player.transform.position);
            Vector2Int currentGridPos = gridManager.WorldToGridPosition(_config._propObject.gameObject.transform.position);

            // 计算与玩家的曼哈顿距离
            int distance = Mathf.Abs(playerGridPos.x - currentGridPos.x) +
                          Mathf.Abs(playerGridPos.y - currentGridPos.y);
            Debug.Log("与玩家的距离：" + distance);
            Debug.Log("当前位置"+currentGridPos);
            
            if (distance <= _config.SafeDistance)
            {
                Vector2Int direction = CalculateFleeDirection(playerGridPos, currentGridPos);
                Vector2Int targetPos = currentGridPos + direction;
                Debug.Log("目标位置"+direction);

                // if (gridManager.CanMoveTo(targetPos) && 
                //     (_config.MovementRestriction.useAreaRestriction == false ||
                //      _config.MovementRestriction.allowedPositions.Contains(targetPos)))
                if(gridManager.CanMoveTo(targetPos))
                {
                    Debug.Log("岩石开始逃跑");
                    LevelEvent.TriggerMoveRequest(new ObjectMovedEventData
                    {
                        
                        Target = _config._propObject,
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
                gridManager.RegisterObject(nextPos, _config._propObject);
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
    private Vector2Int CalculateFleeDirection(Vector2Int playerGridPos, Vector2Int currentGridPos)
    {
        Vector2Int delta = currentGridPos - playerGridPos;
        
        // 情况1：距离过近（相邻或重叠）
        if (Mathf.Abs(delta.x) <= 1 && Mathf.Abs(delta.y) <= 1)
        {
            // 优先选择与玩家位置差最大的方向逃离
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return new Vector2Int((int)Mathf.Sign(delta.x), 0); // 水平逃离（添加int转换）
            else if (Mathf.Abs(delta.y) > 0)
                return new Vector2Int(0, (int)Mathf.Sign(delta.y)); // 垂直逃离（添加int转换）
            else
                return GetRandomNonZeroDirection(); // 完全重叠时随机方向
        }
        // 情况2：正常距离
        else
        {
            // 基于轴对齐方向逃离（保留原有逻辑但确保非零）
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                return new Vector2Int((int)Mathf.Sign(delta.x), 0); // 添加int转换
            else
                return new Vector2Int(0, (int)Mathf.Sign(delta.y)); // 添加int转换
        }
    }
    
    // 新增：随机非零方向生成器
    private Vector2Int GetRandomNonZeroDirection()
    {
        // 定义4个可能的逃离方向
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        
        // 随机选择一个方向
        return directions[Random.Range(0, directions.Length)];
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
            gridManager.MoveObject(_config._propObject, targetPos);
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
