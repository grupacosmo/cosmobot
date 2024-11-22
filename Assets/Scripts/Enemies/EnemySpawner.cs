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
        public GameObject PotentialTarget;
        private List<GameObject> enemies = new List<GameObject>();
        private int childId = 0;


        void Update()
        {
            if (timer > SpawnInterval)
            {
                SpawnEnemy(childId);
                if (enemies.Count == EnemyLimit)
                {
                    ReleaseEnemies();
                    SpawnInterval = 100000;
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
                enemies[i].GetComponent<EnemyBehaviour>().SetTarget(PotentialTarget);
            }
            Debug.Log("Enemies released");
            RemoveEnemy(null);
        }

        void SpawnEnemy(int childId)
        {
            GameObject e = Instantiate(Enemy, CreateSpawnPoint(), Quaternion.identity);
            e.GetComponent<EnemyBehaviour>().SetNest(gameObject);
            e.GetComponent<EnemyBehaviour>().id = childId;
            this.childId++;
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
                enemies.RemoveRange(0, 5);
            else
                enemies.Remove(enemy);
        }
    }
}
