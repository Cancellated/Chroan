using UnityEngine;
using Logger;

namespace Items.WordBlock
{
    /// <summary>
    /// LETTER文字方块
    /// 继承自WordBlock，实现LETTER特定的文字内容
    /// </summary>
    public class WB_Letter : WordBlock
    {
            
        ///<summary>
        /// 日志模块
        /// </summary>
        private const string LOG_MODULE = LogModules.LETTER;
        
        /// <summary>
        /// 重写文字内容属性
        /// 提供LETTER特定的文字内容
        /// </summary>
        public override string WordContent
        {
            get { return "LETTER"; }
        }
        
        /// <summary>
        /// 初始化
        /// 设置文字类型为名词
        /// </summary>
        protected override void Awake()
        {
            // 设置文字类型为名词
            WordType = WordType.Noun;
            
            // 调用父类的Awake方法
            base.Awake();
            
            Log.Info(LogModules.UTILS, "创建LETTER文字方块");
        }
    }
}