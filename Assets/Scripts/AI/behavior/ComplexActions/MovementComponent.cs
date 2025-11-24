using UnityEngine;
using Logger;
using AI.Behavior.Movement;

namespace AI.Behavior
{
    /// <summary>
    /// 复合移动组件 - 组合基础动作组件实现移动功能
    /// 将速度控制、位置控制和障碍物检测组合成完整的移动行为
    /// </summary>
    public class MovementComponent : MonoBehaviour, IActionComponent
    {
        private GameObject _owner;
        private Rigidbody2D _rigidbody;
        private bool _isInitialized = false;
        
        // 基础动作组件引用
        private SpeedControlComponent _speedControl;
        private PositionControlComponent _positionControl;
        private ObstacleDetectionComponent _obstacleDetection;
        
        private float _defaultSpeed = 4f;
        // 降低到达阈值以确保能精确到达网格位置
        private float _arrivalThreshold = 0.05f;
        
        // 决策标志 - 用于串行执行流程，确保只在收到决策组件命令后执行移动
        private bool _hasDecisionCommand = false;

        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName => "MovementComponent";

        /// <summary>
        /// 默认移动速度
        /// </summary>
        public float DefaultSpeed
        {
            get => _defaultSpeed;
            set => _defaultSpeed = Mathf.Max(0, value);
        }

        /// <summary>
        /// 当前移动速度（通过SpeedControlComponent）
        /// </summary>
        public float CurrentSpeed
        {
            get
            {
                if (_speedControl != null)
                {
                    return _speedControl.CurrentSpeed;
                }
                return 0f;
            }
        }
        
        /// <summary>
        /// 设置当前移动速度
        /// </summary>
        /// <param name="speed">目标速度值</param>
        public void SetSpeed(float speed)
        {
            if (_speedControl != null)
            {
                _speedControl.SetSpeedInstantly(Mathf.Max(0, speed));
                Log.DebugLog(LogModules.AI, $"速度已设置为: {speed}", this);
            }
        }
        
        /// <summary>
        /// 通过倍数调整当前速度
        /// </summary>
        /// <param name="multiplier">速度倍数</param>
        public void ModifySpeedByMultiplier(float multiplier)
        {
            if (_speedControl != null)
            {
                float newSpeed = _speedControl.CurrentSpeed * multiplier;
                _speedControl.SetSpeedInstantly(Mathf.Max(0, newSpeed));
                Log.DebugLog(LogModules.AI, $"速度已调整，倍数: {multiplier}, 新速度: {newSpeed}", this);
            }
        }

        /// <summary>
        /// 是否有目标位置（通过PositionControlComponent）
        /// </summary>
        public bool HasTarget
        {
            get
            {
                if (_positionControl != null)
                {
                    return _positionControl.HasTarget;
                }
                return false;
            }
        }

        /// <summary>
        /// 目标位置（通过PositionControlComponent）
        /// </summary>
        public Vector2 TargetPosition
        {
            get
            {
                if (_positionControl != null)
                {
                    return _positionControl.TargetPosition;
                }
                return Vector2.zero;
            }
            set
            {
                if (_positionControl != null)
                {
                    _positionControl.SetTargetPosition(value);
                }
            }
        }

        /// <summary>
        /// 到达目标的最小距离阈值
        /// </summary>
        public float ArrivalThreshold
        {
            get => _arrivalThreshold;
            set => _arrivalThreshold = Mathf.Max(0, value);
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        /// <param name="owner">拥有该组件的GameObject</param>
        public void Initialize(GameObject owner)
        {
            if (_isInitialized) return;

            _owner = owner;
            _rigidbody = _owner.GetComponent<Rigidbody2D>();
            if (_rigidbody == null)
            {
                Log.Error(LogModules.AI, "MovementComponent需要Rigidbody2D组件", this);
                _rigidbody = _owner.AddComponent<Rigidbody2D>();
                _rigidbody.gravityScale = 0;
            }
            
            // 获取或添加基础动作组件
            _speedControl = _owner.GetComponent<SpeedControlComponent>();
            if (_speedControl == null)
            {
                _speedControl = _owner.AddComponent<SpeedControlComponent>();
            }
            _speedControl.Initialize(_owner);
            _speedControl.MaxSpeed = _defaultSpeed;
            
            _positionControl = _owner.GetComponent<PositionControlComponent>();
            if (_positionControl == null)
            {
                _positionControl = _owner.AddComponent<PositionControlComponent>();
            }
            _positionControl.Initialize(_owner);
            
            _obstacleDetection = _owner.GetComponent<ObstacleDetectionComponent>();
            if (_obstacleDetection == null)
            {
                _obstacleDetection = _owner.AddComponent<ObstacleDetectionComponent>();
            }
            _obstacleDetection.Initialize(_owner);
            _obstacleDetection.DetectionDistance = 1f; // 设置与原实现相同的检测距离
            
            _isInitialized = true;
            Log.Info(LogModules.AI, "MovementComponent初始化成功（使用基础动作组合）", this);
        }

        /// <summary>
        /// 检查是否可以执行移动
        /// </summary>
        /// <returns>是否可以执行移动</returns>
        public bool CanExecute()
        {
            return _isInitialized && HasTarget && _rigidbody != null &&
                   _speedControl != null && _positionControl != null && _obstacleDetection != null;
        }

        /// <summary>
        /// 执行移动
        /// 使用三个基础动作组件的组合实现移动功能
        /// </summary>
        /// <returns>移动是否成功执行</returns>
        public bool Execute()
        {
            if (!CanExecute())
                return false;

            // 使用PositionControlComponent计算方向和距离
            Vector2 direction = _positionControl.CalculateDirectionToTarget();
            float distance = _positionControl.CalculateDistanceToTarget();

            // 检查是否到达目标
            if (distance <= ArrivalThreshold || _positionControl.IsAtTarget())
            {
                _rigidbody.velocity = Vector2.zero;
                _positionControl.ClearTarget();
                _speedControl.StopImmediately();
                // 到达目标后清除决策命令标志
                _hasDecisionCommand = false;
                Log.DebugLog(LogModules.AI, $"到达目标位置: {TargetPosition}，已清除决策命令标志", this);
                return true;
            }

            // 对于单步移动，不根据距离调整速度，确保能稳定走到终点
            // 保持恒定速度以确保完整走完一格
            // 注释掉距离调整，使用固定速度
            
            // 使用ObstacleDetectionComponent检测障碍物并找到可行走方向
            Vector2 finalDirection = direction;
            if (_obstacleDetection.HasObstacleInDirection(direction))
            {
                Log.DebugLog(LogModules.AI, "路径上有障碍物，尝试绕行", this);
                // 尝试找到可行走的方向
                Vector2 walkableDirection = _obstacleDetection.FindWalkableDirection(direction);
                if (walkableDirection != Vector2.zero)
                {
                    finalDirection = walkableDirection;
                }
                else
                {
                    // 如果没有找到可行走方向，则尝试向右侧偏移
                    finalDirection = new Vector2(-direction.y, direction.x);
                }
            }
            
            // 执行移动 - 确保使用足够的速度走到终点
            _rigidbody.velocity = finalDirection * _defaultSpeed; // 使用默认速度确保稳定移动

            return true;
        }

        /// <summary>
        /// 更新组件状态
        /// 作为串行执行流程的最后一步，只在收到决策组件的命令后执行移动
        /// </summary>
        public void Update()
        {
            // 更新内部组件
            if (_speedControl != null)
                _speedControl.Update();
                
            if (_positionControl != null)
                _positionControl.Update();
                
            if (_obstacleDetection != null)
                _obstacleDetection.Update();
                
            // 只有在收到决策组件的命令(_hasDecisionCommand)且有目标且未到达目标时才执行移动
            // 这确保移动操作只在决策之后执行，符合串行执行流程
            if (_hasDecisionCommand && HasTarget && !_positionControl.IsAtTarget())
            {
                Execute();
            }
        }

        /// <summary>
        /// 重置组件
        /// 重置所有基础动作组件
        /// </summary>
        public void Reset()
        {
            // 确保重置时清除所有移动状态
            if (_positionControl != null)
            {
                _positionControl.Reset();
            }
            
            if (_speedControl != null)
            {
                _speedControl.Reset();
                _speedControl.MaxSpeed = _defaultSpeed;
            }
            
            if (_obstacleDetection != null)
            {
                _obstacleDetection.Reset();
            }
            
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector2.zero;
            }
            
            // 重置决策命令标志
            _hasDecisionCommand = false;
            Log.DebugLog(LogModules.AI, "MovementComponent已重置，决策命令标志已清除", this);
        }

        /// <summary>
        /// 立即停止移动
        /// 停止所有移动相关的行为
        /// </summary>
        public void StopMovement()
        {
            if (_positionControl != null)
            {
                _positionControl.ClearTarget();
            }
            
            if (_speedControl != null)
            {
                _speedControl.StopImmediately();
            }
            
            if (_rigidbody != null)
            {
                _rigidbody.velocity = Vector2.zero;
            }
            
            // 停止时清除决策命令标志
            _hasDecisionCommand = false;
            Log.DebugLog(LogModules.AI, "移动已停止，决策命令标志已清除", this);
        }

        /// <summary>
        /// 设置移动目标
        /// </summary>
        /// <param name="position">目标位置</param>
        public void SetTarget(Vector2 position)
        {
            TargetPosition = position;
            // 设置决策命令标志，表示已收到决策组件的命令
            _hasDecisionCommand = true;
            Log.DebugLog(LogModules.AI, $"设置新目标位置: {position}，决策命令标志已设置", this);
        }
        
        /// <summary>
        /// 清除决策命令标志
        /// </summary>
        public void ClearDecisionCommand()
        {
            _hasDecisionCommand = false;
            Log.DebugLog(LogModules.AI, "决策命令标志已清除", this);
        }
        
        /// <summary>
        /// 获取决策命令状态
        /// </summary>
        public bool HasDecisionCommand
        {
            get { return _hasDecisionCommand; }
        }

        /// <summary>
        /// 设置障碍物层
        /// </summary>
        /// <param name="layerMask">障碍物层掩码</param>
        public void SetObstacleLayer(LayerMask layerMask)
        {
            if (_obstacleDetection != null)
            {
                _obstacleDetection.ObstacleLayerMask = layerMask;
                Log.DebugLog(LogModules.AI, $"障碍物层已设置: {layerMask}", this);
            }
        }

        /// <summary>
        /// 获取到目标的剩余距离
        /// </summary>
        /// <returns>到目标的距离，如果没有目标则返回0</returns>
        public float GetDistanceToTarget()
        {
            if (!HasTarget || _positionControl == null)
                return 0f;
            
            return _positionControl.CalculateDistanceToTarget();
        }
    }
}
