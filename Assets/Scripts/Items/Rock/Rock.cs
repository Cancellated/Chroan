using UnityEngine;
using UnityEditor;
using Logger;

namespace Items.Rock
{
    /// <summary>
    /// Rock（岩石）类
    /// 表示游戏中的岩石对象，具有沉睡态和苏醒态
    /// </summary>
    public class Rock : MonoBehaviour
    {
        // 岩石状态枚举
        public enum RockState
        {
            Sleeping,  // 沉睡态
            Awake      // 苏醒态
        }
        
        // 状态设置区域
        [Header("状态设置")]
        [Tooltip("初始状态：沉睡或苏醒")]
        public RockState InitialState = RockState.Sleeping;
        
        // 私有变量
        private RockState _currentState;
        private float _playerDistance;
        private Transform _playerTransform;
        private Rigidbody2D _rigidbody;
        private Collider2D _collider;
        
        /// <summary>
        /// 行为树组件
        /// </summary>
        private RockBehaviorTree _behaviorTree;
        
        /// <summary>
        /// 公开的刚体属性，供行为树访问
        /// </summary>
        public Rigidbody2D Rigidbody
        {
            get { return _rigidbody; }
        }
        
        // 属性
        /// <summary>
        /// 当前岩石状态
        /// </summary>
        public RockState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    RockState previousState = _currentState;
                    _currentState = value;
                    OnStateChanged(previousState, value);
                }
            }
        }
        
        /// <summary>
        /// 与玩家的距离
        /// </summary>
        public float PlayerDistance => _playerDistance;
        
        /// <summary>
        /// 初始化
        /// </summary>
        private void Awake()
        {
            _currentState = InitialState;
            EnsureComponents();
            
            // 获取或添加行为树组件
            _behaviorTree = GetComponent<RockBehaviorTree>();
            if (_behaviorTree == null)
            {
                _behaviorTree = gameObject.AddComponent<RockBehaviorTree>();
                Log.Warning("Rock", "自动添加了RockBehaviorTree组件", null);
            }
        }
        
        /// <summary>
        /// 启用时查找玩家
        /// </summary>
        private void OnEnable()
        {
            FindPlayer();
        }
        
        /// <summary>
        /// 更新玩家距离
        /// </summary>
        private void Update()
        {
            if (_playerTransform != null)
            {
                _playerDistance = Vector3.Distance(transform.position, _playerTransform.position);
            }
        }
        
        /// <summary>
        /// 切换岩石状态
        /// </summary>
        /// <param name="newState">新状态</param>
        public void ChangeState(RockState newState)
        {
            if (_currentState != newState)
            {
                RockState oldState = _currentState;
                _currentState = newState;
                
                // 处理状态转换
                OnStateChanged(oldState, newState);
            }
        }
        
        /// <summary>
        /// 查找玩家
        /// </summary>
        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            else
            {
                Log.Warning("Rock", "未找到玩家对象，请确保玩家有正确的Tag", null);
            }
        }
        
        /// <summary>
        /// 确保必要组件存在
        /// </summary>
        private void EnsureComponents()
        {
            if (!TryGetComponent(out _rigidbody))
            {
                Log.Warning("Rock", "未找到Rigidbody2D组件，添加默认组件", null);
                _rigidbody = gameObject.AddComponent<Rigidbody2D>();
            }
            
            if (!TryGetComponent(out _collider))
            {
                Log.Warning("Rock", "未找到Collider2D组件，添加默认组件", null);
                _collider = gameObject.AddComponent<CircleCollider2D>();
            }
            
            // 根据初始状态设置物理属性
            SetPhysicsProperties(CurrentState);
        }
        
        /// <summary>
        /// 状态改变时的回调
        /// </summary>
        /// <param name="previousState">之前的状态</param>
        /// <param name="newState">新的状态</param>
        private void OnStateChanged(RockState previousState, RockState newState)
        {
            SetPhysicsProperties(newState);
            Log.Info("Rock", $"状态从 {previousState} 变为 {newState}", null);
        }
        
        /// <summary>
        /// 设置物理属性
        /// </summary>
        /// <param name="state">当前状态</param>
        private void SetPhysicsProperties(RockState state)
        {
            if (_rigidbody != null)
            {
                switch (state)
                {
                    case RockState.Sleeping:
                        // 沉睡状态：静态，不受物理影响
                        _rigidbody.bodyType = RigidbodyType2D.Static;
                        break;
                    case RockState.Awake:
                        // 苏醒状态：动态，受物理影响
                        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
                        _rigidbody.gravityScale = 0f; // 零重力，漂浮状态
                        break;
                }
            }
        }
        
    #if UNITY_EDITOR
        /// <summary>
        /// 绘制调试信息
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            // 绘制苏醒范围
            Gizmos.color = Color.yellow;
            
            // 显示当前状态
            Handles.Label(transform.position + Vector3.up * 1f,
                $"状态: {CurrentState}\n距离玩家: {_playerDistance:F2}");
        }
    #endif
    }
}
