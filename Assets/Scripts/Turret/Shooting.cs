using System.Collections;
using Cosmobot.Entity;
using UnityEngine;

namespace Cosmobot
{
    [RequireComponent(typeof(Tracking), typeof(TurretStats), typeof(ParticleSystem))]
    public class Shooting : MonoBehaviour
    {
        private bool canFire = true;
        private ParticleSystem particleSystem;
        private Tracking tracking;
        private TurretStats turretStats;

        private void Start()
        {
            tracking = GetComponent<Tracking>();
            particleSystem = GetComponent<ParticleSystem>();
            turretStats = GetComponent<TurretStats>();
        }

        private void Update()
        {
            if (canFire && tracking.Target != null)
            {
                Ray ray = new Ray(transform.position, transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.gameObject == tracking.Target.gameObject)
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
