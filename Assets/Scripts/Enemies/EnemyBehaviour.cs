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
        private Health targetHealth;
        public float damage;
        public float attackInterval;
        public float speed;
        private float timer;
        private bool isMoving = false;
        private bool isAttacking = false;
        public float unblockForce = 5f;
        public int id = 0;


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
                    if (target != null)
                    {
                        if (!isMoving)
                        {
                            ChaseTarget();
                        }
                    }
                    else
                    {
                        state = EnemyBehaviourStates.REST;
                    }
                    break;
                case EnemyBehaviourStates.REST:
                    if (timer > 5 && !isMoving)
                    {
                        StartCoroutine(WanderAround());
                        timer = 0;
                    }
                    else
                    {
                        timer += Time.deltaTime;
                    }
                    if (target != null)
                    {
                        state = EnemyBehaviourStates.ATTACK;
                    }
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
            Random.InitState(gameObject.transform.position.GetHashCode());
            float distance = Random.Range(1, 10);
            isMoving = true;
            Quaternion targetRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Debug.Log(gameObject + " " + id + " started spinning");
            float rotationTimeout = 10f;
            float rotationTime = 0f;
            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, speed * Time.deltaTime);
                rotationTime += Time.deltaTime;

                if (rotationTime > rotationTimeout)
                {
                    Debug.LogWarning(gameObject + " " + id + " rotation timeout reached, breaking out of spin.");
                    break;
                }
                yield return null;
            }
            Debug.Log(gameObject + " " + id + " ended spinning");
            Vector3 targetPosition = transform.position + transform.forward * distance;
            Debug.Log(gameObject + " " + id + " started moving");
            float movingTimeout = 10f;
            float movingTime = 0f;
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
                movingTime += Time.deltaTime;

                if (movingTime > movingTimeout)
                {
                    Debug.LogWarning(gameObject + " " + id + " moving timeout reached, breaking out of spin.");
                    break;
                }
                yield return null;
            }
            Debug.Log(gameObject + " " + id + " ended moving");
            isMoving = false;
        }

        void ChaseTarget()
        {

            if (Vector3.Distance(transform.position, target.transform.position) > 2 && target != null)
            {
                Vector3 direction = (target.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, speed * Time.deltaTime);
                transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
            }
            else
            {
                if (!isAttacking)
                {
                    StartCoroutine(Attack());
                }
            }
        }

        IEnumerator Attack()
        {
            isAttacking = true;
            Health.TakeDamage(target, damage, new DamageSource(gameObject));
            yield return new WaitForSeconds(attackInterval);
            isAttacking = false;
        }

        public void SetTarget(GameObject target)
        {
            this.target = target;
            targetHealth = target.GetComponent<Health>();
            targetHealth.OnDeath += RemoveTarget;
        }

        public void RemoveTarget(Health source, float oldHealth, float damageValue)
        {
            target = null;
            targetHealth = null;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Untagged"))
            {
                Vector3 direction = (collision.transform.position - transform.position).normalized;
                collision.rigidbody.AddForce(direction * unblockForce, ForceMode.Impulse);
            }
        }
    }
}
