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
    [Header("移动区域限制")]
    [SerializeField] public MovementRestriction _movementRestriction;
    
    [Header("能否粘合")]
    [SerializeField] public bool _isSticky;


    public PropObject _propObject;//其父物体
    public float MoveInterval => _moveInterval;
    public int SafeDistance => _safeDistance;
    public MovementRestriction MovementRestriction => _movementRestriction;

    public Sprite DisplaySprite => _displaySprite;

    public IPropBehavior CreateBehavior(Transform parent)
    {
        var instance = Instantiate(_prefab, parent).GetComponent<ConcretePropBehavior>();
        instance.Initialize(this); // 注入配置
        return instance;
    }
    public void SetParentProp(PropObject propObject)
    {
        _propObject = propObject;
    }
}

[System.Serializable]
public class MovementRestriction {
    public bool useAreaRestriction;
    public List<Vector2Int> allowedPositions;
}
