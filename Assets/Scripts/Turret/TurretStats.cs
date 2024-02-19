using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class TurretStats : MonoBehaviour
    {
        public float speed;
        public float angleRange;
        public float hp;
        public float fireRate;
        public float power;

        void Start()
        {
            speed = 40f;
            angleRange = 160f;
            hp = 100f;
            fireRate = 2f;
            power = 12.5f;
        }

        public void Damaged(float damageAmount)
        {
            hp-=damageAmount;
            if(hp<=0)
            {
                Destroy(gameObject);
            }
        }
    }
}
