using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _damage;
    private float _lifeTime; 

    public void InitializeBullet(float speed, float damage, float range)
    {
        _damage = damage;
        _lifeTime = range / speed; 
        Destroy(gameObject, _lifeTime);

      
        GetComponent<Rigidbody2D>().velocity = transform.right * speed;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
      
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.ApplyDamageServerRpc(Mathf.RoundToInt(_damage));
        }

      
        if (other.CompareTag("GroundGun") || other.CompareTag("Player"))
        {
            return; 
        }

       
        Destroy(gameObject);
    }
}