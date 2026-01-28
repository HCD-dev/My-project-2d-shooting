using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;
    public float force = 5f;
    //Trybjective: Implement a bullet behavior that moves forward, deals damage on impact, and destroys itself after a set time or upon collision.
    void Start()
    {
        // Mermiyi ileri (yukar� y�n�ne g�re) f�rlat
        GetComponent<Rigidbody2D>().linearVelocity = transform.up * speed;

        // 3 saniye sonra mermiyi yok et (bo�lukta sonsuza kadar gitmesin)
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        IDamageable target = hitInfo.GetComponent<IDamageable>();
        if (target != null)
        {
            Vector2 dir = (hitInfo.transform.position - transform.position).normalized;
            target.TakeDamage(damage, dir, force);
            Destroy(gameObject); // D��mana �arp�nca yok ol
        }
        else if (hitInfo.CompareTag("Wall")) // Duvara �arparsa yok ol
        {
            Destroy(gameObject);
        }
    }
}