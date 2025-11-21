using UnityEngine;
using System.Collections.Generic;

namespace AI.Behavior.Movement
{
    /// <summary>
    /// 障碍物检测组件 - 最小单元组件，负责检测前进路径上的障碍物
    /// </summary>
    public class ObstacleDetectionComponent : MonoBehaviour, IActionComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName => "ObstacleDetectionComponent";

        /// <summary>
        /// 检测距离
        /// </summary>
        [SerializeField] private float _detectionDistance = 2f;
        public float DetectionDistance
        {
            get => _detectionDistance;
            set => _detectionDistance = Mathf.Max(0, value);
        }

        /// <summary>
        /// 检测半径（用于圆形检测）
        /// </summary>
        [SerializeField] private float _detectionRadius = 0.5f;
        public float DetectionRadius
        {
            get => _detectionRadius;
            set => _detectionRadius = Mathf.Max(0, value);
        }

        /// <summary>
        /// 障碍物层级遮罩
        /// </summary>
        [SerializeField] private LayerMask _obstacleLayerMask = 1 << 8; // 默认使用第8层作为障碍物层
        public LayerMask ObstacleLayerMask
        {
            get => _obstacleLayerMask;
            set => _obstacleLayerMask = value;
        }

        /// <summary>
        /// 碰撞体引用
        /// </summary>
        private Collider2D _collider;

        /// <summary>
        /// 初始化组件
        /// </summary>
        /// <param name="gameObject">游戏对象引用</param>
        public void Initialize(GameObject gameObject)
        {
            _collider = gameObject.GetComponent<Collider2D>();
            if (_collider == null)
            {
                Debug.LogWarning("障碍物检测组件需要游戏对象上有Collider2D组件");
            }
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
        /// <returns>执行结果，障碍物检测组件始终返回true</returns>
        public bool Execute()
        {
            // 障碍物检测组件本身不执行移动，只提供检测功能
            return true;
        }

        /// <summary>
        /// 更新组件状态
        /// </summary>
        public void Update()
        {
            // 障碍物检测组件的核心逻辑由其他方法调用
        }

        /// <summary>
        /// 重置组件状态
        /// </summary>
        public void Reset()
        {
            // 障碍物检测组件不需要特殊的重置逻辑
        }

        /// <summary>
        /// 检测指定方向上是否有障碍物
        /// </summary>
        /// <param name="direction">检测方向</param>
        /// <returns>如果有障碍物则返回true</returns>
        public bool HasObstacleInDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude < Mathf.Epsilon)
                return false;

            direction = direction.normalized;
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, 
                direction, 
                _detectionDistance, 
                _obstacleLayerMask);

            // 确保不检测到自己
            return hit.collider != null && hit.collider != _collider;
        }

        /// <summary>
        /// 获取指定方向上最近的障碍物
        /// </summary>
        /// <param name="direction">检测方向</param>
        /// <returns>障碍物的碰撞信息，如果没有检测到则返回false</returns>
        public RaycastHit2D GetObstacleInDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude < Mathf.Epsilon)
                return default;

            direction = direction.normalized;
            return Physics2D.Raycast(
                transform.position, 
                direction, 
                _detectionDistance, 
                _obstacleLayerMask);
        }

        /// <summary>
        /// 在圆形区域内检测障碍物
        /// </summary>
        /// <returns>检测到的所有障碍物碰撞体</returns>
        public List<Collider2D> DetectObstaclesInRadius()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                transform.position, 
                _detectionRadius, 
                _obstacleLayerMask);

            List<Collider2D> obstacles = new List<Collider2D>();
            foreach (var collider in colliders)
            {
                // 排除自己
                if (collider != _collider)
                {
                    obstacles.Add(collider);
                }
            }

            return obstacles;
        }

        /// <summary>
        /// 查找可行走方向
        /// </summary>
        /// <param name="preferredDirection">首选方向</param>
        /// <param name="angleStep">角度步进值</param>
        /// <param name="maxAttempts">最大尝试次数</param>
        /// <returns>可行走的方向，如果没有找到则返回Vector2.zero</returns>
        public Vector2 FindWalkableDirection(Vector2 preferredDirection, float angleStep = 30f, int maxAttempts = 6)
        {
            // 首先尝试首选方向
            if (!HasObstacleInDirection(preferredDirection))
            {
                return preferredDirection.normalized;
            }

            // 尝试其他方向
            for (int i = 1; i <= maxAttempts; i++)
            {
                // 正方向旋转
                float angle = angleStep * i;
                Vector2 rotatedDirection = Quaternion.Euler(0, 0, angle) * preferredDirection;
                if (!HasObstacleInDirection(rotatedDirection))
                {
                    return rotatedDirection.normalized;
                }

                // 反方向旋转
                rotatedDirection = Quaternion.Euler(0, 0, -angle) * preferredDirection;
                if (!HasObstacleInDirection(rotatedDirection))
                {
                    return rotatedDirection.normalized;
                }
            }

            return Vector2.zero; // 没有找到可行走的方向
        }

        /// <summary>
        /// 可视化检测范围（用于调试）
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectionRadius);
            
            // 绘制检测射线
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)Vector2.right * _detectionDistance);
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)Vector2.left * _detectionDistance);
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)Vector2.up * _detectionDistance);
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)Vector2.down * _detectionDistance);
        }
    }
}