using System;
using UnityEngine;

[Serializable]
public class EnemyStats
{
    [SerializeField] private ScriptableFloat _damage;
    [SerializeField] private ScriptableFloat _speed;
    [SerializeField] private ScriptableFloat _maxHealth;
    [SerializeField] private ScriptableFloat _currentHealth;

    public ScriptableFloat Damage => _damage;
    public ScriptableFloat Speed => _speed;
    public ScriptableFloat MaxHealth => _maxHealth;
    public ScriptableFloat CurrentHealth => _currentHealth;

 
}
