using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Spawn Sýnýrlarý")]
    public float xLimit = 12f; // Sað-Sol duvar çizgisi
    public float yLimit = 6f;  // Üst-Alt duvar çizgisi
    public GameObject enemyPrefab;
    public float spawnRate = 3f;

    [Header("Ganimet Prefablarý")]
    public GameObject medkitPrefab;
    public GameObject grenadePickup;

    void OnEnable() { EnemyZ.OnEnemyDeath += HandleLoot; }
    void OnDisable() { EnemyZ.OnEnemyDeath -= HandleLoot; }

    void Start()
    {
        InvokeRepeating("SpawnEnemy", 2f, spawnRate);
    }

    void SpawnEnemy()
    {
        Vector3 spawnPos = Vector3.zero;

        // 0: Üst Kenar, 1: Alt Kenar, 2: Sað Kenar, 3: Sol Kenar
        int edge = Random.Range(0, 4);

        switch (edge)
        {
            case 0: // Üst çizgi üzerinde rastgele bir nokta
                spawnPos = new Vector3(Random.Range(-xLimit, xLimit), yLimit, 0);
                break;
            case 1: // Alt çizgi üzerinde rastgele bir nokta
                spawnPos = new Vector3(Random.Range(-xLimit, xLimit), -yLimit, 0);
                break;
            case 2: // Sað çizgi üzerinde rastgele bir nokta
                spawnPos = new Vector3(xLimit, Random.Range(-yLimit, yLimit), 0);
                break;
            case 3: // Sol çizgi üzerinde rastgele bir nokta
                spawnPos = new Vector3(-xLimit, Random.Range(-yLimit, yLimit), 0);
                break;
        }

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    void HandleLoot(Vector3 pos)
    {
        float roll = Random.Range(0, 100);
        if (roll < 20) Instantiate(grenadePickup, pos, Quaternion.identity);
        else if (roll < 35) Instantiate(medkitPrefab, pos, Quaternion.identity);
    }
}