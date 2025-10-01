using UnityEngine;

namespace MyGame.UI.SaveLoad
{
    /// <summary>
    /// 存档菜单配置类
    /// 用于定义存档菜单的各种配置项
    /// </summary>
    [CreateAssetMenu(fileName = "SaveLoadMenuConfig", menuName = "UI/SaveLoadMenuConfig")]
    public class SaveLoadMenuConfig : ScriptableObject
    {
        [Header("存档配置")]
        [Tooltip("是否启用自动存档")]
        public bool EnableAutoSave = true;
        
        [Tooltip("自动存档间隔（秒）")]
        public float AutoSaveInterval = 300f; // 5分钟
        
        [Tooltip("最大自动存档数量")]
        public int MaxAutoSaveCount = 1;
        
        [Tooltip("最大手动存档数量")]
        public int MaxManualSaveCount = 10;
        
        [Header("UI配置")]
        [Tooltip("存档槽预制件的Addressable Address")]
        public string SaveSlotPrefabAddress = "SaveSlotPrefab";
        
        [Tooltip("存档菜单动画时长")]
        public float MenuAnimationDuration = 0.3f;
        
        [Tooltip("存档槽动画时长")]
        public float SlotAnimationDuration = 0.2f;
        
        [Header("文本配置")]
        [Tooltip("新建游戏确认文本")]
        public string NewGameConfirmText = "确定要创建新游戏吗？这将覆盖当前进度。";
        
        [Tooltip("删除存档确认文本")]
        public string DeleteSaveConfirmText = "确定要删除该存档吗？此操作不可恢复。";
        
        [Tooltip("加载存档确认文本")]
        public string LoadSaveConfirmText = "确定要加载该存档吗？这将覆盖当前进度。";
        
        [Header("音效配置")]
        [Tooltip("选择存档槽音效")]
        public AudioClip SelectSlotSound = null;
        
        [Tooltip("保存游戏音效")]
        public AudioClip SaveGameSound = null;
        
        [Tooltip("加载游戏音效")]
        public AudioClip LoadGameSound = null;
        
        [Tooltip("删除游戏音效")]
        public AudioClip DeleteGameSound = null;
        
        [Tooltip("创建新游戏音效")]
        public AudioClip CreateNewGameSound = null;
        
        [Header("视觉反馈配置")]
        [Tooltip("选中存档槽颜色")]
        public Color SelectedSlotColor = Color.yellow;
        
        [Tooltip("自动存档槽颜色")]
        public Color AutoSaveSlotColor = Color.blue;
        
        [Tooltip("手动存档槽颜色")]
        public Color ManualSaveSlotColor = Color.green;
        
        [Tooltip("空存档槽颜色")]
        public Color EmptySlotColor = Color.gray;
    }
}