using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.System;
using Level;

namespace MyGame.Control
{
    /// <summary>
    /// 玩家控制器,基于UGUI的事件系统,基于InputSystem的输入系统
    /// </summary>
    public class PlayerController : Singleton<PlayerController>
    {
        #region 字段
        private GameControl _inputActions;
        private Animator _animator;
        public Vector2Int CurrentGridPos { get; private set; }
        private float _moveCooldown = 0.2f;
        private bool _isMoving;
        private LevelManager _levelManager;


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
            _levelManager = FindObjectOfType<LevelManager>();
            _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (_levelManager != null && _levelManager.GridManager != null)
            {
                CurrentGridPos = _levelManager.GridManager.WorldToGridPosition(transform.position);
                Debug.Log("玩家初始网格位置: " + CurrentGridPos);
            }
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void Update()
        {
            HandleMovementInput();
        }

        #endregion

        #region 处理控制
        private void HandleMovementInput()
        {
            if (_isMoving) return;

            Vector2 input = InputActions.GamePlay.Move.ReadValue<Vector2>();
            Vector2Int direction = GetDiscreteDirection(input);

            if (direction != Vector2Int.zero)
            {
                StartCoroutine(MovePlayer(direction));
            }
            //Debug.Log("玩家移动方向:" + direction);
        }

        private IEnumerator MovePlayer(Vector2Int direction)
        {
            // _isMoving = true;

            // Vector2Int targetPos = CurrentGridPos + direction;

            // if (_levelManager.GridManager.CanMoveTo(targetPos))
            // {
            //     // 触发网格移动
            //     _levelManager.GridManager.MoveObject(GetComponent<Player>(), targetPos);

            //     // 更新玩家实际位置（可添加移动动画）
            //     transform.position = _levelManager.GridManager.GridToWorldPosition(targetPos);
            //     CurrentGridPos = targetPos;
            // }

            // yield return new WaitForSeconds(_moveCooldown);
            // _isMoving = false;
            _isMoving = true;

            Vector2Int targetPos = CurrentGridPos + direction;
            Player player = GetComponent<Player>();

            // 创建移动请求事件数据
            ObjectMovedEventData moveData = new ObjectMovedEventData
            {
                Target = player,
                OldPos = CurrentGridPos,
                NewPos = targetPos
            };

            // 发送移动请求事件（不再直接调用MoveObject）
            LevelEvent.TriggerMoveRequest(moveData);
            // 更新玩家实际位置（可添加移动动画）
            transform.position = _levelManager.GridManager.GridToWorldPosition(targetPos);
            CurrentGridPos = targetPos;

            // 等待移动完成（实际移动由GridManager处理）
            yield return new WaitForSeconds(_moveCooldown);
            _isMoving = false;

        }

        private Vector2Int GetDiscreteDirection(Vector2 input)
        {
            // 设置输入阈值避免误触
            const float deadZone = 0.3f;

            if (input.magnitude < deadZone)
            {
                return Vector2Int.zero;
            }

            // 优先判断水平方向输入
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                return input.y > 0 ? Vector2Int.up : Vector2Int.down;
            }
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

        #region 数据共享
        public void SetMoveCoolDown(float value)
        {
            _moveCooldown = value;
        }
        public float GetMoveCoolDown()
        {
            return _moveCooldown;
        }
        #endregion

    }
}