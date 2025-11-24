using UnityEngine;
using Logger;
using System.Collections.Generic;

namespace AI.Behavior
{
    /// <summary>
    /// 感知行为组件
    /// 负责检测周围环境中的物体并提供感知结果给其他行为组件
    /// </summary>
    public class PerceptionComponent : MonoBehaviour, IActionComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        public string ComponentName { get { return "PerceptionComponent"; } }

        /// <summary>
        /// 感知范围半径
        /// </summary>
        [SerializeField] private float _perceptionRadius = 5f;

        /// <summary>
        /// 感知层遮罩
        /// </summary>
        [SerializeField] private LayerMask _perceptionLayers;

        /// <summary>
        /// 感知间隔时间（秒）
        /// </summary>
        [SerializeField] private float _perceptionInterval = 0.5f;

        /// <summary>
        /// 上次感知时间
        /// </summary>
        private float _lastPerceptionTime;

        /// <summary>
        /// 已感知到的物体列表
        /// </summary>
        private List<GameObject> _perceivedObjects = new();

        /// <summary>
        /// 最近的威胁物体
        /// </summary>
        private GameObject _nearestThreat;

        /// <summary>
        /// 初始化组件
        /// </summary>
        public void Initialize(GameObject owner)
        {
            _lastPerceptionTime = 0f;
            _perceivedObjects.Clear();
            _nearestThreat = null;
            
            Log.Info(LogModules.AI, $"{ComponentName}: 初始化完成，感知半径: {_perceptionRadius}", this);
        }

        /// <summary>
        /// 判断是否可以执行感知行为
        /// 感知行为始终可以执行
        /// </summary>
        /// <returns>始终返回true</returns>
        public bool CanExecute()
        {
            // 感知行为始终可以执行，但会根据间隔时间决定是否实际进行感知
            return true;
        }

        /// <summary>
        /// 执行感知行为
        /// 检测周围的物体并更新感知结果
        /// 注意：现在间隔检查已移至Update方法中
        /// </summary>
        /// <returns>执行是否成功</returns>
        public bool Execute()
        {
            // 执行感知
            PerformPerception();
            _lastPerceptionTime = Time.time;
            return true;
        }

        /// <summary>
        /// 更新组件状态
        /// 在每帧更新时调用，根据设定的间隔时间执行感知逻辑
        /// 负责更新威胁信息，作为串行执行流程的第一步
        /// </summary>
        public void Update()
        {
            // 检查是否需要进行感知更新，使用与Execute方法相同的间隔时间
            if (Time.time - _lastPerceptionTime >= _perceptionInterval)
            {
                Execute(); // 按间隔执行感知
            }
            
            // 确保威胁信息是最新的，作为串行执行流程的第一步
            // 这样后续的决策组件就能基于最新的感知信息进行判断
        }
        
        /// <summary>
        /// 获取当前感知间隔时间
        /// </summary>
        /// <returns>感知间隔（秒）</returns>
        public float GetPerceptionInterval()
        {
            return _perceptionInterval;
        }
        
        /// <summary>
        /// 设置感知间隔时间
        /// </summary>
        /// <param name="interval">新的感知间隔（秒）</param>
        public void SetPerceptionInterval(float interval)
        {
            if (interval > 0)
            {
                _perceptionInterval = interval;
                Log.Info(LogModules.AI, $"{ComponentName}: 感知间隔已设置为 {interval} 秒", this);
            }
            else
            {
                Log.Warning(LogModules.AI, $"{ComponentName}: 无效的感知间隔值 {interval}，必须大于0", this);
            }
        }

        /// <summary>
        /// 重置组件状态
        /// 清空感知结果
        /// </summary>
        public void Reset()
        {
            _perceivedObjects.Clear();
            _nearestThreat = null;
            _lastPerceptionTime = 0f;
            
            Log.DebugLog(LogModules.AI, $"{ComponentName}: 已重置", this);
        }

        /// <summary>
        /// 执行实际的感知操作
        /// </summary>
        private void PerformPerception()
        {
            // 清空之前的感知结果
            _perceivedObjects.Clear();
            _nearestThreat = null;
            float minDistance = float.MaxValue;

            // 使用Physics2D.OverlapCircle检测周围物体
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _perceptionRadius, _perceptionLayers);
            
            foreach (Collider2D collider in colliders)
            {
                // 排除自身
                if (collider.gameObject == gameObject)
                    continue;

                _perceivedObjects.Add(collider.gameObject);
                
                // 计算到物体的距离，找出最近的威胁
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    _nearestThreat = collider.gameObject;
                }
            }

            // 记录感知日志
            if (_perceivedObjects.Count > 0)
            {
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 感知到 {_perceivedObjects.Count} 个物体，最近的物体距离: {minDistance:F2}", this);
            }
        }

        /// <summary>
        /// 获取已感知到的所有物体
        /// </summary>
        /// <returns>感知到的物体列表</returns>
        public List<GameObject> GetPerceivedObjects()
        {
            return new List<GameObject>(_perceivedObjects);
        }

        /// <summary>
        /// 获取最近的威胁物体
        /// </summary>
        /// <returns>最近的威胁物体，如果没有则返回null</returns>
        public GameObject GetNearestThreat()
        {
            return _nearestThreat;
        }

        /// <summary>
        /// 检查指定位置是否在感知范围内
        /// </summary>
        /// <param name="position">要检查的位置</param>
        /// <returns>如果在范围内则返回true</returns>
        public bool IsInPerceptionRange(Vector2 position)
        {
            return Vector2.Distance(transform.position, position) <= _perceptionRadius;
        }

        /// <summary>
        /// 获取当前感知半径
        /// </summary>
        /// <returns>感知半径值</returns>
        public float GetPerceptionRadius()
        {
            return _perceptionRadius;
        }
        
        /// <summary>
        /// 设置感知半径
        /// </summary>
        /// <param name="radius">新的感知半径</param>
        public void SetPerceptionRadius(float radius)
        {
            if (radius > 0)
            {
                _perceptionRadius = radius;
                Log.DebugLog(LogModules.AI, $"{ComponentName}: 感知半径已设置为 {radius}", this);
            }
        }

        /// <summary>
        /// 设置感知层遮罩
        /// </summary>
        /// <param name="layers">新的层遮罩</param>
        public void SetPerceptionLayers(LayerMask layers)
        {
            _perceptionLayers = layers;
        }

        /// <summary>
        /// 在编辑器中绘制感知范围
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _perceptionRadius);
        }
    }
}