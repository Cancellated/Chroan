using UnityEngine;

namespace MyGame.Core.Utils
{
    /// <summary>
    /// 池化对象组件
    /// 标记对象为池化对象，管理对象生命周期事件
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        #region 字段

        /// <summary>
        /// 对象所属的池
        /// </summary>
        private ObjectPool pool;

        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置对象所属的池
        /// </summary>
        public ObjectPool Pool
        {
            get => pool;
            set => pool = value;
        }

        #endregion

        #region 生命周期事件

        /// <summary>
        /// 当对象从池中获取时调用
        /// </summary>
        public void OnGetFromPool()
        {
            // 可以在这里添加对象激活时的初始化逻辑
            // 例如：重置状态、播放动画等
        }

        /// <summary>
        /// 当对象回收到池中时调用
        /// </summary>
        public void OnReturnToPool()
        {
            // 可以在这里添加对象回收时的清理逻辑
            // 例如：停止动画、重置位置等
        }

        #endregion
    }
}