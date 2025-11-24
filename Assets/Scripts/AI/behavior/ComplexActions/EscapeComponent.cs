using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger;
using UnityEngine.Tilemaps;
using System.Collections;

namespace AI.Behavior
{
    /// <summary>
    /// 逃离行为组件
    /// 负责处理AI从威胁源逃离的行为逻辑
    /// 最近更新：实现了使用点乘法计算逃离方向权重的功能，使AI选择更合理的逃离方向
    /// 使用向量点积原理来确定最佳逃离方向，点积值越大表示方向越适合逃离
    /// </summary>
    public class EscapeComponent : MonoBehaviour, IActionComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName { get { return "EscapeComponent"; } }

        /// <summary>
        /// 移动组件引用
        /// </summary>
        private MovementComponent _movementComponent;

        /// <summary>
        /// 感知组件引用
        /// </summary>
        private PerceptionComponent _perceptionComponent;

        /// <summary>
        /// 威胁源位置
        /// </summary>
        private Transform _threatSource;

        /// <summary>
        /// 逃离安全距离，
        /// 即AI在威胁源周围的安全距离，低于此距离时才会触发逃离行为
        /// </summary>
        [SerializeField] private float _safetyDistance = 2f;

        /// <summary>
        /// 逃离速度倍数
        /// </summary>
        [SerializeField] private float _escapeSpeedMultiplier = 1.5f;

        /// <summary>
        /// 逃跑触发距离（小于安全距离时触发）
        /// 即AI在威胁源周围的距离，低于此距离时才会触发逃离行为
        /// </summary>
        [SerializeField] private float _escapeTriggerDistance = 1.5f;
        [SerializeField] private float _decisionInterval = 0.5f; // 移动决策间隔时间（秒）
        private bool _canMakeDecision = true; // 是否可以进行决策
        private bool _decisionCooldownRunning = false; // 决策冷却是否正在运行
        private bool _canLog = true; // 是否可以输出日志
        private const float LogCooldownDuration = 1.0f; // 日志冷却持续时间（秒）
        
        /// <summary>
        /// 死胡同响应类型枚举
        /// </summary>
        public enum DeadEndResponseType
        {
            GiveUpEscape,  // 放弃逃离
            ReverseBreakthrough  // 反向突围（朝向威胁源方向移动）
        }
        
        /// <summary>
        /// 死胡同响应策略开关
        /// </summary>
        [SerializeField] private DeadEndResponseType _deadEndResponseType = DeadEndResponseType.GiveUpEscape;
        
        /// <summary>
        /// 调试模式开关
        /// </summary>
        /// <summary>
        /// 调试模式开关 - 设置为true可以在控制台查看详细的方向选择和点乘法计算日志
        /// </summary>
        public bool _debugMode = true;

        /// <summary>
        /// 测试点乘法方向选择功能
        /// 可以在Unity编辑器中手动调用此方法来验证不同威胁方向下的方向选择逻辑
        /// </summary>
        /// <param name="threatDirection">测试用的威胁方向向量</param>
        public void TestDotProductDirectionSelection(Vector2 threatDirection)
        {
            _debugMode = true; // 确保测试时开启调试日志
            Log.Info(LogModules.AI, $"{ComponentName}: 开始测试点乘法方向选择，威胁方向: {GetDirectionName(threatDirection)}");
            
            // 定义四个基本方向进行测试
            Vector2[] testDirections = new Vector2[]
            {
                new(0, 1),  // 上
                new(1, 0),  // 右
                new(0, -1), // 下
                new(-1, 0)  // 左
            };
            
            // 调用增强版角落方向选择算法
            Vector2 bestDirection = EnhancedCornerDirectionSelection(testDirections, threatDirection.normalized);
            
            // 计算并显示每个方向的点积权重
            Vector2 awayFromThreat = -threatDirection.normalized;
            Log.Info(LogModules.AI, $"{ComponentName}: 测试结果 - 远离威胁方向: {GetDirectionName(awayFromThreat)}");
            foreach (Vector2 dir in testDirections)
            {
                float weight = CalculateDirectionWeightUsingDotProduct(dir, awayFromThreat);
                Log.Info(LogModules.AI, $"{ComponentName}: 方向 {GetDirectionName(dir)} 的点积权重: {weight}");
            }
            
            Log.Info(LogModules.AI, $"{ComponentName}: 测试结果 - 选择的最佳方向: {GetDirectionName(bestDirection)}");
        }

        /// <summary>
        /// 是否正在逃离
        /// </summary>
        private bool _isEscaping = false;

        /// <summary>
        /// 原始移动速度
        /// </summary>
        private float _originalSpeed;
        
        /// <summary>
        /// 标记是否正在执行单步移动
        /// </summary>
        private bool _isMovingToTarget = false;

        /// <summary>
        /// 初始化组件
        /// </summary>
        public void Initialize(GameObject owner)
        {
            // 获取移动组件引用
            _movementComponent = gameObject.GetComponent<MovementComponent>();
            if (_movementComponent == null)
            {
                _movementComponent = gameObject.AddComponent<MovementComponent>();
                Log.Warning(LogModules.AI, $"{ComponentName}: 移动组件未找到，已自动添加", this);
            }

            // 初始化移动组件
            _movementComponent.Initialize(gameObject);
            
            // 保存原始移动速度
            _originalSpeed = _movementComponent.CurrentSpeed;
            
            // 获取感知组件引用
            _perceptionComponent = gameObject.GetComponent<PerceptionComponent>();
            if (_perceptionComponent == null)
            {
                _perceptionComponent = gameObject.AddComponent<PerceptionComponent>();
                Log.Warning(LogModules.AI, $"{ComponentName}: 感知组件未找到，已自动添加", this);
                _perceptionComponent.Initialize(gameObject);
            }
            
            // 设置Tilemap引用，用于地面检测
            SetupTilemapReference();
            if (_groundTilemap == null)
            {
                Log.Warning(LogModules.AI, $"{ComponentName}: 未找到Tilemap引用，将使用射线检测作为备用地面检测方法", this);
            }
        }

        /// <summary>
        /// 判断是否可以执行逃离行为
        /// 当检测到威胁且威胁在触发距离内时返回true
        /// </summary>
        /// <returns>是否可以执行逃离行为</returns>
        public bool CanExecute()
        {
            // 使用UpdateThreatSource获取最新的威胁源信息
            UpdateThreatSource();
            
            // 检查是否存在威胁源
            if (_threatSource == null)
            {
                Log.DebugLog(LogModules.AI, $"{ComponentName}: CanExecute - 无威胁源", this);
                return false;
            }
            
            // 检查威胁是否在感知范围内
            if (_perceptionComponent != null)
            {
                float distanceToThreat = Vector2.Distance(transform.position, _threatSource.position);
                float perceptionRadius = _perceptionComponent.GetPerceptionRadius();
                
                bool inPerceptionRange = distanceToThreat <= perceptionRadius;
                bool inTriggerRange = distanceToThreat <= _escapeTriggerDistance;
                
                Log.DebugLog(LogModules.AI, $"{ComponentName}: CanExecute - 到威胁距离: {distanceToThreat}, 感知范围: {perceptionRadius}, 触发距离: {_escapeTriggerDistance}, 在感知范围内: {inPerceptionRange}, 在触发距离内: {inTriggerRange}", this);
                
                // 只有当威胁在感知范围内且在触发距离内时，才可以执行逃离行为
                return inPerceptionRange && inTriggerRange;
            }
            
            Log.DebugLog(LogModules.AI, $"{ComponentName}: CanExecute - 缺少感知组件", this);
            return false;
        }

        /// <summary>
        /// 执行逃离行为
        /// 使用Tilemap网格检测方法确保AI只在有效地面上移动
        /// </summary>
        /// <returns>执行是否成功</returns>
        public bool Execute()
        {
            // 测试辅助日志
            Log.DebugLog(LogModules.AI, $"{ComponentName}: 开始执行逃离行为，当前位置: {transform.position}", this);
            
            if (!CanExecute() || _movementComponent == null)
            {
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 无法执行逃离行为，条件不满足或移动组件为空", this);
                return false;
            }
            
            // 如果正在移动到目标，则不更新方向
            if (_isMovingToTarget)
            {
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 正在移动中，暂不更新方向", this);
                return true;
            }

            // 设置为逃离状态
            _isEscaping = true;

            // 增加移动速度用于逃离
            _movementComponent.ModifySpeedByMultiplier(_escapeSpeedMultiplier);

            // 确保当前位置对齐到网格中心
            AlignToGridCenter();
            
            // 计算逃离方向
            Vector2 escapeDirection = CalculateEscapeDirection();
            
            // 检查是否有有效的逃离方向
            if (escapeDirection == Vector2.zero)
            {
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 无法找到有效逃离方向，逃离失败", this);
                
                // 当返回零向量时，判断是否为主动放弃逃离
                if (_deadEndResponseType == DeadEndResponseType.GiveUpEscape && _perceptionComponent != null && _threatSource != null)
                {
                    // 是主动放弃逃离，停止逃离行为
                    StopEscaping();
                    return false;
                }
                else
                {
                    Log.Warning(LogModules.AI, $"{ComponentName}: 未能找到合适的逃离方向", this);
                    return false;
                }
            }

            // 获取当前位置（已经对齐到网格中心）
            Vector2 currentPosition = transform.position;
            
            // 首先检查_groundTilemap是否有效，如果无效则无法进行可通行性检查
            if (_groundTilemap == null)
            {    
                Log.Warning(LogModules.AI, $"{ComponentName}: _groundTilemap为null，无法进行可通行性检查，取消移动", this);
                return false;
            }
            
            // 使用网格系统计算目标位置 - 确保移动在tilemap格子上对齐
            // 将当前位置转换为网格坐标
            Vector3Int currentGridPos = WorldToGridPosition(currentPosition);
            
            // 限制方向为四向（上下左右）并标准化为单位向量
            Vector2 normalizedDirection = LimitToFourDirections(escapeDirection);
            
            // 计算目标网格坐标（每次精确移动一个格子）
            Vector3Int targetGridPos = currentGridPos + new Vector3Int(
                Mathf.RoundToInt(normalizedDirection.x),
                Mathf.RoundToInt(normalizedDirection.y),
                0
            );
            
            // 使用IsCellWalkable检查目标位置是否可通行（同时检查地面和障碍物）
            // IsCellWalkable内部已包含三层检查：地板存在性、墙体检查、障碍物检查
            bool isCellWalkable = IsCellWalkable(targetGridPos);
            Log.DebugLog(LogModules.AI, $"{ComponentName}: 目标网格位置({targetGridPos})可通行检查: {isCellWalkable}", this);
            
            // 如果目标位置不可通行，尝试寻找其他方向（改进的变向逻辑）
            if (!isCellWalkable)
            { 
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 目标网格位置不可通行，尝试寻找其他方向", this);
                
                // 尝试所有四个基本方向，优先选择远离威胁的方向
                Vector2[] directions = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
                
                // 对方向进行排序，优先选择远离威胁的方向
                List<Vector2> sortedDirections = directions.Where(dir => dir != normalizedDirection)
                                                          .OrderByDescending(dir => Vector2.Dot(dir, escapeDirection))
                                                          .ToList();
                
                foreach (Vector2 dir in sortedDirections)
                { 
                    // 计算新的目标网格坐标
                    Vector3Int newTargetGridPos = currentGridPos + new Vector3Int(
                        Mathf.RoundToInt(dir.x),
                        Mathf.RoundToInt(dir.y),
                        0
                    );
                    
                    // 检查新方向是否可通行
                    if (IsCellWalkable(newTargetGridPos))
                    { 
                        Log.DebugLog(LogModules.AI, $"{ComponentName}: 找到替代方向 {dir}", this);
                        targetGridPos = newTargetGridPos;
                        normalizedDirection = dir;
                        break;
                    }
                }
                
                // 再次检查最终选择的方向是否可通行
                if (!IsCellWalkable(targetGridPos))
                {    
                    Log.DebugLog(LogModules.AI, $"{ComponentName}: 没有找到可通行的目标位置，取消移动", this);
                    return false;
                }
            }
            
            // 将目标网格坐标转换为世界坐标（自动对齐到网格中心）
            Vector3 targetWorldPosition = GridToWorldPosition(targetGridPos);
            Log.DebugLog(LogModules.AI, $"{ComponentName}: 移动目标位置 - 网格: {targetGridPos}, 世界: {targetWorldPosition}", this);
            
            _movementComponent.SetTarget(targetWorldPosition);

            // 设置移动中标志
            _isMovingToTarget = true;
            
            // 在串行执行流程中，移动组件会在自己的Update循环中执行，不需要在这里直接调用Execute
            // 移动组件现在有_hasDecisionCommand标志，确保只在收到决策命令后才执行移动
            Log.DebugLog(LogModules.AI, $"{ComponentName}: 设置移动目标，方向: {escapeDirection}", this);
            
            return true; // 决策已完成，返回成功
        }
        
        
        /// <summary>
        /// 限制方向向量为四向（上、下、左、右）
        /// </summary>
        /// <param name="direction">原始方向向量</param>
        /// <returns>限制后的四向单位向量</returns>
        private Vector2 LimitToFourDirections(Vector2 direction)
        {
            // 归一化方向向量
            direction.Normalize();
            
            // 获取所有四个基本方向
            Vector2[] possibleDirections = new Vector2[] {
                Vector2.up,
                Vector2.down,
                Vector2.left,
                Vector2.right
            };
            
            // 如果没有威胁源，使用原始逻辑
            if (_threatSource == null)
            {
                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                {
                    // 水平方向优先
                    return direction.x > 0 ? Vector2.right : Vector2.left;
                }
                else
                {
                    // 垂直方向优先
                    return direction.y > 0 ? Vector2.up : Vector2.down;
                }
            }
            
            // 计算从角色到威胁源的方向
            Vector2 threatDirection = (_threatSource.position - transform.position).normalized;
            
            // 为每个可能的方向计算评分
            float[] scores = new float[4];
            
            for (int i = 0; i < possibleDirections.Length; i++)
            {
                Vector2 dir = possibleDirections[i];
                
                // 计算方向与理想逃离方向的相似度（点积，值越大表示方向越接近）
                float similarityScore = Vector2.Dot(dir, direction);
                
                // 计算方向与威胁方向的夹角（点积，值越大表示方向越接近威胁）
                float threatAngleScore = Vector2.Dot(dir, threatDirection);
                
                // 增强版评分算法：
                // 1. 高相似度加分
                // 2. 大幅增加威胁接近度的惩罚权重，避免AI向玩家方向移动
                // 3. 如果方向直接朝向威胁源（威胁角度分数大于0.5），给予极端惩罚
                float penaltyMultiplier = threatAngleScore > 0.5f ? 15.0f : 8.0f; // 进一步增加惩罚权重
                scores[i] = similarityScore - (threatAngleScore * penaltyMultiplier);
            }
            
            // 找到评分最高的方向
            int bestDirectionIndex = 0;
            float highestScore = scores[0];
            
            for (int i = 1; i < scores.Length; i++)
            {
                if (scores[i] > highestScore)
                {
                    highestScore = scores[i];
                    bestDirectionIndex = i;
                }
            }
            
            Log.Info(LogModules.AI, $"{ComponentName}: 逃离方向选择 - 原始方向:{direction}, 最佳方向:{possibleDirections[bestDirectionIndex]}", this);
            
            return possibleDirections[bestDirectionIndex];
        }
        
        /// <summary>
        /// 增强版角落方向选择算法
        /// 专门处理角落场景下的方向选择，使用点乘法计算最佳逃离方向
        /// 确保AI选择最远离威胁源的可通行方向
        /// </summary>
        /// <param name="possibleDirections">可能的移动方向</param>
        /// <param name="threatDirection">威胁源方向</param>
        /// <returns>基于点乘法计算的最佳逃离方向</returns>
        private Vector2 EnhancedCornerDirectionSelection(Vector2[] possibleDirections, Vector2 threatDirection)
        {
            // 首先过滤出所有不朝向威胁源的方向
            List<Vector2> safeDirections = new();
            List<Vector2> riskyDirections = new();
            
            foreach (Vector2 dir in possibleDirections)
            {
                float threatDot = Vector2.Dot(dir, threatDirection);
                
                // 检查方向是否可通行
                Vector3Int currentGridPos = WorldToGridPosition(transform.position);
                Vector3Int testGridPos = currentGridPos + new Vector3Int(
                    Mathf.RoundToInt(dir.x),
                    Mathf.RoundToInt(dir.y),
                    0
                );
                
                // 只有可通行的方向才考虑
                if (IsCellWalkable(testGridPos))
                {
                    if (threatDot < 0) // 真正远离威胁源（必须严格小于0）
                    {
                        safeDirections.Add(dir);
                        if (_debugMode) Log.DebugLog(LogModules.AI, $"{ComponentName}: 角落场景中安全方向: {dir} (与威胁点积: {threatDot})");
                    }
                    else // 朝向或垂直于威胁源
                    {
                        riskyDirections.Add(dir);
                        if (_debugMode) Log.DebugLog(LogModules.AI, $"{ComponentName}: 角落场景中风险方向: {dir} (与威胁点积: {threatDot})");
                    }
                }
            }
            
            // 如果有安全方向（不朝向威胁源且可通行），优先选择这些方向
            if (safeDirections.Count > 0)
            {
                // 使用点乘法计算最佳逃离方向 - 基于与远离威胁方向的一致性
                Vector2 awayFromThreatDirection = -threatDirection.normalized;
                Vector2 bestDirection = safeDirections[0];
                float highestWeight = CalculateDirectionWeightUsingDotProduct(bestDirection, awayFromThreatDirection);
                
                foreach (Vector2 dir in safeDirections.Skip(1))
                {
                    float directionWeight = CalculateDirectionWeightUsingDotProduct(dir, awayFromThreatDirection);
                    if (directionWeight > highestWeight) // 点积值越大表示方向越适合逃离
                    {
                        highestWeight = directionWeight;
                        bestDirection = dir;
                    }
                }
                
                if (_debugMode) Log.Info(LogModules.AI, $"{ComponentName}: 角落场景中的最佳安全逃离方向: {GetDirectionName(bestDirection)} (点积权重: {highestWeight})");
                return bestDirection;
            }
            else if (riskyDirections.Count > 0)
            {
                // 如果没有安全方向，只能选择风险最小的方向（朝向威胁源程度最低的）
                Vector2 bestDirection = riskyDirections[0];
                float minThreatDot = Vector2.Dot(bestDirection, threatDirection);
                
                foreach (Vector2 dir in riskyDirections.Skip(1))
                {
                    float threatDot = Vector2.Dot(dir, threatDirection);
                    if (threatDot < minThreatDot) // 朝向威胁源程度更低
                    {
                        minThreatDot = threatDot;
                        bestDirection = dir;
                    }
                }
                
                if (_debugMode) Log.Info(LogModules.AI, $"{ComponentName}: 角落场景中只能选择风险方向: {GetDirectionName(bestDirection)}", this);
                return bestDirection;
            }
            
            // 如果没有找到任何可通行方向，返回零向量
            if (_debugMode) Log.Warning(LogModules.AI, $"{ComponentName}: 角落场景中没有找到可通行方向", this);
            return Vector2.zero;
        }
        /// <summary>
        /// 更新组件状态
        /// 确保在感知范围内持续感知并根据威胁情况动态调整行动
        /// 配合BehaviorComponentManager的串行执行流程
        /// </summary>
        /// <summary>
        /// 协程方法：控制日志输出冷却
        /// </summary>
        private IEnumerator LogCooldownCoroutine()
        {
            _canLog = false;
            yield return new WaitForSeconds(LogCooldownDuration);
            _canLog = true;
        }
        
        /// <summary>
        /// 协程方法：控制决策冷却
        /// </summary>
        private IEnumerator DecisionCooldownCoroutine()
        {
            if (_decisionCooldownRunning)
                yield break; // 避免重复启动协程
                
            _decisionCooldownRunning = true;
            _canMakeDecision = false;
            yield return new WaitForSeconds(_decisionInterval);
            _canMakeDecision = true;
            _decisionCooldownRunning = false;
        }
        
        /// <summary>
        /// 输出日志并启动冷却协程
        /// </summary>
        private void LogWithCooldown(string method, string message, Object context = null)
        {
            if (_canLog)
            {
                switch (method)
                {
                    case "DebugLog":
                        Log.DebugLog(LogModules.AI, message, context);
                        break;
                    case "Info":
                        Log.Info(LogModules.AI, message, context);
                        break;
                }
                StartCoroutine(LogCooldownCoroutine());
            }
        }
        
        public void Update()
        {
            // 主动更新威胁源信息，确保在决策前有最新的威胁数据
            UpdateThreatSource();
            
            // 检查是否已经到达目标位置 - 这部分可以在任何时候检查
            if (_isMovingToTarget && _movementComponent != null && !_movementComponent.HasTarget)
            {
                LogWithCooldown("DebugLog", $"{ComponentName}: 已到达目标位置，重置移动标志", this);
                _isMovingToTarget = false; // 重置移动中标志
            }
            
            // 决策逻辑 - 基于已更新的感知信息进行决策
            if (_threatSource != null && _perceptionComponent != null)
            {
                float distanceToThreat = Vector2.Distance(transform.position, _threatSource.position);
                float perceptionRadius = _perceptionComponent.GetPerceptionRadius();
                
                // 计算威胁方向
                Vector2 threatDirection = (_threatSource.position - transform.position).normalized;
                               
                // 只要在感知范围内，就考虑是否需要逃离或更新逃离方向
                if (distanceToThreat <= perceptionRadius)
                {
                    // 如果已经在逃离状态
                    if (_isEscaping)
                    {
                        // 检查是否达到安全距离
                        if (distanceToThreat >= _safetyDistance)
                        {
                            // 仍在感知范围内但已达到安全距离，继续观察但不调整位置
                            LogWithCooldown("DebugLog", $"{ComponentName}: 已达到安全距离，继续监控威胁", this);
                            // 不立即停止逃离状态，而是继续监控威胁
                        }
                        else
                        {
                            // 威胁仍然太近，但只有在不在移动中且可以进行决策时才更新逃离方向和目标
                            if (!_isMovingToTarget && _canMakeDecision)
                            {
                                LogWithCooldown("DebugLog", $"{ComponentName}: 威胁仍在安全距离内且不在移动中，执行逃离", this);
                                // 添加更多的决策上下文信息
                                LogWithCooldown("Info", $"{ComponentName}: 准备执行逃离 - 威胁方向: {GetDirectionName(threatDirection)}", this);
                                Execute();
                                StartCoroutine(DecisionCooldownCoroutine()); // 启动决策冷却协程
                            }
                        }
                    }
                    else
                    {
                        // 不在逃离状态但威胁在触发距离内且可以进行决策时，开始逃离
                        if (distanceToThreat <= _escapeTriggerDistance && _canMakeDecision)
                        {
                            LogWithCooldown("DebugLog", $"{ComponentName}: 威胁在触发距离内，开始逃离", this);
                            // 添加更多的决策上下文信息
                            LogWithCooldown("Info", $"{ComponentName}: 开始逃离 - 威胁方向: {GetDirectionName(threatDirection)}", this);
                            Execute();
                            StartCoroutine(DecisionCooldownCoroutine()); // 启动决策冷却协程
                        }
                        else
                        {
                            LogWithCooldown("DebugLog", $"{ComponentName}: 威胁在感知范围内但未达到触发距离", this);
                        }
                    }
                }
                else
                {
                    // 威胁超出感知范围，停止逃离
                    if (_isEscaping)
                    {
                        LogWithCooldown("DebugLog", $"{ComponentName}: 威胁超出感知范围，停止逃离", this);
                        StopEscaping();
                    }
                }
            }
            else if (_isEscaping)
            {
                // 没有威胁源但处于逃离状态，停止逃离
                LogWithCooldown("DebugLog", $"{ComponentName}: 无威胁源但处于逃离状态，停止逃离", this);
                StopEscaping();
            }
            else if (_threatSource == null)
            {
                // 只有在没有威胁源时才输出，避免频繁输出
                if (_canLog)
                {
                    Log.DebugLog(LogModules.AI, $"{ComponentName}: 无威胁源", this);
                    StartCoroutine(LogCooldownCoroutine());
                }
            }
            else if (_perceptionComponent == null)
            {
                // 缺少组件是重要错误，不受冷却限制
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 缺少感知组件", this);
            }
        }
        
        /// <summary>
        /// 获取方向的可读名称
        /// 用于日志记录，方便调试
        /// </summary>
        /// <param name="direction">方向向量</param>
        /// <returns>方向的可读名称</returns>        
        private string GetDirectionName(Vector2 direction)
        {
            if (direction == Vector2.up)
                return "上";
            if (direction == Vector2.down)
                return "下";
            if (direction == Vector2.left)
                return "左";
            if (direction == Vector2.right)
                return "右";
            
            // 对于其他方向，返回大致方向
            float angle = Vector2.SignedAngle(Vector2.right, direction);
            if (angle < -135 || angle >= 135)
                return "左";
            if (angle < -45)
                return "上";
            if (angle < 45)
                return "右";
            return "下";
        }
        
        /// <summary>
        /// 更新威胁源
        /// 优先从感知组件获取最近的威胁，确保感知组件持续更新
        /// </summary>
        private void UpdateThreatSource()
        {
            if (_perceptionComponent != null)
            {
                // 获取最新的威胁信息（不强制执行感知，让感知系统自己管理感知频率）
                GameObject nearestThreat = _perceptionComponent.GetNearestThreat();
                if (nearestThreat != null)
                {
                    _threatSource = nearestThreat.transform;
                }
                else
                {
                    // 没有检测到威胁，清空威胁源
                    _threatSource = null;
                }
            }
        }

        /// <summary>
        /// 重置组件状态
        /// </summary>
        public void Reset()
        {
            StopEscaping();
            _threatSource = null;
        }

        /// <summary>
        /// 设置威胁源
        /// </summary>
        /// <param name="threat">威胁源的Transform组件</param>
        public void SetThreatSource(Transform threat)
        {
            _threatSource = threat;
        }

        /// <summary>
        /// 计算逃离方向
        /// 智能计算逃离方向，在死胡同情况下会尝试朝向威胁源寻找出口
        /// 同时考虑障碍物和ground层的限制
        /// 增强版算法特别优化了斜向接近和拐角情况的处理
        /// </summary>
        /// <returns>逃离方向向量</returns>
        
        /// <summary>
        /// 将当前位置对齐到网格中心
        /// 解决位置未对齐网格中心的问题
        /// </summary>
        private void AlignToGridCenter()
        {
            if (_groundTilemap == null)
            {
                Log.Warning(LogModules.AI, $"{ComponentName}: 地面Tilemap未初始化，无法对齐网格中心", this);
                return;
            }
            
            // 获取当前世界位置
            Vector3 currentPosition = transform.position;
            
            // 将世界位置转换为网格坐标
            Vector3Int gridPosition = WorldToGridPosition(currentPosition);
            
            // 获取网格中心的世界坐标
            Vector3 gridCenterPosition = GridToWorldPosition(gridPosition);
            
            // 如果当前位置与网格中心位置差距较大，则移动到网格中心
            if (Vector3.Distance(currentPosition, gridCenterPosition) > 0.1f)
            {
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 对齐到网格中心，从 {currentPosition} 到 {gridCenterPosition}", this);
                transform.position = gridCenterPosition;
            }
        }
        
        private Vector2 CalculateEscapeDirection()
        {
            if (_threatSource == null || _movementComponent == null)
            {
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 无法计算逃离方向，缺少威胁源或移动组件", this);
                return Vector2.zero;
            }

            // 计算从威胁源指向自身的向量（即远离威胁的方向）
            Vector2 awayFromThreat = ((Vector2)transform.position - (Vector2)_threatSource.position).normalized;
            
            // 获取威胁源到角色的精确向量（未归一化）
            Vector2 rawThreatVector = (Vector2)(_threatSource.position - transform.position);
            
            // 检测是否是斜向接近或拐角情况
            bool isDiagonalApproach = Mathf.Abs(rawThreatVector.x) > 0.5f && Mathf.Abs(rawThreatVector.y) > 0.5f;
            
            Log.DebugLog(LogModules.AI, $"{ComponentName}: 计算出远离威胁的方向: {awayFromThreat}, 斜向接近: {isDiagonalApproach}", this);
            
            // 对于斜向接近情况，优化逃离方向选择
            if (isDiagonalApproach)
            {
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 检测到斜向接近情况，使用优化算法", this);
                
                // 计算两个最有效的逃离方向：垂直于威胁方向的两个方向
                Vector2 perpendicularDirection1 = new Vector2(-rawThreatVector.y, rawThreatVector.x).normalized;
                Vector2 perpendicularDirection2 = new Vector2(rawThreatVector.y, -rawThreatVector.x).normalized;
                
                // 检查哪个垂直方向更适合逃离（不被阻挡且在地面上）
                Vector2 testPosition1 = (Vector2)transform.position + perpendicularDirection1 * _safetyDistance;
                Vector2 testPosition2 = (Vector2)transform.position + perpendicularDirection2 * _safetyDistance;
                
                bool dir1Valid = !IsDirectionBlocked(perpendicularDirection1) && IsPositionOnGround(testPosition1);
                bool dir2Valid = !IsDirectionBlocked(perpendicularDirection2) && IsPositionOnGround(testPosition2);
                
                // 计算哪个方向能让角色更远离威胁
                float distance1 = CalculateDistanceAfterMove(perpendicularDirection1);
                float distance2 = CalculateDistanceAfterMove(perpendicularDirection2);
                
                // 优先选择未被阻挡且能增加与威胁距离的方向
                if (dir1Valid && dir2Valid)
                {
                    // 如果两个方向都可用，选择能让角色更远离威胁的方向
                    Vector2 bestDirection = distance1 > distance2 ? perpendicularDirection1 : perpendicularDirection2;
                    Log.DebugLog(LogModules.AI, $"{ComponentName}: 斜向情况 - 两个方向都可用，选择最佳方向:{GetDirectionName(bestDirection)}", this);
                    return bestDirection;
                }
                else if (dir1Valid)
                {
                    Log.DebugLog(LogModules.AI, $"{ComponentName}: 斜向情况 - 选择方向1:{GetDirectionName(perpendicularDirection1)}", this);
                    return perpendicularDirection1;
                }
                else if (dir2Valid)
                {
                    Log.DebugLog(LogModules.AI, $"{ComponentName}: 斜向情况 - 选择方向2:{GetDirectionName(perpendicularDirection2)}", this);
                    return perpendicularDirection2;
                }
                // 如果两个垂直方向都不可用，继续使用常规算法
            }
            
            // 计算目标位置
            Vector2 targetPosition = (Vector2)transform.position + awayFromThreat * _safetyDistance;
            
            // 检查远离威胁的方向是否可行（无障碍物阻挡且在ground层上）
            bool isAwayDirectionBlocked = IsDirectionBlocked(awayFromThreat);
            bool isAwayPositionOnGround = IsPositionOnGround(targetPosition);
            
            Log.DebugLog(LogModules.AI, $"{ComponentName}: 远离威胁方向可行检查 - 障碍物: {isAwayDirectionBlocked}, 地面: {isAwayPositionOnGround}", this);
            
            if (!isAwayDirectionBlocked && isAwayPositionOnGround)
            {
                // 如果远离威胁的方向可行，直接返回该方向
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 选择远离威胁方向作为逃离方向", this);
                return awayFromThreat;
            }
            
            // 远离威胁的方向被阻挡，尝试搜索其他可行方向
            // 使用智能排序，优先考虑那些能让角色更远离威胁的方向
            Vector2[] testDirections = new Vector2[]
            {
                Vector2.up,
                Vector2.down,
                Vector2.left,
                Vector2.right
            };
            
            // 计算每个方向的评分和可行性
            List<DirectionScore> directionScores = new ();
            
            foreach (Vector2 direction in testDirections)
            {
                Vector2 testPos = (Vector2)transform.position + direction * _safetyDistance;
                bool isBlocked = IsDirectionBlocked(direction);
                bool isOnGround = IsPositionOnGround(testPos);
                
                // 计算移动后的预期距离
                float distanceAfterMove = CalculateDistanceAfterMove(direction);
                
                // 计算基础评分
                float score = distanceAfterMove;
                
                // 根据方向与远离威胁方向的相似度增加额外评分
                float similarityBonus = Vector2.Dot(direction.normalized, awayFromThreat) * 0.5f;
                score += similarityBonus;
                
                // 添加到列表
                directionScores.Add(new DirectionScore(direction, score, isBlocked, isOnGround));
            }
            
            // 寻找可行的逃离方向（无障碍物阻挡且在ground层上）
            Log.DebugLog(LogModules.AI, $"{ComponentName}: 使用评分系统搜索最佳可行逃离方向", this);
            
            // 首先尝试找到既不在障碍物中又在地面上的方向，并按评分排序
            var validDirections = directionScores.Where(d => !d.IsBlocked && d.IsOnGround)
                                               .OrderByDescending(d => d.Score)
                                               .ToList();
            
            if (validDirections.Count > 0)
            {
                Vector2 bestDirection = validDirections[0].Direction;
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 选择最佳有效方向: {GetDirectionName(bestDirection)} (评分: {validDirections[0].Score})", this);
                return bestDirection;
            }
            
            // 如果找不到完全符合条件的方向，尝试至少在ground层上的方向（即使有障碍物）
            // 这可以防止物体跑到没有ground的区域
            var groundOnlyDirections = directionScores.Where(d => d.IsOnGround)
                                                    .OrderByDescending(d => d.Score)
                                                    .ToList();
            
            if (groundOnlyDirections.Count > 0)
            {
                Vector2 bestDirection = groundOnlyDirections[0].Direction;
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 选择仅在地面上的方向: {GetDirectionName(bestDirection)} (评分: {groundOnlyDirections[0].Score})", this);
                return bestDirection;
            }
            
            // 根据配置的响应策略处理死胡同情况
            Log.DebugLog(LogModules.AI, $"{ComponentName}: 进入死胡同处理逻辑", this);
            switch (_deadEndResponseType)
            {
                case DeadEndResponseType.GiveUpEscape:
                    Log.Info(LogModules.AI, $"{ComponentName}: 检测到死胡同，放弃逃离", this);
                    return Vector2.zero; // 返回零向量表示放弃逃离
                    
                case DeadEndResponseType.ReverseBreakthrough:
                    Log.DebugLog(LogModules.AI, $"{ComponentName}: 检测到死胡同，尝试朝向威胁源方向寻找出口", this);
                    Vector2 towardsThreat = -awayFromThreat;
                    
                    // 容错模式：即使不在地面上，也尝试反向突围
                    if (!IsDirectionBlocked(towardsThreat))
                    {
                        Log.DebugLog(LogModules.AI, $"{ComponentName}: 容错模式下选择反向突围方向作为逃离方向", this);
                        return towardsThreat.normalized; // 朝向威胁源的方向
                    }
                    else
                    {
                        Log.DebugLog(LogModules.AI, $"{ComponentName}: 反向突围方向被阻挡，放弃逃离", this);
                        return Vector2.zero; // 否则放弃逃离
                    }
                    
                default:
                    Log.Warning(LogModules.AI, $"{ComponentName}: 未知的死胡同响应类型: {_deadEndResponseType}", this);
                    return Vector2.zero; // 默认放弃逃离
            }
        }
        
        /// <summary>
        /// 方向评分辅助类
        /// </summary>
        private class DirectionScore
        {
            public Vector2 Direction { get; private set; }
            public float Score { get; private set; }
            public bool IsBlocked { get; private set; }
            public bool IsOnGround { get; private set; }
            
            public DirectionScore(Vector2 direction, float score, bool isBlocked, bool isOnGround)
            {
                Direction = direction;
                Score = score;
                IsBlocked = isBlocked;
                IsOnGround = isOnGround;
            }
        }
        
        /// <summary>
        /// 使用点乘法计算逃离方向的权重
        /// 基于方向与远离威胁方向的点积来确定权重，点积越大（方向越一致）权重越高
        /// </summary>
        /// <param name="direction">待评估的移动方向</param>
        /// <param name="awayFromThreat">远离威胁源的方向</param>
        /// <returns>基于点积的方向权重（范围：-1到1，值越大表示方向越适合逃离）</returns>
        private float CalculateDirectionWeightUsingDotProduct(Vector2 direction, Vector2 awayFromThreat)
        {
            // 标准化方向向量以确保点积计算的一致性
            Vector2 normalizedDirection = direction.normalized;
            Vector2 normalizedAwayDirection = awayFromThreat.normalized;
            
            // 计算点积：点积值范围为-1到1
            // 值为1表示方向完全一致（最理想的逃离方向）
            // 值为0表示方向垂直
            // 值为-1表示方向完全相反（朝向威胁源，最差的逃离方向）
            float dotProduct = Vector2.Dot(normalizedDirection, normalizedAwayDirection);
            
            if (_debugMode) Log.DebugLog(LogModules.AI, $"{ComponentName}: 方向{GetDirectionName(direction)}与远离威胁方向的点积: {dotProduct}", this);
            
            return dotProduct;
        }
        
        /// <summary>
        /// 计算移动后与威胁源的预期距离
        /// </summary>
        /// <param name="direction">移动方向</param>
        /// <returns>移动后与威胁源的预期距离</returns>
        private float CalculateDistanceAfterMove(Vector2 direction)
        {
            if (_threatSource == null)
                return 0;
                
            // 计算移动后的预期位置
            Vector2 expectedPosition = (Vector2)transform.position + (direction.normalized * _cellSize.x);
            
            // 计算与威胁源的距离
            float distance = Vector2.Distance(expectedPosition, (Vector2)_threatSource.position);
            
            return distance;
        }
        
        /// <summary>
        /// 公共方法：获取计算的逃离方向
        /// 用于测试和调试目的
        /// </summary>
        /// <returns>计算出的逃离方向向量</returns>
        public Vector2 GetCalculatedEscapeDirection()
        {
            return CalculateEscapeDirection();
        }
        
        
        
    [Header("Tilemap设置")]
    [Tooltip("地板瓦片地图")]
    [SerializeField] private Tilemap _groundTilemap; // 地板瓦片地图
    [SerializeField] private Tilemap _wallTilemap; // 墙体瓦片地图
    
    [Tooltip("网格单元格大小")]
    private Vector2 _cellSize = new(1f, 1f); // 默认1x1单位
    
    private void Start()
    {
        // 设置Tilemap引用
        SetupTilemapReference();
    }
    
    /// <summary>
    /// 设置Tilemap引用
    /// 采用与PushableBox相同的名称匹配策略
    /// </summary>
    private void SetupTilemapReference()
    {
        // 获取所有Tilemap
        Tilemap[] allTilemaps = FindObjectsOfType<Tilemap>();
        
        // 如果未指定地面Tilemap，尝试查找
        if (_groundTilemap == null)
        {
            // 1. 精确匹配：查找名称为"ground"的Tilemap（不区分大小写）
            foreach (var tilemap in allTilemaps)
            {
                if (tilemap.name.ToLower() == "ground")
                {
                    _groundTilemap = tilemap;
                    break;
                }
            }
            
            // 2. 如果精确匹配失败，使用智能判断
            if (_groundTilemap == null)
            {
                foreach (var tilemap in allTilemaps)
                {
                    if (IsGroundTilemap(tilemap))
                    {
                        _groundTilemap = tilemap;
                        break;
                    }
                }
            }
            
            // 3. 回退：查找名称包含Ground或地面的Tilemap
            if (_groundTilemap == null)
            {
                foreach (var tm in allTilemaps)
                {
                    if (tm.name.Contains("Ground") || tm.name.Contains("地面"))
                    {
                        _groundTilemap = tm;
                        break;
                    }
                }
            }
            
            // 4. 最后回退到查找场景中的任意Tilemap
            if (_groundTilemap == null && allTilemaps.Length > 0)
            {
                _groundTilemap = allTilemaps[0];
                Log.Warning(LogModules.AI, $"EscapeComponent: 未找到标准地面Tilemap，使用了第一个找到的Tilemap: {_groundTilemap.name}");
            }
        }

        // 如果未指定墙体Tilemap，尝试查找
        if (_wallTilemap == null)
        {
            // 1. 精确匹配：查找名称为"walls"的Tilemap（不区分大小写）
            foreach (var tilemap in allTilemaps)
            {
                if (tilemap.name.ToLower() == "walls")
                {
                    _wallTilemap = tilemap;
                    break;
                }
            }
            
            // 2. 如果精确匹配失败，使用智能判断
            if (_wallTilemap == null)
            {
                foreach (var tilemap in allTilemaps)
                {
                    // 避免使用与地面相同的Tilemap
                    if (tilemap != _groundTilemap && IsWallTilemap(tilemap))
                    {
                        _wallTilemap = tilemap;
                        break;
                    }
                }
            }
            
            // 3. 回退：查找名称包含Wall或墙的Tilemap
            if (_wallTilemap == null)
            {
                foreach (var tm in allTilemaps)
                {
                    // 避免使用与地面相同的Tilemap
                    if (tm != _groundTilemap && (tm.name.Contains("Wall") || tm.name.Contains("墙")))
                    {
                        _wallTilemap = tm;
                        break;
                    }
                }
            }
        }

        // 如果找到了Tilemap，获取其单元格大小
        if (_groundTilemap != null)
        {
            _cellSize = _groundTilemap.cellSize;
        }
        else
        {
            Log.Warning(LogModules.AI, "EscapeComponent: 无法找到地面Tilemap，地面检测功能将无法正常工作");
        }
    }
    
    /// <summary>
    /// 检查是否为地板Tilemap
    /// </summary>
    private bool IsGroundTilemap(Tilemap tilemap)
    {
        // 检查Tilemap的碰撞设置
        if (tilemap.TryGetComponent<TilemapCollider2D>(out var collider))
        {
            // 地板通常设置为Trigger或无碰撞
            return collider.isTrigger || !tilemap.gameObject.layer.Equals(7); // 7是Ground层
        }
        
        // 检查Tilemap下的Tile
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new(x, y, 0);
                TileBase tile = tilemap.GetTile(cell);
                if (tile != null)
                {
                    // 如果有瓦片，根据瓦片名称或类型判断
                    string tileName = tile.name.ToLower();
                    if (tileName.Contains("floor") || tileName.Contains("ground") || 
                        tileName.Contains("grass") || tileName.Contains("dirt"))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 检查是否为墙体Tilemap
    /// </summary>
    private bool IsWallTilemap(Tilemap tilemap)
    {
        // 检查Tilemap的碰撞设置
        TilemapCollider2D collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider != null && !collider.isTrigger)
        {
            // 墙体通常不是Trigger
            return true;
        }
        
        // 检查Tilemap下的Tile
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new(x, y, 0);
                TileBase tile = tilemap.GetTile(cell);
                if (tile != null)
                {
                    // 如果有瓦片，根据瓦片名称或类型判断
                    string tileName = tile.name.ToLower();
                    if (tileName.Contains("wall") || tileName.Contains("rock") || 
                        tileName.Contains("stone") || tileName.Contains("brick"))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 将世界坐标转换为网格坐标
    /// </summary>
    /// <param name="worldPosition">世界坐标</param>
    /// <returns>网格坐标</returns>
    private Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        if (_groundTilemap == null)
            return Vector3Int.zero;
        
        Vector3Int gridPosition = _groundTilemap.WorldToCell(worldPosition);
        return gridPosition;
    }
    
    /// <summary>
    /// 将网格坐标转换为世界坐标并居中
    /// </summary>
    /// <param name="gridPosition">网格坐标</param>
    /// <returns>居中的世界坐标</returns>
    private Vector3 GridToWorldPosition(Vector3Int gridPosition)
    {
        if (_groundTilemap == null)
            return Vector3.zero;
        
        // 获取网格单元格的中心世界坐标
        Vector3 worldPosition = _groundTilemap.GetCellCenterWorld(gridPosition);
        return worldPosition;
    }

    /// <summary>
    /// 检查指定位置是否在地面上
    /// 使用Tilemap方法作为主要检测方式，射线检测作为备用
    /// </summary>
    /// <param name="position">要检查的位置</param>
    /// <returns>如果位置在地面上则返回true，否则返回false</returns>
    private bool IsPositionOnGround(Vector2 position)
    {
        // 只使用Tilemap方法检测地面
        if (_groundTilemap != null)
        {
            Vector3Int gridPosition = WorldToGridPosition(position);
            bool hasGroundTile = _groundTilemap.HasTile(gridPosition);
            Log.DebugLog(LogModules.AI, $"{ComponentName}: 位置({position})的网格坐标{gridPosition}{(hasGroundTile ? "有" : "没有")}地面瓦片", this);
            
            return hasGroundTile;
        }
        
        // 如果没有Tilemap引用，无法进行地面检测
        Log.DebugLog(LogModules.AI, $"{ComponentName}: 位置({position})地面检测 - 没有Tilemap引用，无法检测地面", this);
        return false;
    }
    
    /// <summary>
    /// 检查网格位置是否可通行
    /// 参考玩家控制器的实现
    /// </summary>
    /// <param name="gridPosition">要检查的网格位置</param>
    /// <returns>如果可通行则返回true，否则返回false</returns>
    private bool IsCellWalkable(Vector3Int gridPosition)
    {
        // 1. 检查地板是否存在（必须有地板才能通行）
        if (_groundTilemap == null || !_groundTilemap.HasTile(gridPosition))
        {
            return false;
        }
        
        // 2. 检查是否有墙体（如果有墙体则不可通行）
        if (_wallTilemap != null && _wallTilemap.HasTile(gridPosition))
        {
            return false;
        }
        
        // 3. 使用Physics2D进行精确碰撞检测
        Vector3 worldPosition = GridToWorldPosition(gridPosition);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(worldPosition, 
                                                        new Vector2(_cellSize.x - 0.1f, _cellSize.y - 0.1f), 
                                                        0f);
        
        foreach (Collider2D collider in colliders)
        {
            // 检查是否有不可通行的碰撞体（Obstacle标签）
            if (collider.CompareTag("Obstacle") && collider.gameObject != gameObject)
            {
                return false;
            }
        }
        
        // 位置可通行
        return true;
    }
        
        /// <summary>
        /// 检查特定方向是否被障碍物阻挡
        /// </summary>
        /// <param name="direction">要检查的方向</param>
        /// <returns>如果方向被阻挡则返回true，否则返回false</returns>
        private bool IsDirectionBlocked(Vector2 direction)
        {
            // 创建一个LayerMask，包含Wall层，因为Wall仍然是layer
            LayerMask wallLayer = LayerMask.GetMask("Wall");
            
            // 使用射线检测检查方向是否有碰撞体
            // 使用~0作为layer mask以检测所有层，然后在后续检查tag
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                transform.position,
                direction,
                _safetyDistance // 使用安全距离作为检测长度
            );
            
            // 检查是否击中了带有Obstacle标签的对象或者Wall层的对象
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    // 跳过自身的碰撞体
                    if (hit.collider.gameObject == gameObject)
                    {
                        continue;
                    }
                    
                    // 检查是否是Obstacle标签或Wall层
                    bool isObstacleTag = hit.collider.CompareTag("Obstacle");
                    bool isWallLayer = (wallLayer.value & (1 << hit.collider.gameObject.layer)) > 0;
                    
                    if (isObstacleTag || isWallLayer)
                    {
                        // 输出调试日志
                        string directionName = GetDirectionName(direction);
                        string obstacleType = isObstacleTag ? "Obstacle标签" : "Wall层";
                        Log.DebugLog(LogModules.AI, $"{ComponentName}: 方向 {directionName} ({direction}) 检测到障碍物 ({obstacleType}): {hit.collider.gameObject.name}", this);
                        return true;
                    }
                }
            }
            
            // 未检测到障碍物
            string dirName = GetDirectionName(direction);
            Log.DebugLog(LogModules.AI, $"{ComponentName}: 方向 {dirName} ({direction}) 未检测到障碍物", this);
            return false;
        }
        

        /// <summary>
        /// 停止逃离行为
        /// </summary>
        private void StopEscaping()
        {
            _isEscaping = false;
            _isMovingToTarget = false; // 重置移动中标志
            
            // 恢复原始移动速度
            if (_movementComponent != null)
            {
                _movementComponent.SetSpeed(_originalSpeed);
                _movementComponent.StopMovement();
            }
            
            Log.Info(LogModules.AI, $"{ComponentName}: 停止逃离行为", this);
        }
    }
}