using UnityEngine;
using UnityEngine.Tilemaps;
using Logger;
using AI.Behavior.Movement;

namespace AI.Behavior
{
    /// <summary>
    /// 单步移动组件 - 实现每次只移动一个网格单位的移动行为
    /// 参考MovementComponent实现，确保每次只移动一个单元格的距离
    /// </summary>
    public class StepMoveComponent : MonoBehaviour, IActionComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName => "SingleStepMovementComponent";

        /// <summary>
        /// 拥有该组件的GameObject
        /// </summary>
        private GameObject _owner;
        
        /// <summary>
        /// 刚体组件引用
        /// </summary>
        private Rigidbody2D _rigidbody;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool _isInitialized = false;
        
        /// <summary>
        /// 基础组件引用
        /// </summary>
        private PositionControlComponent _positionControl;  // 位置控制组件
        private SpeedControlComponent _speedControl;        // 速度控制组件
        private ObstacleDetectionComponent _obstacleDetection;  // 障碍物检测组件
        
        /// <summary>
        /// 地面瓦片地图引用
        /// </summary>
        private Tilemap _groundTilemap;
        
        /// <summary>
        /// 网格大小，默认为1x1
        /// </summary>
        [SerializeField] private Vector2 _cellSize = new(1f, 1f);
        public Vector2 CellSize
        {
            get => _cellSize;
            set => _cellSize = value;
        }
        
        /// <summary>
        /// 默认移动速度
        /// </summary>
        private float _defaultSpeed = 5f;
        
        /// <summary>
        /// 到达目标的最小距离阈值
        /// </summary>
        private float _arrivalThreshold = 0.05f;
        
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
            private set
            {
                if (_positionControl != null)
                {
                    _positionControl.SetTargetPosition(value);
                }
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
        /// 是否有决策命令
        /// </summary>
        public bool HasDecisionCommand { get; private set; } = false;
        
        /// <summary>
        /// 是否正在执行单步移动
        /// </summary>
        public bool IsMoving { get; private set; }
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        /// <param name="gameObject">游戏对象引用</param>
        public void Initialize(GameObject gameObject)
        {
            if (_isInitialized) return;

            _owner = gameObject;
            _rigidbody = _owner.GetComponent<Rigidbody2D>();
            if (_rigidbody == null)
            {
                Log.Error(LogModules.AI, "SingleStepMovementComponent需要Rigidbody2D组件", this);
                _rigidbody = _owner.AddComponent<Rigidbody2D>();
                _rigidbody.gravityScale = 0;
            }
            
            // 获取基础动作组件
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
            _obstacleDetection.DetectionDistance = 1f; // 设置检测距离为1格
            
            // 获取地面瓦片地图引用
            _groundTilemap = TilemapHelper.FindGroundTilemap();
            
            // 初始化状态
            IsMoving = false;
            _isInitialized = true;
            
            Log.Info(LogModules.AI, "SingleStepMovementComponent初始化成功", this);
        }
        
        /// <summary>
        /// 设置地面瓦片地图引用
        /// </summary>
        /// <param name="groundTilemap">地面瓦片地图</param>
        public void SetGroundTilemap(Tilemap groundTilemap)
        {
            _groundTilemap = groundTilemap;
        }
        
        /// <summary>
        /// 便捷初始化方法
        /// </summary>
        public void Initialize()
        {
            Initialize(gameObject);
        }
        
        /// <summary>
        /// 执行组件功能
        /// 使用三个基础动作组件的组合实现单步移动功能
        /// </summary>
        /// <returns>执行结果</returns>
        public bool Execute()
        {
            if (!CanExecute())
                return false;

            // 使用PositionControlComponent计算方向和距离
            Vector2 direction = _positionControl.CalculateDirectionToTarget();
            float distance = _positionControl.CalculateDistanceToTarget();

            // 检查是否到达目标
            if (distance <= _arrivalThreshold || _positionControl.IsAtTarget())
            {
                Log.Debug(LogModules.AI, "已到达目标位置，终止移动", this);
                Stop();
                return true;
            }

            _rigidbody.velocity = direction * _defaultSpeed;
            IsMoving = true;

            return true;
        }
        
        /// <summary>
        /// 检查是否可以执行
        /// </summary>
        /// <returns>是否可以执行</returns>
        public bool CanExecute()
        {
            return _isInitialized && HasTarget && _rigidbody != null && 
                   _speedControl != null && _positionControl != null && _obstacleDetection != null;
        }

        
        
        /// <summary>
        /// 设置移动目标
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        public void SetTarget(Vector2 targetPosition)
        {
            if (!_isInitialized || _positionControl == null)
                return;
            
            // 对齐到网格中心以确保移动到正确的网格位置
            if (_groundTilemap != null)
            {
                targetPosition = TilemapHelper.AlignToGridCenter(targetPosition, _groundTilemap);
            }
            
            TargetPosition = targetPosition;
            _positionControl.SetTargetPosition(targetPosition);
            HasDecisionCommand = true;
            Log.Debug(LogModules.AI, $"设置新目标位置: {targetPosition}，决策命令标志已设置", this);
        }
        
        /// <summary>
        /// 移动完成事件委托
        /// </summary>
        public event System.Action OnMovementComplete;
        
        /// <summary>
        /// 停止移动
        /// 停止所有移动相关的行为
        /// </summary>
        public void Stop()
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
            IsMoving = false;
            HasDecisionCommand = false;
            Log.Debug(LogModules.AI, "移动已停止，决策命令标志已清除", this);
            
            // 触发移动完成事件，通知相关组件（如EscapeComponent）
            OnMovementComplete?.Invoke();
        }
        
        /// <summary>
        /// 更新组件状态
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
            
            // 收到决策后才移动
            if (HasDecisionCommand && HasTarget && !_positionControl.IsAtTarget())
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
            
            // 重置状态
            IsMoving = false;
            HasDecisionCommand = false;
            Log.Debug(LogModules.AI, "SingleStepMovementComponent已重置，决策命令标志已清除", this);
        }
    }
}