using UnityEngine;

namespace Cosmobot.Entity
{
    [RequireComponent(typeof(Health))]
    public class Dummy : MonoBehaviour
    {
        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health.OnHealthChange += OnHealthChange;
            health.TakeDamage(-health.MaxHealth, new DamageSource(this));
        }

        private void OnDisable()
        {
            health.OnHealthChange -= OnHealthChange;
        }

        private void OnHealthChange(Health source, float oldHealth, float damageVale)
        {
            if (damageVale < 0) return;
            // auto heal
            source.TakeDamage(-damageVale, new DamageSource(this));
        }
    }
}
