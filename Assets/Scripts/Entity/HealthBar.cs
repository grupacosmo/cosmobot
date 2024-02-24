using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cosmobot.Entity
{
    public class HealthBar : MonoBehaviour
    {

        [Tooltip("Number of decimal places to display in the health text.")]
        public int accuracy = 0;

        public Health TargetHealth
        {
            get => targetHealth;
            set => SetTargetHealth(value);
        }

        [SerializeField]
        private Slider uiHealthBar;
        [SerializeField]

        private TMP_Text uiHealthText;


        [SerializeField]
        private Health targetHealth;

        private void Start()
        {
            if (!ComponentUtils.RequireNotNull(
                    new object[] { targetHealth, uiHealthBar, uiHealthText },
                    new[] { nameof(targetHealth), nameof(uiHealthBar), nameof(uiHealthText) },
                    this))
            {
                enabled = false;
                return;
            }
        }

        private void OnEnable()
        {
            if (targetHealth == null) return;
            targetHealth.OnHealthChange += OnHealthChange;
            targetHealth.OnReset += OnHealthChange;
            UpdateUI();
        }

        private void OnDisable()
        {
            if (targetHealth == null) return;
            targetHealth.OnHealthChange -= OnHealthChange;
            targetHealth.OnReset -= OnHealthChange;
        }

        private void OnHealthChange(Health _, float __, float ___)
        {
            Debug.Log("Health changed: " + _.name);
            UpdateUI();
        }

        private void UpdateUI()
        {
            uiHealthBar.value = targetHealth.CurrentHealthPercentage * uiHealthBar.maxValue;
            uiHealthText.text =
                targetHealth.CurrentHealth.ToString($"F{accuracy}")
                + " / "
                + targetHealth.MaxHealth.ToString($"F{accuracy}");
        }

        private void SetTargetHealth(Health value)
        {
            if (targetHealth == value || value == null)  return;
            if (targetHealth != null && enabled)
            {
                targetHealth.OnHealthChange -= OnHealthChange;
                targetHealth.OnReset -= OnHealthChange;
            }

            targetHealth = value;
            if (enabled) {
                targetHealth.OnHealthChange += OnHealthChange;
                targetHealth.OnReset += OnHealthChange;
                UpdateUI();
            }
        }
    }
}