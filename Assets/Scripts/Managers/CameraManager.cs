using UnityEngine;
using System.Collections;
using Cinemachine;

public class CameraManager : MonoSingleton<CameraManager>
{
    #region Variables

    [SerializeField] private Transform _playerTransform;

    [SerializeField] private Transform _cameraTransform;

    [SerializeField] private float _reductionFactor = 1f;

    [SerializeField] private CinemachineVirtualCamera _followCamClose;
    [SerializeField] private CinemachineVirtualCamera _followCam;
    [SerializeField] private CinemachineVirtualCamera _followCamFar;
    [SerializeField] private CinemachineVirtualCamera _deathCam;

    #endregion

    #region Start, OnDisable

    void Start()
    {
        _playerTransform = PlayerManager.Instance.GetActivePlayer();

        _followCam.Follow = _playerTransform;
        _followCam.LookAt = _playerTransform;

        _followCamClose.Follow = _playerTransform;
        _followCamClose.LookAt = _playerTransform;

        _followCamFar.Follow = _playerTransform;
        _followCamFar.LookAt = _playerTransform;

        PlayerManager.onPlayerChange += PlayerManager_onPlayerChange;
        GameManager.OnGameOver += GameManager_onGameOver;
        GameManager.OnRestartRoom += GameManager_OnRestartRoom;
    }

    private void GameManager_OnRestartRoom()
    {
        //reset camera
        _deathCam.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        PlayerManager.onPlayerChange -= PlayerManager_onPlayerChange;
        GameManager.OnRestartRoom -= GameManager_OnRestartRoom;
    }

    #endregion

    #region Event Methods

    private void GameManager_onGameOver()
    {
        //get new player object to follow
        _playerTransform = PlayerManager.Instance.GetActivePlayer();

        _deathCam.Follow = _playerTransform;
        _deathCam.LookAt = _playerTransform;

        _deathCam.gameObject.SetActive(true);
    }

    private void PlayerManager_onPlayerChange()
    {
        //get new player object to follow
        _playerTransform = PlayerManager.Instance.GetActivePlayer();

        _followCam.Follow = _playerTransform;
        _followCam.LookAt = _playerTransform;

        _followCamClose.Follow = _playerTransform;
        _followCamClose.LookAt = _playerTransform;

        _followCamFar.Follow = _playerTransform;
        _followCamFar.LookAt = _playerTransform;
    }

    #endregion

    #region Camera Shake

    public void ShakeCamera(float shakeDuration, float shakeMagnitude)
    {
        StartCoroutine(ShakeCameraRoutine(shakeDuration, shakeMagnitude));
    }

    private IEnumerator ShakeCameraRoutine(float shakeDuration, float shakeMagnitude)
    {
        while (shakeDuration > 0f)
        {
            _followCam.Follow = null;
            _followCam.LookAt = null;

            Vector3 shakeArea = new Vector3(Random.insideUnitSphere.x, 0, Random.insideUnitSphere.z);

            _cameraTransform.localPosition = _cameraTransform.localPosition + shakeArea * shakeMagnitude;

            shakeDuration -= Time.deltaTime * _reductionFactor;
            shakeMagnitude -= Time.deltaTime * _reductionFactor;

            yield return null;
        }

        _followCam.Follow = _playerTransform;
        _followCam.LookAt = _playerTransform;
    }

    #endregion

    public void ZoomIn()
    {
        if (_followCam.Priority == 11)
        {
            _followCam.Priority = 10;
            _followCamClose.Priority = 11;
        }
        else if (_followCamFar.Priority == 11)
        {
            _followCamFar.Priority = 10;
            _followCam.Priority = 11;
        }
    }

    public void ZoomOut()
    {
        if (_followCam.Priority == 11)
        {
            _followCam.Priority = 10;
            _followCamFar.Priority = 11;
        }
        else if (_followCamClose.Priority == 11)
        {
            _followCamClose.Priority = 10;
            _followCam.Priority = 11;
        }
    }
}
