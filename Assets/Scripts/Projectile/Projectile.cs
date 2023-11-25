using CCS.AI.Enemy;
using UnityEngine;


namespace CCS.Player.Weapons.Projectile
{
    public class Projectile : MonoBehaviour
    {
        #region Variables

        [Header("Adjustable Variables")]
        [SerializeField] private float _speed = 10f;
        [SerializeField] private int _damage = 10; //Damage dealth by the projectile

        [Header("Impact Particles")]
        [SerializeField] private GameObject _impactSpark;
        [SerializeField] private GameObject _impactParticle;
        [SerializeField] private GameObject _rigidbodySparks;

        [Header("Audio")]

        [Tooltip("Metal Impact Sound")]
        [SerializeField] private AudioClip _metalHitClip;

        //cache string names
        private const string _wallTag = "Wall";
        private const string _spawnPoint = "SpawnPoint";
        private const string Enemy = "Enemy";
        private const string Miniboss = "Miniboss";
        private const string Boss = "Boss";
        private const string Shield = "Shield";
        private const string Turret = "Turret";

        private EnemyAgent _enemyAgent;
        private MinibossTestStateMachine _miniBossAgent;
        private LVL1_Boss_Shield _shield;
        private LVL1_Boss _boss;
        private Turret _turret;
        private TestTube _testTube;

        #endregion

        void Update()
        {
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
        }

        #region Collision
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_wallTag))
            {
                Instantiate(_impactSpark, this.transform.position, Quaternion.identity);

                ObjectPool.ReturnObjectToPool(gameObject);
            }

            if (other.CompareTag(Enemy))
            {
                _enemyAgent = other.gameObject.GetComponent<EnemyAgent>();
                
                _enemyAgent.EnemyAgentTakeDamage(_damage);

                Instantiate(_impactParticle, this.transform.position, other.transform.rotation);

                Instantiate(_rigidbodySparks, this.transform.position, other.transform.rotation);

                ObjectPool.ReturnObjectToPool(gameObject);
            }

            if (other.CompareTag(Miniboss))
            {
                _miniBossAgent = other.gameObject.GetComponent<MinibossTestStateMachine>();

                _miniBossAgent.EnemyAgentTakeDamage(_damage);

                Instantiate(_impactParticle, this.transform.position, other.transform.rotation);

                Instantiate(_rigidbodySparks, this.transform.position, other.transform.rotation);

                ObjectPool.ReturnObjectToPool(gameObject);
            }

            if (other.CompareTag(Boss))
            {
                _boss = other.gameObject.GetComponent<LVL1_Boss>();

                _boss.BossAgentTakeDamage(_damage);

                Instantiate(_impactParticle, this.transform.position, other.transform.rotation);

                Instantiate(_rigidbodySparks, this.transform.position, other.transform.rotation);

                ObjectPool.ReturnObjectToPool(gameObject);
            }

            if (other.CompareTag(Shield))
            {
                _shield = other.gameObject.GetComponent<LVL1_Boss_Shield>();
              
                _shield.ShieldTakeDamage(_damage);

                Instantiate(_impactParticle, this.transform.position, other.transform.rotation);

                Instantiate(_rigidbodySparks, this.transform.position, other.transform.rotation);

                ObjectPool.ReturnObjectToPool(gameObject);
            }

            if (other.CompareTag(Turret))
            {
                _turret = other.gameObject.GetComponent<Turret>();

                _turret.TurretTakeDamage(_damage);

                Instantiate(_impactParticle, this.transform.position, other.transform.rotation);

                Instantiate(_rigidbodySparks, this.transform.position, other.transform.rotation);

                ObjectPool.ReturnObjectToPool(gameObject);
            }

            if (other.CompareTag(_spawnPoint))
            {
                Instantiate(_impactSpark, this.transform.position, Quaternion.identity);
              
                _testTube = other.gameObject.GetComponent<TestTube>();
                _testTube.TestTubeTakeDamage(_damage);

                ObjectPool.ReturnObjectToPool(gameObject);
            }
        }

        #endregion
    }
}
