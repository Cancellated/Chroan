using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyGame.Data
{
    /// <summary>
    /// 存档系统接口，定义游戏数据的保存和加载操作。
    /// 遵循单一职责原则，为不同存储实现提供统一接口。
    /// </summary>
    public interface ISaveSystem
    {
        /// <summary>
        /// 保存游戏数据到指定路径。
        /// </summary>
        /// <param name="saveData">要保存的游戏数据对象。</param>
        /// <param name="slotName">存档槽名称，用于区分不同存档。</param>
        /// <returns>保存操作是否成功。</returns>
        bool SaveGame(SaveData saveData, string slotName);
        
        /// <summary>
        /// 异步保存游戏数据到指定路径。
        /// </summary>
        /// <param name="saveData">要保存的游戏数据对象。</param>
        /// <param name="slotName">存档槽名称，用于区分不同存档。</param>
        /// <returns>表示保存操作的任务，结果为是否成功。</returns>
        Task<bool> SaveGameAsync(SaveData saveData, string slotName);
        
        /// <summary>
        /// 从指定路径加载游戏数据。
        /// </summary>
        /// <param name="slotName">存档槽名称，用于指定要加载的存档。</param>
        /// <returns>加载的游戏数据对象，如果加载失败则返回null。</returns>
        SaveData LoadGame(string slotName);
        
        /// <summary>
        /// 异步从指定路径加载游戏数据。
        /// </summary>
        /// <param name="slotName">存档槽名称，用于指定要加载的存档。</param>
        /// <returns>表示加载操作的任务，结果为游戏数据对象。</returns>
        Task<SaveData> LoadGameAsync(string slotName);
        
        /// <summary>
        /// 删除指定存档槽的游戏数据。
        /// </summary>
        /// <param name="slotName">要删除的存档槽名称。</param>
        /// <returns>删除操作是否成功。</returns>
        bool DeleteGame(string slotName);
        
        /// <summary>
        /// 检查指定存档槽是否存在游戏数据。
        /// </summary>
        /// <param name="slotName">要检查的存档槽名称。</param>
        /// <returns>存档是否存在。</returns>
        bool DoesSaveExist(string slotName);
        
        /// <summary>
        /// 获取所有可用的存档槽信息。
        /// </summary>
        /// <returns>存档槽名称列表。</returns>
        List<string> GetAvailableSaves();
    }
}