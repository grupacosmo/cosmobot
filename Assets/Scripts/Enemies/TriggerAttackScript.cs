using System.Collections;
using Cosmobot.Entity;
using Cosmobot.Utils;
using UnityEngine;

namespace Cosmobot
{
    public class TriggerAttackScript : MonoBehaviour
    {
        public bool isAttacking  ;
        public float attackInterval = 2;
        public float damage = 50;

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.CompareTag(Tags.Enemy))
            {
                if (!isAttacking)
                {
                    StartCoroutine(Hit(other.gameObject));
                }
            }
        }

        private IEnumerator Hit(GameObject target)
        {
            isAttacking = true;
            Health.TakeDamage(target, damage, new DamageSource(gameObject));
            yield return new WaitForSeconds(attackInterval);
            isAttacking = false;
        }
    }
}
