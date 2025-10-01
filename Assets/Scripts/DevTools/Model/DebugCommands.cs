using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyGame.Managers;
using MyGame.Events;
using MyGame.UI;
using Logger;

namespace MyGame.DevTool
{
    /// <summary>
    /// 调试命令集，集中管理所有调试命令
    /// </summary>
    public static class DebugCommands
    {
        // 获取控制器实例的辅助方法
        private static DebugConsoleController GetController()
        {
            // 在实际项目中，可以使用依赖注入或单例来获取控制器实例
            // 这里为了简化，假设控制器已经被正确创建并可以通过某种方式获取
            return Object.FindObjectOfType<DebugConsoleController>();
        }
        
        #region 帮助命令
        [DebugCommand("help", Description = "显示帮助信息")]
        internal static void HelpCommand()
        {
            var controller = GetController();
            if (controller == null)
                return;
            
            var helpText = "可用命令:\n";
            var model = new DebugCommandModel();
            model.InitializeCommands();
            
            foreach (var cmd in model.GetAllCommands())
            {
                helpText += $"{cmd.Key}: {cmd.Value}\n";
            }
            
            controller.PrintToConsole(helpText);
        }
        #endregion
        
        #region 游戏核心状态调试命令
        // 重新开始游戏命令
        [DebugCommand("restart", Description = "重新开始游戏")]
        internal static void RestartCommand()
        {
            GameEvents.TriggerGameStart();
            var controller = GetController();
            if (controller != null)
            {
                controller.PrintToConsole("已重开游戏");
            }
        }
        
        // 直接胜利命令
        [DebugCommand("win", Description = "直接胜利")]
        internal static void WinCommand()
        {
            GameEvents.TriggerGameOver(true);
            var controller = GetController();
            if (controller != null)
            {
                controller.PrintToConsole("已触发胜利");
            }
        }
        
        // 直接失败命令
        [DebugCommand("lose", Description = "直接失败")]
        internal static void LoseCommand()
        {
            GameEvents.TriggerGameOver(false);
            var controller = GetController();
            if (controller != null)
            {
                controller.PrintToConsole("已触发失败");
            }
        }
        #endregion
        
        #region UI调试命令
        [DebugCommand("toggleallui", Description = "切换所有UI界面显隐")]
        internal static void ToggleAllUI()
        {
            bool shouldShow = UIManager.Instance.currentState == UIType.None;
            GameEvents.TriggerMenuShow(shouldShow ? UIType.MainMenu : UIType.None, shouldShow);
            
            var controller = GetController();
            if (controller != null)
            {
                controller.PrintToConsole($"所有UI已{(shouldShow ? "显示" : "隐藏")}");
            }
        }
        
        [DebugCommand("togglemainmenu", Description = "切换主菜单显隐")]
        internal static void ToggleMainMenu()
        {
            bool shouldShow = UIManager.Instance.currentState != UIType.MainMenu;
            GameEvents.TriggerMenuShow(shouldShow ? UIType.MainMenu : UIType.None, shouldShow);
            
            var controller = GetController();
            if (controller != null)
            {
                controller.PrintToConsole($"主菜单已{(shouldShow ? "显示" : "隐藏")}");
            }
        }
        
        [DebugCommand("togglepausemenu", Description = "切换暂停菜单显隐")]
        internal static void TogglePauseMenu()
        {
            bool shouldShow = UIManager.Instance.currentState != UIType.PauseMenu;
            GameEvents.TriggerMenuShow(shouldShow ? UIType.PauseMenu : UIType.None, shouldShow);
            
            var controller = GetController();
            if (controller != null)
            {
                controller.PrintToConsole($"暂停菜单已{(shouldShow ? "显示" : "隐藏")}");
            }
        }
        
        [DebugCommand("toggleresultpanel", Description = "切换结算面板显隐")]
        internal static void ToggleResultPanel()
        {
            bool shouldShow = UIManager.Instance.currentState != UIType.ResultPanel;
            GameEvents.TriggerMenuShow(shouldShow ? UIType.ResultPanel : UIType.None, shouldShow);
            
            var controller = GetController();
            if (controller != null)
            {
                controller.PrintToConsole($"结算面板已{(shouldShow ? "显示" : "隐藏")}");
            }
        }
        
        [DebugCommand("togglehud", Description = "切换HUD显隐")]
        internal static void ToggleHUD()
        {
            bool shouldShow = UIManager.Instance.currentState != UIType.HUD;
            GameEvents.TriggerMenuShow(shouldShow ? UIType.HUD : UIType.None, shouldShow);
            
            var controller = GetController();
            if (controller != null)
            {
                controller.PrintToConsole($"HUD已{(shouldShow ? "显示" : "隐藏")}");
            }
        }
        #endregion
    }
}
