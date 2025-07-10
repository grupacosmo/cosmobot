using UnityEngine;

namespace Cosmobot
{
    public class TestEnemy : Enemy
    {
        private void Update()
        {
            switch (state)
            {
                case EnemyBehaviourStates.ATTACK:
                    if (target)
                    {
                        Attack();
                    }
                    else
                    {
                        state = EnemyBehaviourStates.REST;
                    }

                    break;
                case EnemyBehaviourStates.REST:
                    Wander();
                    if (target)
                    {
                        state = EnemyBehaviourStates.ATTACK;
                    }

                    break;
            }
        }

        public override void MoveTo(Vector3 targetPosition)
        {
            Debug.Log("moving to " + targetPosition);
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
    }
}
