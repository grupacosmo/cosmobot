using UnityEngine;

namespace Cosmobot.Entity
{

    /// <summary>
    /// Test helper class
    /// </summary>
    public class DamageDealer : MonoBehaviour
    {
        public Health target;
        public void DealDamage(float damage)
        {
            target.TakeDamage(damage, new DamageSource(this));
        }

        public void ResetHealth()
        {
            target.Reset();
        }

        private void OnEnable()
        {
            target.OnHealthChange += OnHealthChange;
            target.OnDeath += OnDeath;
            target.OnReset += OnReset;
        }

        private void OnDisable()
        {
            target.OnHealthChange -= OnHealthChange;
            target.OnDeath -= OnDeath;
            target.OnReset -= OnReset;
        }

        private void OnHealthChange(Health source, float oldHealth, float damageVale)
        {
            Debug.Log($"{source.name} health changed: {oldHealth} -> {source.CurrentHealth} ({damageVale} damage)");
        }

        private void OnReset(Health source, float _, float __)
        {
            Debug.Log($"{source.name} health reset {source.CurrentHealth}");
        }

        private void OnDeath(Health source, float _, float __)
        {
            Debug.Log($"{source.name} died");
        }
    }
}