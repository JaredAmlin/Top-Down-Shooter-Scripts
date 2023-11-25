using CCS.Health;
using CCS.Health.HealthBar;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace CCS.AI.Enemy
{
    public class EnemyAgent : MonoBehaviour
    {
        #region Variables

        [Header("EnemyAgent Movement Variables")]
        public float maxTime = .5f;
        public float minDistance = 1;
        [SerializeField] private float _minSpeed;
        [SerializeField] private float _maxSpeed;
        [SerializeField] protected float _rotationSpeed = 500f;

        [SerializeField] private Transform[] _waypoints;
        private int _nextWaypoint = 0;
        private const string _waypointTag = "WayPoints";

        private Vector3 _destination;

        private Player.Player _player;
        [HideInInspector] public Transform _playerTransform;
        public Animator _animator;
        private NavMeshAgent _agent;
        private Collider _collider;

        protected enum _AiState { idle, patrol, chase, attack, gameOver }
        [Header("Enemy State")]
        [SerializeField] protected _AiState _currentState;  

        protected enum _AttackType { melee, shooter }
        [Header("Attack Type")]
        [SerializeField] protected _AttackType _attackType;

        private enum _EnemyToughness { weak, strong }
        [Header("EnemyToughness")]
        [SerializeField] private _EnemyToughness _enemyToughness;

        [Header("Enemy Projectile")]
        [SerializeField] protected GameObject _enemyProjectile;
        [SerializeField] protected Transform _firePoint;
        [SerializeField] private float _fireRate = 0.5f;
        private float _canFire = 0f;

        [Header("HealthVariables")]
        [SerializeField] private HealthBar _healthBar;
        [SerializeField] private int _currentHealth = 10;
        [SerializeField] private int _maxHealth = 10;

        [Header("Death Prefab")]
        [SerializeField] private GameObject _deathPrefab;

        [Header("Key Pickup Prefab")]
        [SerializeField] private GameObject _keyPUPrefab;

        [Header("Impact Particles")]
        [SerializeField] private GameObject _impactParticle;
        [SerializeField] private GameObject _sparksParticle;
        [SerializeField] private GameObject _electricityParticle;

        [Header("Damage")]
        [SerializeField] private int _damage = 10; //Damage dealth by the AI
        public UnitHealth _enemyHealth;

        private bool _isGameOver = false;
        private bool _hasTarget = false;

        //temp bool for mini-boss
        [SerializeField] private bool _isBoss = false;

        [Header("Points")]
        [SerializeField] private int _points = 10;

        private Mech _mech;
        private AttackCompanion _companion;
        private MedicCompanion _medic;

        private const string _playerTag = "Player";
        private const string _mechTag = "Mech";
        private const string _attackRoutine = "AttackRoutine";
        private const string _companionTag = "Companion";
        private const string _medicTag = "MedicCompanion";

        #endregion

        #region Awake, Start,

        private void Awake()
        {
            if (TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
            {
                _agent = agent;
            }
            else print("The Agent is NULL.");

            if (TryGetComponent<Animator>(out Animator animator))
            {
                _animator = animator;
            }
            else print("The Animator on the Enemy Agent is NULL.");

            _enemyHealth = new UnitHealth(_currentHealth, _maxHealth);
        }

        private void Start()
        {
            if (TryGetComponent<Collider>(out Collider collider))
            {
                _collider = collider;
            }
            else print("The collider on enemy AI is NULL.");

            _player = PlayerManager.Instance.GetPlayer();
            _playerTransform = PlayerManager.Instance.GetActivePlayer();
            _mech = PlayerManager.Instance.GetMechState();

            _agent.speed = Random.Range(_minSpeed, _maxSpeed);

            if (_currentState == _AiState.patrol)

            //get current waypoints
            _waypoints = WaypointManager.Instance.GetWaypoints();
        }

        #endregion

        #region OnEnable , OnDisable

        private void OnEnable()
        {
            PlayerManager.onPlayerChange += PlayerManager_onPlayerChange;
            GameManager.OnGameOver += GameManager_onGameOver;
            GameManager.OnRoomComplete += GameManager_OnRoomComplete;
        }

        private void OnDisable()
        {
            PlayerManager.onPlayerChange -= PlayerManager_onPlayerChange;
            GameManager.OnGameOver -= GameManager_onGameOver;
            GameManager.OnRoomComplete -= GameManager_OnRoomComplete;
        }

        #endregion

        #region Update, Move Functions

        private void Update()
        {
            AiState();
        }

        protected virtual void AiState()
        {
            if (_attackType == _AttackType.shooter)
            {
                //check distance to player
                float distance = Vector3.Distance(this.transform.position, _playerTransform.position);

                if (distance < 5f)
                {
                    _currentState = _AiState.attack;
                }
            }

            //idle, patrol, chase, attack, death, gameOver
            switch (_currentState)
            {
                case _AiState.idle:
                    _agent.isStopped = true;
                    break;
                case _AiState.patrol:
                    Patrol();
                    break;
                case _AiState.chase:
                    _agent.destination = _playerTransform.position;
                    break;
                case _AiState.attack:
                    Attack();
                    break;
                case _AiState.gameOver:
                    //start game over behavior
                    break;
                default:
                    print("There is no case for this Ai State.");
                    break;
            }
        }

        private void Patrol()
        {
            //move between two waypoints
            if (_waypoints != null)
            {
                _agent.destination = _waypoints[_nextWaypoint].position;
            }

            if (_agent.remainingDistance < 0.1f)
            {
                NewWayPoint();
            }
        }

        private void NewWayPoint()
        {
            //move foreward through waypoints
            _nextWaypoint++;

            //move foreward through waypoints
            if (_nextWaypoint == _waypoints.Length)
            {
                _nextWaypoint = 0;
            }

            _agent.destination = _waypoints[_nextWaypoint].position;
        }

        #endregion

        #region Event Methods

        private void PlayerManager_onPlayerChange()
        {
            //get active player object
            _playerTransform = PlayerManager.Instance.GetActivePlayer();
        }

        private void GameManager_onGameOver()
        {
            _isGameOver = true;

            //move toward spawn point
            Vector3 destination = SpawnManager.Instance.GetSpawnPosition();

            _agent.destination = destination;
        }

        private void GameManager_OnRoomComplete()
        {
            //destroy until ready for object pooling
            Destroy(this.gameObject);
        }

        #endregion

        #region Damage, Death

        public void EnemyAgentTakeDamage(int damage)
        {
            _enemyHealth.DamageUnit(damage);

            //Updates the current health on this Enemy Instance
            _currentHealth = _enemyHealth.Health;

            //Updatedamage Text and enemyHealthBar
            UIManager.Instance.InstantiateDamageText(damage, this.transform);
            UIManager.Instance.InstantiateEnemyHealthBar(_currentHealth,_maxHealth, this.transform);

            if (_enemyHealth.IsDead)
            {
                _collider.enabled = false;

                Die();
            }
            else
            {
                //play hit animation
                _animator.SetTrigger("Hit");
                Instantiate(_impactParticle, this.transform.position, Quaternion.identity);
            }
        }

        private void Die()
        {
            OnDeathFX();

            //use for testing room
            if (SpawnManager.Instance != null)
            {
                SpawnManager.Instance.RemoveEnemyCount();
                GameManager.Instance.AddPoints(_points);
                GameManager.Instance.AddKills();
            }
           
            if (_isBoss)
            {
                //room complete event
                GameManager.Instance.CompleteRoom();
            }

            Destroy(this.gameObject);
        }

        #endregion

        #region StateBehaviors and Coroutines

        public void Attack()
        {
            switch (_attackType)
            {
                case _AttackType.melee:
                    StartCoroutine(MeleeRoutine());
                    break;
                case _AttackType.shooter:
                    StartCoroutine(ShootRoutine());
                    break;
                default:
                    print("There is no case for this enemy attack type.");
                    break;
            }
        }

        private IEnumerator ShootRoutine()
        {
            _agent.isStopped = true;

            //aim and shoot
            while(_hasTarget == false)
            {
                //rotate towards target
                Vector3 targetDirection = _playerTransform.position - this.transform.position;

                //interpolate smooth rotation
                Quaternion towardsRotation = Quaternion.LookRotation(targetDirection, Vector3.up);

                //Rotate player towards movement direction
                transform.rotation = Quaternion.RotateTowards(transform.rotation, towardsRotation, _rotationSpeed * Time.deltaTime);

                //if aim direction is target direction, fire
                if (transform.rotation == towardsRotation)
                {
                    _hasTarget = true;

                    {
                        if (Time.time > _canFire)
                        {
                            _canFire = Time.time + _fireRate;

                            //fire
                            Instantiate(_enemyProjectile, _firePoint.position, this.transform.rotation);

                            //play rifle shoot animation
                            _animator.SetTrigger("Shoot");
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);

            _hasTarget = false;
            _agent.isStopped = false;

            if (_enemyToughness == _EnemyToughness.weak)
            {
                _currentState = _AiState.patrol;
            }
            else _currentState = _AiState.chase;
        }

        private IEnumerator MeleeRoutine()
        {
            _agent.isStopped = true;

            //trigger attack animation
            _animator.SetBool("isAttacking", true);

            yield return new WaitForSeconds(1f);

            _animator.SetBool("isAttacking", false);
            _agent.isStopped = false;
            _currentState = _AiState.chase;
        }

        #endregion

        private void OnDeathFX()
        {
            Instantiate(_electricityParticle, this.transform.position, Quaternion.identity);
            Instantiate(_sparksParticle, this.transform.position, Quaternion.identity);
            Instantiate(_deathPrefab, this.transform.position, this.transform.rotation);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_playerTag))
            {
                _player = PlayerManager.Instance.GetPlayer();
                _player.PlayerTakeDamage(_damage);

                if (_enemyToughness == _EnemyToughness.weak)
                {
                    OnDeathFX();

                    //use for testing room
                    if (SpawnManager.Instance != null)
                    {
                        SpawnManager.Instance.RemoveEnemyCount();
                    }

                    GameManager.Instance.AddKills();
                    Destroy(this.gameObject);
                }
                else if (_enemyToughness == _EnemyToughness.strong)
                {
                    _currentState = _AiState.attack;
                }
            }

            if (other.CompareTag(_mechTag))
            {
                _mech.MechTakeDamage(_damage);

                if (_enemyToughness == _EnemyToughness.weak)
                {
                    OnDeathFX();

                    //use for testing room
                    if (SpawnManager.Instance != null)
                    {
                        SpawnManager.Instance.RemoveEnemyCount();
                    }

                    GameManager.Instance.AddKills();
                    Destroy(this.gameObject);
                }
                else if (_enemyToughness == _EnemyToughness.strong)
                {
                    _currentState = _AiState.attack;
                }
            }

            if (other.CompareTag(_companionTag))
            {
                _companion = other.GetComponent<AttackCompanion>();
                _companion.CompanionTakeDamage(_damage);

                if (_enemyToughness == _EnemyToughness.weak)
                {
                    OnDeathFX();

                    //use for testing room
                    if (SpawnManager.Instance != null)
                    {
                        SpawnManager.Instance.RemoveEnemyCount();
                    }

                    GameManager.Instance.AddKills();
                    Destroy(this.gameObject);
                }
                else if (_enemyToughness == _EnemyToughness.strong)
                {
                    _currentState = _AiState.attack;
                }
            }

            if (other.CompareTag(_medicTag))
            {
                _medic = other.GetComponent<MedicCompanion>();
                _medic.CompanionTakeDamage(_damage);

                if (_enemyToughness == _EnemyToughness.weak)
                {
                    OnDeathFX();

                    //use for testing room
                    if (SpawnManager.Instance != null)
                    {
                        SpawnManager.Instance.RemoveEnemyCount();
                    }
                }
                else if (_enemyToughness == _EnemyToughness.strong)
                {
                    _currentState = _AiState.attack;
                }
            }

            if (other.CompareTag(_waypointTag))
            {
                if (_currentState is _AiState.patrol)
                {
                    NewWayPoint();
                }
            }
        }
    }
}

