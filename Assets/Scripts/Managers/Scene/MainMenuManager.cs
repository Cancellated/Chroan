using UnityEngine;
using UnityEngine.SceneManagement;
using MyGame.System;


namespace MyGame.Managers
{
    public class MainMenuManager : Singleton<MainMenuManager>
    {
        [SerializeField] private string mainMenuScene = "MainMenu";


        public void LoadMainMenu()
        {
            GameEvents.TriggerSceneLoad(mainMenuScene);
        }

        public void UnloadMainMenu()
        {
            GameEvents.TriggerSceneUnload(mainMenuScene);
        }
    }
}
