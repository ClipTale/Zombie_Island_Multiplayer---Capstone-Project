using System;
using UnityEngine;

[Serializable]
public class PlayerStats
{
    [SerializeField] private ScriptableFloat _speed;
    [SerializeField] private ScriptableFloat _maxHealth;
    [SerializeField] private ScriptableFloat _currentHealth;

    public ScriptableFloat Speed => _speed;
    public ScriptableFloat MaxHealth => _maxHealth;
    public ScriptableFloat CurrentHealth => _currentHealth;
}
