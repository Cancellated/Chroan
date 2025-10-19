using UnityEngine;
using UnityEngine.Tilemaps;
using MyGame.Managers;
using System.Collections;
using UnityEngine.InputSystem;
using Logger;

namespace MyGame.Control
{
    /// <summary>
    /// 玩家控制器，负责处理玩家的游戏玩法输入和移动逻辑
    /// 与UIController分离，专注于玩家控制逻辑
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        #region 字段
        private GameControl _inputActions;
        private Rigidbody2D _rb;
        private BoxCollider2D _collider;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;
        
        [Header("移动设置")]
        [Tooltip("移动速度")]
        [SerializeField] private float _moveSpeed = 5f;
        
        [Tooltip("Tilemap引用")]
        [SerializeField] private Tilemap _tilemap;
        
        [Tooltip("角色是否正在移动")]
        private bool _isMoving = false;
        
        [Tooltip("网格单元格大小")]
        private Vector2 _cellSize = new(1f, 1f); // 默认1x1单位
        
        [Tooltip("角色最后的朝向")]
        private Vector2 _lastFacingDirection = Vector2.down; // 默认朝下

        private static readonly string LOG_MODULE = LogModules.PLAYER;
        #endregion

        #region 属性
        /// <summary>
        /// 玩家输入
        /// </summary>
        public GameControl InputActions
        {
            get { return _inputActions; }
        }
        
        /// <summary>
        /// 角色是否正在移动
        /// </summary>
        public bool IsMoving
        {
            get { return _isMoving; }
        }
        #endregion

        #region 生命周期
        private void Awake()
        {
            // 获取组件
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            
            // 确保所有必要组件都已添加
            EnsureComponents();
            
            // 初始化输入系统
            InitializeInputSystem();
            
            // 设置Tilemap引用
            SetupTilemapReference();
            
            // 设置Rigidbody属性
            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
        }
        
        /// <summary>
        /// 确保所有必要组件都已添加到游戏对象
        /// </summary>
        private void EnsureComponents()
        {
            // 如果任何必要组件不存在，添加它们
            if (_rb == null)
            {
                _rb = gameObject.AddComponent<Rigidbody2D>();
                Log.Warning(LOG_MODULE, "Rigidbody2D组件缺失，已自动添加", this);
            }
            
            if (_collider == null)
            {
                _collider = gameObject.AddComponent<BoxCollider2D>();
                Log.Warning(LOG_MODULE, "BoxCollider2D组件缺失，已自动添加", this);
            }
            
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                Log.Warning(LOG_MODULE, "SpriteRenderer组件缺失，已自动添加", this);
            }
            
            if (_animator == null)
            {
                _animator = gameObject.AddComponent<Animator>();
                Log.Warning(LOG_MODULE, "Animator组件缺失，已自动添加", this);
            }
        }
        
        /// <summary>
        /// 初始化输入系统
        /// 处理从主菜单进入和直接运行测试场景的不同情况
        /// </summary>
        private void InitializeInputSystem()
        {
            // 尝试从InputManager获取InputActions实例
            if (InputManager.Instance != null)
            {
                if (InputManager.Instance.InputActions != null)
                {
                    _inputActions = InputManager.Instance.InputActions;
                    // 确保GamePlay动作映射已启用
                    if (!_inputActions.GamePlay.enabled)
                    {
                        _inputActions.GamePlay.Enable();
                    }
                    // 使用Info级别日志，因为DebugLog可能在某些构建中不可用
                    Log.Info(LOG_MODULE, "成功从InputManager获取InputActions实例", this);
                }
                else
                {
                    Log.Error(LOG_MODULE, "InputManager存在但InputActions为null！创建本地GameControl实例", this);
                    _inputActions = new GameControl();
                    _inputActions.GamePlay.Enable();
                }
            }
            else
            {
                // 如果InputManager不存在，创建新实例
                Log.Error(LOG_MODULE, "InputManager不存在，即将创建新实例", this);
                
                // 尝试手动创建InputManager实例
                try
                {
                    GameObject inputManagerObj = new("InputManager");
                    InputManager inputManager = inputManagerObj.AddComponent<InputManager>();
                    
                    // 检查InputManager.Instance是否已初始化
                    if (InputManager.Instance != null && InputManager.Instance.InputActions != null)
                    {
                        _inputActions = InputManager.Instance.InputActions;
                        Log.Info(LOG_MODULE, "成功创建InputManager实例并获取InputActions", this);
                    }
                    else
                    {
                        // 如果手动创建后Instance仍为null，直接创建本地GameControl实例
                        _inputActions = new GameControl();
                        _inputActions.GamePlay.Enable();
                        Log.Warning(LOG_MODULE, "使用本地GameControl实例代替InputManager", this);
                    }
                }
                catch (System.Exception e)
                {
                    // 捕获任何可能的异常
                    _inputActions = new GameControl();
                    _inputActions.GamePlay.Enable();
                    Log.Error(LOG_MODULE, $"创建InputManager实例时出错: {e.Message}", this);
                }
            }
        }
        
        /// <summary>
        /// 设置Tilemap引用
        /// </summary>
        private void SetupTilemapReference()
        {
            // 如果未指定Tilemap，则尝试查找场景中的Tilemap
            if (_tilemap == null)
            {
                _tilemap = FindObjectOfType<Tilemap>();
                if (_tilemap == null)
                {
                    Log.Warning(LOG_MODULE, "未找到Tilemap组件，请确保场景中存在Tilemap", this);
                }
                else
                {
                    // 获取Tilemap单元格大小
                    _cellSize = _tilemap.cellSize;
                }
            }
            else
            {
                // 获取Tilemap单元格大小
                _cellSize = _tilemap.cellSize;
            }
        }
        
        private void Update()
        {
            // 处理玩家输入
            HandlePlayerInput();
        }
        
        private void OnEnable()
        {
            // 确保输入系统已初始化并启用
            if (_inputActions != null && !_inputActions.GamePlay.enabled)
            {
                _inputActions.GamePlay.Enable();
            }
            
            // 添加交互事件监听
            if (_inputActions != null)
            {
                _inputActions.GamePlay.Interact.performed += OnInteractPerformed;
            }
        }
        
        private void OnDisable()
        {
            // 禁用输入系统以避免不必要的处理
            if (_inputActions != null && _inputActions.GamePlay.enabled)
            {
                _inputActions.GamePlay.Disable();
            }
        }
        
        private void OnDestroy()
        {
            // 清理事件监听
            if (_inputActions != null)
            {
                _inputActions.GamePlay.Interact.performed -= OnInteractPerformed;
            }
        }
        #endregion

        #region 玩家控制
        /// <summary>
        /// 处理玩家输入
        /// </summary>
        private void HandlePlayerInput()
        {
            // 处理移动输入
            if (!_isMoving)
            {
                Vector2 moveInput = PlayerMove();
                if (moveInput != Vector2.zero)
                {
                    MoveCharacter(moveInput);
                }
            }
            
            // 处理交互输入
            if (PlayerInteract())
            {
                OnInteractPerformed(default);
            }
        }
        
        /// <summary>
        /// 玩家移动输入
        /// 添加空值检查以防止空引用异常
        /// </summary>
        public Vector2 PlayerMove()
        {
            if (_inputActions == null || !_inputActions.GamePlay.enabled)
            {
                return Vector2.zero;
            }
            return _inputActions.GamePlay.Move.ReadValue<Vector2>();
        }
        
        /// <summary>
        /// 玩家交互输入
        /// 添加空值检查以防止空引用异常
        /// </summary>
        /// <returns></returns>
        public bool PlayerInteract()
        {
            if (_inputActions == null || !_inputActions.GamePlay.enabled)
            {
                return false;
            }
            return _inputActions.GamePlay.Interact.triggered;
        }
        
        #endregion

        #region 移动实现
        /// <summary>
        /// 移动角色
        /// 添加更完善的空值检查和边界条件处理
        /// </summary>
        /// <param name="moveDirection">移动方向</param>
        private void MoveCharacter(Vector2 moveDirection)
        {
            // 如果没有移动输入，直接返回
            if (moveDirection == Vector2.zero || _isMoving || _tilemap == null)
            {
                return;
            }
            
            // 确保角色朝向正确并更新动画参数
            UpdateCharacterFacing(moveDirection);
            
            // 计算目标位置
            Vector3Int currentGridPos = WorldToGridPosition(transform.position);
            Vector3Int targetGridPos = currentGridPos + new Vector3Int(
                Mathf.RoundToInt(moveDirection.x), 
                Mathf.RoundToInt(moveDirection.y), 
                0
            );
            
            // 检查目标位置是否可通行
            if (IsCellWalkable(targetGridPos))
            {
                // 开始移动协程
                StartCoroutine(MoveToGridPosition(targetGridPos));
            }
        }
        
        /// <summary>
        /// 平滑移动到目标网格位置
        /// </summary>
        /// <param name="targetGridPos">目标网格位置</param>
        /// <returns>协程迭代器</returns>
        private IEnumerator MoveToGridPosition(Vector3Int targetGridPos)
        {
            _isMoving = true;
            Vector3 startPos = transform.position;
            Vector3 targetPos = GridToWorldPosition(targetGridPos);
            float journeyLength = Vector3.Distance(startPos, targetPos);
            float startTime = Time.time;
            
            // 计算移动方向并设置动画参数
            Vector3 direction = (targetPos - startPos).normalized;
            Vector2 moveDirection = (Vector2)direction;
            
            // 设置移动状态的动画参数
            UpdateAnimationParameters(moveDirection);
            
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                float distCovered = (Time.time - startTime) * _moveSpeed;
                float fractionOfJourney = distCovered / journeyLength;
                transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
                yield return null;
            }
            
            // 确保精确对齐到网格中心
            transform.position = targetPos;
            _isMoving = false;
            
            // 设置待机状态的动画参数，保持最后朝向
            UpdateAnimationParameters(Vector2.zero);
        }
        
        /// <summary>
        /// 将世界坐标转换为网格坐标
        /// </summary>
        /// <param name="worldPosition">世界坐标</param>
        /// <returns>网格坐标</returns>
        private Vector3Int WorldToGridPosition(Vector3 worldPosition)
        {
            if (_tilemap == null)
                return Vector3Int.zero;
            
            Vector3Int gridPosition = _tilemap.WorldToCell(worldPosition);
            return gridPosition;
        }
        
        /// <summary>
        /// 将网格坐标转换为世界坐标并居中
        /// </summary>
        /// <param name="gridPosition">网格坐标</param>
        /// <returns>居中的世界坐标</returns>
        private Vector3 GridToWorldPosition(Vector3Int gridPosition)
        {
            if (_tilemap == null)
                return Vector3.zero;
            
            // 获取网格单元格的中心世界坐标
            Vector3 worldPosition = _tilemap.GetCellCenterWorld(gridPosition);
            return worldPosition;
        }
        
        /// <summary>
        /// 检查网格位置是否可通行
        /// </summary>
        /// <param name="gridPosition">要检查的网格位置</param>
        /// <returns>如果可通行则返回true，否则返回false</returns>
        private bool IsCellWalkable(Vector3Int gridPosition)
        {
            if (_tilemap == null)
                return true;
            
            // 1. 检查是否在Tilemap边界内
            if (!_tilemap.HasTile(gridPosition))
            {
                return false;
            }
            
            // 2. 使用Physics2D进行精确碰撞检测
            Vector3 worldPosition = GridToWorldPosition(gridPosition);
            Collider2D[] colliders = Physics2D.OverlapBoxAll(worldPosition, 
                                                            new Vector2(_cellSize.x - 0.1f, _cellSize.y - 0.1f), 
                                                            0f);
            
            foreach (Collider2D collider in colliders)
            {
                // 检查是否有不可通行的碰撞体
                if (collider.CompareTag("Obstacle"))
                {
                    return false;
                }
            }
            
            return true;
        }
        #endregion

        #region 动画控制
        /// <summary>
        /// 更新角色朝向
        /// </summary>
        /// <param name="moveDirection">移动方向</param>
        private void UpdateCharacterFacing(Vector2 moveDirection)
        {
            // 更新动画参数并存储最后朝向
            UpdateAnimationParameters(moveDirection);
        }
        
        /// <summary>
        /// 更新动画参数，控制混合树
        /// </summary>
        /// <param name="movementInput">移动输入向量</param>
        private void UpdateAnimationParameters(Vector2 movementInput)
        {
            if (_animator == null)
                return;
            
            // 如果有移动输入
            if (movementInput.magnitude > 0.1f)
            {
                // 更新移动参数和最后朝向
                _animator.SetFloat("MoveX", movementInput.x);
                _animator.SetFloat("MoveY", movementInput.y);
                _animator.SetBool("IsMoving", true);
                _lastFacingDirection = movementInput.normalized;
            }
            else
            {
                // 停止移动时，保持最后朝向
                _animator.SetFloat("MoveX", _lastFacingDirection.x);
                _animator.SetFloat("MoveY", _lastFacingDirection.y);
                _animator.SetBool("IsMoving", false);
            }
        }
        #endregion

        #region 事件处理

        
        /// <summary>
        /// 交互执行
        /// </summary>
        /// <param name="context">输入上下文</param>
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            // 播放交互动画
            PlayInteractAnimation();
            // 实现交互逻辑
            Log.DebugLog(LOG_MODULE, "Interact performed", this);
        }
        
        /// <summary>
        /// 播放交互动画
        /// </summary>
        private void PlayInteractAnimation()
        {
            if (_animator == null)
                return;
            
            // 使用触发器触发交互动画
            _animator.SetTrigger("Interact");
            Log.Info(LOG_MODULE, "Playing interact animation", this);
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 将角色位置对齐到网格中心
        /// </summary>
        public void AlignToGrid()
        {
            if (_tilemap == null)
                return;
            
            Vector3Int gridPosition = WorldToGridPosition(transform.position);
            Vector3 centeredWorldPosition = GridToWorldPosition(gridPosition);
            transform.position = centeredWorldPosition;
        }
        
        /// <summary>
        /// 设置移动速度
        /// </summary>
        /// <param name="speed">新的移动速度</param>
        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = Mathf.Max(0f, speed);
        }
        #endregion
    }
}