using System.Collections;
using System.Collections.Generic;
using Cosmobot.Entity;
using UnityEngine;

namespace Cosmobot
{
    public abstract class Enemy : MonoBehaviour
    {
        private GameObject nest;
        private EnemySpawner enemySpawner;
        private Health targetHealth;
        private Vector3 wanderTarget;
        private bool isAttacking = false;

        public Health health;
        public EnemyBehaviourStates state;
        public GameObject target;
        public float damage;
        public float attackInterval;
        public float speed;
        public float attackRange;
        public float wanderRadius;
        public float unblockForce;

        private void Start()
        {
            health = gameObject.GetComponent<Health>();
            health.OnDeath += Death;
            health.OnHealthChange += OnDamage;
            state = EnemyBehaviourStates.REST;
            wanderTarget = gameObject.transform.position;
        }

        // If enemy takes damage, source of that damage becomes it's target
        private void OnDamage(Health source, float oldHealth, float damageValue)
        {
            SetTarget((GameObject)health.LastDamageSource.Source);
        }

        private void Death(Health source, float oldHealth, float damageValue)
        {
            enemySpawner?.RemoveEnemy(gameObject);
            Destroy(gameObject);
        }

        private IEnumerator Hit()
        {
            isAttacking = true;
            Health.TakeDamage(target, damage, new DamageSource(gameObject));
            yield return new WaitForSeconds(attackInterval);
            isAttacking = false;
        }

        // Anti stuck
        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Untagged"))
            {
                Vector3 direction = (transform.position - collision.transform.position).normalized;
                gameObject.GetComponent<Rigidbody>().AddForce(direction * unblockForce, ForceMode.Impulse);
            }
        }

        // Base method to move to a specified point
        public virtual void MoveTo(Vector3 targetPosition)
        {
            Debug.Log("moving to " + targetPosition);
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }

        // Base method to attack a given target
        public virtual void Attack()
        {
            wanderTarget = gameObject.transform.position;
            if (target == null)
            {
                state = EnemyBehaviourStates.REST;
                return;
            }
            if (Vector3.Distance(transform.position, target.transform.position) < attackRange)
            {
                if (!isAttacking)
                {
                    Debug.Log("attack on " + target.name);
                    StartCoroutine(Hit());
                }
            }
            else
            {
                MoveTo(target.transform.position);
            }
        }

        // Method to generate wandering target, it's virtual because of potential enemies that will move in 3 axes
        public virtual Vector3 RandomWanderingTarget()
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            randomDirection.y = transform.position.y;
            return randomDirection;
        }

        // A method for wandering when there is no target
        public virtual void Wander()
        {
            if (wanderTarget == gameObject.transform.position)
            {
                wanderTarget = RandomWanderingTarget();
            }
            else
            {
                if (Vector3.Distance(transform.position, wanderTarget) > 1.0f)
                {
                    MoveTo(wanderTarget);
                }
                else
                {
                    wanderTarget = gameObject.transform.position;
                }
            }
        }

        public void SetNest(GameObject spawner)
        {
            if (spawner != null)
            {
                nest = spawner;
                enemySpawner = nest.GetComponent<EnemySpawner>();
            }
            else
            {
                nest = null;
                enemySpawner = null;
            }
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
    }
}
