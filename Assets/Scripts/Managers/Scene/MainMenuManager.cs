using UnityEngine;
using UnityEngine.SceneManagement;
using MyGame.System;


namespace MyGame.Managers
{
    public class MainMenuManager : Singleton<MainMenuManager>
    {
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private string levelSelectScene = "LevelSelect";

        #region 生命周期
        //进入主菜单和章节选择界面
        protected override void Awake()
        {
            base.Awake();
            GameEvents.OnGameStart += LoadMainMenu;
            GameEvents.OnChapterSelectMap += LoadChapter;
        }
        private void OnDestroy()
        {
            GameEvents.OnGameStart -= LoadMainMenu;
            GameEvents.OnChapterSelectMap -= LoadChapter;
        }
        #endregion

        #region 事件
        //GameManager进入游戏自动触发事件GameStart，MainMenuManager监听事件，触发场景加载主菜单。
        private void LoadMainMenu()
        {
            SceneSwitcher.Instance.LoadSceneAsync(mainMenuScene);
        }
        //LevelSelectManager监听事件，触发场景加载章节选择界面。
        private void LoadChapter()
        {
            SceneSwitcher.Instance.LoadSceneAsync(levelSelectScene);
        }
        #endregion

    }
}
