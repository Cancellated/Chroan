using UnityEngine;
using Logger;


namespace AI.Behavior
{
    /// <summary>
    /// 威胁检测基础组件，提供检测和管理威胁源的功能
    /// </summary>
    public class ThreatDetectionComponent : MonoBehaviour
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        protected string ComponentName => "ThreatDetectionComponent";
        
        /// <summary>
        /// 威胁源对象
        /// </summary>
        protected GameObject _threatSource;
        
        /// <summary>
        /// 威胁检测范围
        /// </summary>
        [Header("感知配置")]
        [Tooltip("威胁检测范围")]
        [SerializeField] protected float _detectionRange = 2f;
        
        /// <summary>
        /// 触发逃离的距离阈值
        /// </summary>
        [Tooltip("触发逃离的距离阈值")]
        [SerializeField] protected float _triggerDistance = 1.5f;
        
        /// <summary>
        /// 要忽略的自身组件
        /// </summary>
        protected Component _selfComponent;
        
        /// <summary>
        /// 初始化威胁检测组件
        /// </summary>
        /// <param name="selfComponent">要忽略的自身组件</param>
        public virtual void Initialize(Component selfComponent = null)
        {
            _selfComponent = selfComponent != null ? selfComponent : this;
            Log.LogWithCooldown(Log.LogLevel.Debug, LogModules.AI, $"{ComponentName}: 已初始化，检测范围: {_detectionRange}，触发距离: {_triggerDistance}", this, $"{ComponentName}_initialize");
        }
        
        /// <summary>
        /// 设置威胁源
        /// </summary>
        /// <param name="threatSource">威胁源对象</param>
        public virtual void SetThreatSource(GameObject threatSource)
        {
            if (threatSource == null)
            {
                Log.LogWithCooldown(Log.LogLevel.Debug, LogModules.AI, $"{ComponentName}: 清除威胁源", this, $"{ComponentName}_clearThreat");
                _threatSource = null;
                return;
            }
            
            _threatSource = threatSource;
            Log.LogWithCooldown(Log.LogLevel.Debug, LogModules.AI, $"{ComponentName}: 设置威胁源为: {threatSource.name}", this, $"{ComponentName}_setThreat");
        }
        
        /// <summary>
        /// 检查威胁源是否在检测范围内
        /// </summary>
        /// <returns>如果威胁源在检测范围内则返回true</returns>
        public virtual bool IsThreatInRange()
        {
            if (_threatSource == null)
                return false;
            
            float distance = Vector2.Distance(transform.position, _threatSource.transform.position);
            bool inRange = distance <= _detectionRange;
            
            if (inRange)
            {
                Log.LogWithCooldown(Log.LogLevel.Debug, LogModules.AI, $"{ComponentName}: 威胁源在检测范围内，距离: {distance}", this, $"{ComponentName}_threatInRange");
            }
            
            return inRange;
        }
        
        /// <summary>
        /// 检查是否需要触发逃离行为
        /// </summary>
        /// <returns>如果需要触发逃离则返回true</returns>
        public virtual bool ShouldTriggerEscape()
        {
            if (_threatSource == null)
                return false;
            
            float distance = Vector2.Distance(transform.position, _threatSource.transform.position);
            bool shouldTrigger = distance <= _triggerDistance;
            
            if (shouldTrigger)
            {
                Log.LogWithCooldown(Log.LogLevel.Debug, LogModules.AI, $"{ComponentName}: 威胁距离过近，触发逃离，距离: {distance}", this, $"{ComponentName}_triggerEscape");
            }
            
            return shouldTrigger;
        }
        
        /// <summary>
        /// 获取威胁源的相对方向
        /// </summary>
        /// <returns>相对于威胁源的单位方向向量，威胁源不存在时返回零向量</returns>
        public virtual Vector2 GetThreatDirection()
        {
            if (_threatSource == null)
                return Vector2.zero;
            
            Vector2 direction = (Vector2)_threatSource.transform.position - (Vector2)transform.position;
            
            // 检查距离是否为零（避免除以零）
            if (direction.sqrMagnitude > 0.01f)
            {
                return direction.normalized;
            }
            
            return Vector2.zero;
        }
        
        /// <summary>
        /// 更新威胁源信息
        /// </summary>
        public virtual void UpdateThreatSource()
        {
            if (_threatSource == null)
                return;
            
            // 如果威胁源已被销毁，清除引用
            if (_threatSource == null)
            {
                Log.LogWithCooldown(Log.LogLevel.Debug, LogModules.AI, $"{ComponentName}: 威胁源已被销毁，清除引用", this, $"{ComponentName}_threatDestroyed");
                _threatSource = null;
                return;
            }
            
            // 检查威胁源是否超出检测范围
            if (!IsThreatInRange())
            {
                _threatSource = null;
            }
        }
        
        /// <summary>
        /// 重置威胁检测组件
        /// </summary>
        public virtual void ResetDetection()
        {
            _threatSource = null;
            Log.LogWithCooldown(Log.LogLevel.Debug, LogModules.AI, $"{ComponentName}: 已重置威胁检测", this, $"{ComponentName}_resetDetection");
        }
        
        /// <summary>
        /// 获取当前威胁源
        /// </summary>
        /// <returns>当前威胁源对象或null</returns>
        public virtual GameObject GetThreatSource()
        {
            return _threatSource;
        }
        
        /// <summary>
        /// 获取到威胁源的距离
        /// </summary>
        /// <returns>到威胁源的距离，如果威胁源不存在则返回-1</returns>
        public virtual float GetDistanceToThreat()
        {
            if (_threatSource == null)
                return -1f;
            
            return Vector2.Distance(transform.position, _threatSource.transform.position);
        }
    }
}