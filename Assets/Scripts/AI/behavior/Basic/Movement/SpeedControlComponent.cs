using UnityEngine;

namespace AI.Behavior.Movement
{
    /// <summary>
    /// 速度控制组件 - 最小单元组件，负责管理对象的移动速度
    /// </summary>
    public class SpeedControlComponent : MonoBehaviour, IActionComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName => "SpeedControlComponent";

        /// <summary>
        /// 最大移动速度
        /// </summary>
        [SerializeField] private float _maxSpeed = 5f;
        public float MaxSpeed
        {
            get => _maxSpeed;
            set => _maxSpeed = Mathf.Max(0, value);
        }

        /// <summary>
        /// 当前移动速度
        /// </summary>
        public float CurrentSpeed { get; private set; }

        /// <summary>
        /// 加速度
        /// </summary>
        [SerializeField] private float _acceleration = 2f;
        public float Acceleration
        {
            get => _acceleration;
            set => _acceleration = Mathf.Max(0, value);
        }

        /// <summary>
        /// 减速系数
        /// </summary>
        [SerializeField] private float _deceleration = 3f;
        public float Deceleration
        {
            get => _deceleration;
            set => _deceleration = Mathf.Max(0, value);
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        /// <param name="gameObject">游戏对象引用</param>
        public void Initialize(GameObject gameObject)
        {
            CurrentSpeed = 0f;
        }

        /// <summary>
        /// 便捷初始化方法
        /// </summary>
        public void Initialize()
        {
            Initialize(gameObject);
        }

        /// <summary>
        /// 判断组件是否可以执行
        /// </summary>
        /// <returns>如果游戏对象处于激活状态则返回true</returns>
        public bool CanExecute()
        {
            return gameObject.activeInHierarchy;
        }

        /// <summary>
        /// 执行组件行为
        /// </summary>
        /// <returns>执行结果，速度控制组件始终返回true</returns>
        public bool Execute()
        {
            // 速度控制组件本身不执行移动，只管理速度数据
            return true;
        }

        /// <summary>
        /// 更新组件状态
        /// </summary>
        public void Update()
        {
            // 速度控制组件的核心逻辑由其他方法调用，Update中可根据需要添加额外逻辑
            // 由于需要实现接口所以方法不能删
        }

        /// <summary>
        /// 重置组件状态
        /// </summary>
        public void Reset()
        {
            CurrentSpeed = 0f;
        }

        /// <summary>
        /// 设置目标速度
        /// </summary>
        /// <param name="targetSpeed">目标速度值</param>
        public void SetTargetSpeed(float targetSpeed)
        {
            targetSpeed = Mathf.Clamp(targetSpeed, 0, _maxSpeed);
            
            if (Mathf.Approximately(targetSpeed, 0))
            {
                // 减速到停止
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0, _deceleration * Time.deltaTime);
            }
            else
            {
                // 加速到目标速度
                CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, targetSpeed, _acceleration * Time.deltaTime);
            }
        }

        /// <summary>
        /// 直接设置当前速度（不使用加速度）
        /// </summary>
        /// <param name="speed">要设置的速度值</param>
        public void SetSpeedInstantly(float speed)
        {
            CurrentSpeed = Mathf.Clamp(speed, 0, _maxSpeed);
        }

        /// <summary>
        /// 立即停止移动
        /// </summary>
        public void StopImmediately()
        {
            CurrentSpeed = 0f;
        }

        /// <summary>
        /// 根据目标距离调整速度
        /// </summary>
        /// <param name="distanceToTarget">到目标的距离</param>
        /// <param name="stoppingDistance">停止距离</param>
        public void AdjustSpeedByDistance(float distanceToTarget, float stoppingDistance)
        {
            if (distanceToTarget <= stoppingDistance)
            {
                // 在停止距离内，减速
                float ratio = distanceToTarget / stoppingDistance;
                float targetSpeed = _maxSpeed * ratio;
                SetTargetSpeed(targetSpeed);
            }
            else
            {
                // 超出停止距离，加速到最大速度
                SetTargetSpeed(_maxSpeed);
            }
        }

        /// <summary>
        /// 检查是否正在移动
        /// </summary>
        /// <returns>如果速度大于0则返回true</returns>
        public bool IsMoving()
        {
            return CurrentSpeed > Mathf.Epsilon;
        }
    }
}