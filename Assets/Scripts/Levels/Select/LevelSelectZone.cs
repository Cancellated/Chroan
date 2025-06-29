using System.Collections;
using System.Collections.Generic;
using MyGame.Managers;
using UnityEngine;

public class LevelSelectZone : MonoBehaviour
{
   [SerializeField] private LevelData _targetLevel;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LevelSelectManager.Instance.LoadSelectedLevel(_targetLevel);
        }
    }
}
