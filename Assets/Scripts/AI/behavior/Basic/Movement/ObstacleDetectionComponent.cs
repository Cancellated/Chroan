using UnityEngine;
using System.Collections.Generic;
using Logger;
using AI.Behavior;

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
        /// 拥有该组件的游戏对象
        /// </summary>
        private GameObject _owner;

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
        [SerializeField] private LayerMask _obstacleLayerMask = 1 << 6; // 默认使用第6层作为障碍物层(Wall)
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
            _owner = gameObject;
            _collider = gameObject.GetComponent<Collider2D>();
            if (_collider == null)
            {
                Log.Warning(LogModules.AI, "障碍物检测组件需要游戏对象上有Collider2D组件");
            }
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
            
            // 使用TilemapHelper的IsDirectionBlocked方法进行障碍物检测
            return TilemapHelper.IsDirectionBlocked(
                _owner.transform.position,     // 起始位置
                direction,              // 检测方向
                _detectionDistance,     // 检测距离
                _owner,                 // 忽略自身
                this);
        }


        /// <summary>
        /// 查找可行走方向
        /// 使用TilemapHelper检查方向是否可行走
        /// </summary>
        /// <param name="preferredDirection">首选方向</param>
        /// <returns>可行走的方向，如果没有找到则返回Vector2.zero</returns>
        public Vector2 FindWalkableDirection(Vector2 preferredDirection)
        {
            // 首选方向有效性检查
            if (preferredDirection.sqrMagnitude < Mathf.Epsilon)
                return Vector2.zero;
                
            // 提前归一化首选方向
            preferredDirection = preferredDirection.normalized;
            
            // 使用TilemapHelper检查首选方向是否可行走
            if (!TilemapHelper.IsDirectionBlocked(_owner.transform.position, preferredDirection, _detectionDistance, _owner, this))
            {
                return preferredDirection;
            }
            
            // 如果首选方向被阻挡，尝试四个基本方向（上、右、下、左）
            Vector2[] basicDirections = new Vector2[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };
            foreach (Vector2 dir in basicDirections)
            {
                if (!TilemapHelper.IsDirectionBlocked(_owner.transform.position, dir, _detectionDistance, _owner, this))
                {
                    return dir;
                }
            }
            
            // 没有找到可行走的方向
            return Vector2.zero;
        }
    }
}