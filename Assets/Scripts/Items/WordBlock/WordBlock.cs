using UnityEngine;
using Logger;

namespace Items.WordBlock
{
    /// <summary>
    /// 文字方块类型枚举
    /// </summary>
    public enum WordType
    {
        None,
        Noun,        // 名词：ROCK, GLASS, LETTER
        Verb,        // 动词：IS
        Adjective    // 形容词：ALIVE
    }

    /// <summary>
    /// 可推动的文字方块，是游戏核心玩法的基础
    /// 继承自PushableBox，实现可推动功能
    /// </summary>
    public class WordBlock : PushableBox
    {
        [Header("文字方块设置")]
        [Tooltip("文字类型")]
        [SerializeField] private WordType _wordType;
        
        [Header("视觉设置")]
        [Tooltip("文字方块的精灵贴图")]
        [SerializeField] private Sprite _wordSprite;

        // 组件引用
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider;
        
        ///<summary>
        /// 日志模块
        /// </summary>
        private const string LOG_MODULE = LogModules.WORD_BLOCK;

        /// <summary>
        /// 文字内容（由子类实现）
        /// </summary>
        public virtual string WordContent
        {
            get { return ""; }
        }

        /// <summary>
        /// 文字类型属性
        /// </summary>
        public WordType WordType
        {
            get { return _wordType; }
            set { _wordType = value; }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Awake()
        {
            // 先调用父类的Awake方法，确保Tilemap初始化
            base.Awake();
            
            // 获取或添加必要组件
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                Log.Warning(LogModules.UTILS, "文字方块缺少SpriteRenderer组件，已自动添加", gameObject);
            }
            
            _boxCollider = GetComponent<BoxCollider2D>();
            if (_boxCollider == null)
            {
                _boxCollider = gameObject.AddComponent<BoxCollider2D>();
                _boxCollider.isTrigger = false;
                Log.Warning(LogModules.UTILS, "文字方块缺少BoxCollider2D组件，已自动添加", gameObject);
            }
            
            // 设置初始精灵
            if (_wordSprite != null)
            {
                _spriteRenderer.sprite = _wordSprite;
            }
            
            // 设置文字方块标签和图层
            gameObject.tag = "Word";
            gameObject.layer = LayerMask.NameToLayer("Obstacle");
            
            Log.Info(LogModules.UTILS, $"创建文字方块: {WordContent} (类型: {_wordType})");
        }

        /// <summary>
        /// 绘制调试信息
        /// </summary>
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 绘制文字内容
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, WordContent);
        }
#endif
    }
}