using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int enemyCount = 3;               // Số enemy sẽ spawn
    public float spawnRadius = 1f;           // Bán kính nhỏ quanh spawner
    public float spawnDelay = 1f;            // Thời gian delay giữa mỗi con

    private bool hasSpawned = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasSpawned) return;

        if (collision.CompareTag("Player"))
        {
            StartCoroutine(SpawnEnemiesCoroutine());
            hasSpawned = true;
        }
    }

    IEnumerator SpawnEnemiesCoroutine()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            // Spawn đúng ngay vị trí spawner
            Vector2 spawnPos = transform.position;

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // Gán player cho enemy
            EnemyFollow enemyFollow = enemy.GetComponent<EnemyFollow>();
            if (enemyFollow != null)
            {
                enemyFollow.player = GameObject.FindGameObjectWithTag("Player").transform;
            }

            yield return new WaitForSeconds(spawnDelay); // Chờ rồi mới spawn tiếp
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
