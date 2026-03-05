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
        public const string UTILS = "Utils";

        // UI模块
        public const string UI = "UI";
        public const string MAINMENU = "MainMenu";
        public const string SETTINGS = "Settings";
        public const string ABOUT = "About";
        public const string PAUSEMENU = "PauseMenu";
        public const string HUD = "HUD";
        public const string UI_COMPONENTS = "UIComponents";
        public const string DIALOGUE = "Dialogue";
        

        // 游戏数据模块
        public const string GAMEDATA = "GameData";
        public const string SAVE = "Save";

        // 调试模块
        public const string DEVTOOLS = "DevTools";

        // 游戏逻辑模块
        public const string PLAYER = "Player";
        public const string AUDIO = "Audio";
        public const string INVENTORY = "Inventory";

        // 地图模块
        public const string TILEMAP = "TileMap";

        // 箱子模块
        public const string BOX = "Box";

        // 智能体模块
        public const string AI = "AI";
        public const string CREATURE = "Creature";

        //游戏物体模块
        public const string WORD_BLOCK = "WordBlock";
        public const string ROCK = "Rock";
        public const string GLASS = "Glass";
        public const string LETTER = "Letter";
        public const string ALIVE = "Alive";
        public const string IS = "Is";

    }
}