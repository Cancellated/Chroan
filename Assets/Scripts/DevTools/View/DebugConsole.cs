using System;
using UnityEngine;
using TMPro;
using System.Linq;
using MyGame.Managers;
using MyGame.Events;
using Logger;
using MyGame.UI;


namespace MyGame.DevTool
{
    /// <summary>
    /// 调试控制台视图，负责UI展示和用户输入事件捕获
    /// </summary>
    public class DebugConsole : BaseView<DebugConsoleController>
    {
        private const string LOG_MODULE = LogModules.DEBUGCONSOLE;
        //最大日志保留数
        private const int MAX_LOG_LINES = 100;

        private DebugConsoleController m_Controller;
        private static DebugConsole s_instance;

        /// <summary>
        /// 静态实例，用于全局访问
        /// </summary>
        public static DebugConsole Instance
        {
            get { return s_instance; }
        }
        #region UI引用

        [Header("UI组件引用")]
        [Tooltip("命令输入框")]
        public TMP_InputField inputField;

        [Tooltip("输出文本框")]
        public TMP_Text outputText;

        [Tooltip("滚动视图组件")]
        public UnityEngine.UI.ScrollRect scrollRect;

        [Header("层级设置")]
        [Tooltip("控制台Canvas的Sorting Order。值越高，显示层级越高，不易被其他UI遮挡。")]
        public int canvasSortingOrder = 900; // 设置较高的默认值，确保控制台显示在大多数UI上层
        #endregion

        #region 生命周期函数

        /// <summary>
        /// 初始化组件
        /// </summary>
        protected override void Awake()
        {
            // 设置面板类型为Console
            m_panelType = UIType.Console;
            
            base.Awake();

            // 设置静态实例
            s_instance = this;

            if (outputText != null)
                outputText.text = "调试控制台已启动。输入 help 查看命令。";

            // 绑定输入框事件处理器
            if (inputField != null)
            {
                inputField.onEndEdit.AddListener(delegate { OnCommandEntered(); });
                // 设置输入行为模式为提交时结束编辑
                inputField.lineType = TMP_InputField.LineType.SingleLine;
            }

            // 设置Canvas排序层级
            SetCanvasSortingOrder();
        }

        /// <summary>
        /// 尝试绑定控制器
        /// </summary>
        protected override void TryBindController()
        {
            // 查找并关联控制器
            m_Controller = FindObjectOfType<DebugConsoleController>();
            if (m_Controller == null)
            {
                Log.Error(LOG_MODULE, "未找到DebugConsoleController组件，已添加新组件", this);
                m_Controller = gameObject.AddComponent<DebugConsoleController>();
            }

            // 设置控制器的视图引用
            m_Controller.SetView(this);

            // 绑定控制器到视图
            BindController(m_Controller);
        }

        /// <summary>
        /// 设置Canvas的Sorting Order，确保控制台显示在其他UI上层
        /// </summary>
        private void SetCanvasSortingOrder()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = canvasSortingOrder;
            }
            else
            {
                Log.Warning(LOG_MODULE, "未找到父级Canvas组件，无法设置排序层级。", this);
            }
        }

        /// <summary>
        /// 初始化面板，显式实现IUIPanel接口
        /// </summary>
        public override void Initialize()
        {

            Log.Info(LOG_MODULE, "DebugConsole.PanelType 设置为: " + m_panelType);
        }

        #endregion

        #region 公共方法
        // 重写Show方法-控制台不需要动画控制显隐
        public override void Show()
        {
            Log.Info(LOG_MODULE, "显示调试控制台", this);

            // 确保游戏对象被激活
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                Log.Info(LOG_MODULE, "游戏对象已被激活", this);
            }

            if (m_canvasGroup != null)
            {
                m_canvasGroup.alpha = 1f;
                m_canvasGroup.interactable = true;
                m_canvasGroup.blocksRaycasts = true;
            }

            IsVisible = true;
        }

        // 重写BaseUI的Hide方法
        public override void Hide()
        {
            Log.Info(LOG_MODULE, "隐藏调试控制台", this);

            // 确保游戏对象被禁用
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }

            if (m_canvasGroup != null)
            {
                m_canvasGroup.alpha = 0f;
                m_canvasGroup.interactable = false;
                m_canvasGroup.blocksRaycasts = false;
            }

            IsVisible = false;
        }

        /// <summary>
        /// 当输入命令时调用
        /// </summary>
        /// <param name="input">输入框的当前文本内容（TMP_InputField的onEndEdit事件会传入）</param>
        public void OnCommandEntered(string input = null)
        {
            // 获取输入文本
            string cmd = inputField.text.Trim().ToLower();
            if (string.IsNullOrEmpty(input))
            {
                // 清除输入框
                inputField.text = "";

                // 将命令转发给控制器处理
                if (m_Controller != null)
                {
                    m_Controller.HandleCommand(cmd);
                }

                // 重新激活输入框以便继续输入
                inputField.ActivateInputField();
            }
        }

        /// <summary>
        /// 在控制台中显示文本消息
        /// </summary>
        /// <param name="msg">要显示的消息</param>
        public void DisplayText(string msg)
        {
            if (outputText != null)
            {
                // 分割现有日志为行数组
                var lines = outputText.text.Split('\n');


                // 如果超过最大行数，移除最早的行
                if (lines.Length >= MAX_LOG_LINES)
                {
                    lines = lines.Skip(1).ToArray();
                }

                // 添加新日志并重新组合
                outputText.text = string.Join("\n", lines) + $"\n{msg}";

                // 强制布局更新
                Canvas.ForceUpdateCanvases();

                // 当Scroll Rect存在且内容需要滚动时执行滚动到底部
                if (scrollRect != null && outputText.preferredHeight > outputText.rectTransform.rect.height)
                {
                    scrollRect.verticalNormalizedPosition = 0f; // 0f 表示滚动到底部
                }
            }
            Log.Info(LOG_MODULE, msg, this);
        }

        /// <summary>
        /// 在控制台中显示文本消息（DisplayText的别名方法）
        /// </summary>
        /// <param name="msg">要显示的消息</param>
        public void Print(string msg)
        {
            DisplayText(msg);
        }
        #endregion
    }

    /// <summary>
    /// 调试命令特性，用于标记命令方法
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DebugCommand : Attribute
    {
        /// <summary>命令名称</summary>
        public string CommandName { get; }
        
        /// <summary>命令描述</summary> 
        public string Description { get; set; }
        
        /// <summary>
        /// 创建调试命令特性
        /// </summary>
        /// <param name="name">命令名称</param>
        public DebugCommand(string name)
        {
            CommandName = name;
        }
    }
}