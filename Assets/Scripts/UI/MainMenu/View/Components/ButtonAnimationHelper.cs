using UnityEngine; 
using UnityEngine.UI; 

namespace MyGame.UI.MainMenu.View.Components 
{ 
    /// <summary> 
    /// 按钮动画辅助类，提供触发按钮动画的扩展方法 
    /// </summary> 
    public static class ButtonAnimationHelper 
    { 
        // 动画触发器名称的哈希值，用于性能优化 
        private static readonly int ClickedTriggerHash = Animator.StringToHash("Clicked"); 
        
        /// <summary> 
        /// 触发按钮的点击动画 
        /// </summary> 
        /// <param name="button">要触发动画的按钮</param> 
        public static void TriggerClickAnimation(this Button button) 
        { 
            if (button == null) 
                return; 
            
            // 获取按钮上的Animator组件 
            Animator animator = button.GetComponent<Animator>(); 
            
            if (animator != null && animator.isActiveAndEnabled) 
            { 
                // 触发Click动画trigger 
                animator.SetTrigger(ClickedTriggerHash); 
            } 
        } 
        
        /// <summary> 
        /// 重置按钮的点击动画状态 
        /// </summary> 
        /// <param name="button">要重置动画的按钮</param> 
        public static void ResetClickAnimation(this Button button) 
        { 
            if (button == null) 
                return; 
            
            Animator animator = button.GetComponent<Animator>(); 
            
            if (animator != null && animator.isActiveAndEnabled) 
            { 
                // 重置Click动画trigger 
                animator.ResetTrigger(ClickedTriggerHash); 
            } 
        } 
    } 
}