/// <summary>
/// 日志的常量模块管理器
/// 为各个模块提供统一的日志标识符
/// </summary>
namespace Logger
{
    public static class LogModules
    {
        // 系统模块
        public const string SYSTEM = "System";
        public const string GAMEMANAGER = "GameManager";
        public const string GAMEEVENTS = "GameEvents";
        public const string INPUT = "Input";
        public const string SCENE = "Scene";
        public const string UIMANAGER = "UIManager";
        public const string DEBUGCONSOLE = "DebugConsole";
        public const string MANAGERBOOTSTRAP = "ManagerBootstrap";
        public const string LOADING = "Loading";

        // UI模块
        public const string UI = "UI";
        public const string MAINMENU = "MainMenu";
        public const string SETTINGS = "Settings";
        public const string ABOUT = "About";
        public const string PAUSEMENU = "PauseMenu";
        public const string HUD = "HUD";
        public const string UI_COMPONENTS = "UIComponents";
        

        // 游戏数据模块
        public const string GAMEDATA = "GameData";
        public const string SAVE = "Save";

        // 调试模块
        public const string DEVTOOLS = "DevTools";

        // 游戏逻辑模块
        public const string PLAYER = "Player";
        public const string AUDIO = "Audio";
        public const string INVENTORY = "Inventory";
    }
}