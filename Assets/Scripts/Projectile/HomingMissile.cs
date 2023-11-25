using UnityEngine;
using CCS.AI.Enemy;

public class HomingMissile : MonoBehaviour
{
    #region Variables

    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _rotationSpeed = 150f;
    [SerializeField] private int _damage = 10; //Damage dealth by the projectile
    
    [SerializeField] private GameObject _impactSpark;
    [SerializeField] Transform _sparkParent;
    private float _distance;
    private float _closestDistance;

    [SerializeField] private bool _hasEnemyTarget = false;

    private GameObject[] _enemies;
    private EnemyAgent _enemyAgent;
    private Transform _enemyTarget = null;

    private TestTube _testTube;

    private const string _spawnPoint = "SpawnPoint";
    private const string _wallTag = "Wall";
    private const string _enemyTag = "Enemy";

    private AudioManager _audioManager;
    [SerializeField] private AudioClip _metalHitClip;

    #endregion

    #region Awake, Start, Update

    private void Awake()
    {
        _audioManager = FindObjectOfType<AudioManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        FindEnemyTargets();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    #endregion

    #region Methods

    private void Movement()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);

        if (_enemyTarget != null)
        {
            Vector3 direction = (Vector3)_enemyTarget.position - transform.position;

            direction.Normalize();

            direction.y = 0;

            //interpolate smooth rotation
            Quaternion towardsRotation = Quaternion.LookRotation(direction, Vector3.up);

            //missile rotation towards target
            transform.rotation = Quaternion.RotateTowards(transform.rotation, towardsRotation, _rotationSpeed * Time.deltaTime);
        }
        else
        {
            FindEnemyTargets();
        }
    }

    void FindEnemyTargets()
    {
        _hasEnemyTarget = false;
        _closestDistance = Mathf.Infinity;

        if (_hasEnemyTarget == false)
        {
            _enemies = GameObject.FindGameObjectsWithTag(_enemyTag);

            foreach (var enemy in _enemies)
            {
                _distance = (enemy.transform.position - this.transform.position).sqrMagnitude;

                if (_distance < _closestDistance)
                {
                    _closestDistance = _distance;

                    _enemyTarget = enemy.transform;

                    _hasEnemyTarget = true;
                }
            }
        }
    }

    #endregion

    #region Collision

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_wallTag))
        {
            ObjectPool.SpawnObject(_impactSpark, this.transform.position, Quaternion.identity, ObjectPool.PoolType.Gameobject);
 
            ObjectPool.ReturnObjectToPool(gameObject);
        }

        if (other.CompareTag(_enemyTag))
        {
            _enemyAgent = other.gameObject.GetComponent<EnemyAgent>();

            //damage the enemy health
            _enemyAgent.EnemyAgentTakeDamage(_damage);

            ObjectPool.SpawnObject(_impactSpark, this.transform.position, Quaternion.identity, ObjectPool.PoolType.Gameobject);

            ObjectPool.ReturnObjectToPool(gameObject);
        }

        if (other.CompareTag(_spawnPoint))
        {
            ObjectPool.SpawnObject(_impactSpark, this.transform.position, Quaternion.identity, ObjectPool.PoolType.Gameobject);

            _testTube = other.gameObject.GetComponent<TestTube>();
            _testTube.TestTubeTakeDamage(_damage);

            if (_testTube._enemyHealth.Health > 10)
            {
                _audioManager.PlaySound(_metalHitClip, _audioManager.WeaponsFX, .5f);
            }

            ObjectPool.ReturnObjectToPool(gameObject);
        }
    }

    #endregion
}
