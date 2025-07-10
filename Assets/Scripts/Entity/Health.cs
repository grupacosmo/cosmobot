using System;
using UnityEngine;

namespace Cosmobot.Entity
{
    /// <summary>
    ///     - OnHealthChange event is triggered when the health changes even if it's zero or negative.
    ///     - OnDeath event is triggered when the health reaches zero or negative.
    ///     - OnHealthChange is triggered before OnDeath in the same frame.
    /// </summary>
    public class Health : MonoBehaviour
    {
        public delegate void HealthEvent(Health source, float oldHealth, float damageValue);

        public float MaxHealth;

        [SerializeField]
        protected float currentHealth;

        public float CurrentHealth => currentHealth;
        public float CurrentHealthPercentage => currentHealth / MaxHealth;
        public bool IsDead => currentHealth <= 0;

        /// <summary> Can be <see cref="DamageSource.Empty"> </summary>
        public DamageSource LastDamageSource { get; private set; }

        private void OnValidate()
        {
            if (MaxHealth < 0) MaxHealth = 0;
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
        }

        public event HealthEvent OnHealthChange;
        public event HealthEvent OnDeath;
        public event HealthEvent OnReset;

        public virtual void TakeDamage(float damage, DamageSource damageSource)
        {
            if (IsDead) return;
            if (damageSource.IsEmpty)
            {
                throw new ArgumentException(
                    "Damage source cannot be empty.", nameof(damageSource));
            }

            float oldHealth = currentHealth;
            currentHealth = Mathf.Clamp(currentHealth - damage, 0, MaxHealth);
            LastDamageSource = damageSource;

            OnHealthChange?.Invoke(this, oldHealth, damage);
            if (IsDead) OnDeath?.Invoke(this, oldHealth, damage);
        }

        public virtual void ResetHealth(bool clearEventListeners = false)
        {
            currentHealth = MaxHealth;
            LastDamageSource = DamageSource.Empty;
            if (clearEventListeners)
            {
                OnHealthChange = null;
                OnDeath = null;
            }
            else
            {
                OnReset?.Invoke(this, 0, 0);
            }
        }

        /// <summary>
        ///     If the target has a Health component, it will take damage and return true,
        ///     otherwise it will return false.
        /// </summary>
        public static bool TakeDamage(GameObject target, float damage, DamageSource damageSource)
        {
            Health health = target.GetComponent<Health>();
            if (health == null) return false;
            health.TakeDamage(damage, damageSource);
            return true;
        }
    }
}
