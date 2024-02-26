using System;
using System.Collections;
using System.Collections.Generic;
using Cosmobot.Entity;
using UnityEngine;

namespace Cosmobot
{
    public class EnemyAi : MonoBehaviour
    {
        public Transform pointA;
        public Transform pointB;
        private Health health;
        public float speed;

        // Update is called once per frame
        void Start()
        {
            health = gameObject.GetComponent<Health>();
            health.OnDeath += Death;
        }

        private void Death(Health source, float oldHealth, float damageValue)
        {
            Destroy(gameObject);
        }

        void Update()
        {
            transform.position = Vector3.Lerp(pointA.position, pointB.position, Mathf.Pow(Mathf.Sin(Time.time * speed), 2));
        }
    }

}
