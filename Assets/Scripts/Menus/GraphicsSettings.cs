using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettings : MonoBehaviour
{
    #region Graphics Settings

    // This class is responsible for controlling the graphics menu in the game. It contains fields that
    // reference the UI elements used in the menu.

    // Fields for the resolutions section of the menu
    [Header("Resolutions")]
    [SerializeField] private TextMeshProUGUI _currentResolutionText;  // Reference to the text displaying the current resolution
    [SerializeField] private Button _nextResolutionButton;            // Reference to the button that selects the next resolution
    [SerializeField] private Button _previousResolutionButton;        // Reference to the button that selects the previous resolution
    public Resolution[] _resolutions;    // An array of available resolutions
    public int _currentResolutionIndex;  // The current resolution index

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

    private void Start()
    {
        // This method loads the current quality, resolution, anti-aliasing, VSync, and fullscreen settings and sets the
        // corresponding UI elements to display the current settings.

        LoadQualitySettings();      // Load the current quality setting
        LoadResolutions();          // Load the available resolutions
        LoadAntiAliasingSetting();  // Load the current anti-aliasing setting
        LoadVSyncSetting();         // Load the current VSync setting
        LoadFullscreenSetting();    // Load the current fullscreen setting

        LoadGraphicsBackButtonSetting();    // Load the BackButton settings
    }

    #region Graphics Function

    #region Quality Settings

    // This method loads the current quality setting from PlayerPrefs and sets the initial quality level.
    private void LoadQualitySettings()
    {
        // Find and attach the buttons dynamically
        _increaseQualityButton = GameObject.Find("IncreaseQualityButton").GetComponent<Button>();
        _decreaseQualityButton = GameObject.Find("DecreaseQualityButton").GetComponent<Button>();

        // Find the Quality Text GameObject by its name
        GameObject qualityTextObject = GameObject.Find("QualityText ");

        // Check if the GameObject is found
        if (qualityTextObject != null)
        {
            // Get the TextMeshProUGUI component from the found GameObject
            _qualityText = qualityTextObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("Quality Text GameObject not found!");
        }

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
        // Find and attach the buttons dynamically
        _nextResolutionButton = GameObject.Find("NextResolutionButton").GetComponent<Button>();
        _previousResolutionButton = GameObject.Find("PreviousResolutionButton").GetComponent<Button>();

        // Find the Resolution Text GameObject by its name
        GameObject resolutionTextObject = GameObject.Find("ResolutionText");

        // Check if the GameObject is found
        if (resolutionTextObject != null)
        {
            // Get the TextMeshProUGUI component from the found GameObject
            _currentResolutionText = resolutionTextObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("Resolution Text GameObject not found!");
        }

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
            print("Loading from player Prefs " + savedWidth + " x " + savedHeight);
        }
        else
        {
            // If no resolution has been saved, set the current resolution to the current screen resolution
            _currentResolutionIndex = Array.FindIndex(_resolutions, r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
            print("set the current resolution to the current screen resolution " + Screen.currentResolution.width + " x " + Screen.currentResolution.height);
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
        // Find and attach the buttons dynamically
        _increaseAntiAliasingButton = GameObject.Find("AA_Increase_ButtonText").GetComponent<Button>();
        _decreaseAntiAliasingButton = GameObject.Find("AA_Decrease_ButtonText ").GetComponent<Button>();

        // Find the Anti Aliasing Text GameObject by its name
        GameObject antiAliasingTextObject = GameObject.Find("AntiAliasingText");

        // Check if the GameObject is found
        if (antiAliasingTextObject != null)
        {
            // Get the TextMeshProUGUI component from the found GameObject
            _antiAliasingText = antiAliasingTextObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("AntiAliasing Text GameObject not found!");
        }

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
        // Find and attach the buttons dynamically
        _increaseVSyncButton = GameObject.Find("V-Sync_Increase_ButtonText").GetComponent<Button>();
        _decreaseVSyncButton = GameObject.Find("V_Sync_Decrease_ButtonText").GetComponent<Button>();

        // Find the Vsync Text GameObject by its name
        GameObject vSyncTextObject = GameObject.Find("Vsync_Text ");

        // Check if the GameObject is found
        if (vSyncTextObject != null)
        {
            // Get the TextMeshProUGUI component from the found GameObject
            _vsyncText = vSyncTextObject.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("VSync Text GameObject not found!");
        }

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
        // Find the Fullscreen Toggle GameObject by its name
        GameObject fullscreenToggleObject = GameObject.Find("FullScreenToggle");

        // Check if the GameObject is found
        if (fullscreenToggleObject != null)
        {
            // Get the Toggle component from the found GameObject
            _fullscreenToggle = fullscreenToggleObject.GetComponent<Toggle>();

            // Load FullScreen setting from PlayerPrefs
            int fullscreenValue = PlayerPrefs.GetInt("fullscreen", 1); // Default to 1 if no value is found

            // Set the value of the FullScreen toggle based on the loaded value
            _fullscreenToggle.isOn = fullscreenValue == 1 ? true : false;

            // Add a listener to the toggle's onValueChanged event to handle changes
            _fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);
        }
        else
        {
            Debug.LogError("Fullscreen Toggle GameObject not found!");
        }
    }

    // Called when the FullScreen toggle is clicked
    public void OnFullscreenToggle(bool isFullscreen)
    {
        // Set the FullScreen mode to the value of the toggle
        Screen.fullScreen = isFullscreen;

        // Save the state of the toggle to PlayerPrefs
        PlayerPrefs.SetInt("fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    #endregion

    #region GraphicsBack Button Settings

    public void LoadGraphicsBackButtonSetting()
    {
        // Find and attach the buttons dynamically
        _graphicsBackButton = GameObject.Find("GraphicsBackButton").GetComponent<Button>();
        _graphicsBackButton.onClick.AddListener(OnGraphicsBackButtonClicked);
    }
    public void OnGraphicsBackButtonClicked()
    {
        _clickButtonAudio.Play();
        _graphicsMenu.SetActive(false);
        _settingsPanel.SetActive(true);
    }

    #endregion

    #endregion
}
