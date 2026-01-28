using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float speed = 15f;    // Havada gidiþ hýzý
    public float delay = 1f;     // Hedefe ulaþtýktan ne kadar sonra patlayacak
    public float radius = 5f;    // Patlama etki alaný
    public float damage = 100f;  // Patlama hasarý

    private Vector3 target;
    private bool isLaunched = false;

    // PlayerController'dan bu fonksiyon çaðrýlýr
    public void Launch(Vector3 targetPos)
    {
        target = targetPos;
        target.z = 0; // 2D oyun olduðu için Z'yi sýfýrlýyoruz
        isLaunched = true;
    }

    void Update()
    {
        if (!isLaunched) return;

        // Bombayý fýrlatýlan hedefe doðru hareket ettir
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Hedefe vardýðýnda dur ve patlama sayacýný baþlat
        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            isLaunched = false;
            Invoke("Explode", delay);
        }
    }

    void Explode()
    {
        // Belirlenen yarýçap içindeki tüm nesneleri bul
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var obj in objects)
        {
            // Eðer çarptýðýn nesne hasar alabiliyorsa (IDamageable interface'i varsa)
            IDamageable hit = obj.GetComponent<IDamageable>();
            if (hit != null)
            {
                Vector2 dir = (obj.transform.position - transform.position).normalized;
                hit.TakeDamage(damage, dir, 15f); // Hasar ver ve geri it
            }
        }

        // Patlama efektini burada Instantiate edebilirsin (isteðe baðlý)
        // Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Destroy(gameObject); // Bombayý sahneden sil
    }

    // Patlama alanýný Unity içinde görmek için (Gizmos)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}