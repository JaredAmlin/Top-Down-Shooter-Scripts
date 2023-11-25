using UnityEngine;
using UnityEngine.UI;

namespace CCS.Health.HealthBar
{
    public class HealthBar : MonoBehaviour
    {
        public Color _minHealthColor, _maxHealthColor;

        private Image _healthBar;
        private float _fillAmount;
        private void Awake()
        {
            _healthBar= GetComponent<Image>();
        }

        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            _fillAmount = currentHealth / maxHealth;
            _healthBar.fillAmount = _fillAmount;

            //color lerp
            _healthBar.color = Color.Lerp(_minHealthColor, _maxHealthColor, _fillAmount);
        }

        public void UpdateMechHealth(float currentHealth, float maxHealth)
        {
            // Calculate the health percentage
            float healthPercentage = currentHealth / maxHealth;

            // Calculate the new scale for the health image
            Vector3 newScale = new Vector3(healthPercentage, _healthBar.transform.localScale.y, _healthBar.transform.localScale.z);

            // Set the new scale for the health image
            _healthBar.transform.localScale = newScale;

            //color lerp
            _healthBar.color = Color.Lerp(_minHealthColor, _maxHealthColor, healthPercentage);
        }
    }
}
