using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using Logger;
using MyGame.Managers;

namespace MyGame.UI.MainMenu.View.Components
{
    /// <summary>
    /// 主菜单按钮选择管理器，处理键盘导航和焦点选择
    /// </summary>
    public class MainMenuButtonIndicator : MonoBehaviour
    {
        [Tooltip("选择指示器对象")]
        [SerializeField] private GameObject m_selectionIndicator;

        [Tooltip("垂直输入灵敏度")]
        [SerializeField] private float m_inputSensitivity = 0.2f;

        [Tooltip("指示器与按钮之间的水平偏移量")]
        [SerializeField] private float m_horizontalOffset = 32f; // 默认偏移32像素到左边

        [Tooltip("指示器与按钮之间的垂直偏移量")]
        [SerializeField] private float m_verticalOffset = -10f; // 默认偏移10像素到下面

        private int m_selectedButtonIndex = 0;
        private float m_lastInputTime = 0f;
        private EventSystem m_eventSystem;
        private bool m_hasInitializedPosition = false; // 标记是否已初始化位置
        private const string LOG_MODULE = LogModules.MAINMENU;

        /// <summary>
        /// 菜单按钮列表（公共访问）
        /// </summary>
        [field: Header("选择器配置")]
        [field: Tooltip("主菜单中的所有按钮")]
        [field: SerializeField]
        public List<Button> MenuButtons { get; } = new List<Button>();

        /// <summary>
        /// 选择指示器（公共访问）
        /// </summary>
        public GameObject SelectionIndicator
        {
            get { return m_selectionIndicator; }
            set { m_selectionIndicator = value; }
        }

        /// <summary>
        /// 当前选中的按钮索引
        /// </summary>
        public int SelectedButtonIndex
        {
            get { return m_selectedButtonIndex; }
            set
            {
                if (MenuButtons.Count == 0) return;

                // 确保索引在有效范围内
                m_selectedButtonIndex = Mathf.Clamp(value, 0, MenuButtons.Count - 1);
                UpdateSelectionIndicator();
                SetButtonFocus(MenuButtons[m_selectedButtonIndex]);
            }
        }

        /// <summary>
        /// 初始化选择器
        /// </summary>
        private void Awake()
        {
            m_eventSystem = EventSystem.current;
            if (m_eventSystem == null)
            {
                Log.Error(LOG_MODULE, "MainMenuButtonSelector: No EventSystem found in scene!");
            }

            // 过滤掉空按钮引用
            MenuButtons.RemoveAll(button => button == null);
        }

        /// <summary>
        /// 当选择器启用时
        /// </summary>
        private void OnEnable()
        {
            
            // 确保有按钮可供选择
            if (MenuButtons.Count > 0)
            {
                SelectedButtonIndex = 0; // 默认选中第一个按钮
                UpdateSelectionIndicator(); // 只确保可见，不修改位置
            }
        }

        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void Update()
        {
            if (MenuButtons.Count == 0 || Time.time - m_lastInputTime < m_inputSensitivity) return;

            // 使用InputSystem获取垂直输入
            float verticalInput = 0f;
            if (InputManager.Instance != null && InputManager.Instance.InputActions != null)
            {
                // 使用Navigate操作的y分量作为垂直输入
                verticalInput = InputManager.Instance.InputActions.UI.Navigate.ReadValue<Vector2>().y;
            }
            
            if (verticalInput > 0f) // W键或上箭头
            {
                m_hasInitializedPosition = true; // 键盘导航时设置为true，开始计算位置
                SelectedButtonIndex--;
                m_lastInputTime = Time.time;
            }
            else if (verticalInput < 0f) // S键或下箭头
            {
                m_hasInitializedPosition = true; // 键盘导航时设置为true，开始计算位置
                SelectedButtonIndex++;
                m_lastInputTime = Time.time;
            }

            // 处理确认选择
            if (InputManager.Instance != null && InputManager.Instance.InputActions != null)
            {
                if (InputManager.Instance.InputActions.UI.Submit.triggered)
                {
                    if (MenuButtons.Count > 0 && MenuButtons[m_selectedButtonIndex] != null)
                    {
                        MenuButtons[m_selectedButtonIndex].onClick.Invoke();
                    }
                }
            }
            else
            {
                // 降级处理：使用旧的输入系统
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    if (MenuButtons.Count > 0 && MenuButtons[m_selectedButtonIndex] != null)
                    {
                        MenuButtons[m_selectedButtonIndex].onClick.Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// 更新选择指示器状态和位置
        /// </summary>
        private void UpdateSelectionIndicator()
        {
            if (m_selectionIndicator == null || MenuButtons.Count == 0 || MenuButtons[m_selectedButtonIndex] == null) return;

            // 确保指示器可见
            m_selectionIndicator.SetActive(true);
            
            // 仅在键盘导航时计算位置，初始化时保持用户手动放置的位置
            if (m_hasInitializedPosition)
            {
                // 获取按钮和指示器的RectTransform
                RectTransform buttonRect = MenuButtons[m_selectedButtonIndex].GetComponent<RectTransform>();
                RectTransform indicatorRect = m_selectionIndicator.GetComponent<RectTransform>();

                if (buttonRect != null && indicatorRect != null)
                {
                    // 计算按钮左侧的位置
                    Vector3 buttonWorldPos = buttonRect.position;
                    float offset = buttonRect.rect.width * 0.5f + indicatorRect.rect.width * 0.5f + m_horizontalOffset;
                    Vector3 indicatorWorldPos = new(buttonWorldPos.x - offset, buttonWorldPos.y + m_verticalOffset, buttonWorldPos.z);
                    // 直接设置位置（不使用平滑移动）
                    indicatorRect.position = indicatorWorldPos;
                }
            }
            
        }

        /// <summary>
        /// 设置按钮焦点
        /// </summary>
        /// <param name="button">要设置焦点的按钮</param>
        private void SetButtonFocus(Button button)
        {
            if (m_eventSystem != null && button != null)
            {
                m_eventSystem.SetSelectedGameObject(button.gameObject);
            }
        }

        /// <summary>
        /// 添加按钮到选择列表
        /// </summary>
        /// <param name="button">要添加的按钮</param>
        public void AddButton(Button button)
        {
            if (button != null && !MenuButtons.Contains(button))
            {
                MenuButtons.Add(button);
                // 添加按钮点击事件监听
                button.onClick.AddListener(() => OnButtonClicked(button));
            }
        }

        /// <summary>
        /// 处理按钮点击事件，更新指示器位置
        /// </summary>
        /// <param name="clickedButton">被点击的按钮</param>
        private void OnButtonClicked(Button clickedButton)
        {
            if (MenuButtons.Contains(clickedButton))
            {
                // 获取被点击按钮在列表中的索引
                int clickedButtonIndex = MenuButtons.IndexOf(clickedButton);
                if (clickedButtonIndex != -1)
                {
                    // 更新选中索引并设置位置已初始化标志
                    m_hasInitializedPosition = true;
                    SelectedButtonIndex = clickedButtonIndex;
                }
            }
        }

        /// <summary>
        /// 移除按钮从选择列表
        /// </summary>
        /// <param name="button">要移除的按钮</param>
        public void RemoveButton(Button button)
        {
            if (button != null && MenuButtons.Contains(button))
            {
                // 移除按钮点击事件监听
                button.onClick.RemoveListener(() => OnButtonClicked(button));
                MenuButtons.Remove(button);
                // 如果移除的是当前选中的按钮，则重新选择一个
                if (m_selectedButtonIndex >= MenuButtons.Count && MenuButtons.Count > 0)
                {
                    SelectedButtonIndex = MenuButtons.Count - 1;
                }
            }
        }
    }
}