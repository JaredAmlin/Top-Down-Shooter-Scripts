using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;
using System.Collections.Generic;
using CCS.Player;
using Steamworks;

public class PauseMenu : MonoSingleton<PauseMenu>
{
    #region Variables

    public Player _player;

    [Header("PauseMenuPanel")]
    [SerializeField] public GameObject _pauseMenuPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _leaderboardPanel;
    [SerializeField] private GameObject _pauseText;
    [SerializeField] private Button _quitButton;
    [SerializeField] private TextMeshProUGUI _versionText;

    [Header("SettingsPanel")]
    [SerializeField] private GameObject _graphicsMenu;
    [SerializeField] private GameObject _audioMenuPanel;
    [SerializeField] private GameObject _inputMenuPanel;

    [Header("AudioPanel")]
    [SerializeField] AudioMixer _audioMixer;
    [SerializeField] Slider _masterVolumeSlider;
    [SerializeField] Slider _sfxVolumeSlider;
    [SerializeField] Slider _soundtrackVolumeSlider;
    [SerializeField] Slider _weaponSfxVolumeSlider;
    [SerializeField] AudioClip _pauseClip;
    [SerializeField] AudioClip _weaponsFX;
    [SerializeField] AudioClip _sfx;

    [Header("InputPanel")]
    //[SerializeField] private bool _isMouseEnabled;
    //[SerializeField] private Toggle _mouseToggle;

    private bool _isMouseActive = false;

    #region Graphics Settings

    // This class is responsible for controlling the graphics menu in the game. It contains fields that
    // reference the UI elements used in the menu.

    // Fields for the resolutions section of the menu
    [Header("Resolutions")]
    [SerializeField] private TextMeshProUGUI _currentResolutionText;  // Reference to the text displaying the current resolution
    [SerializeField] private Button _nextResolutionButton;            // Reference to the button that selects the next resolution
    [SerializeField] private Button _previousResolutionButton;        // Reference to the button that selects the previous resolution
    private Resolution[] _resolutions;    // An array of available resolutions
    private int _currentResolutionIndex;  // The current resolution index

    // Fields for the quality settings section of the menu
    [Header("QualitySettings")]
    [SerializeField] private TextMeshProUGUI _qualityText;     // Reference to the text displaying the current quality setting
    [SerializeField] private Button _increaseQualityButton;    // Reference to the button that increases the quality setting
    [SerializeField] private Button _decreaseQualityButton;    // Reference to the button that decreases the quality setting

    // Fields for the VSync section of the menu
    [Header("VSync")]
    [SerializeField] private TextMeshProUGUI _vsyncText;   // Reference to the text displaying the current VSync setting
    [SerializeField] private Button _increaseVSyncButton;  // Reference to the button that increases the VSync setting
    [SerializeField] private Button _decreaseVSyncButton;  // Reference to the button that decreases the VSync setting
    private int _currentVSyncIndex; // The current VSync setting index
    private int _numVSyncOptions;   // The number of VSync options available

    // Fields for the anti-aliasing section of the menu
    [Header("Anti Aliasing")]
    [SerializeField] private TextMeshProUGUI _antiAliasingText;    // Reference to the text displaying the current anti-aliasing setting
    [SerializeField] private Button _increaseAntiAliasingButton;   // Reference to the button that increases the anti-aliasing setting
    [SerializeField] private Button _decreaseAntiAliasingButton;   // Reference to the button that decreases the anti-aliasing setting

    // Field for the fullscreen toggle in the menu
    [Header("Fullscreen Toggle")]
    [SerializeField] private Toggle _fullscreenToggle;  // Reference to the toggle for toggling fullscreen mode

    [SerializeField] private Button _graphicsBackButton;

    #endregion

    public bool isPaused = false;

    private GameObject _lastSelected;
    private bool _selected;

    [Header("FirstButton")]
    [SerializeField] private GameObject _pauseFirstButton;
    [SerializeField] private GameObject _settingsFirstButton;
    [SerializeField] private GameObject _audioFirstButton;
    [SerializeField] private GameObject _graphicsFirstButton;
    [SerializeField] private GameObject _inputFirstButton;
    [SerializeField] private GameObject _leaderboardFirstButton;

    [Header("CloseButton")]   
    [SerializeField] private GameObject _settingsCloseButton;
    [SerializeField] private GameObject _audioCloseButton;
    [SerializeField] private GameObject _graphicsCloseButton;
    [SerializeField] private GameObject _inputCloseButton;
    [SerializeField] private GameObject _leaderboardCloseButton;

    #endregion

    #region Start OnDestroy, and TogglePause funtions
   
    private void Start()
    {
        // Make sure the pause menu is hidden at the start of the game
        _pauseMenuPanel.SetActive(false);

        LoadQualitySettings();      // Load the current quality setting
        LoadResolutions();          // Load the available resolutions
        LoadAntiAliasingSetting();  // Load the current anti-aliasing setting
        LoadVSyncSetting();         // Load the current VSync setting
        LoadFullscreenSetting();    // Load the current fullscreen setting
        //LoadAudioBackButtonSetting();// Load the BackButton settings
        LoadGraphicsBackButtonSetting();    // Load the BackButton settings
        InitPauseMenuPanel();

        #region AudioPanelSettings inside Start
        // Set the initial slider values to the saved PlayerPrefs values
        float masterVolumeSaved = PlayerPrefs.GetFloat("MasterVolume", 1f);
        _masterVolumeSlider.value = masterVolumeSaved;

        float sfxVolumeSaved = PlayerPrefs.GetFloat("SFXVolume", 1f);
        _sfxVolumeSlider.value = sfxVolumeSaved;

        float soundtrackVolumeSaved = PlayerPrefs.GetFloat("SoundtrackVolume", 1f);
        _soundtrackVolumeSlider.value = soundtrackVolumeSaved;

        float weaponSfxVolumeSaved = PlayerPrefs.GetFloat("WeaponSFXVolume", 1f);
        _weaponSfxVolumeSlider.value = weaponSfxVolumeSaved;

        // Set the mixer values to match the saved values
        float mixerValueMasterSaved = Mathf.Log10(masterVolumeSaved) * 20f;
        _audioMixer.SetFloat("MasterVolume", mixerValueMasterSaved);

        float mixerValueSFXSaved = Mathf.Log10(sfxVolumeSaved) * 20f;
        _audioMixer.SetFloat("SFXVolume", mixerValueSFXSaved);

        float mixerValueSoundtrackSaved = Mathf.Log10(soundtrackVolumeSaved) * 20f;
        _audioMixer.SetFloat("SoundtrackVolume", mixerValueSoundtrackSaved);

        float mixerValueWeaponSFXSaved = Mathf.Log10(weaponSfxVolumeSaved) * 20f;
        _audioMixer.SetFloat("WeaponSFXVolume", mixerValueWeaponSFXSaved);

        #endregion

        #region input settings inside Start

        if (_isMouseActive)
        {
            //Clear Selected Object
            EventSystem.current.SetSelectedGameObject(null);

        }
        else
        {
            //Clear Selected Object
            EventSystem.current.SetSelectedGameObject(null);
            //set a new selected Object
            EventSystem.current.SetSelectedGameObject(_pauseFirstButton);
            _lastSelected = _pauseFirstButton;
        }
        
        // Get the current scene index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        Subscribe();
        #endregion
    }

    private void Subscribe()
    {
        //_input.UI.Pause.performed += OnMenuAction;
        //_playerInput.UI.MouseAction.performed += MouseAction_performed;
        //_playerInput.UI.Navigate.performed += Navigate_performed;
    }
    private void OnDisable()
    {
        // Disable the UI action map
        //_input.UI.Pause.performed -= OnMenuAction;
        //_playerInput.UI.MouseAction.performed -= MouseAction_performed;
        //_playerInput.UI.Navigate.performed -= Navigate_performed; ;
    }

    private void Navigate_performed(InputAction.CallbackContext obj)
    {
        if (_isMouseActive == true)
        {
            //deactivate mouse rotation
            _isMouseActive = false;
            
            //jared testing
            if (isPaused)
                UIManager.Instance.SetMouse(_isMouseActive);
        }

        //if the this gameobject is been selected then return else mainmenufirst
        if (!_selected)
        {
            //Clear Selected Object
            EventSystem.current.SetSelectedGameObject(null);
            //set a new selected Object
            EventSystem.current.SetSelectedGameObject(_lastSelected);
            
            _selected = true;
        }

    }

    public void Navigate()
    {
        /*
        if (_isMouseActive == true)
        {
            //deactivate mouse rotation
            _isMouseActive = false;

            //jared testing
            if (isPaused)
                UIManager.Instance.SetMouse(_isMouseActive);
        }
        */

        //if the this gameobject is been selected then return else mainmenufirst
        if (!_selected)
        {
            //Clear Selected Object
            EventSystem.current.SetSelectedGameObject(null);
            //set a new selected Object
            EventSystem.current.SetSelectedGameObject(_lastSelected);

            _selected = true;
        }
    }


    private void MouseAction_performed(InputAction.CallbackContext obj)
    {
        if (_isMouseActive == false)
        {
            //activate mouse rotation
            _isMouseActive = true;

            //jared testing
            if (isPaused)
                UIManager.Instance.SetMouse(_isMouseActive);
        }

        EventSystem.current.SetSelectedGameObject(null);
        _selected = false;

    }

    public void MouseAction()
    {
        /*
        if (_isMouseActive == false)
        {
            //activate mouse rotation
            _isMouseActive = true;

            //jared testing
            if (isPaused)
                UIManager.Instance.SetMouse(_isMouseActive);
        }
        */

        EventSystem.current.SetSelectedGameObject(null);
        _selected = false;
    }

    private void InitPauseMenuPanel()
    {
#if UNITY_EDITOR
        _quitButton.interactable = false;
#endif
        //VersionTextMainMenu
        _versionText.text = "Version: " + Application.version;
    }


    private void OnMenuAction(InputAction.CallbackContext context)
    {
        // Toggle the pause state
        isPaused = !isPaused;

        // If the game is paused, show the pause menu
        if (isPaused)
        {
            Time.timeScale = 0; // Stop time
            _pauseMenuPanel.SetActive(true); // Show pause menu
                                             //Clear Selected Object
            EventSystem.current.SetSelectedGameObject(null);
            //set a new selected Object
            EventSystem.current.SetSelectedGameObject(_pauseFirstButton);

            // Play the pause clip on the soundtrack mixer
            AudioManager.Instance.PlaySound(_pauseClip, AudioManager.Instance.SoundTrack);
        }

        else
        {
            Time.timeScale = 1; // Resume time
            _pauseMenuPanel.SetActive(false); // Hide pause menu
            AudioManager.Instance.StopSoundSFX();
        }
    }

    //new pause method
    public void Pause()
    {
        Debug.Log("The Game is Paused");
        // Toggle the pause state
        isPaused = !isPaused;

        // If the game is paused, show the pause menu
        if (isPaused)
        {
            Time.timeScale = 0; // Stop time
            _pauseMenuPanel.SetActive(true); // Show pause menu
                                             //Clear Selected Object
            EventSystem.current.SetSelectedGameObject(null);
            //set a new selected Object
            EventSystem.current.SetSelectedGameObject(_pauseFirstButton);

            // Play the pause clip on the soundtrack mixer
            AudioManager.Instance.PlaySound(_pauseClip, AudioManager.Instance.SoundTrack);
        }

        else
        {
            Time.timeScale = 1; // Resume time
            _pauseMenuPanel.SetActive(false); // Hide pause menu
            AudioManager.Instance.StopSoundSFX();
        }
    }
   
      #endregion

        #region PauseMenuPanel

    public void ResumeGame()
    {
        Debug.Log("The Game is UnPaused");

        // Set the pause state to false
        isPaused = false;

        // Resume time
        Time.timeScale = 1;

        // Hide the pause menu
        _pauseMenuPanel.SetActive(false);
        AudioManager.Instance.StopSoundSFX();
        // Get the current scene index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        InputManager.Instance.UnPause();
    }

    public void RestartGame()
    {
        // Resume time
        Time.timeScale = 1;

        GameManager.Instance.RestartLevel();
    }

    public void OpenSettings()
    {
        // Hide the pause menu
        _pauseMenuPanel.SetActive(false);

        // Show the settings panel
        _settingsPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_settingsFirstButton);
        _lastSelected = _settingsFirstButton;
        OpenMenu();
    }

    public void OpenLeaderboardPanel()
    {
        _leaderboardPanel.SetActive(true);
        // Hide the pause menu
        _pauseMenuPanel.SetActive(false);

        EventSystem.current.SetSelectedGameObject(_leaderboardFirstButton);
        _lastSelected = _leaderboardFirstButton;
        OpenMenu();

        // Check if Steamworks is initialized
        if (!SteamAPI.IsSteamRunning())
        {
            return;
        }
        else
        {
            // Load the current leaderboard data
            LeaderBoardManager.Instance.GetLeaderBoardData();
        }
    }

    public void OnLeaderboardBackButton()
    {
        _leaderboardPanel.SetActive(false);
        // Hide the pause menu
        _pauseMenuPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_leaderboardCloseButton);
        _lastSelected = _leaderboardCloseButton;
        OpenMenu();
    }
    public void BackToMainMenu()
    {
        // Resume time
        Time.timeScale = 1;

        // Load scene with index 0 (assuming that's the scene for the main menu)
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
    }

    #endregion

    #region SettingsPanel

    public void OnGraphicsButtonClicked()
    {
        //_clickButtonAudio.Play();
        _settingsPanel.SetActive(false);
        _graphicsMenu.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_graphicsFirstButton);
        _lastSelected = _graphicsFirstButton;
        OpenMenu();
    }

    public void OnAudioButtonClicked()
    {
        //_clickButtonAudio.Play();
        _settingsPanel.SetActive(false);
        _audioMenuPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_audioFirstButton);
        _lastSelected = _audioFirstButton;
        OpenMenu();
    }

    public void OnInputButtonClicked()
    {
        //_clickButtonAudio.Play();
        _settingsPanel.SetActive(false);
        _inputMenuPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_inputFirstButton);
        _lastSelected = _inputFirstButton;
        OpenMenu();
    }

    public void OnSettingsBackButtonClicked()
    {
        //_clickButtonAudio.Play();
        _settingsPanel.SetActive(false);
        _pauseMenuPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_settingsCloseButton);
        _lastSelected = _settingsCloseButton;
        OpenMenu();
    }

    #endregion

    #region Graphics Function

    #region Quality Settings

    // This method loads the current quality setting from PlayerPrefs and sets the initial quality level.
    private void LoadQualitySettings()
    {
        // Load quality settings from PlayerPrefs.
        int qualityValue = PlayerPrefs.GetInt("QualitySettings", 1);

        // Set the quality level.
        QualitySettings.SetQualityLevel(qualityValue);

        // Set the initial quality name in the UI.
        _qualityText.text = QualitySettings.names[qualityValue];

        // Set up quality increase and decrease buttons.
        _increaseQualityButton.onClick.AddListener(IncreaseQuality);
        _decreaseQualityButton.onClick.AddListener(DecreaseQuality);
    }

    // This method is called when the user clicks the "increase quality" button.
    private void IncreaseQuality()
    {
        // Get the current quality level.
        int currentQualityIndex = QualitySettings.GetQualityLevel();

        // Calculate the next quality level.
        int nextQualityIndex = currentQualityIndex + 1;

        // Check if the next quality level is within the range of available quality levels.
        if (nextQualityIndex < QualitySettings.names.Length)
        {
            // Set the new quality level.
            QualitySettings.SetQualityLevel(nextQualityIndex);

            // Update the quality name in the UI.
            _qualityText.text = QualitySettings.names[nextQualityIndex];

            // Save the new quality level in PlayerPrefs.
            PlayerPrefs.SetInt("QualitySettings", nextQualityIndex);
            PlayerPrefs.Save();
        }
    }

    // This method is called when the user clicks the "decrease quality" button.
    private void DecreaseQuality()
    {
        // Get the current quality level.
        int currentQualityIndex = QualitySettings.GetQualityLevel();

        // Calculate the previous quality level.
        int prevQualityIndex = currentQualityIndex - 1;

        // Check if the previous quality level is within the range of available quality levels.
        if (prevQualityIndex >= 0)
        {
            // Set the new quality level.
            QualitySettings.SetQualityLevel(prevQualityIndex);

            // Update the quality name in the UI.
            _qualityText.text = QualitySettings.names[prevQualityIndex];

            // Save the new quality level in PlayerPrefs.
            PlayerPrefs.SetInt("QualitySettings", prevQualityIndex);
            PlayerPrefs.Save();
        }
    }

    #endregion

    #region Resolution Settings

    // This function loads the available screen resolutions and sets the initial resolution
    public void LoadResolutions()
    {
        // Get all available screen resolutions
        _resolutions = Screen.resolutions;

        // Check if _resolutions is empty
        if (_resolutions.Length == 0)
        {
            Debug.LogError("No resolutions available!");
            return;
        }

        // Use a hashset to store unique resolution options
        HashSet<string> uniqueResolutions = new HashSet<string>();

        // Loop through all available resolutions
        foreach (Resolution resolution in _resolutions)
        {
            // Create a string option for each resolution
            string option = resolution.width + " x " + resolution.height;

            // Add the option to the hashset if it is not already there
            if (!uniqueResolutions.Contains(option))
            {
                uniqueResolutions.Add(option);
            }
        }

        // Convert the hashset to a list and set the initial resolution text to the first option
        List<string> resolutionOptions = new List<string>(uniqueResolutions);
        _currentResolutionText.text = resolutionOptions[0];

        // Check if a resolution has been previously saved in PlayerPrefs
        if (PlayerPrefs.HasKey("ScreenResolutionWidth") && PlayerPrefs.HasKey("ScreenResolutionHeight"))
        {
            int savedWidth = PlayerPrefs.GetInt("ScreenResolutionWidth");
            int savedHeight = PlayerPrefs.GetInt("ScreenResolutionHeight");

            // Find the index of the saved resolution in the available resolutions array
            _currentResolutionIndex = Array.FindIndex(_resolutions, r => r.width == savedWidth && r.height == savedHeight);
        }
        else
        {
            // If no resolution has been saved, set the current resolution to the current screen resolution
            _currentResolutionIndex = Array.FindIndex(_resolutions, r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
        }

        // Set the resolution to the initial resolution
        SetResolution(_currentResolutionIndex);

        // Set up quality increase and decrease buttons.
        _nextResolutionButton.onClick.AddListener(NextResolution);
        _previousResolutionButton.onClick.AddListener(PreviousResolution);
    }

    // This function sets the screen resolution based on the index of the selected resolution
    public void SetResolution(int resolutionIndex)
    {
        // Get the resolution from the resolutions array
        Resolution resolution = _resolutions[resolutionIndex];

        // Set the screen resolution
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        // Update the current resolution text to reflect the new resolution
        _currentResolutionText.text = resolution.width + " x " + resolution.height;

        // Save the resolution to PlayerPrefs
        PlayerPrefs.SetInt("ScreenResolutionWidth", resolution.width);
        PlayerPrefs.SetInt("ScreenResolutionHeight", resolution.height);
    }

    // This function selects the next available resolution in the resolutions array
    public void NextResolution()
    {
        // Increment the current resolution index by 1 and wrap around to the beginning of the array if it goes out of bounds
        _currentResolutionIndex++;
        if (_currentResolutionIndex >= _resolutions.Length)
        {
            _currentResolutionIndex = 0;
        }

        // Set the new resolution
        SetResolution(_currentResolutionIndex);
    }

    // This function selects the previous available resolution in the resolutions array
    public void PreviousResolution()
    {
        // Decrement the current resolution index by 1 and wrap around to the end of the array if it goes out of bounds
        _currentResolutionIndex--;
        if (_currentResolutionIndex < 0)
        {
            _currentResolutionIndex = _resolutions.Length - 1;
        }

        // Set the new resolution
        SetResolution(_currentResolutionIndex);
    }

    #endregion

    #region Anti-Aliasing Settings

    // This method loads the anti-aliasing setting and sets up the UI buttons for increasing/decreasing it.
    public void LoadAntiAliasingSetting()
    {
        // Load anti-aliasing setting from player preferences
        int antiAliasingValue = PlayerPrefs.GetInt("AntiAliasing", 1);
        // Set the anti-aliasing level based on the loaded value
        QualitySettings.antiAliasing = antiAliasingValue * 2;

        // Set the initial text of the anti-aliasing UI element
        _antiAliasingText.text = GetAntiAliasingName(antiAliasingValue);

        // Set up the button click listeners for increasing/decreasing the anti-aliasing level
        _increaseAntiAliasingButton.onClick.AddListener(IncreaseAntiAliasing);
        _decreaseAntiAliasingButton.onClick.AddListener(DecreaseAntiAliasing);
    }

    // This method increases the anti-aliasing level when the corresponding button is clicked.
    private void IncreaseAntiAliasing()
    {
        // Get the current anti-aliasing value by dividing the QualitySettings value by 2.
        int currentAntiAliasingValue = QualitySettings.antiAliasing / 2;
        // Calculate the next anti-aliasing value.
        int nextAntiAliasingValue = currentAntiAliasingValue + 1;
        // Check if the next value is within the valid range of anti-aliasing levels (0-3).
        if (nextAntiAliasingValue < 4)
        {
            // Set the new anti-aliasing level in QualitySettings.
            QualitySettings.antiAliasing = nextAntiAliasingValue * 2;
            // Update the UI text to reflect the new level.
            _antiAliasingText.text = GetAntiAliasingName(nextAntiAliasingValue);
            // Save the new anti-aliasing level to player preferences.
            PlayerPrefs.SetInt("AntiAliasing", nextAntiAliasingValue);
            PlayerPrefs.Save();
        }
    }

    // This method decreases the anti-aliasing level when the corresponding button is clicked.
    private void DecreaseAntiAliasing()
    {
        // Get the current anti-aliasing value by dividing the QualitySettings value by 2.
        int currentAntiAliasingValue = QualitySettings.antiAliasing / 2;
        // Calculate the previous anti-aliasing value.
        int prevAntiAliasingValue = currentAntiAliasingValue - 1;
        // Check if the previous value is within the valid range of anti-aliasing levels (0-3).
        if (prevAntiAliasingValue >= 0)
        {
            // Set the new anti-aliasing level in QualitySettings.
            QualitySettings.antiAliasing = prevAntiAliasingValue * 2;
            // Update the UI text to reflect the new level.
            _antiAliasingText.text = GetAntiAliasingName(prevAntiAliasingValue);
            // Save the new anti-aliasing level to player preferences.
            PlayerPrefs.SetInt("AntiAliasing", prevAntiAliasingValue);
            PlayerPrefs.Save();
        }
    }

    // This method returns the name of the anti-aliasing level for a given integer value.
    private string GetAntiAliasingName(int antiAliasingValue)
    {
        switch (antiAliasingValue)
        {
            case 0:
                return "Disabled";
            case 1:
                return "2x";
            case 2:
                return "4x";
            case 3:
                return "8x";
            default:
                return "";
        }
    }

    #endregion

    #region VSync Settings

    // This method is called to load the current VSync setting
    public void LoadVSyncSetting()
    {
        // Initialize the number of VSync options
        _numVSyncOptions = 3;

        // Load the VSync setting from player prefs, or if it doesn't exist, use the current system VSync count
        _currentVSyncIndex = PlayerPrefs.GetInt("VSync", QualitySettings.vSyncCount);
        QualitySettings.vSyncCount = _currentVSyncIndex;

        // Set the VSync text based on the current VSync setting
        _vsyncText.text = GetVSyncOptionText(_currentVSyncIndex);

        // Set up the VSync increase and decrease buttons
        _increaseVSyncButton.onClick.AddListener(IncreaseVSync);
        _decreaseVSyncButton.onClick.AddListener(DecreaseVSync);
    }

    // This method returns the text associated with a given VSync index
    private string GetVSyncOptionText(int index)
    {
        switch (index)
        {
            case 0:
                return "Don't Sync";
            case 1:
                return "Every V Blank";
            case 2:
                return "Every Second V Blank";
            default:
                return "";
        }
    }

    // This method is called when the user clicks the "Increase VSync" button
    private void IncreaseVSync()
    {
        // Increment the current VSync index and wrap around if necessary
        _currentVSyncIndex = (_currentVSyncIndex + 1) % _numVSyncOptions;

        // Set the new VSync count and update the text on the screen
        QualitySettings.vSyncCount = _currentVSyncIndex;
        _vsyncText.text = GetVSyncOptionText(_currentVSyncIndex);

        // Save the new VSync setting to player prefs
        PlayerPrefs.SetInt("VSync", _currentVSyncIndex);
        PlayerPrefs.Save();
    }

    // This method is called when the user clicks the "Decrease VSync" button
    private void DecreaseVSync()
    {
        // Decrement the current VSync index and wrap around if necessary
        _currentVSyncIndex = (_currentVSyncIndex - 1 + _numVSyncOptions) % _numVSyncOptions;

        // Set the new VSync count and update the text on the screen
        QualitySettings.vSyncCount = _currentVSyncIndex;
        _vsyncText.text = GetVSyncOptionText(_currentVSyncIndex);

        // Save the new VSync setting to player prefs
        PlayerPrefs.SetInt("VSync", _currentVSyncIndex);
        PlayerPrefs.Save();
    }

    #endregion

    #region Fullscreen Settings

    public void LoadFullscreenSetting()
    {
        // Load FullScreen setting from PlayerPrefs
        int fullscreenValue = PlayerPrefs.GetInt("fullscreen", 1); // Default to 1 if no value is found

        // Set the value of the FullScreen toggle based on the loaded value
        _fullscreenToggle.isOn = fullscreenValue == 1 ? true : false;
    }

    // Called when the FullScreen toggle is clicked
    public void OnFullScreenToggle()
    {
        // Set the FullScreen mode to the value of the toggle
        Screen.fullScreen = _fullscreenToggle.isOn;

        // Save the state of the toggle to PlayerPrefs
        PlayerPrefs.SetInt("fullscreen", _fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    #endregion

    #region GraphicsBack Button Settings

    public void LoadGraphicsBackButtonSetting()
    {
        _graphicsBackButton.onClick.AddListener(OnGraphicsBackButtonClicked);
    }
    public void OnGraphicsBackButtonClicked()
    {
        //_clickButtonAudio.Play();
        _graphicsMenu.SetActive(false);
        _settingsPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_graphicsCloseButton);
        _lastSelected = _graphicsCloseButton;
        OpenMenu();
    }

    #endregion

    #endregion

    #region AudioPanel

    public void SetMasterVolume(float volume)
    {
        // Convert the slider value to decibels and set the mixer value
        float mixerValue = Mathf.Log10(volume) * 20f;
        _audioMixer.SetFloat("MasterVolume", mixerValue);

        // Save the value in PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", volume);

        // Get the saved value and update the slider
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        _masterVolumeSlider.value = masterVolume;

        // Set the mixer value to match the saved value
        float mixerValueSaved = Mathf.Log10(masterVolume) * 20f;
        _audioMixer.SetFloat("MasterVolume", mixerValueSaved);

    }

    public void SetSFXVolume(float volume)
    {
        // Convert the slider value to decibels and set the mixer value
        float mixerValue = Mathf.Log10(volume) * 20f;
        _audioMixer.SetFloat("SFXVolume", mixerValue);

        // Save the value in PlayerPrefs
        PlayerPrefs.SetFloat("SFXVolume", volume);

        // Get the saved value and update the slider
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        _sfxVolumeSlider.value = sfxVolume;

        // Set the mixer value to match the saved value
        float mixerValueSaved = Mathf.Log10(sfxVolume) * 20f;
        _audioMixer.SetFloat("SFXVolume", mixerValueSaved);
    }

    public void SetSoundtrackVolume(float volume)
    {
        // Convert the slider value to decibels and set the mixer value
        float mixerValue = Mathf.Log10(volume) * 20f;
        _audioMixer.SetFloat("SoundtrackVolume", mixerValue);

        // Save the value in PlayerPrefs
        PlayerPrefs.SetFloat("SoundtrackVolume", volume);

        // Get the saved value and update the slider
        float savedVolume = PlayerPrefs.GetFloat("SoundtrackVolume", 1f);
        _soundtrackVolumeSlider.value = savedVolume;

        // Set the mixer value to match the saved value
        float mixerValueSaved = Mathf.Log10(savedVolume) * 20f;
        _audioMixer.SetFloat("SoundtrackVolume", mixerValueSaved);
    }

    public void SetWeaponSFXVolume(float volume)
    {
        // Convert the slider value to decibels and set the mixer value
        float mixerValue = Mathf.Log10(volume) * 20f;
        _audioMixer.SetFloat("WeaponSFXVolume", mixerValue);

        // Save the value in PlayerPrefs
        PlayerPrefs.SetFloat("WeaponSFXVolume", volume);

        // Get the saved value and update the slider
        float savedVolume = PlayerPrefs.GetFloat("WeaponSFXVolume", 1f);
        _weaponSfxVolumeSlider.value = savedVolume;

        // Set the mixer value to match the saved value
        float mixerValueSaved = Mathf.Log10(savedVolume) * 20f;
        _audioMixer.SetFloat("WeaponSFXVolume", mixerValueSaved);
    }
    public void OnAudioBackButtonClicked()
    {
        _audioMenuPanel.SetActive(false);
        _settingsPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_audioCloseButton);
        _lastSelected = _audioCloseButton;
        OpenMenu();
    }

    public void PlayTestWeapons()
    {
        AudioManager.Instance.PlaySound(_weaponsFX, AudioManager.Instance.WeaponsFX);
    }

    public void PlayTestSFX()
    {
        AudioManager.Instance.PlaySound(_sfx, AudioManager.Instance.SFX); ;
    }

    #endregion

    #region InputSettings
   
    public void OnInputBackButtonClicked()
    {
        //_clickButtonAudio.Play();
        _inputMenuPanel.SetActive(false);
        _settingsPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(_inputCloseButton);
        _lastSelected = _inputCloseButton;
        OpenMenu();

        //if (_isMouseActive)
        //{
        //    //Clear Selected Object
        //    EventSystem.current.SetSelectedGameObject(null);
        //}
        //else
        //{
        //    EventSystem.current.SetSelectedGameObject(_inputCloseButton);
        //    _lastSelected = _inputCloseButton;
        //}

        /*
        //Clear Selected Object
        EventSystem.current.SetSelectedGameObject(null);
        //set a new selected Object
        EventSystem.current.SetSelectedGameObject(_inputCloseButton);
        _lastSelected=_inputCloseButton;
        */
    }

    private void OpenMenu()
    {
        if (_isMouseActive)
        {
            //Clear Selected Object
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(_lastSelected);
            //_lastSelected = _inputCloseButton;
        }
    }

    #endregion

    public void SetMouse(bool mouseEnabled)
    {
        _isMouseActive = mouseEnabled;

        Debug.Log("Initial Set mouse value is " + _isMouseActive);

        if(_isMouseActive)
        {
            MouseAction();
        }
        else
        {
            Navigate();
        }
    }
}
