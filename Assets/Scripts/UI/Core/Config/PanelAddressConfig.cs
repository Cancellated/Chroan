using UnityEngine;
using System.Collections.Generic;

namespace MyGame.UI.Core.Config
{
    /// <summary>
    /// 面板地址配置类
    /// 用于管理UI面板类型与Addressable资源地址的映射关系
    /// 通过ScriptableObject实现可在编辑器中配置的配置表
    /// </summary>
    [CreateAssetMenu(fileName = "PanelAddressConfig", menuName = "UI/PanelAddressConfig")]
    public class PanelAddressConfig : ScriptableObject
    {
        /// <summary>
        /// 面板地址映射结构，用于在Inspector中可视化编辑
        /// </summary>
        [System.Serializable]
        public struct PanelAddressMapping
        {
            [Tooltip("UI面板类型")]
            public UIType panelType;
        
            [Tooltip("Addressable资源地址")]
            public string addressableAddress;
        }
        
        [Header("面板地址映射列表")]
        [Tooltip("定义所有UI面板类型与Addressable资源地址的映射关系")]
        [SerializeField]
        private List<PanelAddressMapping> _panelMappings = new();
        
        /// <summary>
        /// 获取面板地址映射字典
        /// </summary>
        /// <returns>面板类型与地址的映射字典</returns>
        public Dictionary<UIType, string> GetAddressMap()
        {
            Dictionary<UIType, string> map = new();
            
            foreach (var mapping in _panelMappings)
            {
                if (!map.ContainsKey(mapping.panelType) && !string.IsNullOrEmpty(mapping.addressableAddress))
                {
                    map.Add(mapping.panelType, mapping.addressableAddress);
                }
            }
            
            return map;
        }
        
        /// <summary>
        /// 检查是否包含指定面板类型的映射
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>是否包含映射</returns>
        public bool ContainsPanel(UIType panelType)
        {
            foreach (var mapping in _panelMappings)
            {
                if (mapping.panelType == panelType && !string.IsNullOrEmpty(mapping.addressableAddress))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 获取指定面板类型的Addressable地址
        /// </summary>
        /// <param name="panelType">面板类型</param>
        /// <returns>Addressable资源地址，如果不存在则返回null</returns>
        public string GetAddressForPanel(UIType panelType)
        {
            foreach (var mapping in _panelMappings)
            {
                if (mapping.panelType == panelType)
                {
                    return mapping.addressableAddress;
                }
            }
            return null;
        }
    }
}