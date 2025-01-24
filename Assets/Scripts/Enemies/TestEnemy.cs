using System.Collections;
using System.Collections.Generic;
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
                    if (target != null)
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
                    if (target != null)
                    {
                        state = EnemyBehaviourStates.ATTACK;
                    }
                    break;
            }
        }
    }
}
