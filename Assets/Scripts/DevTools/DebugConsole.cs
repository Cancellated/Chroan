using MyGame.Managers;
using MyGame.System;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UI.Managers;
using System.Linq;

namespace MyGame.DevTool
{
    /// <summary>
    /// 调试控制台组件，提供游戏内命令输入功能
    /// </summary>
    [AddComponentMenu("MyGame/Debug/DebugConsole")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(InputField))]
    [RequireComponent(typeof(Text))]

    
    public class DebugConsole : Singleton<DebugConsole>
    {
        //最大日志保留数
        private const int MAX_LOG_LINES = 100;
        #region 特性定义
        
        /// <summary>
        /// 调试命令特性，用于标记命令方法
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class DebugCommandAttribute : Attribute
        {
            /// <summary>命令名称</summary>
            public string CommandName { get; }
            
            /// <summary>命令描述</summary>
            public string Description { get; set; }
            
            /// <summary>
            /// 创建调试命令特性
            /// </summary>
            /// <param name="name">命令名称</param>
            public DebugCommandAttribute(string name)
            {
                CommandName = name;
            }
        }
        
        #endregion

        #region UI引用
        
        [Header("UI组件引用")]
        [Tooltip("命令输入框")]
        public InputField inputField;
        
        [Tooltip("输出文本框")]
        public Text outputText;
        //调试按钮
        [SerializeField] private GameObject buttonPrefab;

        #endregion

        #region 命令系统
        
        // 命令字典：键为命令名称，值为(执行方法, 描述)
        private Dictionary<string, (Action action, string description)> _commands = new();

        /// <summary>
        /// 初始化组件
        /// </summary>
        override protected void Awake()
        {
            base.Awake();
            InitializeCommands();
            
            if (outputText != null)
                outputText.text = "调试控制台已启动。输入 help 查看命令。";
        }

        /// <summary>
        /// 初始化所有命令
        /// </summary>
        void InitializeCommands()
        {
            // 使用反射获取所有标记了DebugCommand特性的方法
            foreach (var method in GetType().GetMethods(
                BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var attr = method.GetCustomAttribute<DebugCommandAttribute>();
                if (attr != null)
                {
                    // 将方法添加到命令字典
                    _commands[attr.CommandName] = (
                        () => method.Invoke(this, null),
                        attr.Description
                    );
                }
            }
        }
        
        #endregion

        #region 命令方法
        #region 帮助命令
        [DebugCommand("help", Description = "显示帮助信息")]
        private void HelpCommand()
        {
            var helpText = "可用命令:\n";
            foreach (var cmd in _commands)
            {
                helpText += $"{cmd.Key}: {cmd.Value.description}\n";
            }
            Print(helpText);
        }
        #endregion
        #region 游戏核心状态调试命令
        // 重新开始游戏命令
        [DebugCommand("restart", Description = "重新开始游戏")]
        private void RestartCommand()
        {
            GameEvents.TriggerGameStart();
            Print("已重开游戏");
        }

        // 直接胜利命令
        [DebugCommand("win", Description = "直接胜利")]
        private void WinCommand()
        {
            GameEvents.TriggerGameOver(true);
            Print("已触发胜利");
        }

        // 直接失败命令
        [DebugCommand("lose", Description = "直接失败")]
        private void LoseCommand()
        {
            GameEvents.TriggerGameOver(false);
            Print("已触发失败");
        }
        #endregion

        #region UI调试命令
        [DebugCommand("toggleallui", Description = "切换所有UI界面显隐")]
        private void ToggleAllUI()
        {
            bool shouldShow = UIManager.Instance.currentState == UIManager.UIState.None;
            GameEvents.TriggerMenuShow(shouldShow ? UIManager.UIState.MainMenu : UIManager.UIState.None, shouldShow);
            Print($"所有UI已{(shouldShow ? "显示" : "隐藏")}");
        }

        [DebugCommand("togglemainmenu", Description = "切换主菜单显隐")]
        private void ToggleMainMenu()
        {
            bool shouldShow = UIManager.Instance.currentState != UIManager.UIState.MainMenu;
            GameEvents.TriggerMenuShow(shouldShow ? UIManager.UIState.MainMenu : UIManager.UIState.None, shouldShow);
            Print($"主菜单已{(shouldShow ? "显示" : "隐藏")}");
        }

        [DebugCommand("togglepausemenu", Description = "切换暂停菜单显隐")]
        private void TogglePauseMenu()
        {
            bool shouldShow = UIManager.Instance.currentState != UIManager.UIState.PauseMenu;
            GameEvents.TriggerMenuShow(shouldShow ? UIManager.UIState.PauseMenu : UIManager.UIState.None, shouldShow);
            Print($"暂停菜单已{(shouldShow ? "显示" : "隐藏")}");
        }

        [DebugCommand("toggleresultpanel", Description = "切换结算面板显隐")]
        private void ToggleResultPanel()
        {
            bool shouldShow = UIManager.Instance.currentState != UIManager.UIState.ResultPanel;
            GameEvents.TriggerMenuShow(shouldShow ? UIManager.UIState.ResultPanel : UIManager.UIState.None, shouldShow);
            Print($"结算面板已{(shouldShow ? "显示" : "隐藏")}");
        }

        [DebugCommand("togglehud", Description = "切换HUD显隐")]
        private void ToggleHUD()
        {
            bool shouldShow = UIManager.Instance.currentState != UIManager.UIState.HUD;
            GameEvents.TriggerMenuShow(shouldShow ? UIManager.UIState.HUD : UIManager.UIState.None, shouldShow);
            Print($"HUD已{(shouldShow ? "显示" : "隐藏")}");
        }
        #endregion
        
        #endregion

        #region 公共方法
        
        /// <summary>
        /// 当输入命令时调用
        /// </summary>
        public void OnCommandEntered()
        {
            string cmd = inputField.text.Trim().ToLower();
            inputField.text = "";
            
            if (_commands.TryGetValue(cmd, out var command))
            {
                command.action();
            }
            else
            {
                Print("未知命令，输入 help 查看可用命令。");
            }
        }

        /// <summary>
        /// 输出信息到控制台
        /// </summary>
        /// <param name="msg">要输出的消息</param>
        void Print(string msg)
        {
                if (outputText != null) {
                // 分割现有日志为行数组
                var lines = outputText.text.Split('\n');

                
                // 如果超过最大行数，移除最早的行
                if (lines.Length >= MAX_LOG_LINES) {
                    lines = lines.Skip(1).ToArray();
                }
                
                // 添加新日志并重新组合
                outputText.text = string.Join("\n", lines) + $"\n{msg}";
            }
            Debug.Log($"[控制台] {msg}");
        }
        
        #endregion

        #region 控制台图形界面

        /// <summary>
        /// 添加调试按钮
        /// </summary>
        public void AddButton(string buttonName, Action onClick, Transform parent = null)
        {
            if (buttonPrefab == null) return;
            
            var buttonObj = Instantiate(buttonPrefab, parent != null ? parent : transform);
            var button = buttonObj.GetComponent<Button>();
            var text = buttonObj.GetComponentInChildren<Text>();
            
            if (text != null)
                text.text = buttonName;
                
            button.onClick.AddListener(() => {
                onClick?.Invoke();
                Print($"已执行按钮: {buttonName}");
            });
        }

        // 创建UI调试按钮面板
        // 在CreateDebugButtons方法中添加布局组件
        private void CreateDebugButtons()
         {
            // 创建滚动视图
            var scrollView = new GameObject("ButtonPanel");
            var scrollRect = scrollView.AddComponent<ScrollRect>();
            
            // 创建内容容器并添加垂直布局
            var content = new GameObject("Content");
            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 5;
            
            // 创建功能分组
            CreateButtonGroup(content.transform, "游戏控制", CreateGameStateDebugButtons);
            CreateButtonGroup(content.transform, "UI控制", CreateUIDebugButtons);
            // 设置滚动视图
            scrollRect.content = content.GetComponent<RectTransform>();
        }
            /// <summary>
            /// 创建游戏状态调试按钮
            /// </summary>
            private void CreateGameStateDebugButtons(Transform parent)
            {
                AddButton("重新开始", RestartCommand, parent);
                AddButton("直接胜利", WinCommand, parent);
                AddButton("直接失败", LoseCommand, parent);
            }

            /// <summary>
            /// 创建UI调试按钮
            /// </summary>
            private void CreateUIDebugButtons(Transform parent)
            {
                AddButton("切换主菜单", ToggleMainMenu, parent);
                AddButton("切换暂停菜单", TogglePauseMenu, parent);
                AddButton("切换结算面板", ToggleResultPanel, parent);
                AddButton("切换HUD", ToggleHUD, parent);
            }
        // 创建分组容器
        private void CreateButtonGroup(Transform parent, string title, Action<Transform> createButtons) {
            var group = new GameObject(title);
            group.transform.SetParent(parent);
            
            // 添加布局组件
            var layout = group.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(5, 5, 5, 5);
            
            // 创建标题
            var titleText = new GameObject("Title").AddComponent<Text>();
            titleText.text = title;
            titleText.transform.SetParent(group.transform);
            
            // 创建按钮网格
            var buttonGrid = new GameObject("Buttons").AddComponent<GridLayoutGroup>();
            buttonGrid.cellSize = new Vector2(120, 30);
            buttonGrid.spacing = new Vector2(5, 5);
            buttonGrid.transform.SetParent(group.transform);
            
            // 创建按钮
            createButtons(buttonGrid.transform);
        }

        /// <summary>
        /// 切换控制台显示状态
        /// </summary>
        public void ToggleConsole()
        {
            gameObject.SetActive(!gameObject.activeSelf);
            
            if (gameObject.activeSelf)
            {
                inputField.Select();
                inputField.ActivateInputField();
            }
        }

        #endregion
    }
}
