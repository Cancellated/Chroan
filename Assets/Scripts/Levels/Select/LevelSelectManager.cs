using UnityEngine;
using UnityEngine.SceneManagement;
using MyGame.System;


namespace MyGame.Managers
{
    public class LevelSelectManager : Singleton<LevelSelectManager>
    {
        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            SaveLoad.Instance.LoadGame();
            GameEvents.OnLevelSelected += LoadSelectedLevel;
        }

        private void OnDestroy()
        {
            GameEvents.OnLevelSelected -= LoadSelectedLevel;
        }
        #endregion

        #region 事件
        /// <summary>
        /// 加载选择的关卡
        /// </summary>
        /// <param name="level"></param>
        private void LoadSelectedLevel(LevelData level)
        {
            GameEvents.TriggerSceneLoad(level.sceneName);
        }
        #endregion
    }
}
