using UnityEngine;
using AI.BehaviorTree;
using Logger;
using AI.Behavior;

/// <summary>
/// RockBehaviorTree（岩石行为树）类
/// 使用行为树框架控制岩石的行为逻辑
/// </summary>
public class RockBehaviorTree : MonoBehaviour
{
    [Header("岩石状态设置")]
    [Tooltip("岩石苏醒状态下的安全距离")]
    [SerializeField] private float _safeDistance = 6f;
    
    [Header("移动设置")]
    [Tooltip("移动速度")]
    [SerializeField] private float _movementSpeed = 2f;
    [Tooltip("移动冷却时间（秒）")]
    [SerializeField] private float _movementCooldown = 2f;
    
    [Header("引用")]
    [Tooltip("玩家游戏对象")]
    [SerializeField] private GameObject _player;
    [Tooltip("岩石游戏对象的引用")]
    private Rock _rockComponent;
    
    // 移动相关变量
    private Vector3 _moveTarget;
    private bool _isMoving = false;
    private float _lastMovementTime = -Mathf.Infinity;
    private int _escapeAttemptCount = 0;
    private PerceptionComponent _perceptionComponent;
    
    // 行为树相关
    private BehaviorTreeExecutor _behaviorTreeExecutor;
    
    private const string LOG_MODULE = "RockBehaviorTree";
    
    private void Awake()
    {
        // 获取Rock组件引用
        _rockComponent = GetComponent<Rock>();
        
        // 获取或添加感知组件
        _perceptionComponent = GetComponent<PerceptionComponent>();
        if (_perceptionComponent == null)
        {
            _perceptionComponent = gameObject.AddComponent<PerceptionComponent>();
            Log.Info(LogModules.AI, $"RockBehaviorTree: 添加了PerceptionComponent", this);
        }
        _perceptionComponent.Initialize(gameObject);
    }
    
    private void OnEnable()
    {
        // 查找玩家
        FindPlayer();
        
        // 创建并启动行为树
        SetupBehaviorTree();
    }
    
    private void OnDisable()
    {
        // 停止行为树
        if (_behaviorTreeExecutor != null)
        {
            _behaviorTreeExecutor.Stop();
            _behaviorTreeExecutor = null;
        }
    }
    
    private void Update()
    {
        // 确保玩家引用
        FindPlayer();
        
        // 如果正在移动，执行移动
        if (_isMoving && _moveTarget != Vector3.zero)
        {
            MoveTowardsTarget();
        }
    }
    
    /// <summary>
    /// 设置岩石的行为树
    /// 使用选择器作为根节点，包含逃离序列、观察序列和待机动作
    /// </summary>
    private void SetupBehaviorTree()
    {
        // 获取行为树管理器
        var btManager = FindObjectOfType<BehaviorTreeManager>();
        if (btManager == null)
        {
            Log.Error(LOG_MODULE, "未找到BehaviorTreeManager组件", gameObject);
            return;
        }
        
        // 创建根节点 - 选择器
        var root = new BTSelector("RockBehaviorRoot");
        
        // 创建逃离序列
        var escapeSequence = CreateEscapeSequence();
        
        // 创建观察序列 - 玩家在感知范围内但安全
        var observeSequence = new BTSequence("ObserveSequence");
        observeSequence.AddChild(new BTConditionNode(() => CheckIfAwake(), "CheckIfAwake"));
        observeSequence.AddChild(new BTConditionNode(() => IsPlayerInPerceptionRangeButSafe(), "CheckIfPlayerInPerceptionRangeButSafe"));
        observeSequence.AddChild(new BTActionNode(() => { ObservePlayer(); return BTNodeState.Success; }, "ObservePlayer"));
        
        // 创建待机动作
        var idleAction = new BTActionNode(
            () => {
                Idle();
                return BTNodeState.Success;
            },
            "IdleAction"
        );
        
        // 添加子节点到根选择器（优先级：逃跑 > 观察 > 待机）
        root.AddChild(escapeSequence);
        root.AddChild(observeSequence);
        root.AddChild(idleAction);
        
        // 注册行为树
        _behaviorTreeExecutor = btManager.RegisterTree(
            $"RockAI_{gameObject.GetInstanceID()}",
            root,
            0.1f,  // 更新间隔
            true   // 自动开始
        );
        
        // 监听行为树状态变化
        _behaviorTreeExecutor.OnTreeUpdated += OnTreeUpdated;
        _behaviorTreeExecutor.OnTreeCompleted += OnTreeCompleted;
    }
    
    /// <summary>
    /// 创建逃离序列节点
    /// 包含状态检查、玩家距离检查、冷却时间检查和逃离动作
    /// </summary>
    /// <returns>逃离序列节点</returns>
    private BTSequence CreateEscapeSequence()
    {
        var escapeSequence = new BTSequence("EscapeSequence");
        
        // 1. 检查是否苏醒
        var awakeCondition = new BTConditionNode(
            () => CheckIfAwake(),
            "CheckIfAwake"
        );
        
        // 2. 检查玩家是否太靠近
        var playerDistanceCondition = new BTConditionNode(
            () => CheckIfPlayerTooClose(),
            "CheckPlayerDistance"
        );
        
        // 3. 检查移动冷却是否完成
        var cooldownCondition = new BTConditionNode(
            () => CheckMovementCooldown(),
            "CheckMovementCooldown"
        );
        
        // 4. 执行逃离动作
        var escapeAction = new BTActionNode(
            () => {
                if (ExecuteEscapeFromPlayer())
                    return BTNodeState.Success;
                return BTNodeState.Failure;
            },
            "EscapeFromPlayer"
        );
        
        // 添加子节点到序列
        escapeSequence.AddChild(awakeCondition);
        escapeSequence.AddChild(playerDistanceCondition);
        escapeSequence.AddChild(cooldownCondition);
        escapeSequence.AddChild(escapeAction);
        
        return escapeSequence;
    }
    
    /// <summary>
    /// 查找玩家
    /// </summary>
    private void FindPlayer()
    {
        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player");
        }
    }
    
    /// <summary>
    /// 使用感知组件检测玩家
    /// </summary>
    /// <summary>
    /// 检查玩家是否在安全范围内但仍在感知范围内
    /// 用于实现岩石在安全范围内不移动但持续感知环境的功能
    /// </summary>
    private bool IsPlayerInPerceptionRangeButSafe()
    {
        if (_perceptionComponent == null)
            return false;
            
        GameObject nearestThreat = _perceptionComponent.GetNearestThreat();
        if (nearestThreat != null && nearestThreat.CompareTag("Player"))
        {
            float distance = Vector3.Distance(transform.position, nearestThreat.transform.position);
            // 在感知范围内但超过安全距离
            return distance >= _safeDistance && distance <= _perceptionComponent.GetPerceptionRadius();
        }
        return false;
    }
    
    /// <summary>
    /// 检查岩石是否处于苏醒状态
    /// </summary>
    /// <returns>如果处于苏醒状态返回true，否则返回false</returns>
    private bool CheckIfAwake()
    {
        return _rockComponent != null && _rockComponent.CurrentState == Rock.RockState.Awake;
    }
    
    /// <summary>
    /// 检查玩家是否太靠近
    /// </summary>
    /// <returns>如果玩家太靠近返回true，否则返回false</returns>
    private bool CheckIfPlayerTooClose()
    {
        // 完全使用感知组件检测玩家距离
        if (_perceptionComponent != null)
        {
            GameObject nearestThreat = _perceptionComponent.GetNearestThreat();
            if (nearestThreat != null && nearestThreat.CompareTag("Player"))
            {
                _player = nearestThreat;
                float playerDistance = Vector3.Distance(transform.position, nearestThreat.transform.position);
                return playerDistance < _safeDistance;
            }
        }
        
        // 备用方案
        if (_player == null)
        {
            FindPlayer();
            if (_player == null) return false;
        }
        
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        return distance < _safeDistance;
    }
    
    /// <summary>
    /// 检查移动冷却时间
    /// </summary>
    /// <returns>如果冷却完成返回true，否则返回false</returns>
    private bool CheckMovementCooldown()
    {
        return Time.time >= _lastMovementTime + _movementCooldown;
    }
    
    /// <summary>
    /// 执行远离玩家的行为
    /// </summary>
    /// <returns>是否成功开始逃离</returns>
    private bool ExecuteEscapeFromPlayer()
    {
        if (_player == null)
        {
            return false;
        }
        
        // 计算远离玩家的方向
        Vector3 awayDirection = (transform.position - _player.transform.position).normalized;
        
        // 计算逃离目标位置
        Vector3 targetPosition = FindSafeEscapePosition(awayDirection);
        
        if (targetPosition != Vector3.zero)
        {
            SetMoveTarget(targetPosition);
            Log.Info(LOG_MODULE, $"岩石开始远离玩家: {targetPosition}", gameObject);
            return true;
        }
        
        Log.Warning(LOG_MODULE, "无法找到安全的逃离路径", gameObject);
        return false;
    }
    
    /// <summary>
    /// 设置移动目标
    /// </summary>
    /// <param name="target">目标位置</param>
    public void SetMoveTarget(Vector3 target)
    {
        _moveTarget = target;
        _isMoving = true;
        _lastMovementTime = Time.time;
        _escapeAttemptCount++;
    }
    
    /// <summary>
    /// 查找安全的逃离位置
    /// </summary>
    /// <param name="awayDirection">远离玩家的方向</param>
    /// <returns>安全的逃离位置，如果没有找到则返回Vector3.zero</returns>
    private Vector3 FindSafeEscapePosition(Vector3 awayDirection)
    {
        // 尝试多个方向，找到一个可行的逃离位置
        Vector3[] directions = new Vector3[]
        {
            awayDirection,
            Quaternion.Euler(0, 0, 45) * awayDirection,
            Quaternion.Euler(0, 0, -45) * awayDirection,
            Quaternion.Euler(0, 0, 90) * awayDirection,
            Quaternion.Euler(0, 0, -90) * awayDirection
        };
        
        foreach (Vector3 dir in directions)
        {
            Vector3 targetPos = transform.position + dir * _safeDistance;
            
            // 简单的碰撞检测，可以根据实际项目需求扩展
            if (!Physics2D.OverlapCircle(targetPos, 0.5f))
            {
                return targetPos;
            }
        }
        
        // 如果所有方向都不行，尝试随机方向
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
        return transform.position + randomDir * _safeDistance;
    }
    
    /// <summary>
    /// 向目标位置移动
    /// </summary>
    private void MoveTowardsTarget()
    {
        if (_moveTarget != Vector3.zero)
        {
            // 计算方向和距离
            Vector3 direction = (_moveTarget - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, _moveTarget);
            
            // 如果接近目标，停止移动
            if (distance < 0.1f)
            {
                transform.position = _moveTarget;
                _isMoving = false;
                _moveTarget = Vector3.zero;
                Log.Info(LOG_MODULE, "岩石到达目标位置", gameObject);
            }
            else
            {
                // 移动向目标
                transform.position += _movementSpeed * Time.deltaTime * direction;
            }
        }
    }
    
    /// <summary>
    /// 待机行为
    /// </summary>
    private void Idle()
    {
        // 如果正在移动，停止移动
        if (_isMoving)
        {
            _isMoving = false;
            _moveTarget = Vector3.zero;
        }
        // 待机状态
    }
    
    /// <summary>
    /// 观察玩家行为
    /// 当玩家在安全范围内但仍在感知范围内时执行
    /// </summary>
    private void ObservePlayer()
    {
        // 确保停止移动
        if (_isMoving)
        {
            _isMoving = false;
            _moveTarget = Vector3.zero;
        }
        
        // 持续通过感知组件监控玩家
        if (_perceptionComponent != null)
        {
            _perceptionComponent.Execute(); // 强制更新感知
            
            Log.DebugLog(LogModules.AI, "RockBehaviorTree: 正在观察玩家", this);
        }
    }
    
    /// <summary>
    /// 行为树更新回调
    /// </summary>
    private void OnTreeUpdated(BTNodeState state)
    {
        Log.DebugLog(LOG_MODULE, $"行为树更新，当前状态: {state}", gameObject);
    }
    
    /// <summary>
    /// 行为树完成回调
    /// </summary>
    private void OnTreeCompleted(BTNodeState state)
    {
        Log.DebugLog(LOG_MODULE, $"行为树完成，最终状态: {state}", gameObject);
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器中绘制调试信息
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制安全距离范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _safeDistance);
        
        // 绘制移动目标
        if (_isMoving && _moveTarget != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_moveTarget, 0.3f);
            Gizmos.DrawLine(transform.position, _moveTarget);
        }
        
        // 绘制当前状态标签
        GUIStyle style = new();
        style.normal.textColor = Color.white;
        style.fontStyle = FontStyle.Bold;
        
        UnityEditor.Handles.Label(transform.position + new Vector3(0, 1.5f, 0), 
            $"Behavior: {(_isMoving ? "Moving" : "Idle")}\nEscape Attempts: {_escapeAttemptCount}", style);
    }
#endif
}