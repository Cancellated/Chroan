using UnityEngine;
using Logger;

namespace Items.WordBlock
{
    /// <summary>
    /// ALIVE文字方块
    /// 继承自WordBlock，实现ALIVE特定的文字内容
    /// </summary>
    public class WB_Alive : WordBlock
    {
        ///<summary>
        /// 日志模块
        /// </summary>
        private const string LOG_MODULE = LogModules.ALIVE;
        
        /// <summary>
        /// 重写文字内容属性
        /// 提供ALIVE特定的文字内容
        /// </summary>
        public override string WordContent
        {
            get { return "ALIVE"; }
        }
        
        /// <summary>
        /// 初始化
        /// 设置文字类型为形容词
        /// </summary>
        protected override void Awake()
        {
            // 设置文字类型为形容词
            WordType = WordType.Adjective;
            
            // 调用父类的Awake方法
            base.Awake();
            
            Log.Info(LogModules.UTILS, "创建ALIVE文字方块");
        }
    }
}