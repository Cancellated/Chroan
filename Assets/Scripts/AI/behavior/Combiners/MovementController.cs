using UnityEngine;
using AI.Behavior.Movement;


namespace AI.Behavior.Combiners
{
    /// <summary>
    /// 移动控制器 - 组合移动相关的组件，提供高级移动控制
    /// </summary>
    public class MovementController : ComponentCombiner
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        public new string ComponentName => "MovementController";

        /// <summary>
        /// 位置控制组件引用
        /// </summary>
        private PositionControlComponent _positionControl;
        public PositionControlComponent PositionControl => _positionControl;

        /// <summary>
        /// 速度控制组件引用
        /// </summary>
        private SpeedControlComponent _speedControl;
        public SpeedControlComponent SpeedControl => _speedControl;

        /// <summary>
        /// 障碍物检测组件引用
        /// </summary>
        private ObstacleDetectionComponent _obstacleDetection;
        public ObstacleDetectionComponent ObstacleDetection => _obstacleDetection;

        /// <summary>
        /// 是否启用自动障碍物规避
        /// </summary>
        [SerializeField] private bool _enableObstacleAvoidance = true;
        public bool EnableObstacleAvoidance
        {
            get => _enableObstacleAvoidance;
            set => _enableObstacleAvoidance = value;
        }

        /// <summary>
        /// 障碍物规避平滑系数
        /// </summary>
        [SerializeField] private float _avoidanceSmoothingFactor = 0.2f;
        public float AvoidanceSmoothingFactor
        {
            get => _avoidanceSmoothingFactor;
            set => _avoidanceSmoothingFactor = Mathf.Clamp01(value);
        }

        /// <summary>
        /// 当前移动方向
        /// </summary>
        public Vector2 CurrentDirection { get; private set; }

        /// <summary>
        /// 是否正在移动
        /// </summary>
        public bool IsMoving => _speedControl != null && _speedControl.CurrentSpeed > 0.01f;

        /// <summary>
        /// 是否到达目标位置
        /// </summary>
        public bool HasReachedTarget => _positionControl != null && _positionControl.IsAtTarget();

        /// <summary>
        /// 初始化移动控制器
        /// </summary>
        /// <param name="gameObject">游戏对象引用</param>
        public new void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
            
            // 获取或创建必要的组件
            _positionControl = GetOrCreateComponent<PositionControlComponent>(gameObject);
            _speedControl = GetOrCreateComponent<SpeedControlComponent>(gameObject);
            _obstacleDetection = GetOrCreateComponent<ObstacleDetectionComponent>(gameObject);
            
            // 添加这些组件到子组件列表（如果尚未添加）
            AddComponent(_positionControl);
            AddComponent(_speedControl);
            AddComponent(_obstacleDetection);
            
            // 设置执行模式为顺序执行
            Mode = ExecutionMode.Sequential;
            
            // 重置状态
            CurrentDirection = Vector2.zero;
        }

        /// <summary>
        /// 便捷初始化方法
        /// </summary>
        public new void Initialize()
        {
            Initialize(gameObject);
        }

        /// <summary>
        /// 执行移动控制器
        /// </summary>
        /// <returns>执行结果</returns>
        public new bool Execute()
        {  if (!CanExecute())
                return false;
            
            // 计算移动方向
            CalculateMovementDirection();
            
            // 如果启用了障碍物规避，处理障碍物
            if (_enableObstacleAvoidance)
            {
                ApplyObstacleAvoidance();
            }
            
            // 执行子组件
            return base.Execute();
        }

        /// <summary>
        /// 设置移动目标
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="arrivalThreshold">到达阈值</param>
        public void SetTarget(Vector2 targetPosition, float arrivalThreshold = 0.5f)
        {  if (_positionControl != null)
            {
                _positionControl.SetTargetPosition(targetPosition);
                
                // 注意：ArrivalThreshold在PositionControlComponent中是只读的，如果需要修改，需要在PositionControlComponent中添加setter
                // 这里只设置目标位置
            }
        }

        /// <summary>
        /// 设置移动目标（使用游戏对象）
        /// </summary>
        /// <param name="target">目标游戏对象</param>
        /// <param name="arrivalThreshold">到达阈值</param>
        public void SetTarget(GameObject target, float arrivalThreshold = 0.5f)
        {  if (_positionControl != null && target != null)
            {
                _positionControl.SetTargetPosition(target.transform.position);
                
                // 注意：ArrivalThreshold在PositionControlComponent中是只读的
            }
        }

        /// <summary>
        /// 设置移动速度
        /// </summary>
        /// <param name="speed">目标速度</param>
        public void SetSpeed(float speed)
        {  if (_speedControl != null)
            {
                _speedControl.SetTargetSpeed(speed);
            }
        }

        /// <summary>
        /// 立即设置速度（不使用加速度）
        /// </summary>
        /// <param name="speed">速度值</param>
        public void SetImmediateSpeed(float speed)
        {  if (_speedControl != null)
            {
                _speedControl.SetSpeedInstantly(speed);
            }
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public void Stop()
        {  if (_speedControl != null)
            {
                _speedControl.StopImmediately();
                
                // 停止物理移动
                Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                }
            }
            
            if (_positionControl != null)
            {
                _positionControl.ClearTarget();
            }
            
            CurrentDirection = Vector2.zero;
        }

        /// <summary>
        /// 移动到目标位置
        /// </summary>
        public void MoveToTarget()
        {            if (_positionControl != null && _positionControl.HasTarget)
            {
                // 计算方向和距离
                Vector2 direction = _positionControl.CalculateDirectionToTarget();
                float distance = _positionControl.CalculateDistanceToTarget();
                
                // 调整速度
                _speedControl.AdjustSpeedByDistance(distance, 1.5f);
                
                // 检查障碍物并调整方向
                if (_obstacleDetection != null)
                {
                    Vector2 adjustedDirection = _obstacleDetection.FindWalkableDirection(direction);
                    if (adjustedDirection != Vector2.zero)
                    {
                        ApplyMovement(adjustedDirection);
                    }
                    else
                    {
                        ApplyMovement(direction);
                    }
                }
                else
                {
                    ApplyMovement(direction);
                }
            }
        }
        
        /// <summary>
        /// 移动到指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="speed">移动速度</param>
        /// <param name="arrivalThreshold">到达阈值</param>
        public void MoveTo(Vector2 targetPosition, float speed, float arrivalThreshold = 0.5f)
        {  SetTarget(targetPosition, arrivalThreshold);
            SetSpeed(speed);
            MoveToTarget();
        }

        /// <summary>
        /// 移动到指定游戏对象
        /// </summary>
        /// <param name="target">目标游戏对象</param>
        /// <param name="speed">移动速度</param>
        /// <param name="arrivalThreshold">到达阈值</param>
        public void MoveTo(GameObject target, float speed, float arrivalThreshold = 0.5f)
        {  SetTarget(target, arrivalThreshold);
            SetSpeed(speed);
            MoveToTarget();
        }
        
        /// <summary>
        /// 应用移动
        /// </summary>
        /// <param name="direction">移动方向</param>
        private void ApplyMovement(Vector2 direction)
        {
            // 检查是否有Rigidbody2D组件
            Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * _speedControl.CurrentSpeed;
            }
            else
            {
                // 如果没有Rigidbody2D，使用transform直接移动
                transform.position += (Vector3)(direction * _speedControl.CurrentSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// 按指定方向移动
        /// </summary>
        /// <param name="direction">移动方向</param>
        /// <param name="speed">移动速度</param>
        public void MoveInDirection(Vector2 direction, float speed)
        {  if (_positionControl != null && _speedControl != null)
            {
                // 取消目标跟随
                _positionControl.ClearTarget();
                
                // 设置移动方向和速度
                CurrentDirection = direction.normalized;
                _speedControl.SetTargetSpeed(speed);
                
                // 检查障碍物
                if (_obstacleDetection != null && direction.sqrMagnitude > Mathf.Epsilon)
                {
                    Vector2 adjustedDirection = _obstacleDetection.FindWalkableDirection(direction);
                    if (adjustedDirection != Vector2.zero)
                    {
                        ApplyMovement(adjustedDirection);
                    }
                    else
                    {
                        ApplyMovement(direction);
                    }
                }
                else if (direction.sqrMagnitude > Mathf.Epsilon)
                {
                    ApplyMovement(direction.normalized);
                }
            }
        }

        /// <summary>
        /// 计算移动方向
        /// </summary>
        private void CalculateMovementDirection()
        {  if (_positionControl == null)
                return;
            
            // 如果有目标位置，计算朝向目标的方向
            if (_positionControl.HasTarget && !_positionControl.IsAtTarget())
            {
                CurrentDirection = _positionControl.CalculateDirectionToTarget();
            }
        }

        /// <summary>
        /// 应用障碍物规避
        /// </summary>
        private void ApplyObstacleAvoidance()
        {  if (_obstacleDetection == null || _positionControl == null)
                return;
            
            // 检测前方障碍物
            RaycastHit2D obstacleHit = _obstacleDetection.GetObstacleInDirection(CurrentDirection);
            if (obstacleHit.collider != null && !object.ReferenceEquals(obstacleHit.collider, gameObject.GetComponent<Collider2D>()))
            {
                // 寻找可行走的方向
                Vector2 avoidanceDirection = _obstacleDetection.FindWalkableDirection(CurrentDirection);
                if (avoidanceDirection != Vector2.zero)
                {
                    // 平滑过渡到规避方向
                    CurrentDirection = Vector2.Lerp(CurrentDirection, avoidanceDirection, _avoidanceSmoothingFactor);
                    
                    // 如果有目标位置，根据规避方向调整目标
                    if (_positionControl.HasTarget && !_positionControl.IsAtTarget())
                    {
                        // 使用SetTargetPosition方法设置目标位置
                        Vector2 targetPos = _positionControl.TargetPosition;
                        Vector2 adjustedTarget = targetPos + avoidanceDirection * 2f; // 调整目标位置
                        _positionControl.SetTargetPosition(adjustedTarget);
                    }
                }
            }
        }

        /// <summary>
        /// 获取或创建指定类型的组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="gameObject">游戏对象</param>
        /// <returns>组件实例</returns>
        private T GetOrCreateComponent<T>(GameObject gameObject) where T : MonoBehaviour, IActionComponent
        {  T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
                component.Initialize(gameObject);
            }
            return component;
        }
    }
}