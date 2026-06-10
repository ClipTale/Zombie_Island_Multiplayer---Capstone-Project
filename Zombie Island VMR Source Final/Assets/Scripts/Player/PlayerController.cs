using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    private GameManager gameManager;
    [SerializeField] private Weapon _weapon;
    [SerializeField] private PlayerStats _stats;

    private Rigidbody2D _rb;
    private Vector2 _moveDirection;

    private bool _isReloading;
    private float _reloadStartTime;

    [SerializeField] private float _invincibleFrames = 0.5f;
    private bool _isImmune;

    [SerializeField] private Image _healthBarImage;
    [SerializeField] private Image _healthRegenBarImage;
    [SerializeField] private float _healthRegenAmountPerFrame = 10f;
    [SerializeField] private float _healthRegenTimeToMaxHealth = 5f;
    private float _accumulatedRegenedHealth;
    [SerializeField] private float _healthToRegen = 30f;
    private Coroutine _healthRegenCoroutine;

    public NetworkVariable<float> NetworkHealth = new NetworkVariable<float>();
    public NetworkVariable<float> NetworkMaxHealth = new NetworkVariable<float>();
    public NetworkVariable<float> NetworkAccumulatedRegenedHealth = new NetworkVariable<float>();

    private void Awake()
    {
        /*Vais buscar os stats ao scriptable object*/
        _stats.CurrentHealth.Value = _stats.MaxHealth.Value;
        NetworkHealth.Value = _stats.CurrentHealth.Value;
        NetworkMaxHealth.Value = _stats.MaxHealth.Value;
        /*vai buscar o GameManager Object e vai buscar as barras de vida*/
        gameManager = GameObject.FindObjectOfType<GameManager>();
        _healthBarImage = GameObject.FindGameObjectWithTag("HealthBar").GetComponent<Image>();
        _healthRegenBarImage = GameObject.FindGameObjectWithTag("RegenBar").GetComponent<Image>();
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _weapon.AimGun(); // Initialize aiming for the initial weapon
    }

    private void LogAmmoInfo()
    {
        Debug.Log($"Current ammo in magazine: {_weapon.CurrentAmmoInMagazine}");
    }

    private void Update()
    {
        /*Quando a vida chegar a 0, destroi o gameobject dos players e inicia a funçao onDeath*/
        if (_stats.CurrentHealth.Value <= 0)
        {
            Destroy(gameObject);
            gameManager.onDeath();
        }

        if (!IsOwner) return;

        ProcessInputs();
        _weapon.AimGun(); // Always aim the current weapon

        if (_weapon.IsAutomatic)
        {
            if (Input.GetMouseButton(0) && Time.time >= _weapon.FiringSpeed + _weapon.LastFireTime)
            {
                _weapon.Shoot();
                LogAmmoInfo();
                if (_isReloading && _weapon.CurrentAmmoInMagazine > 0)
                {
                    _isReloading = false;
                    Debug.Log("Reload interrupted by shooting.");
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                _weapon.Shoot();
                LogAmmoInfo();
                if (_isReloading && _weapon.CurrentAmmoInMagazine > 0)
                {
                    _isReloading = false;
                    Debug.Log("Reload interrupted by shooting.");
                }
            }
        }

        if (_weapon.CurrentAmmoInMagazine == 0 && !_isReloading)
        {
            StartReload();
        }

        if (Input.GetKeyDown(KeyCode.R) && !_isReloading)
        {
            StartReload();
        }

        if (_isReloading)
        {
            float reloadTimeElapsed = Time.time - _reloadStartTime;
            if (reloadTimeElapsed >= _weapon.ReloadSpeed)
            {
                FinishReload();
            }
        }
        /*caso receba dano, começa a couroutine de regeneraçao*/
        if (_stats.CurrentHealth.Value < _stats.MaxHealth.Value && _healthRegenCoroutine == null)
        {
            _healthRegenCoroutine = StartCoroutine(HealthRegenCoroutine());
        }

        /*da update as variaveis*/
        NetworkHealth.Value = _stats.CurrentHealth.Value;
        NetworkMaxHealth.Value = _stats.MaxHealth.Value;
        NetworkAccumulatedRegenedHealth.Value = _accumulatedRegenedHealth;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void ProcessInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        _moveDirection = new Vector2(moveX, moveY).normalized;
    }

    private void Move()
    {
        _rb.velocity = _moveDirection * _stats.Speed.Value;
    }

    private void StartReload()
    {
        if (_weapon.CurrentAmmoInMagazine < _weapon.MagazineSize)
        {
            _isReloading = true;
            _reloadStartTime = Time.time;
            Debug.Log("Reloading...");
        }
    }

    private void FinishReload()
    {
        _isReloading = false;
        _weapon.CurrentAmmoInMagazine = _weapon.MagazineSize; // Refill magazine to full
        Debug.Log("Reloaded. Magazine refilled.");
    }

    private IEnumerator HandleImmunity()
    {
        _isImmune = true;
        yield return new WaitForSeconds(_invincibleFrames);
        _isImmune = false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (!_isImmune)
            {
                float damage = collision.gameObject.GetComponent<Enemy>().GetDamage();
                ApplyDamage(damage);
            }
        }
    }

    private void ApplyDamage(float damage)
    {
        /*retira a vida ao ir buscar o dano do enemy e retira a vida a regenerar que esta a acumular*/
        _stats.CurrentHealth.Value -= damage;
        _accumulatedRegenedHealth = 0f;
        StartCoroutine(HandleImmunity());
        /*serve para atualizar tambem para o cliente*/
        UpdateHealthUIClientRpc(NetworkHealth.Value, NetworkMaxHealth.Value, NetworkAccumulatedRegenedHealth.Value);
    }

    private IEnumerator HealthRegenCoroutine()
    {
        while (_stats.CurrentHealth.Value < _stats.MaxHealth.Value)
        {
            float regenRate = _healthRegenAmountPerFrame / _healthRegenTimeToMaxHealth;
            _accumulatedRegenedHealth += regenRate * Time.deltaTime;
            if (_accumulatedRegenedHealth >= _healthToRegen)
            {
                
                _stats.CurrentHealth.Value += _healthRegenAmountPerFrame;
                _accumulatedRegenedHealth -= _healthToRegen;
                /*atualiza os values para o cliente*/
                UpdateHealthUIClientRpc(NetworkHealth.Value, NetworkMaxHealth.Value, NetworkAccumulatedRegenedHealth.Value);
            }

            _stats.CurrentHealth.Value = Mathf.Clamp(_stats.CurrentHealth.Value, 0f, _stats.MaxHealth.Value);
            UpdateHealthUIClientRpc(NetworkHealth.Value, NetworkMaxHealth.Value, NetworkAccumulatedRegenedHealth.Value);
            yield return null;
        }

        _healthRegenCoroutine = null; 
    }
    /*este client rpc serve para atualizar os dados do cliente quando for chamado*/
    [ClientRpc]
    private void UpdateHealthUIClientRpc(float currentHealth, float maxHealth, float accumulatedRegenedHealth)
    {
        _healthBarImage.fillAmount = currentHealth / maxHealth;
        _healthRegenBarImage.fillAmount = accumulatedRegenedHealth / maxHealth;
    }
}
