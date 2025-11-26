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
        /// 基础组件引用
        /// </summary>
        private ThreatDetectionComponent _threatDetectionComponent;
        private SingleStepMovementComponent _movementComponent;

        /// <summary>
        /// 逃跑触发距离（小于安全距离时触发）
        /// 即AI在威胁源周围的距离，低于此距离时才会触发逃离行为
        /// </summary>

        [SerializeField] private float _decisionInterval = 0.5f; // 移动决策间隔时间（秒）
        private bool _canMakeDecision = true; // 是否可以进行决策
        private bool _decisionCooldownRunning = false; // 决策冷却是否正在运行

        
        /// <summary>
        /// 死胡同响应类型枚举
        /// </summary>
        public enum DeadEndResponseType
        {
            GiveUpEscape,  // 放弃逃离
            ReverseBreakthrough  // 反向突围（朝向威胁源方向移动）
        }
        
        /// <summary>
        /// 移动完成回调方法
        /// 在单步移动完成后被调用，重置移动状态并重新进行感知和判断
        /// </summary>
        private void OnMovementComplete()
        {            
            // 重置移动中标志
            _isMovingToTarget = false;
            
            Log.LogWithCooldown(Log.LogLevel.Debug, LogModules.AI, $"{ComponentName}: 单步移动完成，重置状态并重新感知", this, $"{ComponentName}_movementComplete");
            
            // 移动完成后重新感知威胁
            _threatDetectionComponent.UpdateThreatSource();
            
            // 再次检查是否需要继续逃离
            if (!CanExecute())
            {
                StopEscaping();
            }
            else
            {                
                // 如果仍然需要逃离，准备执行下一步决策
                Log.LogWithCooldown(Log.LogLevel.Debug, LogModules.AI, $"{ComponentName}: 准备执行下一次逃离决策", this, $"{ComponentName}_prepareDecision");
            }
        }
        
        /// <summary>
        /// 死胡同响应策略开关
        /// </summary>
        [SerializeField] private DeadEndResponseType _deadEndResponseType = DeadEndResponseType.GiveUpEscape;

        /// <summary>
        /// 标记是否正在执行单步移动
        /// </summary>
        private bool _isMovingToTarget = false;

        /// <summary>
        /// 初始化组件
        /// </summary>
        public void Initialize(GameObject owner)
        {
            // 初始化基础组件
            _threatDetectionComponent = gameObject.GetComponent<ThreatDetectionComponent>();
            if (_threatDetectionComponent == null)
            {
                _threatDetectionComponent = gameObject.AddComponent<ThreatDetectionComponent>();
                Log.Warning(LogModules.AI, $"{ComponentName}: 威胁检测组件未找到，已自动添加", this);
            }
            _threatDetectionComponent.Initialize(this);
            
            _movementComponent = gameObject.GetComponent<SingleStepMovementComponent>();
            if (_movementComponent == null)
            {
                _movementComponent = gameObject.AddComponent<SingleStepMovementComponent>();
                Log.Warning(LogModules.AI, $"{ComponentName}: 单步移动组件未找到，已自动添加", this);
            }
            _movementComponent.Initialize(gameObject);
            
            // 设置Tilemap引用，使用TilemapHelper
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
            // 检查威胁是否在触发范围内
            return _threatDetectionComponent.IsThreatInRange();
        }

        /// <summary>
        /// 执行逃离行为
        /// 实现单步逃离逻辑：判断后向计算出的方向走一格，然后再感知和判断
        /// </summary>
        /// <returns>执行是否成功</returns>
        public bool Execute()
        {
            // 检查必要组件
            if (_threatDetectionComponent == null || _movementComponent == null)
            {
                Log.Error(LogModules.AI, $"{ComponentName}: 缺少必要的基础组件", this);
                return false;
            }
            
            // 如果正在执行单步移动，则不重复决策
            if (_isMovingToTarget)
            {
                Log.LogWithCooldown(Log.LogLevel.Debug, LogModules.AI, $"{ComponentName}: 正在执行单步移动中，等待移动完成", this, $"{ComponentName}_movingInProgress");
                return false;
            }
            
            // 检查是否可以进行决策
            if (!_canMakeDecision)
            {
                return false;
            }
            
            // 检查是否应该执行逃离行为
            if (!CanExecute())
            {
                // 停止逃离行为
                StopEscaping();
                return false;
            }
            
            // 获取威胁源
            GameObject threatSourceObj = _threatDetectionComponent.GetThreatSource();
            if (threatSourceObj == null)
            {
                Log.Error(LogModules.AI, $"{ComponentName}: 没有威胁源", this);
                return false;
            }
            
            // 对齐到网格中心
            transform.position = TilemapHelper.AlignToGridCenter(transform.position, _groundTilemap);
            
            // 调用最佳逃离方向选择方法
            Vector2 escapeDirection = SelectBestEscapeDirection();
            
            // 如果没有有效逃离方向，停止移动并记录
            if (escapeDirection == Vector2.zero)
            {
                Log.LogWithCooldown(Log.LogLevel.Debug, LogModules.AI, $"{ComponentName}: 无法找到有效逃离方向，停止移动", this, $"{ComponentName}_noValidDirection");
                _movementComponent.Stop();
                return false;
            }
            
            // 开始执行单步移动
            _isMovingToTarget = true;
            
            // 计算目标位置（当前位置加上一个单元格的方向移动）
            // 确保移动距离正好是一个单元格
            Vector3 targetPosition = transform.position + (Vector3)(escapeDirection * _cellSize.x);
            
            // 网格对齐目标位置，确保精确的单步移动
            targetPosition = TilemapHelper.AlignToGridCenter(targetPosition, _groundTilemap);
            
            // 设置移动目标位置
            _movementComponent.SetTarget(targetPosition);
            
            // 启动决策冷却，避免过于频繁的决策
            StartCoroutine(DecisionCooldownCoroutine());
            
            // 记录日志
            Log.LogWithCooldown(Log.LogLevel.Info, LogModules.AI, $"{ComponentName}: 逃离，方向: {escapeDirection}", this, $"{ComponentName}_escapeDirection");
            
            return true;
        }

        #region 冷却方法
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
    #endregion
        /// <summary>
        /// 更新方法
        /// 持续更新组件状态，确保逃离行为的连贯性
        /// 检测移动完成状态并处理
        /// </summary>
        public void Update()
        {
            // 如果没有在移动且可以做决策，尝试执行逃离行为
            if (!_isMovingToTarget && _canMakeDecision)
            {
                // 在Update中定期检查是否需要执行逃离
                // 实际的Execute调用由行为树管理器控制，但这里可以做一些准备工作
                _threatDetectionComponent.UpdateThreatSource();
            }
            
            // 如果正在移动中，持续更新威胁源信息，以便在下一次决策前获取最新状态
            if (_isMovingToTarget && _movementComponent != null)
            {
                _threatDetectionComponent.UpdateThreatSource();
                
                // 检查是否已经到达目标位置
                // 由于MovementComponent已经处理了到达目标的逻辑，这里只需要检查移动组件是否还有目标
                if (!_movementComponent.HasTarget || !_movementComponent.HasDecisionCommand)
                {
                    // 移动已完成，调用回调方法
                    OnMovementComplete();
                }
            }
        }
        
        /// <summary>
        /// 重置组件状态
        /// </summary>
        public void Reset()
        {
            StopEscaping();
            _threatDetectionComponent.SetThreatSource(null);
        }

        /// <summary>
        /// 设置威胁源
        /// </summary>
        /// <param name="threat">威胁源对象</param>
        public void SetThreatSource(Transform threat)
        {
            if (threat != null)
            {
                _threatDetectionComponent.SetThreatSource(threat.gameObject);
            }
            else
            {
                _threatDetectionComponent.SetThreatSource(null);
            }
        }
        
        /// <summary>
        /// 选择最佳逃离方向
        /// 检查上下左右四个方向的通行性，计算各方向的逃跑权重，选择最佳方向
        /// 使用DirectionSelector工具类实现方向选择逻辑
        /// </summary>
        /// <returns>最佳逃离方向向量，如果没有有效方向则返回Vector2.zero</returns>
        private Vector2 SelectBestEscapeDirection()
        {
            // 获取威胁源位置
            GameObject threatSourceObj = _threatDetectionComponent.GetThreatSource();
            if (threatSourceObj == null)
            {
                Log.Error(LogModules.AI, $"{ComponentName}: 选择逃离方向时威胁源为空", this);
                return Vector2.zero;
            }
            
            // 计算远离威胁源的方向（对于逃离行为）
            Vector2 awayFromThreat = (transform.position - threatSourceObj.transform.position).normalized;
            
            // 使用DirectionSelector选择最佳方向，true表示按最高权重排序（逃离模式）
            Vector2 bestDirection = DirectionSelector.SelectBestDirection(
                transform.position,
                awayFromThreat,
                _groundTilemap,
                IsCellWalkable,
                true // 逃离模式：选择权重最大的方向
            );
            
            // 如果没有可通行方向，根据死胡同响应策略处理
            if (bestDirection == Vector2.zero)
            {
                Log.Warning(LogModules.AI, $"{ComponentName}: 没有可通行的逃离方向", this);
                
                // 根据死胡同响应策略选择行为
                if (_deadEndResponseType == DeadEndResponseType.ReverseBreakthrough)
                {
                    // 反向突围：朝向威胁源方向移动
                    Log.Debug(LogModules.AI, $"{ComponentName}: 执行反向突围策略", this);
                    return (threatSourceObj.transform.position - transform.position).normalized;
                }
            }
            else
            {
                Log.Debug(LogModules.AI, $"{ComponentName}: 选择最佳逃离方向: {bestDirection}", this);
            }
            
            return bestDirection;
        }
        
        
        /// <summary>
        /// 停止逃离行为
        /// 重置相关状态并停止移动
        /// </summary>
        private void StopEscaping()
        {            
            // 重置移动状态
            _isMovingToTarget = false;
            
            // 通过MovementComponent停止移动
            if (_movementComponent != null)
            {
                _movementComponent.Stop();
            }
            
            Log.Debug(LogModules.AI, $"{ComponentName}: 停止逃离行为", this);
        }
        
        #region Tilemap设置
        [Header("Tilemap设置")]
        [Tooltip("地板瓦片地图")]
        [SerializeField] private Tilemap _groundTilemap; // 地板瓦片地图
        [SerializeField] private Tilemap _wallTilemap; // 墙体瓦片地图
        
        [Tooltip("网格单元格大小")]
        private Vector2 _cellSize = new(1f, 1f); // 默认1x1单位
        
        /// <summary>
        /// 设置Tilemap引用
        /// 查找时先将名称转换为了小写，因此查询时填入的参数全为小写
        /// </summary>
        private void SetupTilemapReference()
        {
            // 使用TilemapHelper查找地面Tilemap
            _groundTilemap = TilemapHelper.FindGroundTilemap();
            
            // 使用TilemapHelper查找墙体Tilemap
            _wallTilemap = TilemapHelper.FindWallTilemap();

            // 如果找到了Tilemap，获取其单元格大小
            if (_groundTilemap != null)
            {
                _cellSize = _groundTilemap.cellSize;
            }
            else
            {
                Log.Warning(LogModules.AI, "EscapeComponent: 无法找到地面Tilemap，地面检测功能将无法正常工作", this);
            }
        }
        
        /// <summary>
        /// 检查网格位置是否可通行
        /// 使用TilemapHelper的IsCellWalkable方法
        /// </summary>
        /// <param name="gridPosition">要检查的网格位置</param>
        /// <returns>如果可通行则返回true，否则返回false</returns>
        private bool IsCellWalkable(Vector2Int gridPosition)
        {
            Vector3Int position = new(gridPosition.x, gridPosition.y, 0);
            return TilemapHelper.IsCellWalkable(position, _groundTilemap, _wallTilemap, _cellSize);
        }
        #endregion
    }
}
