using UnityEngine;
using Logger;

namespace AI.Behavior
{
    /// <summary>
    /// 逃离行为组件
    /// 负责处理AI从威胁源逃离的行为逻辑
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
        /// 逃离安全距离
        /// </summary>
        [SerializeField] private float _safetyDistance = 4f;

        /// <summary>
        /// 逃离速度倍数
        /// </summary>
        [SerializeField] private float _escapeSpeedMultiplier = 1.5f;

        /// <summary>
        /// 逃跑触发距离（小于安全距离时触发）
        /// </summary>
        [SerializeField] private float _escapeTriggerDistance = 3f;

        /// <summary>
        /// 是否正在逃离
        /// </summary>
        private bool _isEscaping = false;

        /// <summary>
        /// 原始移动速度
        /// </summary>
        private float _originalSpeed;

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
        }

        /// <summary>
        /// 判断是否可以执行逃离行为
        /// </summary>
        /// <returns>如果有威胁且距离在触发范围内则返回true</returns>
        public bool CanExecute()
        {
            // 首先尝试从感知组件获取威胁
            UpdateThreatSource();
            
            if (_threatSource == null)
                return false;

            float distanceToThreat = Vector2.Distance(transform.position, _threatSource.position);
            return distanceToThreat <= _escapeTriggerDistance;
        }

        /// <summary>
        /// 执行逃离行为
        /// </summary>
        /// <returns>执行是否成功</returns>
        public bool Execute()
        {
            if (!CanExecute() || _movementComponent == null)
                return false;

            // 设置为逃离状态
            _isEscaping = true;

            // 增加移动速度用于逃离
            _movementComponent.ModifySpeedByMultiplier(_escapeSpeedMultiplier);

            // 计算逃离方向
            Vector2 escapeDirection = CalculateEscapeDirection();
            
            // 检查是否有有效的逃离方向
            if (escapeDirection == Vector2.zero)
            {
                Log.Warning(LogModules.AI, $"{ComponentName}: 未能找到合适的逃离方向", this);
                return false;
            }

            // 设置目标位置
            Vector2 targetPosition = (Vector2)transform.position + escapeDirection * _safetyDistance;
            _movementComponent.SetTarget(targetPosition);

            // 执行移动
            return _movementComponent.Execute();
        }

        /// <summary>
        /// 更新组件状态
        /// 确保在感知范围内持续感知并根据威胁情况动态调整行动
        /// </summary>
        public void Update()
        {
            // 无论是否正在逃离，只要在感知范围内就持续更新感知信息
            UpdateThreatSource();
            
            // 如果有威胁源且在感知范围内
            if (_threatSource != null && _perceptionComponent != null)
            {
                float distanceToThreat = Vector2.Distance(transform.position, _threatSource.position);
                float perceptionRadius = _perceptionComponent.GetPerceptionRadius();
                
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
                            // 不立即停止逃离状态，而是继续监控威胁
                            // 保持在感知范围内持续感知的行为
                        }
                        else
                        {
                            // 威胁仍然太近，更新逃离方向和目标
                            Execute();
                        }
                    }
                    else
                    {
                        // 不在逃离状态但威胁在触发距离内，开始逃离
                        if (distanceToThreat <= _escapeTriggerDistance)
                        {
                            Execute();
                        }
                    }
                }
                else
                {
                    // 威胁超出感知范围，停止逃离
                    if (_isEscaping)
                    {
                        StopEscaping();
                    }
                }
            }
            else if (_isEscaping)
            {
                // 没有威胁源但处于逃离状态，停止逃离
                StopEscaping();
            }
        }
        
        /// <summary>
        /// 更新威胁源
        /// 优先从感知组件获取最近的威胁，确保感知组件持续更新
        /// </summary>
        private void UpdateThreatSource()
        {
            if (_perceptionComponent != null)
            {
                // 主动执行感知，确保获取最新的威胁信息
                _perceptionComponent.Execute();
                
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
        /// </summary>
        /// <returns>逃离方向向量</returns>
        private Vector2 CalculateEscapeDirection()
        {
            if (_threatSource == null)
                return Vector2.zero;

            // 计算从威胁源指向自身的向量（即逃离方向）
            Vector2 direction = ((Vector2)transform.position - (Vector2)_threatSource.position).normalized;
            return direction;
        }

        /// <summary>
        /// 停止逃离行为
        /// </summary>
        private void StopEscaping()
        {
            _isEscaping = false;
            
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