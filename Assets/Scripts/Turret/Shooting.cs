using System;
using System.Collections;
using System.Collections.Generic;
using Cosmobot.Entity;
using UnityEngine;

namespace Cosmobot
{
    [RequireComponent(typeof(Tracking), typeof(TurretStats), typeof(ParticleSystem))]
    public class Shooting : MonoBehaviour
    {
        private ParticleSystem particleSystem;
        private Tracking tracking;
        private TurretStats turretStats;
        private bool canFire = true;

        void Start()
        {
            tracking = GetComponent<Tracking>();
            particleSystem = GetComponent<ParticleSystem>();
            turretStats = GetComponent<TurretStats>();
        }

        void Update()
        {
            if (canFire && tracking.target != null)
            {
                Ray ray = new Ray(transform.position, transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.gameObject == tracking.target.gameObject)
                    {
                        StartCoroutine(Shoot(hit.collider.gameObject));
                    }
                }
            }
        }

        public IEnumerator Shoot(GameObject target)
        {
            canFire = false;
            particleSystem.Play();
            Health.TakeDamage(target, turretStats.power, new DamageSource(gameObject));
            yield return new WaitForSeconds(turretStats.fireRate);
            canFire = true;
            particleSystem.Stop();
        }
    }
}
