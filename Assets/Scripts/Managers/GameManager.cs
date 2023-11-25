using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using CCS.Player;
using UnityEngine.Playables;

public class GameManager : MonoSingleton<GameManager>
{
    #region Variables
    
    [Header("Completed Totals")]
    public int _roomsCompleted;
    public int _levelsCompleted;
    public int _points;
    public int _totalPoints;
    public int _carrots;
    public int _totalCarrots;
    public int _destructableMultipliers;
    public int _turretsDestroyedCount;
    public int _totalTurretsDestroyedCount;
    public int _consolesDestroyedCount;
    public int _totalConsolesDestroyedCount;
    public int _enemiesKilled;
    public int _totalEnemiesKilled;
    public string _playerName;

    [Header("Time")]
    [SerializeField] private float _gameStartTime;
    [SerializeField] private float _roomStartTime;
    public float _roomElapsedTime;

    [Header("Bools")]
    [SerializeField] private bool _roomComplete = false;

    [Header("Time")]
    [SerializeField] private float _gameTime;
    [SerializeField] public float _timeRemaining = 60f;

    [Header("Room Objects")]
    [SerializeField] public GameObject[] _rooms;
    [SerializeField] private PlayableDirector _restartRoomDirector;

    //events
    public static event Action OnRoomComplete;//Display stats and Objective completed
    
    public static event Action OnRoomStart;//deactivate the UI

    public static event Action OnGameOver;//ui gameover

    public static event Action OnLevelComplete;//win Scenario

    public static event Action OnRestartRoom;//reload last room 

    #endregion

    public void OnEnable()
    {
        GameManager.OnRoomComplete += GameManager_onRoomComplete;
    }

    public void OnDisable()
    {
        GameManager.OnRoomComplete -= GameManager_onRoomComplete;
    }

    private void Awake()
    {
        if (LeaderBoardManager.Instance == null)
        {
            Debug.LogWarning("GameManager: LeaderboardManager instance is null!");
        }
    }

    #region Methods

    public void RoomComplete()
    {
        if (_roomsCompleted == _rooms.Length - 1)
        {
            _levelsCompleted++;

            //used for available levels
            PlayerPrefs.SetInt("CompletedLevelNumber", _levelsCompleted);

            OnLevelComplete?.Invoke();

            return;
        }

        if (_roomComplete == false)
        {
            _roomComplete = true;
            
            _roomsCompleted++;

            if (_roomsCompleted != _rooms.Length)
            {
                _rooms[_roomsCompleted].SetActive(true);
            }

            //raise event
            OnRoomComplete?.Invoke();
        }
    }

    public void GameOver()
    {
        _points = GetTotalPoints();
        
        OnGameOver?.Invoke();
    }

    public void StartNextRoom()
    {
        SaveGameData();
        _roomStartTime = Time.time; // Record the start time for the current room
        StartCoroutine(RoomTimerRoutine());
        
        _roomComplete = false;

        //not sure we need this 
        int previousRoom = _roomsCompleted - 1;

        int currentRoomIndex = _roomsCompleted;
        AudioSoundtrackManager.Instance.PlayAudioForRoom(currentRoomIndex);

        //add to total enemies killed
        _totalEnemiesKilled += _enemiesKilled;

        //reset enemies killed
        _enemiesKilled = 0;

        //invoke event OnRoomStart
        OnRoomStart?.Invoke();
    }
    
    public void RestartLevel()
    {
        // Remove any listeners that have been added to onGameOver event
        OnGameOver = null;
        // Reloads the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        // Reloads the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion

    #region GetData

    public int RoomsCompleted()
    {
        return _roomsCompleted;
    }

    public void AddKills()
    {
        _enemiesKilled++;

        if (_roomsCompleted == 0)
        {
            UIScoreManager.Instance.UpdateScoreText();
            CheckKillCount();
        }
            
    }

    private void CheckKillCount()
    {
        if (_enemiesKilled == 20)
        {
            //add to rooms completed
            _roomsCompleted++;

            //raise room complete event. 
            OnRoomComplete?.Invoke();
        }
    }

    public int GetKillCount()
    {
        return _enemiesKilled;
    }

    public void AddPoints(int points)
    {
        _points += points;
        UIScoreManager.Instance.UpdateScoreText();
    }

    public int GetTotalPoints()
    {
        _totalPoints += _points;
        return _totalPoints;
    }

    public void MultiplyPoints(int points)
    {
        _points *= points;
        UIScoreManager.Instance.UpdateScoreText();
    }

    public void AddCarrots(int carrots)
    {
        _carrots += carrots;
        UIScoreManager.Instance.UpdateScoreText();
    }

    public int GetTotalCarrots()
    {
        _totalCarrots += _carrots;
        return _totalCarrots;
    }

    public void SubtractCarrots(int carrots)
    {
        _carrots -= carrots;
        UIScoreManager.Instance.UpdateScoreText();
    }

    public void AddTurretsDestoyed(int turrets)
    {
        _turretsDestroyedCount += turrets;
        UIScoreManager.Instance.UpdateScoreText();
    }
    public int GetTotalTurretsDestroyed()
    {
        _totalTurretsDestroyedCount += _turretsDestroyedCount;
        return _totalTurretsDestroyedCount;
    }

    public void AddConsoleDestoyed(int consoles)
    {
        _consolesDestroyedCount += consoles;
        UIScoreManager.Instance.UpdateScoreText();
    }

    public int GetTotalConsoleDestroyed()
    {
        _totalConsolesDestroyedCount += _consolesDestroyedCount;
        return _totalConsolesDestroyedCount;
    }

    public void AddDestructableMult(int multiplier)
    {
        _destructableMultipliers += multiplier;
        UIScoreManager.Instance.UpdateScoreText();
    }

    public void UpdateScore(int scoreValue, int DestructaleMultiplier)
    {
        _points = scoreValue;
        _destructableMultipliers= DestructaleMultiplier;

        if(_destructableMultipliers <= 0)
        {
            _destructableMultipliers= 0;
        }
    }

    public int GetPoints()
    {
        return _points;
    }

    public int GetCarrots()
    {
        return _carrots;
    }

    #endregion

    public void StartTimer(float duration)
    {
        _timeRemaining = duration;
        UIScoreManager.Instance.SetTimerText();
        StartCoroutine(TimerRoutine());
    }

    public void CompleteRoom()
    {
        _roomsCompleted++;
        print(_roomsCompleted);

        if (_roomsCompleted == 10)
        {
            OnLevelComplete?.Invoke();
        }
        else
            //raise event
            OnRoomComplete?.Invoke();
    }

    private void GameManager_onRoomComplete()
    {
        _roomElapsedTime = Time.time - _roomStartTime; // Calculate the time taken for the current room

        if (_roomsCompleted == 1)
        {
            print(_roomsCompleted);
            SteamAchievements.Instance.CompleteFirstRoom();
        }

        SetBestRoomScores();
    }

    public void SetBestRoomScores()
    {
        int currentRoomIndex = _roomsCompleted - 1;
        RoomData currentRoomData = UIScoreManager.Instance._roomData[currentRoomIndex];

        // Update the best scores in the ScriptableObject directly
        if (_roomElapsedTime < currentRoomData.bestTime)
        {
            currentRoomData.bestTime = _roomElapsedTime;
            string formattedTime = currentRoomData.bestTime.ToString("F2");
            UIScoreManager.Instance._bestTimeToCompleteText.text = "Time To Complete Room: " + formattedTime + " New High!";
            UIScoreManager.Instance._bestTimeToCompleteText.color = Color.green; // Change text color to green
        }
        else if(_roomElapsedTime >= currentRoomData.bestTime)
        {
            // Condition not met, update the current room data with the current values
            UIScoreManager.Instance._bestTimeToCompleteText.text = "Time To Complete Room: " + currentRoomData.bestTime;
            UIScoreManager.Instance._bestTimeToCompleteText.color = Color.white; // Change text color back to white
        }

        if (_carrots > currentRoomData.bestCarrots || currentRoomData.bestCarrots == 0f)
        {
            currentRoomData.bestCarrots = _carrots;
            UIScoreManager.Instance._bestCarrotsText.text = "Carrots Collected: " + currentRoomData.bestCarrots + "   (New High!)";
            UIScoreManager.Instance._bestCarrotsText.color = Color.green;
        }
        else if(_carrots<= currentRoomData.bestCarrots)
        {
            // Condition not met, update the current room data with the current values
            UIScoreManager.Instance._bestCarrotsText.text = "Carrots Collected: " + currentRoomData.bestCarrots;
            UIScoreManager.Instance._bestCarrotsText.color = Color.white;
        }

        if (_turretsDestroyedCount > currentRoomData.bestTurretsDestroyedCount || currentRoomData.bestTurretsDestroyedCount == 0f)
        {
            currentRoomData.bestTurretsDestroyedCount = _turretsDestroyedCount;
            UIScoreManager.Instance._bestTurretsDestoyedText.text = "Turrets Destroyed: " + currentRoomData.bestTurretsDestroyedCount + "   (New High!)";
            UIScoreManager.Instance._bestTurretsDestoyedText.color = Color.green;
        }

        else if(_turretsDestroyedCount <= currentRoomData.bestTurretsDestroyedCount )
        {
            // Condition not met, update the current room data with the current values
            UIScoreManager.Instance._bestTurretsDestoyedText.text = "Turrets Destroyed: " + currentRoomData.bestTurretsDestroyedCount;
            UIScoreManager.Instance._bestTurretsDestoyedText.color = Color.white;
        }

        if (_consolesDestroyedCount > currentRoomData.bestConsolesDestroyedCount || currentRoomData.bestConsolesDestroyedCount == 0f)
        {
            currentRoomData.bestConsolesDestroyedCount = _consolesDestroyedCount;
            UIScoreManager.Instance._bestConsolesDestroyedText.text = "Consoles Destroyed: " + currentRoomData.bestConsolesDestroyedCount + "   (New High!)";
            UIScoreManager.Instance._bestConsolesDestroyedText.color= Color.green;
        }

        else if(_consolesDestroyedCount <= currentRoomData.bestConsolesDestroyedCount)
        {
            // if Condition not met, update the current room data with the current values
            UIScoreManager.Instance._bestConsolesDestroyedText.text = "Consoles Destroyed: " + currentRoomData.bestConsolesDestroyedCount;
            UIScoreManager.Instance._bestConsolesDestroyedText.color = Color.white;
        }

        if (_enemiesKilled > currentRoomData.bestEnemiesKilled || currentRoomData.bestEnemiesKilled == 0f)
        {
            currentRoomData.bestEnemiesKilled = _enemiesKilled;
            UIScoreManager.Instance._bestEnemiesKilledText.text = "Enemies Killed: " + currentRoomData.bestEnemiesKilled + "   (New High!)";
            UIScoreManager.Instance._bestEnemiesKilledText.color= Color.green;
        }

        else if(_enemiesKilled <= currentRoomData.bestEnemiesKilled)
        {
            // Condition not met, update the current room data with the current values
            UIScoreManager.Instance._bestEnemiesKilledText.text = "Enemies Killed: " + currentRoomData.bestEnemiesKilled;
            UIScoreManager.Instance._bestEnemiesKilledText.color = Color.white;
        }

        // After updating the best data, save it to the ScriptableObject
        UIScoreManager.Instance.SaveBestRoomData(currentRoomIndex, currentRoomData);
    }

    public void SaveGameData()
    {
        // Save the data to PlayerPrefs or any other storage mechanism you prefer
        SaveManager.SaveGame(_roomsCompleted, _points, _carrots);
    }

    public void LoadGameData()
    {
        UIManager.Instance.gameOverPanel.SetActive(false); // Show the game over panel
        
        int roomCompleted, points, carrots;
        SaveManager.LoadGame(out roomCompleted, out points, out carrots);

        _roomsCompleted = roomCompleted;
        _points = points;
        _carrots = carrots;

        UIScoreManager.Instance.UpdateScoreText();
        CarrotManager.Instance.RestartRoomCarrots(_roomsCompleted);
    }

    public void RestartRoomDirector()
    {
        AudioManager.Instance.StopSoundSFX();
        _restartRoomDirector.Play();
    }

    public void RestartRoom()
    {
        OnRestartRoom?.Invoke();
        LoadGameData();
    }

    private IEnumerator TimerRoutine()
    {
        while(_timeRemaining > 0f)
        {
            //subtract real time from time remaining
            _timeRemaining -= Time.deltaTime;

            if (_timeRemaining <= 0)
                _timeRemaining = 0;
            UIScoreManager.Instance.SetTimerText();
            yield return null;
        }

        CompleteRoom();
    }

    private IEnumerator RoomTimerRoutine()
    {
        while (true) // The timer will run until the game is complete
        {
            if (!PauseMenu.Instance.isPaused) // Only update time if the game is not paused
            {
                _roomElapsedTime = Time.time - _roomStartTime; // Calculate the total time taken for the game level
                                                               // Use 'totalElapsedTime' for tracking the total time taken for the entire game level
            }

            yield return null;
        }
    }
}
