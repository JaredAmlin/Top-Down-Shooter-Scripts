using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    #region Audio Settings

    [Header("AudioPanel")]
    [SerializeField] AudioMixer _audioMixer;
    [SerializeField] Slider _masterVolumeSlider;
    [SerializeField] Slider _sfxVolumeSlider;
    [SerializeField] Slider _soundtrackVolumeSlider;
    [SerializeField] Slider _weaponSfxVolumeSlider;
    [SerializeField] Button _testWeapon;
    [SerializeField] Button _testSFX;
    [SerializeField] Button _audioBackButton;
    [SerializeField] AudioSource _sfxSource;
    [SerializeField] AudioSource _weaponsSource;
    [SerializeField] AudioSource _clickButtonAudio;

    #endregion

    private void Start()
    {
        LoadAudioPrefs();
        LoadAudioBackButtonSetting();// Load the BackButton settings
    }

    #region Audio Functions

    private void LoadAudioPrefs()
    {
        // Find and attach the buttons and sliders dynamically by their names
        _masterVolumeSlider = GameObject.Find("MasterVolumeSlider").GetComponent<Slider>();
        _sfxVolumeSlider = GameObject.Find("SFX VolumeSlider ").GetComponent<Slider>();
        _soundtrackVolumeSlider = GameObject.Find("SoundTrackVolumeSlider ").GetComponent<Slider>();
        _weaponSfxVolumeSlider = GameObject.Find("WeaponsFXVolumeSlider ").GetComponent<Slider>();
        _testWeapon = GameObject.Find("PlayWeapon_Clip ").GetComponent<Button>();
        _testSFX = GameObject.Find("PlaySFX_Clip").GetComponent<Button>();
        _audioBackButton = GameObject.Find("BackButtonText").GetComponent<Button>();
        _sfxSource = GameObject.Find("PlaySFX_Clip").GetComponent<AudioSource>();
        _weaponsSource = GameObject.Find("PlayWeapon_Clip ").GetComponent<AudioSource>();
        _clickButtonAudio = GameObject.Find("BackButtonText").GetComponent<AudioSource>();
        
        _testSFX.onClick.AddListener(PlayTestSFX);
        _testWeapon.onClick.AddListener(PlayTestWeapons);

        // Set the initial slider values to the saved PlayerPrefs values
        float masterVolumeSaved = PlayerPrefs.GetFloat("MasterVolume", 1f);
        _masterVolumeSlider.value = masterVolumeSaved;
        _masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        float sfxVolumeSaved = PlayerPrefs.GetFloat("SFXVolume", 1f);
        _sfxVolumeSlider.value = sfxVolumeSaved;
        _sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        float soundtrackVolumeSaved = PlayerPrefs.GetFloat("SoundtrackVolume", 1f);
        _soundtrackVolumeSlider.value = soundtrackVolumeSaved;
        _soundtrackVolumeSlider.onValueChanged.AddListener(SetSoundtrackVolume);

        float weaponSfxVolumeSaved = PlayerPrefs.GetFloat("WeaponSFXVolume", 1f);
        _weaponSfxVolumeSlider.value = weaponSfxVolumeSaved;
        _weaponSfxVolumeSlider.onValueChanged.AddListener(SetWeaponSFXVolume);

        // Set the mixer values to match the saved values
        float mixerValueMasterSaved = Mathf.Log10(masterVolumeSaved) * 20f;
        _audioMixer.SetFloat("MasterVolume", mixerValueMasterSaved);

        float mixerValueSFXSaved = Mathf.Log10(sfxVolumeSaved) * 20f;
        _audioMixer.SetFloat("SFXVolume", mixerValueSFXSaved);

        float mixerValueSoundtrackSaved = Mathf.Log10(soundtrackVolumeSaved) * 20f;
        _audioMixer.SetFloat("SoundtrackVolume", mixerValueSoundtrackSaved);

        float mixerValueWeaponSFXSaved = Mathf.Log10(weaponSfxVolumeSaved) * 20f;
        _audioMixer.SetFloat("WeaponSFXVolume", mixerValueWeaponSFXSaved);
    }

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

    public void PlayTestWeapons()
    {
        _weaponsSource.Play();
    }

    public void PlayTestSFX()
    {
        _sfxSource.Play();
    }

    public void LoadAudioBackButtonSetting()
    {
        _audioBackButton.onClick.AddListener(OnAudioBackButtonClicked);
    }

    public void OnAudioBackButtonClicked()
    {
        _clickButtonAudio.Play();
    }

    #endregion
}
