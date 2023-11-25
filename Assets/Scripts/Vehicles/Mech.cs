using CCS.Health;
using CCS.Health.HealthBar;
using CCS.Player;
using System.Collections;
using UnityEngine;


public class Mech : MonoBehaviour
{
    #region Variables

    [Header("Character Controller")]
    [SerializeField] private CharacterController _controller;

    [Header("Base Variables")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 500f;
    [SerializeField] private float _currentSpeed;
    private float _speedDurationValue;
    private float _speedMultiplier;

    //Movement and Rotation vectors
    //Input Direction and Rotation
    private Vector2 _direction;
    private Vector2 _rotation;

    //Adjusted Player Move Direction
    private Vector3 _moveDirection;

    //Cache Vector Zero
    private Vector3 _vectorZero = Vector3.zero;
    private Vector2 _mousePosition;

    //Spawn Position
    [Header("Spawn Position")]
    [SerializeField] private GameObject _mechPickup;

    [Header("Turret Variables")]
    [SerializeField] private Transform _turretTransform;
    [SerializeField] private Transform _bodyTransform;
    public Transform leftGun;
    public Transform rightGun;
    public float recoilDistance = 0.1f;
    public float recoilDuration = 0.1f;
    private Vector3 leftGunOriginalPosition;
    private Vector3 rightGunOriginalPosition;

    //projectile variables
    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _muzzleFlashLeft, _muzzleFlashRight;
    [SerializeField] private GameObject _firePointLeft, _firePointRight, _firePointMissileLeft, _firePointMissileRight;
    [SerializeField] private float _fireRate = 0.5f;
    private float _canFire = 0f;
    private WaitForSeconds _fireSeconds;
    private const string _fireRoutine = "FireRoutine";
    [SerializeField] private GameObject _objectToShoot;
    [SerializeField] public Sprite _projectileIcon;

    [Header("Special Weapon Variables")]
    [SerializeField] private GameObject[] _specialWeapon;
    private int _specialWeaponID;
    private float _duration;
    private const string _fireSpecialRoutine = "FireSpecialRoutine";
    private const string _specialTimerRoutine = "SpecialWeaponTimer";
    private const string _speedRoutine = "SpeedRoutine";
    private bool _missilesActive = false;
    [SerializeField] public Sprite[] _specialWeaponsIcon;

    [Header("Health Variables")]
    [SerializeField] private HealthBar _healthBar;
    [SerializeField] private int _currentHealth = 100;
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private GameObject _deathPrefab; // can delete later. Just for quick implementation
    public UnitHealth _mechHealth;

    [Header("Toggles")]
    //toggle on and off to set firing
    [SerializeField] private bool _isFiring = false;
    //toggle on and off to use mouse
    private bool _mouseEnabled;
    
    //Animation
    private Animator _animator;

    //Cache String Names
    //animation states
    private const string _isMoving = "isMoving";
    private const string _isShooting = "isShooting";
    private const string _isDead = "isDead";

    [SerializeField] private AudioClip _laserSound;

    private AudioManager _audioManager;

    #endregion

    #region Awake, Start, Update, NullCheck

    private void Awake()
    {       
        _fireSeconds = new WaitForSeconds(_fireRate); //assign fire rate to coroutine wait
        _animator = GetComponent<Animator>(); //assign Animator Component to the _animator Variable
        _mechHealth = new UnitHealth(_currentHealth, _maxHealth);
        _mouseEnabled = PlayerPrefs.GetInt("IsMouseEnabled", 1) == 1;

        transform.position = _mechPickup.transform.position;
        transform.rotation = _mechPickup.transform.rotation;
    }

    private void OnEnable()
    {
        _objectToShoot = _projectile;

        if (PlayerManager.Instance != null)
        //start fire routine?
            _isFiring = PlayerManager.Instance.GetFiringState();

        if (_isFiring)
            StartCoroutine(_fireRoutine);
    }

    void Start()
    {

        UpdateIcon();
        
        _currentSpeed = _moveSpeed;

        //Null check components
        NullChecks();
    }

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

    private void NullChecks()
    {
        if (_animator == null)
        {
            Debug.LogError("The Animator on the MECH is NULL.");
        }
    }

    #endregion

    #region Movement

    //assign direction from Input Manager
    public void SetMovement(Vector2 direction)
    {
        _direction = direction;
    }

    private void Movement()
    {
        //reassign Y input value to Z value for foreward movement
        Vector3 forewardMovement = new Vector3(_direction.x, 0, _direction.y);

        //interpolate smooth movement
        _moveDirection = Vector3.Lerp(_moveDirection, forewardMovement, Time.deltaTime * 10f);

        //Mech Movement (Old Rigidbody)
        transform.Translate(_moveDirection * _currentSpeed * Time.deltaTime, Space.World);

        //Mech Body Rotation
        if (forewardMovement != _vectorZero)
        {
            //interpolate smooth rotation
            Quaternion towardsRotation = Quaternion.LookRotation(forewardMovement, Vector3.up);

            //Rotate Mech Body towards movement direction
            _bodyTransform.rotation = Quaternion.RotateTowards(_bodyTransform.rotation, towardsRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    #endregion

    #region Rotation

    //assign Rotation from Input Manager
    public void SetRotation(Vector2 rotation)
    {
        _rotation = rotation;

        if (_rotation == Vector2.zero)
        {
            StopFireRoutine();
        }
    }

    //Aim with arrow keys/ right joystick
    private void Rotation()
    {
        //reassign Y input value to Z value for foreward movement
        Vector3 rotationDirection = new Vector3(_rotation.x, 0, _rotation.y);

        //check for rotation input
        if (rotationDirection != Vector3.zero)
        {
            //interpolate smooth rotation
            Quaternion towardsRotation = Quaternion.LookRotation(rotationDirection, Vector3.up);

            if (_turretTransform.rotation != towardsRotation)
            {
                //Rotate Turret towards rotation direction
                _turretTransform.rotation = Quaternion.RotateTowards(_turretTransform.rotation, towardsRotation, _rotationSpeed * Time.deltaTime);
            }
            else if (Time.time > _canFire)
            {
                _canFire = Time.time + _fireRate;
                StartFireRoutine();
            }
        }
    }

    //assign Mouse Rotation from Input Manager
    public void SetMouseRotation(Vector2 mouseposition)
    {
        _mousePosition = mouseposition;
    }

    public void StartFireRoutine()
    {
        if (_isFiring == false)
        {
            _isFiring = true;
            StartCoroutine(_fireRoutine);
        }
    }

    public void StopFireRoutine()
    {
        _isFiring = false;
        StopCoroutine(_fireRoutine);
    }

    //Mouse Aim
    private void MouseRotation()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(_mousePosition.x, _mousePosition.y, Camera.main.transform.position.y));
        Vector3 lookDirection = mouseWorldPosition - _turretTransform.position;
        lookDirection.y = 0;
        lookDirection = lookDirection.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        Quaternion finalRotation = Quaternion.Slerp(_turretTransform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
        finalRotation.eulerAngles = new Vector3(0, finalRotation.eulerAngles.y, 0);

        _turretTransform.rotation = finalRotation;
    }

    #endregion

    #region Health and Damage

    public void MechTakeDamage(int damage)
    {
        _mechHealth.DamageUnit(damage);
        
        _healthBar.UpdateMechHealth(_mechHealth.Health,_mechHealth.MaxHealth);
        
        if (_mechHealth.IsDead)
        {
            UIManager.Instance.ResetMechTransform();
            Die();
        }
    }

    public void MechHeal(int healing)
    {
        _mechHealth.HealUnit(healing);
        UpdateHealthBar(_mechHealth.Health, _mechHealth.MaxHealth);
    }

    public void RestoreHealth(int maxHealth)
    {
        _mechHealth.Health = maxHealth;
        _healthBar.UpdateHealth(_mechHealth.Health, _mechHealth.MaxHealth);
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        Debug.Log("UpdateHealth");
        _healthBar.UpdateMechHealth(currentHealth, maxHealth);
    }

    private void Die()
    {
        //dead mech
       
        int previousState = PlayerManager.Instance.GetPreviousID();
        PlayerManager.Instance.SetFiringState(_isFiring);
        PlayerManager.Instance.SetPlayerState(previousState);

        InputManager.Instance.DeactivateMech();
    }

    #endregion

    #region Activate Pickups

    public void ActivateSpecialWeapon(int weaponID, float duration)
    {
        _duration = duration;
        _specialWeaponID = weaponID;
        AddSpecialWeaponsIcon(_specialWeaponID);
        StartCoroutine(_specialTimerRoutine);
    }

    public void ActivateSpeedMultiplier(float speedMultiplier, float durationValue)
    {
        _speedDurationValue = durationValue;
        _speedMultiplier = speedMultiplier;
        StartCoroutine(_speedRoutine);
    }

    public void UpdateIcon()
    {
            // Get the corresponding icon from the icons array
            Sprite icon = _projectileIcon;

            //Set the sprite of the _iconHolderImage to the retrieved icon
            UIManager.Instance.ProjectileIconHolderImage.sprite = icon;
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

    #endregion

    #region Coroutines

    private IEnumerator FireRoutine()
    {
        while (_isFiring)
        {
            _muzzleFlashLeft.SetActive(true);
            _muzzleFlashRight.SetActive(true);
            
            ObjectPool.SpawnObject(_objectToShoot, _firePointLeft.transform.position, _firePointLeft.transform.rotation, ObjectPool.PoolType.Gameobject);
            _audioManager = FindObjectOfType<AudioManager>();

            _audioManager.PlayRandomizedSound(_laserSound);
            
            ObjectPool.SpawnObject(_objectToShoot, _firePointRight.transform.position, _firePointRight.transform.rotation, ObjectPool.PoolType.Gameobject);
            _audioManager = FindObjectOfType<AudioManager>();

            _audioManager.PlayRandomizedSound(_laserSound);
            yield return _fireSeconds;
        }
    }

    private IEnumerator SpecialWeaponTimer()
    {
        if (_specialWeaponID == 0)
        {
            _objectToShoot = _specialWeapon[_specialWeaponID];
        }
        else if (_specialWeaponID == 1)
        {
            _missilesActive = true;
            StartCoroutine(_fireSpecialRoutine);
        }

        yield return new WaitForSeconds(_duration);

        _missilesActive = false;
        RemoveSpecialWeaponsIcon();
        StopCoroutine(_fireSpecialRoutine);
        _objectToShoot = _projectile;
    }

    private IEnumerator FireSpecialRoutine()
    {
        UIManager.Instance.ActivateWeaponsIconSystem(_duration);

        while (_missilesActive)
        {
            yield return _fireSeconds;
            Instantiate(_specialWeapon[_specialWeaponID], _firePointMissileLeft.transform.position, _firePointMissileLeft.transform.rotation);
            yield return _fireSeconds;
            Instantiate(_specialWeapon[_specialWeaponID], _firePointMissileRight.transform.position, _firePointMissileRight.transform.rotation);
        }
    }

    IEnumerator SpeedRoutine()
    {
        _currentSpeed = _currentSpeed * _speedMultiplier;
        yield return new WaitForSeconds(_speedDurationValue);
        _currentSpeed = _moveSpeed;
    }

    #endregion

    public void Fire()
    {
        // Recoil left gun
        StartCoroutine(Recoil(leftGun, recoilDistance, recoilDuration));

        // Recoil right gun
        StartCoroutine(Recoil(rightGun, recoilDistance, recoilDuration));
    }

    private IEnumerator Recoil(Transform gun, float distance, float duration)
    {
        float timer = 0f;
        Vector3 startPos = gun.localPosition;
        Vector3 endPos = startPos - gun.forward * distance;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            gun.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        gun.localPosition = startPos;
    }
}
