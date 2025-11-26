using UnityEngine;

namespace AI.Behavior.Movement
{
    /// <summary>
    /// 位置控制组件 - 最小单元组件，负责目标位置的设置和管理
    /// </summary>
    public class PositionControlComponent : MonoBehaviour, IActionComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName => "PositionControlComponent";

        /// <summary>
        /// 当前目标位置
        /// </summary>
        public Vector2 TargetPosition { get; private set; }

        /// <summary>
        /// 到达目标位置的阈值距离
        /// </summary>
        [SerializeField] private float _arrivalThreshold = 0.05f;
        public float ArrivalThreshold => _arrivalThreshold;

        /// <summary>
        /// 是否有有效的目标位置
        /// </summary>
        public bool HasTarget { get; private set; }

        /// <summary>
        /// 初始化组件
        /// </summary>
        /// <param name="gameObject">游戏对象引用</param>
        public void Initialize(GameObject gameObject)
        {
            HasTarget = false;
        }
        
        /// <summary>
        /// 初始化组件的便捷方法
        /// </summary>
        public void Initialize()
        {
            Initialize(gameObject);
        }

        /// <summary>
        /// 判断组件是否可以执行
        /// </summary>
        /// <returns>如果有目标位置且在场景中则返回true</returns>
        public bool CanExecute()
        {
            return HasTarget && gameObject.activeInHierarchy;
        }

        /// <summary>
        /// 执行组件行为
        /// </summary>
        /// <returns>执行结果，位置控制组件始终返回true</returns>
        public bool Execute()
        {
            // 位置控制组件本身不执行移动，只管理位置数据
            return true;
        }

        /// <summary>
        /// 更新组件状态
        /// </summary>
        public void Update()
        {
            // 检查是否到达目标位置
            if (HasTarget && IsAtTarget())
            {
                HasTarget = false;
            }
        }

        /// <summary>
        /// 重置组件状态
        /// </summary>
        public void Reset()
        {
            HasTarget = false;
        }

        /// <summary>
        /// 设置目标位置
        /// </summary>
        /// <param name="target">目标位置</param>
        public void SetTargetPosition(Vector2 target)
        {
            TargetPosition = target;
            HasTarget = true;
        }

        /// <summary>
        /// 清除当前目标
        /// </summary>
        public void ClearTarget()
        {
            HasTarget = false;
        }

        /// <summary>
        /// 检查是否到达目标位置
        /// </summary>
        /// <returns>如果到达目标位置则返回true</returns>
        public bool IsAtTarget()
        {
            if (!HasTarget)
                return false;
            
            return Vector2.Distance(transform.position, TargetPosition) <= _arrivalThreshold;
        }

        /// <summary>
        /// 计算到目标位置的方向向量
        /// </summary>
        /// <returns>归一化的方向向量</returns>
        public Vector2 CalculateDirectionToTarget()
        {
            if (!HasTarget)
                return Vector2.zero;
            
            Vector2 direction = TargetPosition - (Vector2)transform.position;
            return direction.normalized;
        }

        /// <summary>
        /// 计算到目标位置的距离
        /// </summary>
        /// <returns>距离值</returns>
        public float CalculateDistanceToTarget()
        {
            if (!HasTarget)
                return float.MaxValue;
            
            return Vector2.Distance(transform.position, TargetPosition);
        }
    }
}
