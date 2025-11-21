using UnityEngine;
using System.Collections.Generic;

namespace AI.Behavior.Perception
{
    /// <summary>
    /// 检测组件 - 最小单元组件，负责基本的范围检测和物体收集
    /// </summary>
    public class DetectionComponent : MonoBehaviour, IActionComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName => "DetectionComponent";

        /// <summary>
        /// 检测范围
        /// </summary>
        [SerializeField] private float _detectionRange = 5f;
        public float DetectionRange
        {
            get => _detectionRange;
            set => _detectionRange = Mathf.Max(0, value);
        }

        /// <summary>
        /// 检测层遮罩
        /// </summary>
        [SerializeField] private LayerMask _detectionLayerMask = 1 << 9; // 默认使用第9层作为可检测对象层
        public LayerMask DetectionLayerMask
        {
            get => _detectionLayerMask;
            set => _detectionLayerMask = value;
        }

        /// <summary>
        /// 检测间隔时间（秒）
        /// </summary>
        [SerializeField] private float _detectionInterval = 0.2f;
        public float DetectionInterval
        {
            get => _detectionInterval;
            set => _detectionInterval = Mathf.Max(0.01f, value);
        }

        /// <summary>
        /// 碰撞体引用
        /// </summary>
        private Collider2D _collider;

        /// <summary>
        /// 上一次检测时间
        /// </summary>
        private float _lastDetectionTime;

        /// <summary>
        /// 最近检测到的物体列表
        /// </summary>
        public List<GameObject> DetectedObjects { get; private set; }

        /// <summary>
        /// 初始化组件
        /// </summary>
        /// <param name="gameObject">游戏对象引用</param>
        public void Initialize(GameObject gameObject)
        {
            _collider = gameObject.GetComponent<Collider2D>();
            if (_collider == null)
            {
                Debug.LogWarning("检测组件需要游戏对象上有Collider2D组件");
            }
            
            DetectedObjects = new List<GameObject>();
            _lastDetectionTime = -_detectionInterval; // 确保在第一次Update时就进行检测
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
        /// <returns>执行结果，检测组件始终返回true</returns>
        public bool Execute()
        {
            PerformDetection();
            return true;
        }

        /// <summary>
        /// 更新组件状态
        /// </summary>
        public void Update()
        {
            if (Time.time - _lastDetectionTime >= _detectionInterval)
            {
                PerformDetection();
                _lastDetectionTime = Time.time;
            }
        }

        /// <summary>
        /// 重置组件状态
        /// </summary>
        public void Reset()
        {
            DetectedObjects.Clear();
            _lastDetectionTime = -_detectionInterval;
        }

        /// <summary>
        /// 执行检测
        /// </summary>
        private void PerformDetection()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                transform.position, 
                _detectionRange, 
                _detectionLayerMask);

            DetectedObjects.Clear();
            foreach (var collider in colliders)
            {
                // 排除自己
                if (collider != _collider)
                {
                    DetectedObjects.Add(collider.gameObject);
                }
            }
        }

        /// <summary>
        /// 立即执行一次检测
        /// </summary>
        public void DetectImmediately()
        {
            PerformDetection();
            _lastDetectionTime = Time.time;
        }

        /// <summary>
        /// 检查特定对象是否在检测范围内
        /// </summary>
        /// <param name="target">要检查的目标对象</param>
        /// <returns>如果目标在范围内则返回true</returns>
        public bool IsObjectInRange(GameObject target)
        {
            if (target == null || !target.activeInHierarchy)
                return false;

            float distance = Vector2.Distance(transform.position, target.transform.position);
            return distance <= _detectionRange;
        }

        /// <summary>
        /// 获取检测范围内最近的对象
        /// </summary>
        /// <returns>最近的对象，如果没有检测到则返回null</returns>
        public GameObject GetNearestObject()
        {
            if (DetectedObjects.Count == 0)
                return null;

            GameObject nearest = null;
            float minDistance = float.MaxValue;

            foreach (var obj in DetectedObjects)
            {
                if (obj == null) continue;
                
                float distance = Vector2.Distance(transform.position, obj.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = obj;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 获取指定距离内的对象
        /// </summary>
        /// <param name="maxDistance">最大距离</param>
        /// <returns>指定距离内的对象列表</returns>
        public List<GameObject> GetObjectsInDistance(float maxDistance)
        {
            List<GameObject> result = new();

            foreach (var obj in DetectedObjects)
            {
                if (obj == null) continue;
                
                float distance = Vector2.Distance(transform.position, obj.transform.position);
                if (distance <= maxDistance)
                {
                    result.Add(obj);
                }
            }

            return result;
        }

        /// <summary>
        /// 可视化检测范围（用于调试）
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
        }
    }
}