using CCS.Health;
using CCS.Health.HealthBar;
using System.Collections;
using UnityEngine;

namespace CCS.Player
{
    [RequireComponent(typeof(CharacterController))]
    [System.Serializable]
    public class Player : MonoBehaviour
    {
        #region Variables

        [Header("CharacterController")]
        [SerializeField] private CharacterController _controller;

        [Header("Movement")]
        //movement speeds
        [SerializeField] public float _moveSpeed = 5f;
        [SerializeField] private float _currentSpeed;
        [SerializeField] private float _rotationSpeed = 500f;
        [SerializeField] private float _dashSpeed = 20f;
        [SerializeField] private float _gravity = 1f;
        [SerializeField] private float _yVelocity;
        [SerializeField] public int _playerID;
        private float _speedDurationValue;
        private float _speedMultiplier;
        public bool isMoving;

        //movement and rotation vectors
        private Vector2 _direction;
        private Vector2 _rotation;
        private Vector2 _mousePosition;
        private Vector3 _moveDirection;
        private Vector3 _vector3Zero = Vector3.zero;
        private Vector2 _vector2Zero = Vector2.zero;

        [Header("Animator")]
        //animation
        [SerializeField] public Animator _animator;

        //cache string names

        //animation states
        private const string _isMoving = "isMoving";
        private const string _isDead = "isDead";
        private const string _isShooting = "isShooting";
        //coroutines
        private const string _fireRoutine = "FireRoutine";
        private const string _mouseFireRoutine = "MouseFireRoutine";
        private const string _specialTimerRoutine = "SpecialWeaponTimer";
        private const string _dashRoutine = "DashRoutine";
        private const string _afterImageRoutine = "AfterImageRoutine";

        [Header("Weapon Variables")]
        //projectile variables
        [SerializeField] public GameObject[] _projectile;
        [SerializeField] private GameObject _muzzleFlash;
        [SerializeField] private GameObject _firePoint;
        [SerializeField] private static int _weaponLevel;
        [SerializeField] private float _fireRate = 0.3f;
        private float _canFire = 0f;
        private WaitForSeconds _fireSeconds;
        [SerializeField] private GameObject _objectToShoot;
        [SerializeField] public Sprite[] _projectileIcon;
        [SerializeField] private Rifle _rifle;

        [Header("Special Weapon Variables")]
        [SerializeField] private GameObject[] _specialWeapon;
        private int _specialWeaponID;
        private float _duration;
        private bool _specialWeaponActive = false;
        [SerializeField] public Sprite[] _specialWeaponsIcon;

        [Header("Health Variables")]
        public HealthBar _healthBar;
        public int _currentHealth = 100;
        public int _maxHealth = 100;
        public UnitHealth _playerHealth;

        [Header("Dash Variables")]
        private SkinnedMeshRenderer[] _skinnedMeshRenderers;
        [SerializeField] private Transform _positionToSpawn;
        [SerializeField] private Material _material;

        [Header("Toggles")]
        //toggle on and off to use mouse
        private bool _mouseEnabled;
        //toggle on and off to set firing
        private bool _isFiring = false;
        //check for mouse firing from input manager
        private bool _mouseFiring = false;

        //dash checks
        private bool _canDash = false;
        private bool _isDashing = false;

        public delegate void WeaponLevelChangedEventHandler(int newLevel);

        public event WeaponLevelChangedEventHandler WeaponLevelChanged;

        [SerializeField] private AudioClip _laserSound;

        private AudioManager _audioManager;

        #endregion

        #region Start, Enable, Disable, Update and NullCheck Funtions

        private void Awake()
        {
            GetInputMode();
        }

        // Start is called before the first frame update
        void Start()
        {
            UpdateIcon(GetWeaponsLevel());

            _playerHealth = new UnitHealth(_currentHealth, _maxHealth);
            _currentHealth = _maxHealth;
            _healthBar.UpdateHealth(_currentHealth, _maxHealth);
            _currentSpeed = _moveSpeed;

            //for current after image effect. subject to change
            _skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            if (_playerID == 2 || _playerID == 3)
            {
                if (PlayerManager.Instance.HealedArmor)
                {
                    RestoreHealth(_playerHealth.MaxHealth);
                }
            }

            //assign fire rate to coroutine wait
            _fireSeconds = new WaitForSeconds(_fireRate);

            //subscribe to onPlayerChange event
            PlayerManager.onPlayerChange += PlayerManager_OnPlayerChange;
        }

        public void GetInputMode()
        {
            int isMouseEnabled = PlayerPrefs.GetInt("IsMouseEnabled", 1);
            _mouseEnabled = isMouseEnabled == 1;
        }

        private void OnEnable()
        {
            //subscribe to game manager events
            GameManager.OnRoomComplete += GameManager_onRoomComplete;
            GameManager.OnGameOver += GameManager_onGameOver;
            GameManager.OnRoomStart += GameManager_onRoomStart;
            GameManager.OnLevelComplete += GameManager_onLevelComplete;
            GameManager.OnRestartRoom += GameManager_OnRestartRoom;

            //get rotation input
            _mouseEnabled = PlayerManager.Instance.GetMouseInput();

            //fire routine check
            _isFiring = PlayerManager.Instance.GetFiringState();
            _objectToShoot = _projectile[_weaponLevel];

            if (_isFiring)
                StartCoroutine(_fireRoutine);

            UpdateIcon(GetWeaponsLevel());

            _controller.transform.position = PlayerManager.Instance.GetPosition();

            _isDashing = false;
            _canDash = true;
        }

        private void GameManager_OnRestartRoom()
        {
            //set player position
            ResetPlayer();
        }

        private void OnDisable()
        {
            //unsibscribe to game manager events
            GameManager.OnRoomComplete -= GameManager_onRoomComplete;
            GameManager.OnGameOver -= GameManager_onGameOver;
            GameManager.OnRoomStart -= GameManager_onRoomStart;
            GameManager.OnRestartRoom -= GameManager_OnRestartRoom;

            //stop shooting
            _isFiring = false;
            StopCoroutine(_fireRoutine);
        }

        // Update is called once per frame
        void Update()
        {
            Movement();

            //change rotation if mouse is enabled
            if (_mouseEnabled)
            {
                MouseRotation();
            }
            else
                Rotation();
        }

        #endregion

        #region Event Methods

        private void GameManager_onLevelComplete()
        {
            //player level complete to be implemented
            print("Player Level Complete");
        }

        private void GameManager_onGameOver()
        {
            _isFiring = false;
            StopCoroutine(_fireRoutine);
            print("Player Game Over");
        }

        private void GameManager_onRoomComplete()
        {
            //player room complete to be implemented
            print("Player Room Complete");
        }

        private void GameManager_onRoomStart()
        {
            //player room start to be implemented
            print("Player Room Start");
        }

        private void PlayerManager_OnPlayerChange()
        {
            //Player instance changed
            print("Player Changed");
        }

        #endregion

        #region Move and Rotate Functions

        public void SetMovement(Vector2 direction)
        {
            //assign direction from input manager
            _direction = direction;

            //check input to set walk animation
            if (_direction.sqrMagnitude > 0.2f)
            {
                //set animator to walk animation
                _animator.SetBool(_isMoving, true);
                isMoving = true;
            }
            else 
            { 
                //set animator to idle animation
                _animator.SetBool(_isMoving, false);
                isMoving = false;
            }
        }

        public void SetRotation(Vector2 rotation)
        {
            //assign rotation to input manager
            _rotation = rotation;

            if (_rotation == Vector2.zero)
            {
                //stop shooting
                StopFireRoutine();
            }
        }

        public void SetMouseRotation(Vector2 mousePosition)
        {
            //assign mouse rotation to input manager
            _mousePosition = mousePosition;
        }

        private void Movement()
        {
            //set Y value to Z value for movement
            Vector3 forewardMovement = new Vector3(_direction.x, 0, _direction.y);

            //interpolate smooth movement
            _moveDirection = Vector3.Lerp(_moveDirection, forewardMovement, Time.deltaTime * 10f);

            //check if controller is grounded
            if (_controller.isGrounded)
            {
                //set Y velocity to zero
                _yVelocity = 0f;
            }
            else _yVelocity -= _gravity; //simulate gravity

            //assign controller velocity on the Y
            _moveDirection.y = _yVelocity;

            //move the character controller
            _controller.Move(_moveDirection * _currentSpeed * Time.deltaTime);

            //check if aiming with mouse or gamepad / keyboard
            if (_mouseEnabled)
            {
                //rotate towards movement direction if not shooting
                if (!_mouseFiring)
                {
                    if (forewardMovement != _vector3Zero)
                    {
                        //interpolate smooth rotation
                        Quaternion towardsRotation = Quaternion.LookRotation(forewardMovement, Vector3.up);

                        //Rotate player towards movement direction
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, towardsRotation, _rotationSpeed * Time.deltaTime);
                    }
                }
            }
            else
            {
                //if using gamepad / keyboard and not aiming, rotate towards movement direction
                if (_rotation == _vector2Zero)
                {
                    if (forewardMovement != _vector3Zero)
                    {
                        //interpolate smooth rotation
                        Quaternion towardsRotation = Quaternion.LookRotation(forewardMovement, Vector3.up);

                        //Rotate player towards movement direction
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, towardsRotation, _rotationSpeed * Time.deltaTime);
                    }
                }
            }
        }

        //gamepad / keyboard rotate towards aim direction
        private void Rotation()
        {
            //set Y value to Z value for movement
            Vector3 rotationDirection = new Vector3(_rotation.x, 0, _rotation.y);

            //check for rotation input
            if (rotationDirection != Vector3.zero)
            {
                //interpolate smooth rotation
                Quaternion towardsRotation = Quaternion.LookRotation(rotationDirection, Vector3.up);

                //check if facing aim direction before rotating towards it
                if (transform.rotation != towardsRotation)
                {
                    //player rotation
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, towardsRotation, _rotationSpeed * Time.deltaTime);
                }
                else if (Time.time > _canFire) //check shoot cooldown before shooting
                {
                    //reset shooting cooldown
                    _canFire = Time.time + _fireRate;

                    //start shooting
                    StartFireRoutine();
                }
            }
        }

        //mouse aim direciton
        public void MouseRotation()
        {
            if (_mouseFiring)
            {
                //use camera to get cursor position
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(_mousePosition.x, _mousePosition.y, Camera.main.transform.position.y));

                //direction to aim
                Vector3 lookDirection = mouseWorldPosition - transform.position;

                //lock Y axis 
                lookDirection.y = 0;
                lookDirection = lookDirection.normalized;

                //calculate rotation
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                Quaternion finalRotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);

                //calculate final rotation
                finalRotation.eulerAngles = new Vector3(0, finalRotation.eulerAngles.y, 0);

                //start shooting when within tolerance of final aim direction
                bool isFireReady = Quaternion.Angle(transform.rotation, finalRotation) <= 0.5f;

                if (isFireReady)
                {
                    if (Time.time > _fireRate) //check shooting cooldown
                    {
                        _canFire = Time.time + _fireRate;

                        //start shooting
                        StartMouseFireRoutine();

                        //continue rotating toward the cursor
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, finalRotation, _rotationSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    //rotate towards mouse cursor
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, finalRotation, _rotationSpeed * Time.deltaTime);
                }
            }
        }

        #endregion

        #region Damage, Heal and Death Functions

        public void PlayerTakeDamage(int damage)
        {
            //invincible while dashing
            if (_isDashing)
                return;

            //weapons upgrade reset
            if (_weaponLevel > 0)
            {
                //decrement weapon level when damaged
                _weaponLevel--;

                //get object to shoot after damage according to current weapon level
                _objectToShoot = _projectile[_weaponLevel];

                //set Ui to show current weapon
                UpdateIcon(_weaponLevel);
            }

            //player damage
            _playerHealth.DamageUnit(damage);

            //update health UI
            UpdateHealthBar(_playerHealth.Health, _playerHealth.MaxHealth);

            //base players only
            if (_playerID is 0 or 1)
            {
                //shake camera
                CameraManager.Instance.ShakeCamera(0.5f, 0.5f);

                if (_playerHealth.Health <= 0)
                {
                    Die();
                }
            }

            //red and blue armor
            if (_playerID is 2 or 3)
            {
                //shake camera
                CameraManager.Instance.ShakeCamera(0.2f, 0.2f);

                //get base player ID to revert to after armor is lost
                int originalPlayerID = PlayerManager.Instance.GetOriginalPlayerID();

                if (_playerHealth.Health <= 0)
                {
                    //set values before change
                    PlayerManager.Instance.SetFiringState(_isFiring);
                    PlayerManager.Instance.SetPlayerState(originalPlayerID);

                    UIManager.Instance.ResetTransform();

                    //heal armor for next pickup
                    RestoreHealth(_maxHealth);
                }
            }
        }

        //partial heal
        public void PlayerHeal(int healing)
        {
            _playerHealth.HealUnit(healing);
            UpdateHealthBar(_playerHealth.Health, _playerHealth.MaxHealth);
        }

        //full heal
        public void RestoreHealth(int maxHealth)
        {
            _playerHealth.Health = maxHealth;
            _playerHealth.RestoreHealth();
            _healthBar.UpdateHealth(_playerHealth.Health, _playerHealth.MaxHealth);

        }

        //set health bar UI
        public void UpdateHealthBar(int currentHealth, int maxHealth)
        {
            _healthBar.UpdateHealth(currentHealth, maxHealth);
        }

        //player death
        private void Die()
        {
            //raise game over event on game manager
            GameManager.Instance.GameOver();

            //death visuals
            _animator.SetBool(_isDead, true); //Set the Animator to isDead state

            //detach and drop rifle
            _rifle.DropRifle();
        }

        #endregion

        #region Public Coroutine Start Methods

        public void StartDashRoutine()
        {
            if (_canDash is true)
            {
                _canDash = false;
                _isDashing = true;
                StartCoroutine(_dashRoutine);
                StartCoroutine(_afterImageRoutine);
            }
        }

        public void ActivateSpecialWeapon(int weaponID, float duration)
        {
            _duration = duration;
            _specialWeaponID = weaponID;
            AddSpecialWeaponsIcon(_specialWeaponID);
            StartCoroutine(_specialTimerRoutine);
        }

        public void StartFireRoutine()
        {
            if (_isFiring == false)
            {
                _isFiring = true;
                _animator.SetBool(_isShooting, true);
                StartCoroutine(_fireRoutine);
            }
        }

        public void StopFireRoutine()
        {
            _isFiring = false;
            _animator.SetBool(_isShooting, false);
            StopCoroutine(_fireRoutine);
        }

        public void StartMouseFireRoutine()
        {
            if (_isFiring == false)
            {
                _isFiring = true;
                _animator.SetBool(_isShooting, true);
                StartCoroutine(_mouseFireRoutine);
            }
        }

        public void StopMouseFireRoutine()
        {
            _mouseFiring = false;
            _isFiring = false;
            _animator.SetBool(_isShooting, false);
            StopCoroutine(_mouseFireRoutine);
        }

        public void Teleport(Vector3 nextPosition)
        {
            StartCoroutine(TeleportRoutine(nextPosition));
        }

        public void ResetPlayer()
        {
            _animator.SetBool(_isDead, false); //Set the Animator to isDead state

            // Reset health variables
            _currentHealth = _maxHealth;
            _playerHealth = new UnitHealth(_currentHealth, _maxHealth);
            _healthBar.UpdateHealth(_currentHealth, _maxHealth);

            StartCoroutine(RespawnRoutine());
        }

        #endregion

        #region Get, Set Methods

        public void MouseFiring()
        {
            _mouseFiring = true;
        }

        public void SetFireState()
        {
            PlayerManager.Instance.SetFiringState(_isFiring);
        }

        public int GetWeaponsLevel()
        {
            return _weaponLevel;
        }

        public GameObject[] GetProjectiles()
        {
            return _projectile;
        }

        public void SetMouse(bool mouseEnabled)
        {
            _mouseEnabled = mouseEnabled;
        }

        public Vector3 GetDirection()
        {
            return _direction;
        }

        public bool GetMouseActive()
        {
            return _mouseEnabled;
        }

        #endregion

        #region Weapon Upgrades, Pickups

        public void WeaponUpgrade()
        {
            _weaponLevel++;

            _weaponLevel = Mathf.Clamp(_weaponLevel, 0, _projectile.Length - 1);

            if (_specialWeaponActive == false)
                _objectToShoot = _projectile[_weaponLevel];

            UpdateIcon(_weaponLevel);

            // Raise the WeaponLevelChanged event
            WeaponLevelChanged?.Invoke(_weaponLevel);
        }

        public void UpdateIcon(int newLevel)
        {
            Sprite icon = null;

            // Check if the new level is within the bounds of the weapon level array
            if (newLevel >= 0 && newLevel < _projectile.Length)
            {
                // Get the corresponding icon from the icons array
                icon = _projectileIcon[newLevel];

                //Set the sprite of the _iconHolderImage to the retrieved icon
                UIManager.Instance.ProjectileIconHolderImage.sprite = icon;
            }
            else
            {
                Debug.LogError("Invalid weapon level: " + newLevel);
            }
        }

        private void AddSpecialWeaponsIcon(int ID)
        {
            // Get the corresponding icon from the icons array
            Sprite icon = _specialWeaponsIcon[ID];

            UIManager.Instance.SpecialWeaponsIconHolderImage.enabled = true;
            UIManager.Instance.SpecialWeaponsIconHolderImage.sprite = icon;
        }

        private void RemoveSpecialWeaponsIcon()
        {
            UIManager.Instance.SpecialWeaponsIconHolderImage.enabled = false;
        }

        public void ActivateSpeedMultiplier(float speedMultiplier, float durationValue)
        {
            _speedDurationValue = durationValue;
            _speedMultiplier = speedMultiplier;
            StartCoroutine(SpeedRoutine());
        }

        #endregion

        #region Coroutines

        IEnumerator SpeedRoutine()
        {
            _currentSpeed = _currentSpeed * _speedMultiplier;
            yield return new WaitForSeconds(_speedDurationValue);
            _currentSpeed = _moveSpeed;
        }

        private IEnumerator DashRoutine()
        {
            _currentSpeed = _dashSpeed;
            UIManager.Instance.UpdateDashUI(_isDashing);

            //dash duration
            yield return new WaitForSeconds(0.3f);

            _isDashing = false;
            UIManager.Instance.UpdateDashUI(_isDashing);
            _currentSpeed = _moveSpeed;

            //dash cooldown timer
            yield return new WaitForSeconds(5f);

            _canDash = true;
        }

        private IEnumerator AfterImageRoutine()
        {
            while (_isDashing)
            {
                for (int i = 0; i < _skinnedMeshRenderers.Length; i++)
                {
                    GameObject afterImage = new GameObject();
                    afterImage.transform.SetPositionAndRotation(_positionToSpawn.position, _positionToSpawn.rotation);

                    MeshRenderer mr = afterImage.AddComponent<MeshRenderer>();
                    MeshFilter mf = afterImage.AddComponent<MeshFilter>();

                    Mesh mesh = new Mesh();
                    _skinnedMeshRenderers[i].BakeMesh(mesh);
                    mf.mesh = mesh;
                    mr.material = _material;

                    Destroy(afterImage, 0.5f);
                }

                yield return new WaitForSeconds(0.01f);
            }
        }

        private IEnumerator FireRoutine()
        {
            _isFiring= true;
            while (_isFiring)
            {
                _muzzleFlash.SetActive(true);

                //get pooled object to shoot
                ObjectPool.SpawnObject(_objectToShoot, _firePoint.transform.position, _firePoint.transform.rotation, ObjectPool.PoolType.Gameobject);

                _audioManager = FindObjectOfType<AudioManager>();
                _audioManager.PlayRandomizedSound(_laserSound);

                yield return _fireSeconds;
            }
        }

        private IEnumerator MouseFireRoutine()
        {
            _isFiring = true;
            while (_isFiring)
            {
                _muzzleFlash.SetActive(true);
               
                ObjectPool.SpawnObject(_objectToShoot, _firePoint.transform.position, _firePoint.transform.rotation, ObjectPool.PoolType.Gameobject);

                _audioManager = FindObjectOfType<AudioManager>();
                _audioManager.PlayRandomizedSound(_laserSound);

                yield return _fireSeconds;
            }
        }

        private IEnumerator SpecialWeaponTimer()
        {
            _specialWeaponActive = true;

            //change projectile to special 
            _objectToShoot = _specialWeapon[_specialWeaponID];
           
            yield return new WaitForSeconds(_duration);

            //change back to regular weapon
            _objectToShoot = _projectile[_weaponLevel];
            
            _specialWeaponActive = false;
            RemoveSpecialWeaponsIcon();
        }

        private IEnumerator TeleportRoutine(Vector3 nextPosition)
        {
            yield return new WaitForSeconds(0.2f);
            transform.position = nextPosition;
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(0.2f);

            Vector3 respawnPos = PlayerManager.Instance.GetSpawnPosition().position;
            
            transform.position = respawnPos;
            
            transform.rotation = PlayerManager.Instance.GetSpawnPosition().rotation;
        }

        #endregion
    }
}
