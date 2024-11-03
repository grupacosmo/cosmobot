using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class EnemySpawner : MonoBehaviour
    {
        public GameObject Enemy;
        public int SpawnInterval;
        public int EnemyLimit;
        private float timer;
        private List<GameObject> enemies = new List<GameObject>();


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

        void ReleaseEnemies()
        {
            for (int i = 0; i < EnemyLimit; i++)
            {
                //call function to change state to f.e. attack
            }
            Debug.Log("Enemies released");
            //for test
            RemoveEnemy(null);
        }

        void SpawnEnemy()
        {
            enemies.Add(Instantiate(Enemy, CreateSpawnPoint(), Quaternion.identity));
        }

        Vector3 CreateSpawnPoint()
        {
            int offset1 = 0;
            int offset2 = 0;
            switch (Random.Range(1, 3))
            {
                case 1:
                    offset1 = Random.Range(5, 15);
                    break;
                case 2:
                    offset1 = Random.Range(-5, -15);
                    break;
            }
            switch (Random.Range(1, 3))
            {
                case 1:
                    offset2 = Random.Range(5, 15);
                    break;
                case 2:
                    offset2 = Random.Range(-5, -15);
                    break;
            }
            Vector3 spawnPosition = new Vector3(gameObject.transform.position.x + offset1, gameObject.transform.position.y, gameObject.transform.position.z + offset2);
            return spawnPosition;
        }

        public void RemoveEnemy(GameObject enemy)
        {
            //for tests
            enemies.RemoveRange(0, 5);
        }
    }
}
