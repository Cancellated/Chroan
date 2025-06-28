using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "PropBehavior", menuName = "Game/Prop Behavior")]

public class PropBehaviorSO : ScriptableObject
{
    public ObjectType Type;
    [SerializeField] public GameObject _prefab; // 预制体
    [SerializeField] public Sprite _displaySprite; // 显示精灵
    [Header("移动设置,针对Rock")]
    [SerializeField] public float _moveInterval = 0.5f;
    [SerializeField] public int _safeDistance = 3;
    [Header("冰块参数")]
    [SerializeField] public float _growSpeed = 0.2f;
    [SerializeField] public float _maxGrowTime = 2f;

    [Header("邮箱参数")]
    [SerializeField] public int _storyId;


    
    public float MoveInterval => _moveInterval;
    public int SafeDistance => _safeDistance;

    public Sprite DisplaySprite => _displaySprite;

    public IPropBehavior CreateBehavior()
    {
        var instance = Instantiate(_prefab).GetComponent<ConcretePropBehavior>();
        instance.Initialize(this); // 注入配置
        return instance;
    }
}