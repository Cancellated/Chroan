using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger;

namespace MyGame.Data
{
    /// <summary>
    /// 基于JSON文件的存档系统实现。
    /// 提供游戏数据的JSON序列化和文件存储功能。
    /// </summary>
    public class JsonSaveSystem : ISaveSystem
    {
        private const string LOG_MODULE = "SaveSystem";
        private const string SAVE_FILE_EXTENSION = ".json";
        private const string SAVE_FOLDER_NAME = "Saves";
        
        private readonly string m_saveDirectoryPath;
        
        /// <summary>
        /// 初始化JSON存档系统。
        /// </summary>
        public JsonSaveSystem()
        {
            // 使用Unity持久化数据路径作为存档根目录
            m_saveDirectoryPath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER_NAME);
            
            // 确保存档文件夹存在
            if (!Directory.Exists(m_saveDirectoryPath))
            {
                Directory.CreateDirectory(m_saveDirectoryPath);
                Log.Info(LOG_MODULE, $"创建存档文件夹: {m_saveDirectoryPath}");
            }
        }
        
        /// <summary>
        /// 获取指定存档槽的完整文件路径。
        /// </summary>
        /// <param name="slotName">存档槽名称。</param>
        /// <returns>存档文件的完整路径。</returns>
        private string GetSaveFilePath(string slotName)
        {
            // 确保存档名称有效
            if (string.IsNullOrEmpty(slotName))
            {
                throw new ArgumentException("存档名称不能为空", nameof(slotName));
            }
            
            // 清理存档名称，移除可能导致文件路径问题的字符
            string sanitizedName = SanitizeFileName(slotName);
            
            // 返回完整的文件路径
            return Path.Combine(m_saveDirectoryPath, sanitizedName + SAVE_FILE_EXTENSION);
        }
        
        /// <summary>
        /// 清理文件名，移除不允许的字符。
        /// </summary>
        /// <param name="fileName">原始文件名。</param>
        /// <returns>清理后的文件名。</returns>
        private string SanitizeFileName(string fileName)
        {
            // 获取系统不允许的文件名字符
            char[] invalidChars = Path.GetInvalidFileNameChars();
            
            // 替换所有不允许的字符为下划线
            StringBuilder sb = new(fileName);
            foreach (char invalidChar in invalidChars)
            {
                sb.Replace(invalidChar, '_');
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 同步保存游戏数据到文件。
        /// </summary>
        /// <param name="saveData">要保存的游戏数据。</param>
        /// <param name="slotName">存档槽名称。</param>
        /// <returns>保存操作是否成功。</returns>
        public bool SaveGame(SaveData saveData, string slotName)
        {
            try
            {
                // 检查数据有效性
                if (saveData == null)
                {
                    Log.Error(LOG_MODULE, "保存失败：存档数据为空");
                    return false;
                }
                
                // 更新存档元数据
                saveData.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                saveData.version = Application.version;
                
                // 序列化数据为JSON
                string jsonData = JsonUtility.ToJson(saveData, true); // true表示格式化输出
                
                // 写入文件
                string filePath = GetSaveFilePath(slotName);
                File.WriteAllText(filePath, jsonData, Encoding.UTF8);
                
                Log.Info(LOG_MODULE, $"游戏数据已保存到: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LOG_MODULE, $"保存游戏失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 异步保存游戏数据到文件。
        /// </summary>
        /// <param name="saveData">要保存的游戏数据。</param>
        /// <param name="slotName">存档槽名称。</param>
        /// <returns>表示保存操作的任务。</returns>
        public async Task<bool> SaveGameAsync(SaveData saveData, string slotName)
        {
            // 使用Task.Run在后台线程执行文件操作
            return await Task.Run(() => SaveGame(saveData, slotName));
        }
        
        /// <summary>
        /// 同步加载游戏数据。
        /// </summary>
        /// <param name="slotName">存档槽名称。</param>
        /// <returns>加载的游戏数据，如果失败则返回null。</returns>
        public SaveData LoadGame(string slotName)
        {
            try
            {
                string filePath = GetSaveFilePath(slotName);
                
                // 检查文件是否存在
                if (!File.Exists(filePath))
                {
                    Log.Warning(LOG_MODULE, $"存档文件不存在: {filePath}");
                    return null;
                }
                
                // 读取文件内容
                string jsonData = File.ReadAllText(filePath, Encoding.UTF8);
                
                // 反序列化JSON数据
                SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
                
                Log.Info(LOG_MODULE, $"游戏数据已从: {filePath} 加载");
                return saveData;
            }
            catch (Exception ex)
            {
                Log.Error(LOG_MODULE, $"加载游戏失败: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 异步加载游戏数据。
        /// </summary>
        /// <param name="slotName">存档槽名称。</param>
        /// <returns>表示加载操作的任务。</returns>
        public async Task<SaveData> LoadGameAsync(string slotName)
        {
            // 使用Task.Run在后台线程执行文件操作
            return await Task.Run(() => LoadGame(slotName));
        }
        
        /// <summary>
        /// 删除指定存档。
        /// </summary>
        /// <param name="slotName">存档槽名称。</param>
        /// <returns>删除操作是否成功。</returns>
        public bool DeleteGame(string slotName)
        {
            try
            {
                string filePath = GetSaveFilePath(slotName);
                
                // 检查文件是否存在
                if (!File.Exists(filePath))
                {
                    Log.Warning(LOG_MODULE, $"要删除的存档文件不存在: {filePath}");
                    return false;
                }
                
                // 删除文件
                File.Delete(filePath);
                
                Log.Info(LOG_MODULE, $"存档已删除: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(LOG_MODULE, $"删除存档失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 检查指定存档是否存在。
        /// </summary>
        /// <param name="slotName">存档槽名称。</param>
        /// <returns>存档是否存在。</returns>
        public bool DoesSaveExist(string slotName)
        {
            try
            {
                string filePath = GetSaveFilePath(slotName);
                return File.Exists(filePath);
            }
            catch (Exception ex)
            {
                Log.Error(LOG_MODULE, $"检查存档存在性失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 获取所有可用的存档槽列表。
        /// </summary>
        /// <returns>存档槽名称列表。</returns>
        public List<string> GetAvailableSaves()
        {
            List<string> saveSlots = new();
            
            try
            {
                // 检查存档文件夹是否存在
                if (!Directory.Exists(m_saveDirectoryPath))
                {
                    return saveSlots;
                }
                
                // 获取所有符合扩展名的文件
                string[] files = Directory.GetFiles(m_saveDirectoryPath, "*" + SAVE_FILE_EXTENSION);
                
                // 处理文件列表，提取存档槽名称
                foreach (string file in files)
                {
                    // 获取文件名（不含扩展名）作为存档槽名称
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    saveSlots.Add(fileName);
                }
            }
            catch (Exception ex)
            {
                Log.Error(LOG_MODULE, $"获取可用存档列表失败: {ex.Message}");
            }
            
            return saveSlots;
        }
    }
}