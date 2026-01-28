using UnityEngine;
using System;

public class EnemyZ : MonoBehaviour, IDamageable
{
    public float health = 50f;
    public float speed = 2.5f;
    public float damage = 5f;
    public float damageInterval = 1f; // Hasar verme sýklýðý (saniyede bir)

    private Transform player;
    private Rigidbody2D rb;
    private float nextDamageTime; // Bir sonraki hasar ne zaman verilecek?

    public static event Action<Vector3> OnEnemyDeath;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            Vector2 dir = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void TakeDamage(float amount, Vector2 direction, float force)
    {
        health -= amount;
        if (rb != null) rb.AddForce(direction * force, ForceMode2D.Impulse);
        if (health <= 0) Die();
    }

    void Die()
    {
        OnEnemyDeath?.Invoke(transform.position);
        Destroy(gameObject);
    }

    // Çarpýþma devam ettiði sürece her karede kontrol eder
    private void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            // Zamanlayýcý kontrolü: Þu anki zaman, bir sonraki hasar zamanýndan büyük mü?
            if (Time.time >= nextDamageTime)
            {
                PlayerController pc = col.gameObject.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.TakeDamage(damage);
                    // Zamaný güncelle (Þu anki zaman + bekleme süresi)
                    nextDamageTime = Time.time + damageInterval;
                }
            }
        }
    }
}