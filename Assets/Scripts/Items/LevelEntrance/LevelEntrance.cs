using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using MyGame.Events;
using MyGame.Input;
using Logger;
using Items.LevelEntrance;
using MyGame.Managers;

namespace Items.LevelEntrance
{
    /// <summary>
    /// 关卡入口组件
    /// 设计为Tile大小，玩家走上去时弹出进入提示
    /// 在解锁和未解锁时具备两种贴图
    /// </summary>
    public class LevelEntrance : MonoBehaviour
    {
        #region 配置字段
        
        [Header("关卡配置")]
        [Tooltip("关卡唯一标识")]
        [SerializeField] private string levelId = "level_001";
        
        [Tooltip("关卡显示名称")]
        [SerializeField] private string levelName = "关卡1";
        
        [Tooltip("目标场景路径")]
        [SerializeField] private string scenePath = "GameScene";
        
        [Header("Tile设置")]
        [Tooltip("Tile大小（单位：Unity单位）")]
        [SerializeField] private Vector2 tileSize = new(1f, 1f);
        
        [Tooltip("交互检测偏移（防止边缘检测问题）")]
        [SerializeField] private float interactionOffset = 0.1f;
        
        [Header("贴图设置")]
        [Tooltip("解锁状态下的贴图")]
        [SerializeField] private Sprite unlockedSprite;
        
        [Tooltip("未解锁状态下的贴图")]
        [SerializeField] private Sprite lockedSprite;
        
        [Header("交互设置")]
        [Tooltip("激活延迟时间（秒）")]
        [SerializeField] private float activationDelay = 0.2f;
        
        [Tooltip("交互提示UI预设")]
        [SerializeField] private GameObject interactionHintPrefab;
        
        [Tooltip("提示UI的垂直偏移")]
        [SerializeField] private float hintVerticalOffset = 1.5f;
        
        [Header("关卡状态")]
        [Tooltip("是否已解锁")]
        [SerializeField] private bool isUnlocked = false;
        
        [Tooltip("是否已完成")]
        [SerializeField] private bool isCompleted = false;
        
        #endregion
        
        #region 私有字段
        
        private bool isPlayerOnTile = false;    // 是否玩家当前在Tile上
        private float playerEnterTime = 0f;    // 玩家进入Tile的时间
        private bool isActivated = false;    // 是否已激活
        private Transform playerTransform = null;    // 玩家Transform引用
        private SpriteRenderer spriteRenderer = null;    // SpriteRenderer引用
        private GameObject interactionHintInstance = null;    // 交互提示实例引用
        private float lastCenteredTime = 0f;    // 最后一次在中心的时间（用于防抖）
        
        private const string LOG_MODULE = "LevelEntrance";
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 关卡唯一标识
        /// </summary>
        public string LevelId => levelId;
        
        /// <summary>
        /// 关卡显示名称
        /// </summary>
        public string LevelName => levelName;
        
        /// <summary>
        /// 是否已解锁
        /// </summary>
        public bool IsUnlocked => isUnlocked;
        
        /// <summary>
        /// 是否已完成
        /// </summary>
        public bool IsCompleted => isCompleted;
        
        #endregion
        
        #region 生命周期方法
        
        /// <summary>
        /// 初始化
        /// </summary>
        private void Awake()
        {
            InitializeComponents();
            SetupTileSize();
            SnapToGrid();
            SetupRenderingOrder(); // 设置渲染顺序
            CheckUnlockStatus();
            UpdateVisualState();
            
            // 注册到管理器
            RegisterToManager();
            
            // 调试信息
            Log.Info(LOG_MODULE, $"LevelEntrance初始化完成: {levelName}", gameObject);
        }
        
        /// <summary>
        /// 设置渲染顺序，确保不被Tilemap遮挡
        /// </summary>
        private void SetupRenderingOrder()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingLayerName = "Items";
                spriteRenderer.sortingOrder = 1; // 确保在Tilemap之上
                
                Log.Info(LOG_MODULE, "设置渲染顺序完成", gameObject);
            }
            else
            {
                Log.Warning(LOG_MODULE, "SpriteRenderer未找到，无法设置渲染顺序", gameObject);
            }
        }
        
        /// <summary>
        /// 每帧更新
        /// </summary>
        private void Update()
        {
            // 如果已激活且玩家在Tile上，处理交互输入
            if (isActivated && isPlayerOnTile)
            {
                HandleInteractionInput();
            }
        }
        
        /// <summary>
        /// 销毁时清理
        /// </summary>
        private void OnDestroy()
        {
            // 清理提示UI
            if (interactionHintInstance != null)
            {
                Destroy(interactionHintInstance);
            }
        }
        
        #endregion
        
        #region 初始化方法
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponents()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            // 确保有碰撞体（Tile大小）
            if (!TryGetComponent<Collider2D>(out var existingCollider))
            {
                var collider = gameObject.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(tileSize.x - interactionOffset, tileSize.y - interactionOffset);
                collider.isTrigger = true;
                Log.Info(LOG_MODULE, "自动添加了Tile大小的碰撞体", gameObject);
            }
            else
            {
                Log.Info(LOG_MODULE, $"已存在碰撞体: {existingCollider.GetType().Name}", gameObject);
                existingCollider.isTrigger = true;
            }
        }
        
        /// <summary>
        /// 设置Tile尺寸
        /// </summary>
        private void SetupTileSize()
        {
            // 确保SpriteRenderer使用正确的尺寸
            if (spriteRenderer != null)
            {
                spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                spriteRenderer.size = tileSize;
            }
        }
        
        /// <summary>
        /// 确保Tile位置对齐网格
        /// </summary>
        private void SnapToGrid()
        {
            if (TryGetTilemapGrid(out var grid))
            {
                Vector3Int cellPosition = grid.WorldToCell(transform.position);
                transform.position = grid.GetCellCenterWorld(cellPosition);
                Log.Info(LOG_MODULE, $"关卡入口对齐到网格位置: {cellPosition}", gameObject);
            }
        }
        
        /// <summary>
        /// 尝试获取Tilemap网格
        /// </summary>
        private bool TryGetTilemapGrid(out Grid grid)
        {
            grid = FindObjectOfType<Grid>();
            return grid != null;
        }
        
        /// <summary>
        /// 注册到管理器
        /// </summary>
        private void RegisterToManager()
        {
            // 查找场景中的LevelEntranceManager实例
            var manager = FindObjectOfType<MyGame.Managers.LevelEntranceManager>();
            if (manager != null)
            {
                manager.RegisterLevelEntrance(this);
            }
            else
            {
                Log.Warning(LOG_MODULE, "场景中未找到LevelEntranceManager，无法注册关卡入口");
            }
        }
        
        #endregion
        
        #region 碰撞检测
        
        /// <summary>
        /// 玩家进入Tile区域
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            Log.Info(LOG_MODULE, $"检测到碰撞进入: {other.tag} - {other.name}", gameObject);
            
            if (other.CompareTag("Player"))
            {
                Log.Info(LOG_MODULE, $"玩家进入Tile区域: {other.name}", gameObject);
                playerTransform = other.transform;
                isPlayerOnTile = true;
                
                // 直接开始激活检测，不需要复杂的位置检测
                StartCoroutine(CheckActivation());
                Log.Info(LOG_MODULE, $"开始激活检测协程", gameObject);
            }
            else
            {
                Log.Warning(LOG_MODULE, $"非玩家对象进入Tile区域: {other.tag} - {other.name}", gameObject);
            }
        }
        
        /// <summary>
        /// 玩家离开Tile区域
        /// </summary>
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Log.Info(LOG_MODULE, $"玩家离开Tile区域: {other.name}", gameObject);
                // 玩家完全离开，关闭激活状态
                OnPlayerExitTile();
            }
        }
        
        /// <summary>
        /// 检查玩家位置（确保在Tile中心）
        /// </summary>
        private void CheckPlayerPosition()
        {
            if (playerTransform == null) return;
            
            bool isCentered = IsPlayerCenteredOnTile(playerTransform);
            
            // 只有当玩家在Tile上时才进行检测
            if (isPlayerOnTile)
            {
                if (!isCentered)
                {
                    // 玩家离开Tile中心，但需要确认是否真的离开（避免抖动）
                    if (Time.time - lastCenteredTime > 0.1f) // 离开中心超过0.1秒才认为是真的离开
                    {
                        OnPlayerExitTile();
                    }
                    else
                    {
                        Log.Info(LOG_MODULE, $"玩家暂时离开中心，但仍在检测范围内", gameObject);
                    }
                }
                else if (!isActivated) // 玩家在Tile中心且未激活
                {
                    // 记录最后一次在中心的时间
                    lastCenteredTime = Time.time;
                    
                    // 玩家在Tile中心，但还没有调用OnPlayerEnterTile
                    if (playerEnterTime == 0f) // 第一次检测到在中心
                    {
                        OnPlayerEnterTile();
                    }
                    else
                    {
                        // 玩家仍在Tile中心
                        Log.Info(LOG_MODULE, $"玩家位置检测: 在Tile中心", gameObject);
                    }
                }
            }
        }
        
        /// <summary>
        /// 检查玩家是否在Tile中心位置
        /// </summary>
        private bool IsPlayerCenteredOnTile(Transform player)
        {
            Vector3 tileCenter = transform.position;
            Vector3 playerPosition = player.position;
            
            // 计算玩家与Tile中心的距离
            float distanceX = Mathf.Abs(playerPosition.x - tileCenter.x);
            float distanceY = Mathf.Abs(playerPosition.y - tileCenter.y);
            
            // 允许的偏移范围（更宽松的范围，避免抖动）
            float allowedOffset = tileSize.x * 0.4f; // 从0.3增加到0.4
            
            bool isCentered = distanceX <= allowedOffset && distanceY <= allowedOffset;
            
            // 调试信息
            if (isPlayerOnTile)
            {
                Log.Info(LOG_MODULE, $"中心检测: 距离X={distanceX:F2}, 距离Y={distanceY:F2}, 允许偏移={allowedOffset:F2}, 是否在中心={isCentered}", gameObject);
            }
            
            return isCentered;
        }
        
        #endregion
        
        #region 玩家交互处理
        
        /// <summary>
        /// 玩家进入Tile处理
        /// </summary>
        private void OnPlayerEnterTile()
        {
            playerEnterTime = Time.time;
            isActivated = false;
            
            ShowInteractionHint(true);
            
            Log.Info(LOG_MODULE, $"玩家进入关卡Tile: {levelName}", gameObject);
        }
        
        /// <summary>
        /// 玩家离开Tile处理
        /// </summary>
        private void OnPlayerExitTile()
        {
            isPlayerOnTile = false;
            isActivated = false;
            
            ShowInteractionHint(false);
            StopAllCoroutines();
            
            Log.Info(LOG_MODULE, $"玩家离开关卡Tile: {levelName}", gameObject);
        }
        
        /// <summary>
        /// 检查激活状态
        /// </summary>
        private IEnumerator CheckActivation()
        {
            Log.Info(LOG_MODULE, $"开始激活检测协程，延迟时间: {activationDelay}秒", gameObject);
            
            while (isPlayerOnTile && !isActivated)
            {
                float elapsedTime = Time.time - playerEnterTime;
                Log.Info(LOG_MODULE, $"激活检测中... 已等待: {elapsedTime:F1}秒", gameObject);
                
                if (elapsedTime >= activationDelay)
                {
                    Log.Info(LOG_MODULE, $"激活条件满足，开始激活", gameObject);
                    ActivateLevelEntrance();
                    yield break;
                }
                yield return new WaitForSeconds(0.1f); // 每0.1秒检测一次
            }
            
            Log.Info(LOG_MODULE, $"激活检测协程结束，玩家离开或已激活", gameObject);
        }
        
        /// <summary>
        /// 激活关卡入口（准备进入状态）
        /// </summary>
        private void ActivateLevelEntrance()
        {
            if (!isUnlocked)
            {
                Log.Warning(LOG_MODULE, $"尝试进入未解锁的关卡: {levelName}", gameObject);
                return;
            }
            
            isActivated = true;
            
            // 播放激活效果
            PlayTileActivationEffect();
            
            // 更新提示文本，提示按下交互键
            UpdateInteractionHintForActivation();
            
            Log.Info(LOG_MODULE, $"关卡入口激活，等待交互键: {levelName}", gameObject);
        }
        
        /// <summary>
        /// 处理交互键输入
        /// </summary>
        private void HandleInteractionInput()
        {
            if (isActivated && isPlayerOnTile && isUnlocked)
            {
                // 检查交互键是否按下（使用项目的输入系统）
                if (IsInteractionButtonPressed())
                {
                    EnterLevelDirectly();
                    Log.Info(LOG_MODULE, $"按下交互键，进入关卡: {levelName}", gameObject);
                }
            }
        }
        
        /// <summary>
        /// 检查交互键是否按下（集成项目输入系统）
        /// </summary>
        private bool IsInteractionButtonPressed()
        {
            // 使用项目的输入系统检查Interact按钮
            if (InputManager.Instance != null && 
                InputManager.Instance.InputActions != null)
            {
                return InputManager.Instance.InputActions.GamePlay.Interact.triggered;
            }
            
            return false;
        }
        
        /// <summary>
        /// 更新交互提示为激活状态
        /// </summary>
        private void UpdateInteractionHintForActivation()
        {
            if (interactionHintInstance != null)
            {
                var hintText = interactionHintInstance.GetComponentInChildren<UnityEngine.UI.Text>();
                if (hintText != null)
                {
                    hintText.text = $"{levelName}\n按下E键进入";
                }
            }
        }
        
        #endregion
        
        #region 视觉和状态管理
        
        /// <summary>
        /// 检查解锁状态（公共方法，供管理器调用）
        /// </summary>
        public void CheckUnlockStatus()
        {
            // 这里可以集成存档系统
            // 暂时使用Inspector中设置的值
            // var saveData = SaveManager.Instance.GetCurrentSaveData();
            // isUnlocked = saveData.gameProgress.unlockedLevels.Contains(levelId);
            // isCompleted = saveData.gameProgress.completedLevels.Contains(levelId);
        }
        
        /// <summary>
        /// 更新视觉状态（公共方法，供管理器调用）
        /// </summary>
        public void UpdateVisualState()
        {
            if (spriteRenderer != null)
            {
                if (isUnlocked)
                {
                    if (unlockedSprite != null)
                    {
                        spriteRenderer.sprite = unlockedSprite;
                    }
                    spriteRenderer.color = Color.white;
                }
                else
                {
                    if (lockedSprite != null)
                    {
                        spriteRenderer.sprite = lockedSprite;
                    }
                    spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // 灰暗效果
                }
            }
        }
        
        /// <summary>
        /// 播放Tile激活效果
        /// </summary>
        private void PlayTileActivationEffect()
        {
            // Tile脉冲效果
            StartCoroutine(TilePulseEffect());
        }
        
        /// <summary>
        /// Tile脉冲效果
        /// </summary>
        private IEnumerator TilePulseEffect()
        {
            var originalScale = transform.localScale;
            var targetScale = originalScale * 1.1f;
            
            float duration = 0.2f;
            float elapsed = 0f;
            
            // 放大
            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 缩小
            elapsed = 0f;
            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.localScale = originalScale;
        }
        
        #endregion
        
        #region UI交互
        
        /// <summary>
        /// 显示/隐藏交互提示
        /// </summary>
        private void ShowInteractionHint(bool show)
        {
            if (interactionHintPrefab == null) return;
            
            if (show && interactionHintInstance == null)
            {
                // 创建提示UI（在Tile上方）
                Vector3 hintPosition = transform.position + Vector3.up * hintVerticalOffset;
                interactionHintInstance = Instantiate(interactionHintPrefab, hintPosition, Quaternion.identity);
                
                // 设置提示文本
                var hintText = interactionHintInstance.GetComponentInChildren<UnityEngine.UI.Text>();
                if (hintText != null)
                {
                    string statusText = isUnlocked ? "进入" : "未解锁";
                    hintText.text = $"{levelName}\n{statusText}";
                }
                
                Log.Info(LOG_MODULE, $"显示交互提示: {levelName}", gameObject);
            }
            else if (!show && interactionHintInstance != null)
            {
                Destroy(interactionHintInstance);
                interactionHintInstance = null;
                
                Log.Info(LOG_MODULE, $"隐藏交互提示: {levelName}", gameObject);
            }
        }
        

        
        /// <summary>
        /// 直接进入关卡（无确认界面时使用）
        /// </summary>
        private void EnterLevelDirectly()
        {
            if (!string.IsNullOrEmpty(scenePath))
            {
                Log.Info(LOG_MODULE, $"开始进入关卡流程: {levelName} -> {scenePath}", gameObject);
                
                // 使用事件触发关卡进入
                MyGame.Events.GameEvents.TriggerLevelEntered(levelId);
                Log.Info(LOG_MODULE, $"已触发关卡进入事件: {levelId}", gameObject);
                
                // 使用事件触发场景切换
                MyGame.Events.GameEvents.TriggerSceneLoadStart(scenePath);
                Log.Info(LOG_MODULE, $"已触发场景加载开始事件: {scenePath}", gameObject);
                
                Log.Info(LOG_MODULE, $"关卡进入流程完成: {levelName} -> {scenePath}", gameObject);
            }
            else
            {
                Log.Error(LOG_MODULE, $"关卡场景路径为空: {levelName}", gameObject);
            }
        }
        
        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 设置关卡解锁状态
        /// </summary>
        public void SetUnlocked(bool unlocked)
        {
            isUnlocked = unlocked;
            UpdateVisualState();
            
            Log.Info(LOG_MODULE, $"设置关卡解锁状态: {levelName} -> {unlocked}", gameObject);
        }
        
        /// <summary>
        /// 设置关卡完成状态
        /// </summary>
        public void SetCompleted(bool completed)
        {
            isCompleted = completed;
            UpdateVisualState();
            
            Log.Info(LOG_MODULE, $"设置关卡完成状态: {levelName} -> {completed}", gameObject);
        }
        
        #endregion
    }
}
