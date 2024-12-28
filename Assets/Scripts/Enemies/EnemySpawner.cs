using System.Collections;
using System.Collections.Generic;
using Cosmobot.Entity;
using UnityEngine;

namespace Cosmobot
{
    public class EnemySpawner : MonoBehaviour
    {

        public Health health;
        public GameObject Enemy;
        public int SpawnInterval;
        public int EnemyLimit;
        private float timer;

        // Probably will be changed in future development
        public GameObject PotentialTarget;
        private List<GameObject> enemies = new List<GameObject>();


        void Start()
        {
            health = gameObject.GetComponent<Health>();
            health.OnDeath += Death;
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

        void Update()
        {
            if (timer > SpawnInterval)
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

        // Releasing children to attack
        void ReleaseEnemies()
        {
            for (int i = 0; i < EnemyLimit; i++)
            {
                if (enemies[i] != null)
                {
                    enemies[i].GetComponent<Enemy>().SetTarget(PotentialTarget);
                }
            }
            Debug.Log("Enemies released");
            RemoveEnemy(null);
        }

        // Spawning a child
        void SpawnEnemy()
        {
            GameObject e = Instantiate(Enemy, CreateSpawnPoint(), Quaternion.identity);
            e.GetComponent<Enemy>().SetNest(gameObject);
            enemies.Add(e);

        }

        Vector3 CreateSpawnPoint()
        {
            Random.InitState((int)Time.time);
            int offset1 = 0;
            int offset2 = 0;
            switch (Random.Range(1, 3))
            {
                case 1:
                    Random.InitState((int)Time.time);
                    offset1 = Random.Range(5, 15);
                    break;
                case 2:
                    Random.InitState((int)Time.time);
                    offset1 = Random.Range(-5, -15);
                    break;
            }
            switch (Random.Range(1, 3))
            {
                case 1:
                    Random.InitState((int)Time.time);
                    offset2 = Random.Range(5, 15);
                    break;
                case 2:
                    Random.InitState((int)Time.time);
                    offset2 = Random.Range(-5, -15);
                    break;
            }
            Vector3 spawnPosition = new Vector3(gameObject.transform.position.x + offset1, gameObject.transform.position.y, gameObject.transform.position.z + offset2);
            return spawnPosition;
        }

        public void RemoveEnemy(GameObject enemy)
        {
            if (enemy == null)
                enemies.RemoveRange(0, EnemyLimit);
            else
                enemies.Remove(enemy);
        }

    }
}
