using System.Collections.Generic;
using Cosmobot.Entity;
using UnityEngine;
using Random = System.Random;

namespace Cosmobot
{
    public class EnemySpawner : MonoBehaviour
    {
        // Probably will be changed in future development
        public GameObject PotentialTarget;
        public Health health;
        public GameObject Enemy;
        public int SpawnInterval;
        public int EnemyLimit;
        public int MinSpawnRange;
        public int MaxSpawnRange;
        private readonly List<GameObject> enemies = new List<GameObject>();
        private float timer;

        private void Start()
        {
            health = gameObject.GetComponent<Health>();
            health.OnDeath += Death;
        }

        private void Update()
        {
            if (timer >= SpawnInterval)
            {
                SpawnEnemy();
                if (enemies.Count == EnemyLimit)
                {
                    ReleaseEnemies();
                }

                timer = 0;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }

        private void Death(Health source, float oldHealth, float damageValue)
        {
            RemoveNest();
            Destroy(gameObject);
        }

        private void RemoveNest()
        {
            foreach (var enemy in enemies)
            {
                enemy.GetComponent<Enemy>().SetNest(null);
            }
        }

        // Releasing children to attack
        private void ReleaseEnemies()
        {
            for (int i = 0; i < EnemyLimit; i++)
            {
                if (enemies[i])
                {
                    enemies[i].GetComponent<Enemy>().SetTarget(PotentialTarget);
                }
            }

            Debug.Log("Enemies released");
            RemoveEnemy(null);
        }

        // Spawning a child
        private void SpawnEnemy()
        {
            GameObject e = Instantiate(Enemy, CreateSpawnPoint(), Quaternion.identity);
            e.GetComponent<Enemy>().SetNest(gameObject);
            enemies.Add(e);
        }

        private Vector3 CreateSpawnPoint()
        {
            Random random = new Random((int)Time.time);
            int offset1 = random.Next(0, 2) == 0
                ? random.Next(MinSpawnRange, MaxSpawnRange)
                : random.Next(-MaxSpawnRange, -MinSpawnRange);
            int offset2 = random.Next(0, 2) == 0
                ? random.Next(MinSpawnRange, MaxSpawnRange)
                : random.Next(-MaxSpawnRange, -MinSpawnRange);
            Vector3 spawnPosition = new Vector3(gameObject.transform.position.x + offset1,
                gameObject.transform.position.y, gameObject.transform.position.z + offset2);
            return spawnPosition;
        }

        public void RemoveEnemy(GameObject enemy)
        {
            if (!enemy)
            {
                enemies.RemoveRange(0, EnemyLimit);
            }
            else
            {
                enemies.Remove(enemy);
            }
        }
    }
}
