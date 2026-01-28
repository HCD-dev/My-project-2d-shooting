using UnityEngine;
using UnityEngine.SceneManagement; // Sahne yüklemek için ÞART

public class GameManager : MonoBehaviour
{
    [Header("Spawn Sýnýrlarý")]
    public float xLimit = 12f;
    public float yLimit = 6f;
    public GameObject enemyPrefab;
    public float spawnRate = 3f;

    [Header("Ganimet Prefablarý")]
    public GameObject medkitPrefab;
    public GameObject grenadePickup;

    // --- YENÝ EKLENEN RESTART FONKSÝYONU ---
    public void RestartGame()
    {
        // Zamaný 1 yapmazsak yeni sahne donuk baþlar!
        Time.timeScale = 1f;

        // Mevcut sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    // ---------------------------------------

    void OnEnable() { EnemyZ.OnEnemyDeath += HandleLoot; }
    void OnDisable() { EnemyZ.OnEnemyDeath -= HandleLoot; }

    void Start()
    {
        InvokeRepeating("SpawnEnemy", 2f, spawnRate);
    }

    void SpawnEnemy()
    {
        Vector3 spawnPos = Vector3.zero;
        int edge = Random.Range(0, 4);

        switch (edge)
        {
            case 0: spawnPos = new Vector3(Random.Range(-xLimit, xLimit), yLimit, 0); break;
            case 1: spawnPos = new Vector3(Random.Range(-xLimit, xLimit), -yLimit, 0); break;
            case 2: spawnPos = new Vector3(xLimit, Random.Range(-yLimit, yLimit), 0); break;
            case 3: spawnPos = new Vector3(-xLimit, Random.Range(-yLimit, yLimit), 0); break;
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