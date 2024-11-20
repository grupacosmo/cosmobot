using System.Collections;
using System.Collections.Generic;
using Cosmobot.Entity;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace Cosmobot
{
    public class EnemyBehaviour : MonoBehaviour
    {
        public Health health;
        private GameObject nest;
        private EnemySpawner enemySpawner;
        public EnemyBehaviourStates state;
        public GameObject target;
        public float speed;
        private float timer;
        private bool isMoving = false;


        void Start()
        {
            health = gameObject.GetComponent<Health>();
            health.OnDeath += Death;
            state = EnemyBehaviourStates.REST;
        }

        void Update()
        {
            switch (state)
            {
                case EnemyBehaviourStates.ATTACK:
                    if (!isMoving)
                        ChaseTarget();
                    break;
                case EnemyBehaviourStates.REST:
                    if (timer > 5 && !isMoving)
                    {
                        StartCoroutine(WanderAround());
                        timer = 0;
                    }
                    else
                        timer += Time.deltaTime;
                    break;
            }

        }

        void Death(Health source, float oldHealth, float damageValue)
        {
            enemySpawner.RemoveEnemy(gameObject);
            Destroy(gameObject);
        }

        public void SetNest(GameObject spawner)
        {
            nest = spawner;
            enemySpawner = nest.GetComponent<EnemySpawner>();
        }

        IEnumerator WanderAround()
        {
            Random.InitState((int)Time.time);
            float distance = Random.Range(1, 10);
            isMoving = true;
            Quaternion targetRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, speed * Time.deltaTime);
                yield return null;
            }
            Vector3 targetPosition = transform.position + transform.forward * distance;
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }
            isMoving = false;
        }

        void ChaseTarget()
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.transform.position) > 2)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
            }
            else
            {
                Debug.Log("close enough");
            }
        }
    }
}
