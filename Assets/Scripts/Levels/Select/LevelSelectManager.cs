using UnityEngine;
using UnityEngine.SceneManagement;
using MyGame.System;
using MyGame.Data;


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
        public void LoadSelectedLevel(LevelData level)
        {
            var progress = GameManager.Instance.GetGameProgress();
            
            // 根据关卡解锁状态加载不同贴图
            if (progress.levelProgressDict.TryGetValue(level.levelIndex - 1, out bool isCompleted))
            {
                level.previewImage = isCompleted ? 
                    level.completedPreview : 
                    level.uncompletedPreview;
            }
            
            GameEvents.TriggerSceneLoad(level.sceneName);
        }
        #endregion
    }
}
