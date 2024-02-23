using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cosmobot.Entity
{
    public class HealthBar : MonoBehaviour
    {

        [Tooltip("Number of decimal places to display in the health text.")]
        public int accuracy = 0;

        [SerializeField]
        private Health targetHealth;

        [SerializeField]
        private Slider uiHealthBar;
        [SerializeField]

        private TMP_Text uiHealthText;

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
            targetHealth.OnHealthChange += OnHealthChange;
            targetHealth.OnReset += OnHealthChange;
            UpdateUI();
        }

        private void OnDisable()
        {
            targetHealth.OnHealthChange -= OnHealthChange;
            targetHealth.OnReset -= OnHealthChange;
        }

        private void OnHealthChange(Health _, float __, float ___)
        {
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
    }
}