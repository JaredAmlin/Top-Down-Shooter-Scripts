
namespace CCS.Health
{

    public class UnitHealth
    {
        #region Fields

        private int _currentHealth;
        private int _currentMaxHealth;
        private bool _isDead;

        #endregion

        #region Properties

        public int Health
        {
            get { return _currentHealth; }
            set { _currentHealth = value; }
        }

        public int MaxHealth
        {
            get { return _currentMaxHealth; }
            set { _currentMaxHealth = value; }
        }

        public bool IsDead
        {
            get { return _isDead; }
            set { _isDead = value; }
        }

        #endregion

        #region Constructor

        public UnitHealth(int health, int maxHealth)
        {
            _currentHealth = health;
            _currentMaxHealth = maxHealth;
        }

        #endregion

        #region Methods

        public void DamageUnit(int damageAmount)
        {
            if (_currentHealth > 0)
            {
                _currentHealth -= damageAmount;
            }
            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                Die();
            }
        }

        public void HealUnit(int healAmount)
        {
            if (_currentHealth < _currentMaxHealth)
            {
                _currentHealth += healAmount;
            }

            if (_currentHealth > _currentMaxHealth)
            {
                _currentHealth = _currentMaxHealth;
            }
        }

        public void RestoreHealth()
        {
            _currentHealth = _currentMaxHealth;
        }

        private void Die()
        {
            IsDead= true;
        }

        #endregion
    }
}
