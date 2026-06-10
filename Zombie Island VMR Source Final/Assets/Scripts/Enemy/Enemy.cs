using UnityEngine;
using System;
using System.Linq;
using Unity.Netcode;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private EnemyStats _stats;
    private NetworkVariable<float> _currentHealth = new NetworkVariable<float>();
    private Transform _targetPlayer;
    private Rigidbody2D _rigidbody2D;

    public event Action<GameObject> OnEnemyKilled;

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        if (_rigidbody2D == null)
        {
            Debug.LogError("No Rigidbody2D component found on this GameObject. Please add a Rigidbody2D component.");
        }

        if (IsServer)
        {
            _currentHealth.Value = _stats.MaxHealth.Value;
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        UpdateTargetPlayer();

        if (_targetPlayer != null)
        {
            MoveTowardsPlayer();
        }
    }

    private void UpdateTargetPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length > 0)
        {
            _targetPlayer = players
                .Select(player => player.transform)
                .OrderBy(playerTransform => Vector3.Distance(playerTransform.position, transform.position))
                .FirstOrDefault();
        }
    }

    private void MoveTowardsPlayer()
    {
        if (_targetPlayer == null) return;

        Vector2 direction = (_targetPlayer.position - transform.position).normalized;
        Vector2 newPosition = (Vector2)transform.position + direction * _stats.Speed.Value * Time.fixedDeltaTime;

        _rigidbody2D.MovePosition(newPosition);
    }

    public void SetMaxHealth(float maxHealth)
    {
        if (IsServer)
        {
            _stats.MaxHealth.Value = maxHealth;
            _currentHealth.Value = maxHealth;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyDamageServerRpc(int damage)
    {
        TakeDamage(damage);
    }

    private void TakeDamage(int damage)
    {
        if (IsServer)
        {
            _currentHealth.Value -= damage;
            if (_currentHealth.Value <= 0)
            {
                OnEnemyKilled?.Invoke(gameObject);
                DieServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DieServerRpc()
    {
        DieClientRpc();
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        HandleDeath();
    }

    private void HandleDeath()
    {
        Debug.Log("Enemy died.");
       
        Destroy(gameObject);
    }

    public void IncreaseMaxHealth(int increment)
    {
        if (IsServer)
        {
            _stats.MaxHealth.Value += increment;
            _currentHealth.Value = _stats.MaxHealth.Value;
        }
    }

    public float GetDamage()
    {
        return _stats.Damage.Value;
    }
}
