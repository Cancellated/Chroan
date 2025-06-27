using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.System;

namespace MyGame.Control
{
    /// <summary>
    /// 玩家控制器,基于UGUI的事件系统,基于InputSystem的输入系统
    /// </summary>
    public class PlayerController : Singleton<PlayerController>
    {
        #region 字段
        private GameControl _inputActions;
        #endregion

        #region 属性
        /// <summary>
        /// 玩家输入
        /// </summary>
        public GameControl InputActions
        {
            get { return _inputActions; }
        }
        #endregion

        #region 生命周期
        protected override void Awake()
        {
            base.Awake();
            _inputActions = new GameControl();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }
        #endregion

        #region 玩家控制
        /// <summary>
        /// 玩家移动
        /// </summary>
        public Vector2 PlayerMove()
        {
            return _inputActions.GamePlay.Move.ReadValue<Vector2>();
        }
        /// <summary>
        /// 玩家交互
        /// </summary>
        /// <returns></returns>
        public bool PlayerInteract()
        {
            return _inputActions.GamePlay.Interact.triggered;
        }
        /// <summary>
        /// 玩家攻击
        /// </summary>
        /// <returns></returns>
        public bool PlayerAttack()
        {
            return _inputActions.GamePlay.Attack.triggered;
        }
        /// <summary>
        /// 玩家跳跃
        /// </summary>
        /// <returns></returns>
        public bool PlayerJump()
        {
            return _inputActions.GamePlay.Jump.triggered;

        }
        //TodoList:根据玩家控制实现移动,交互,攻击,跳跃等功能
        #endregion

    }
}