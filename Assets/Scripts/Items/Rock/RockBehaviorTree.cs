using UnityEngine;
using AI.BehaviorTree;
using AI.Behavior;
using Logger;
using static Logger.Log;

namespace Items.Rock
{

/// <summary>
/// Rock行为树管理器
/// 负责管理Rock对象的行为树逻辑
/// </summary>
public class RockBehaviorTree : MonoBehaviour
    {
        /// <summary>
        /// 行为树执行器
        /// </summary>
        private BehaviorTreeExecutor _treeExecutor;
        
        /// <summary>
        /// 关联的Rock组件
        /// </summary>
        private Rock _rock;
        
        /// <summary>
        /// 行为树根节点
        /// </summary>
        private BTNode _rootNode;
        
        /// <summary>
        /// 逃离行为组件
        /// 负责处理岩石从玩家处逃离的行为
        /// </summary>
        private EscapeComponent _escapeComponent;
        
        /// <summary>
    /// 威胁检测组件
    /// 负责检测和管理威胁源
    /// </summary>
    private ThreatDetectionComponent _threatDetection;
    
    // 行为树配置 - 动画和唤醒逻辑由其他系统控制
    
    /// <summary>
    /// 初始化
    /// </summary>
    private void Awake()
    {
        _rock = GetComponent<Rock>();
        if (_rock == null)
        {
            Log.Error(LogModules.ROCK, "未找到关联的Rock组件", null);
            enabled = false;
            return;
        }
        
        // 初始化行为组件
        InitializeBehaviorComponents();
        
        // 初始化行为树
        InitializeBehaviorTree();
    }
    
    /// <summary>
    /// 初始化行为组件
    /// 设置并配置所有必要的行为组件
    /// </summary>
    private void InitializeBehaviorComponents()
    {
        try
        {
            // 添加威胁检测组件
            _threatDetection = GetComponent<ThreatDetectionComponent>();
            if (_threatDetection == null)
            {
                _threatDetection = gameObject.AddComponent<ThreatDetectionComponent>();
                Log.Info(LogModules.ROCK, "添加ThreatDetectionComponent组件", this);
            }
            _threatDetection.Initialize();
            
            // 添加逃离组件
            _escapeComponent = GetComponent<EscapeComponent>();
            if (_escapeComponent == null)
            {
                _escapeComponent = gameObject.AddComponent<EscapeComponent>();
                Log.Info(LogModules.ROCK, "添加EscapeComponent组件", this);
            }
            _escapeComponent.Initialize(gameObject);
            
            Log.Info(LogModules.ROCK, "岩石行为组件初始化完成", this);
        }
        catch (System.Exception e)
        {
            Log.Error(LogModules.ROCK, $"初始化行为组件时出错: {e.Message}", this);
        }
    }
    
    /// <summary>
    /// 启用时启动行为树
    /// </summary>
    private void OnEnable()
    {
        if (_treeExecutor != null)
        {
            _treeExecutor.Start();
            Log.Info(LogModules.ROCK, "行为树开始执行", null);
        }
    }
    
    /// <summary>
    /// 更新行为树
    /// </summary>
    private void Update()
    {
        _treeExecutor?.Update();
    }
    
    /// <summary>
    /// 禁用时停止行为树
    /// </summary>
    private void OnDisable()
    {
        if (_treeExecutor != null)
        {
            _treeExecutor.Stop();
            Log.Info(LogModules.ROCK, "行为树停止执行", null);
        }
    }
    
    /// <summary>
    /// 初始化行为树
    /// </summary>
    private void InitializeBehaviorTree()
    {
        // 创建根节点（选择器）
        _rootNode = new BTSelector("RockBehaviorSelector");
        
        // 创建行为树执行器
        _treeExecutor = new BehaviorTreeExecutor(_rootNode, "RockBehaviorTree", 0.1f);
        
        // 设置状态转换条件
        SetupStateTransitions();
        
        Log.Info(LogModules.ROCK, "行为树初始化完成", null);
    }
    
    /// <summary>
    /// 设置状态转换条件和行为
    /// </summary>
    private void SetupStateTransitions()
    {
        // 沉睡状态序列
        BTSequence sleepingSequence = new("SleepingSequence");
        sleepingSequence.AddChild(new BTConditionNode(
            () => _rock.CurrentState == Rock.RockState.Sleeping,
            "IsSleeping"
        ));
        sleepingSequence.AddChild(new BTActionNode(
            PerformSleepingBehavior,
            "SleepingBehavior"
        ));
        
        // 苏醒状态序列
        BTSequence awakeSequence = new("AwakeSequence");
        awakeSequence.AddChild(new BTConditionNode(
            () => _rock.CurrentState == Rock.RockState.Awake,
            "IsAwake"
        ));
        awakeSequence.AddChild(new BTActionNode(
            PerformAwakeBehavior,
            "AwakeBehavior"
        ));
        
        // 添加到根选择器
        _rootNode.AddChild(sleepingSequence);
        _rootNode.AddChild(awakeSequence);
        
        // 注意：唤醒逻辑由其他系统控制，不再通过行为树实现
    }

    /// <summary>
    /// 执行沉睡状态的行为
    /// </summary>
    /// <returns>执行状态</returns>
    private BTNodeState PerformSleepingBehavior()
    {
        // 执行基础的待机检查
        CheckStandByConditions();
        
        // 沉睡状态是一个持续运行的行为
        return BTNodeState.Running;
    }
    
    /// <summary>
    /// 执行苏醒状态的行为
    /// </summary>
    /// <returns>执行状态</returns>
    private BTNodeState PerformAwakeBehavior()
    {
        try
        {
            // 苏醒状态的行为逻辑
            CheckAwakeConditions();
            // 苏醒状态是一个持续运行的行为
            return BTNodeState.Running;
        }
        catch (System.Exception e)
        {
            Log.Error(LogModules.ROCK, $"执行苏醒行为时出错: {e.Message}", this);
            return BTNodeState.Failure;
        }
    }
    
    /// <summary>
    /// 检查苏醒条件和执行苏醒状态的行为
    /// </summary>
    private void CheckAwakeConditions()
        {
            try
            {
                // 确保岩石处于正确的物理状态
                if (_rock.CurrentState == Rock.RockState.Awake && _rock.Rigidbody != null)
                {
                    // 设置威胁源为玩家（如果存在）
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null && _threatDetection != null)
                    {
                        // 确保组件已初始化
                        _threatDetection.SetThreatSource(player);
                        
                        // 更新威胁源信息 - 只传递玩家对象，避免参数不匹配问题
                        _threatDetection.UpdateThreatSource();
                        
                        // 检查是否应该执行逃离行为
                        if ((_threatDetection.ShouldTriggerEscape()) && _escapeComponent != null)
                        {
                            // 确保状态正确
                            if (_escapeComponent.CanExecute())
                            {
                                LogWithCooldown(LogLevel.Info, LogModules.ROCK, "准备执行逃跑行为", this);
                                // 执行逃跑行为
                                _escapeComponent.Execute();
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Log.Error(LogModules.ROCK, $"检查苏醒条件时出错: {e.Message}", this);
            }
        }
        
        /// <summary>
        /// 检查待机条件
        /// </summary>
        /// <returns>条件满足返回true</returns>
        private bool CheckStandByConditions()
        {
            // 确保岩石处于正确的物理状态
            // 注意：物理状态的设置已经由Rock类的SetPhysicsProperties方法处理
            // 这里只做日志记录
            if (_rock.CurrentState == Rock.RockState.Sleeping && _rock.Rigidbody != null)
            {
                Log.Debug(LogModules.ROCK, "确认岩石处于沉睡状态的物理设置", null);
            }
            
            return true;
        }
    }
}