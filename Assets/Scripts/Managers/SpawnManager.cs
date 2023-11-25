using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoSingleton<SpawnManager>
{
    #region Variables

    [Header("Spawn Waves")]
    [SerializeField] private List<SpawnWave> _spawnWaves = new List<SpawnWave>();
    [SerializeField] private int _currentWave;

    [Header("Spawn Points")]
    [SerializeField] public List<GameObject> _spawnPoints;
    [SerializeField] private GameObject[] _roomOneSpawnPoints;
    [SerializeField] private GameObject[] _roomTwoSpawnPoints;
    [SerializeField] private GameObject[] _roomThreeSpawnPoints;
    [SerializeField] private GameObject[] _roomFourSpawnPoints;
    [SerializeField] private GameObject[] _roomFiveSpawnPoints;
    [SerializeField] private GameObject[] _roomSixSpawnPoints;
    [SerializeField] private GameObject[] _roomSevenSpawnPoints;
    [SerializeField] private GameObject[] _roomEightSpawnPoints;
    [SerializeField] private GameObject[] _roomNineSpawnPoints;
    [SerializeField] private GameObject[] _roomTenSpawnPoints;
    [SerializeField] private GameObject[] _nextSpawnPoints;
    private int _randomSpawnPoint;

    [Header("Enemies")]
    [SerializeField] private GameObject[] _enemies;
    [SerializeField] private int _enemyCount;
    [SerializeField] private GameObject[] _enemiesRemaining;
    [SerializeField] private List<GameObject> _currentEnemies = new List<GameObject>();

    //boss objects
    [SerializeField] private GameObject _miniBoss;
    [SerializeField] private GameObject _miniBoss2;
    [SerializeField] private GameObject _daBoss;

    [Header("Rooms Completed")]
    [SerializeField] private int _roomsCompleted;

    [Header("Spawning Toggle")]
    [SerializeField] public bool _isSpawning;

    [Header("Spawn Timer")]
    private WaitForSeconds _spawnTimer;
    [SerializeField] private float _spawnRate = 4f;

    [Header("Pool Manager")]
    [SerializeField] private Transform _enemyParent;

    private const string _spawnRoutine = "SpawnRoutine";
    private const string _spawnWaveRoutine = "SpawnWaveRoutine";
    private const string _spawnPointRoutine = "SpawnPointRoutine";

    #endregion

    #region Start and OnDisable

    void Start()
    {
        GameManager.OnGameOver += GameManager_onGameOver;
        GameManager.OnRoomComplete += GameManager_onRoomComplete;
        GameManager.OnRoomStart += GameManager_onRoomStart;
        GameManager.OnLevelComplete += GameManager_onLevelComplete;

        foreach (GameObject spawnPoint in _roomOneSpawnPoints)
        {
            _spawnPoints.Add(spawnPoint);
        }

        _spawnTimer = new WaitForSeconds(_spawnRate);
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= GameManager_onGameOver;
        GameManager.OnRoomComplete -= GameManager_onRoomComplete; 
        GameManager.OnRoomStart -= GameManager_onRoomStart;
        GameManager.OnLevelComplete -= GameManager_onLevelComplete;
    }

    #endregion

    #region Event Methods

    private void GameManager_onLevelComplete()
    {
        _isSpawning = false;
        StopCoroutine(_spawnRoutine);
    }

    private void GameManager_onGameOver()
    {
        _isSpawning = false;
        StopCoroutine(_spawnRoutine);
    }

    private void GameManager_onRoomComplete()
    {
        //stop all spawning routines
        _isSpawning = false;

        //clear spawn points from list
        _spawnPoints.Clear();

        //check completed rooms to spawn correct routine
        _roomsCompleted = GameManager.Instance.RoomsCompleted();

        //old method to assign next spawn points
        switch (_roomsCompleted)
        {
            case 0:
                //assign player var to this Player script
                _nextSpawnPoints = _roomOneSpawnPoints;
                break;
            case 1:
                //assign player var to this Player script
                _nextSpawnPoints = _roomTwoSpawnPoints;
                break;
            case 2:
                //assign player var to this Player script
                _nextSpawnPoints = _roomThreeSpawnPoints;
                break;
            case 3:
                //assign player var to this Player script
                _nextSpawnPoints = _roomFourSpawnPoints;
                break;
            case 4:
                _nextSpawnPoints = _roomFiveSpawnPoints;
                break;
            case 5:
                _nextSpawnPoints = _roomSixSpawnPoints;
                break;
            case 6:
                _nextSpawnPoints = _roomSevenSpawnPoints;
                break;
            case 7:
                _nextSpawnPoints = _roomEightSpawnPoints;
                break;
            case 8:
                _nextSpawnPoints = _roomNineSpawnPoints;
                break;
            case 9:
                _nextSpawnPoints = _roomTenSpawnPoints;
                break;
            default:
                Debug.LogWarning("There is no case for this spawn point array");
                break;
        }

        foreach (GameObject spawnPoint in _nextSpawnPoints)
        {
            _spawnPoints.Add(spawnPoint); 
        }
    }

    private void GameManager_onRoomStart()
    {
        _isSpawning = true;

        //check completed rooms to spawn correct routine
        _roomsCompleted = GameManager.Instance.RoomsCompleted();

        switch (_roomsCompleted)
        {
            case 0:
                //spawn room 1 wave
                StartCoroutine(_spawnWaveRoutine);
                break;
            case 1:
                //reset enemy count
                _enemyCount = 0;
                //spawn room 2 routine
                StartCoroutine(_spawnPointRoutine);
                break;
            case 2:
                //spawn room 3 wave
                StartCoroutine(_spawnRoutine);
                GameManager.Instance.StartTimer(60f);
                break;
            case 3:
                //spawn room 4 wave
                StartCoroutine(_spawnRoutine);
                break;
            case 4:
                //spawn room 5 wave
                StartCoroutine(_spawnRoutine);
                GameManager.Instance.StartTimer(60f);
                break;
            case 5:
                //spawn room 6 wave
                StartCoroutine(_spawnRoutine);

                //set mini-boss active
                _miniBoss.SetActive(true);
                break;
            case 6:
                //spawn room 7 wave
                StartCoroutine(_spawnRoutine);
                GameManager.Instance.StartTimer(30f);
                break;
            case 7:
                //spawn room 8 wave
                //Lost in Space may need patrols to be set active
                break;
            case 8:
                //spawn room 9 wave
                StartCoroutine(_spawnRoutine);

                //set mini-boss active
                _miniBoss2.SetActive(true);
                break;
            case 9:
                //spawn room 10 wave
                StartCoroutine(_spawnRoutine);

                //set mini-boss active
                _daBoss.SetActive(true);
                break;
            default:
                Debug.LogWarning("There is no case for this room to spawn.");
                break;
        }
    }

    private void RoomComplete()
    {
        _spawnRate -= 1f;
        _spawnTimer = new WaitForSeconds(_spawnRate);

        //raise event
        GameManager.Instance.RoomComplete();
    }

    #endregion

    #region Remove Enemies and Spawn Points, Get Spawn Position

    public void RemoveEnemyCount()
    {
        _enemyCount--;
    }

    public void RemoveSpawnPoint(GameObject spawnPointOBJ)
    {
        _spawnPoints.Remove(spawnPointOBJ);

        if (_spawnPoints.Count == 0)
        {
            _isSpawning = false;
            StopCoroutine(_spawnRoutine);

            if (_enemyCount <= 0)
            {
                RoomComplete();
            }
            else
            {
                StartCoroutine(CheckForEnemiesRoutine());
            }
        }
    }

    public Vector3 GetSpawnPosition()
    {
        int randomPosition = Random.Range(0, _spawnPoints.Count);
        return _spawnPoints[randomPosition].transform.position;
    }

    #endregion

    public int SpawnWaveCount()
    {
        return _spawnWaves.Count;
    }

    public void DestroyAllEnemies()
    {
        foreach(GameObject enemy in _currentEnemies)
        {
            if (enemy != null)
                enemy.gameObject.SetActive(false);
        }

        _currentEnemies.Clear();
    }

    #region Coroutines

    private IEnumerator SpawnWaveRoutine()
    {
        while (_isSpawning)
        {
            List<GameObject> spawnSequence = _spawnWaves[_currentWave].spawnSequence;

            foreach (GameObject obj in spawnSequence)
            {
                _randomSpawnPoint = UnityEngine.Random.Range(0, _spawnPoints.Count);

                Instantiate(obj, _roomOneSpawnPoints[_randomSpawnPoint].transform.position, Quaternion.identity, _enemyParent.transform);

                //add spawned enemy to List
                _currentEnemies.Add(obj);

                yield return _spawnTimer;
            }

            if (_currentWave == _spawnWaves.Count - 1)
            {
                _isSpawning = false;
            }

            _currentWave++;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (_isSpawning)
        {
            _randomSpawnPoint = UnityEngine.Random.Range(0, _spawnPoints.Count);
            int randomEnemy = Random.Range(0, _enemies.Length);

            Instantiate(_enemies[randomEnemy], _spawnPoints[_randomSpawnPoint].transform.position, _spawnPoints[_randomSpawnPoint].transform.rotation, _enemyParent);

            //add spawned enemy to List
            _currentEnemies.Add(_enemies[randomEnemy]);

            _enemyCount++;

            yield return _spawnTimer;
        }
    }

    private IEnumerator SpawnPointRoutine()
    {
        while (_isSpawning)
        {
            if (_spawnPoints.Count <= 0)
            {
                _isSpawning = false;
                StopCoroutine(_spawnRoutine);
            }
            else
            {
                _randomSpawnPoint = UnityEngine.Random.Range(0, _spawnPoints.Count);
                int randomEnemy = Random.Range(0, _enemies.Length);

                Instantiate(_enemies[randomEnemy], _spawnPoints[_randomSpawnPoint].transform.position, _spawnPoints[_randomSpawnPoint].transform.rotation, _enemyParent);

                _enemyCount++;
            }

            yield return _spawnTimer;
        }
    }

    private IEnumerator CheckForEnemiesRoutine()
    {
        while (_enemyCount > 0)
        {
            yield return new WaitForSeconds(1f);
        }

        RoomComplete();
    }

    #endregion
}