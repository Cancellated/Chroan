using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "PropBehavior", menuName = "Game/Prop Behavior")]

public class PropBehaviorSO : ScriptableObject
{
    public ObjectType Type;
    [SerializeField] private GameObject _prefab; // 预制体
    [SerializeField] private Sprite _displaySprite; // 显示精灵
    [Header("移动设置,针对Rock")]
    [SerializeField] private float _moveInterval = 0.5f;
    [SerializeField] private int _safeDistance = 3;
    
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