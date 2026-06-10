using System;
using UnityEngine;

[Serializable]
public class Weapon
{
    [SerializeField] private float _damage;  
    [SerializeField] private float _reloadSpeed;
    [SerializeField] private float _range;
    [SerializeField] private float _bulletSpeed;
    [SerializeField] private float _firingSpeed;
    private float _lastFireTime;

    [SerializeField] private int _magazineSize;
    [SerializeField] private int _currentAmmoInMagazine;

    [SerializeField] public Transform _weaponTransform;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Vector3 _bulletSpawnOffset;

    private float _rotationAngle = 180f;
    private float _mirrorAngle = 90f;

    [SerializeField] private Vector3 _weaponOffset;
    [SerializeField] private bool _isAutomatic = false;

    public float LastFireTime => _lastFireTime;

    public bool IsAutomatic => _isAutomatic;

    public float Damage => _damage;
    public float ReloadSpeed => _reloadSpeed;
    public float Range => _range;
    public float BulletSpeed => _bulletSpeed;

    public float FiringSpeed => _firingSpeed;

    public Vector3 WeaponOffset => _weaponOffset;

    public Vector3 BulletSpawnOffset => _bulletSpawnOffset;

    public int MagazineSize => _magazineSize;
    public int CurrentAmmoInMagazine
    {
        get => _currentAmmoInMagazine;
        set => _currentAmmoInMagazine = value;
    }


    
    public void AimGun()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePosition - _weaponTransform.position;
        direction.z = 0;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, -_rotationAngle, _rotationAngle);
        _weaponTransform.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 originalScale = _weaponTransform.localScale;
        if (angle > _mirrorAngle || angle < -_mirrorAngle)
        {
            _weaponTransform.localScale = new Vector3(originalScale.x, -Mathf.Abs(originalScale.y), originalScale.z);
        }
        else
        {
            _weaponTransform.localScale = new Vector3(originalScale.x, Mathf.Abs(originalScale.y), originalScale.z);
        }
    }

    public void Shoot()
    {
        if (Time.time < _lastFireTime + _firingSpeed || _currentAmmoInMagazine <= 0)
        {
            return;
        }

        Vector3 spawnPosition = _weaponTransform.TransformPoint(_bulletSpawnOffset);
        GameObject bullet = UnityEngine.Object.Instantiate(_bulletPrefab, spawnPosition, _weaponTransform.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.InitializeBullet(_bulletSpeed, _damage, _range);
        }

        _currentAmmoInMagazine--;
        _lastFireTime = Time.time;
    }


}