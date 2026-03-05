using UnityEngine;
using Logger;

namespace Items.WordBlock
{
    /// <summary>
    /// IS文字方块
    /// 继承自WordBlock，实现IS特定的文字内容
    /// </summary>
    public class WB_Is : WordBlock
    {
        ///<summary>
        /// 日志模块
        /// </summary>
        private const string LOG_MODULE = LogModules.IS;
        
        /// <summary>
        /// 重写文字内容属性
        /// 提供IS特定的文字内容
        /// </summary>
        public override string WordContent
        {
            get { return "IS"; }
        }
        
        /// <summary>
        /// 初始化
        /// 设置文字类型为动词
        /// </summary>
        protected override void Awake()
        {
            // 设置文字类型为动词
            WordType = WordType.Verb;
            
            // 调用父类的Awake方法
            base.Awake();
            
            Log.Info(LogModules.UTILS, "创建IS文字方块");
        }
    }
}