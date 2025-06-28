using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MyGame.System;
using MyGame.Managers;

/// <summary>
/// 保存加载管理器，负责游戏数据的保存和加载
/// </summary>
namespace MyGame.Data{
    public class SaveLoad : Singleton<SaveLoad>
    {
        private const string SAVE_FILE_NAME = "gameSaveData.chroan";
        private string saveFilePath;
        private BinaryFormatter binaryFormatter = new();
        private FileStream fileStream;
        public GameProgress gameProgress;
        public SaveData saveData;
        protected override void Awake()
        {
            base.Awake();
            saveFilePath = Path.Combine(Application.streamingAssetsPath, SAVE_FILE_NAME);
            GameEvents.OnSaveRequest += SaveGame;
        }
        public void SaveGame()
        {
            // 获取最新数据
            saveData = SaveData.FromProgress(gameProgress);
            fileStream = new FileStream(saveFilePath, FileMode.Create);
            binaryFormatter.Serialize(fileStream, saveData);
            fileStream.Close();
        }
        public void LoadGame()
        {
            if (File.Exists(saveFilePath))
            {
                fileStream = new FileStream(saveFilePath, FileMode.Open);
                saveData = (SaveData)binaryFormatter.Deserialize(fileStream);
                fileStream.Close();
            }
            else
            {
                Debug.LogWarning("No save file found at path: " + saveFilePath);
            }
        }
    }
}