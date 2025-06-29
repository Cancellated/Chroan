using UnityEngine;
using UnityEngine.UI;
using MyGame.System;

namespace MyGame.UI
{
    public class MainMenuUIManager : Singleton<MainMenuUIManager>
    {
        #region 字段
        [Header("UI元素引用")]
        public Button startButton;
        public Button settingsButton;
        public Button quitButton;

        #endregion

        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            
            //startButton.onClick.AddListener(() => GameEvents.TriggerChapterSelectMap());
            startButton.onClick.AddListener(() => OnStartButtonClick());
            settingsButton.onClick.AddListener(() => GameEvents.TriggerSettingsShow(true));
            quitButton.onClick.AddListener(() => Application.Quit());
        }

        #endregion

        #region 事件
        private void OnEnable()
        {
            GameEvents.OnMainMenuShow += ShowMainMenu;
            GameEvents.OnSettingsShow += ShowSettings;
        }

        private void OnDisable()
        {
            GameEvents.OnMainMenuShow -= ShowMainMenu;
            GameEvents.OnSettingsShow -= ShowSettings;
        }
        #endregion

        #region 方法
        public void ShowMainMenu(bool show)
        {
            gameObject.SetActive(show);
            Cursor.visible = show;
            Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        }
         public void ShowSettings(bool show)
        {
            settingsButton.gameObject.SetActive(show);
            quitButton.gameObject.SetActive(show);
        }

        public void OnStartButtonClick()
        {
            ShowMainMenu(false);
            GameEvents.TriggerSceneLoad("LevelSelect");
        }
        #endregion
    }
}
