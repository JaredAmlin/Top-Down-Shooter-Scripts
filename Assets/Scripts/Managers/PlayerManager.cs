using UnityEngine;
using System;
using CCS.Player;
using CCS.Types;

public class PlayerManager : MonoSingleton<PlayerManager>
{
    #region Variables

    [Header("Player Scripts")]
    [SerializeField] private Player[] _player;

    [SerializeField] private Mech _mech;

    [Header("Player Objects")]
    [SerializeField] private GameObject[] _playerObjects;

    [SerializeField] private Transform[] _playerTransforms;

    private Vector3 _previousPosition;
    private Quaternion _previousRotation;
    private bool _isFiring;

    private bool _isMouseActive = true;

    public static event Action onPlayerChange;

    public bool HealedArmor;

    [Header("Player State")]
    [SerializeField] private PlayerState _playerState;

    //int reference for element number in player array
    //0 = PlayerA, 1 = PlayerB, 2 = RedArmor, 3 = BlueArmor, 4 = Mech
    private int _playerID;

    //base player ID
    private int _originalID;

    private int _playerObjectID;

    private int _previousID;

    [SerializeField] private Transform[] _restartSpawnPosition;

    #endregion

    #region Start And Initialize

    void Start()
    {
        _originalID = PlayerPrefs.GetInt("selectedCharacter");

        SetOiginalPlayerID(_originalID);

        if (_originalID is 0)
        {
            _playerState = PlayerState.PlayerA;
        }
        else if (_originalID is 1)
        {
            _playerState = PlayerState.PlayerB; ;
        }
        else print("There is no Player State for Original.");

        SetPlayerObjects();

        //raise event
        onPlayerChange?.Invoke();
    }

    #endregion

    #region Player State (Get)

    public Player GetPlayer()
    {
        switch (_playerState)
        {
            case PlayerState.PlayerA:
                _playerID = 0;
                break;
            case PlayerState.PlayerB:
                _playerID = 1;
                break;
            case PlayerState.RedArmor:
                _playerID = 2;
                break;
            case PlayerState.BlueArmor:
                _playerID = 3;
                break;
            case PlayerState.Mech:
                _playerID = 4;
                break;
            default:
               // Debug.Log("There is no case for this Player State");
                break;
        }

        if (_playerID is 4)
        {
            _playerID = _previousID;
        }

        return _player[_playerID];
    }

    public int GetPreviousID()
    {
        return _previousID;
    }

    public int GetOriginalPlayerID()
    {
        return _originalID;
    }

    public bool GetFiringState()
    {
        return _isFiring;
    }
    public Mech GetMechState()
    {
        return _mech;
    }

    #endregion

    #region Player State (Set)

    public void SetPlayerState(int playerID)
    {
        StorePosition(_playerTransforms[_playerObjectID].position, _playerTransforms[_playerObjectID].rotation);

        Vector2 previousDirection = _player[_playerID].GetDirection();

        //store mouse active
        _isMouseActive = _player[_playerID].GetMouseActive();

        _previousID = _playerID;

        //assign type to player state
        _playerID = playerID;

        switch (_playerID)
        {
            case 0:
                _playerState = PlayerState.PlayerA;
                break;
            case 1:
                _playerState = PlayerState.PlayerB;
                break;
            case 2:
                _playerState = PlayerState.RedArmor;
                break;
            case 3:
                _playerState = PlayerState.BlueArmor;
                break;
            case 4:
                _playerState = PlayerState.Mech;
                InputManager.Instance.ActivateMech();
                break;
            default:
                Debug.Log("There is no case for this Player State");
                break;
        }

        //raise event
        onPlayerChange?.Invoke();

        //set player position
        SetPosition();

        //handle player object visibility
        SetPlayerObjects();

        //set player input direction
        _player[_playerID].SetMovement(previousDirection);
    }

    public void SetPreviousID(int playerID)
    {
        _previousID = playerID;
    }

    public void SetOiginalPlayerID(int ID)
    {
        _originalID = ID;

        if (ID == 0)
        {
            // Set original player ID to PlayerA
            _playerState = PlayerState.PlayerA;
        }
        else if (ID == 1)
        {
            // Set original player ID to PlayerB
            _playerState = PlayerState.PlayerB;
        }
        else
        {
            // If selected character index is invalid, default to PlayerA
            _playerState = PlayerState.PlayerA;
            Debug.LogWarning("Invalid selected character index. Defaulting to PlayerA.");
        }
    }

    public void SetFiringState(bool isFiring)
    {
        _isFiring = isFiring;
    }

    public bool GetMouseInput()
    {
        return _isMouseActive;
    }

    #endregion

    #region Object Transforms (Get)

    public Transform GetActivePlayer()
    {
        switch (_playerState)
        {
            case PlayerState.PlayerA:
                //assign player var to this Player script
                _playerObjectID = 0;
                break;
            case PlayerState.PlayerB:
                //assign player var to this Player script
                _playerObjectID = 1;
                break;
            case PlayerState.RedArmor:
                //assign player var to this Player script
                _playerObjectID = 2;
                break;
            case PlayerState.BlueArmor:
                //assign player var to this Player script
                _playerObjectID = 3;
                break;
            case PlayerState.Mech:
                //use mech action map and set player inactive
                _playerObjectID = 4;
                break;
            default:
               // Debug.Log("There is no case for this Player State");
                break;
        }

        return _playerTransforms[_playerObjectID];
    }
    
    #endregion

    #region Object Transforms (Set)

    private void SetPlayerObjects()
    {
        switch (_playerState)
        {
            case PlayerState.PlayerA:
                //handle object visibility
                _playerObjects[0].gameObject.SetActive(true);
                _playerObjects[1].gameObject.SetActive(false);
                _playerObjects[2].gameObject.SetActive(false);
                _playerObjects[3].gameObject.SetActive(false);
                _playerObjects[4].gameObject.SetActive(false);
                UIManager.Instance.ActivateShieldSystemAndChangeSpriteColor(Color.red, false);
                UIManager.Instance.DeactivateWeaponsIconSystem();
                break;
            case PlayerState.PlayerB:
                //handle object visibility
                _playerObjects[0].gameObject.SetActive(false);
                _playerObjects[1].gameObject.SetActive(true);
                _playerObjects[2].gameObject.SetActive(false);
                _playerObjects[3].gameObject.SetActive(false);
                _playerObjects[4].gameObject.SetActive(false);
                UIManager.Instance.ActivateShieldSystemAndChangeSpriteColor(Color.red, false);
                UIManager.Instance.DeactivateWeaponsIconSystem();
                break;
            case PlayerState.RedArmor:
                //handle object visibility
                _playerObjects[0].gameObject.SetActive(false);
                _playerObjects[1].gameObject.SetActive(false);
                _playerObjects[2].gameObject.SetActive(true);
                _playerObjects[3].gameObject.SetActive(false);
                _playerObjects[4].gameObject.SetActive(false);
                UIManager.Instance.AnimateHealthSystem(UIManager.Instance.ArmorHealthSystem);
                UIManager.Instance.ActivateShieldSystemAndChangeSpriteColor(Color.red, true);
                UIManager.Instance.ArmorFill.color = Color.red;
                UIManager.Instance.DeactivateWeaponsIconSystem();
                break;
            case PlayerState.BlueArmor:
                //handle object visibility
                _playerObjects[0].gameObject.SetActive(false);
                _playerObjects[1].gameObject.SetActive(false);
                _playerObjects[2].gameObject.SetActive(false);
                _playerObjects[3].gameObject.SetActive(true);
                _playerObjects[4].gameObject.SetActive(false);
                UIManager.Instance.AnimateHealthSystem(UIManager.Instance.ArmorHealthSystem);
                UIManager.Instance.ActivateShieldSystemAndChangeSpriteColor(Color.blue, true);
                UIManager.Instance.ArmorFill.color = Color.blue;
                UIManager.Instance.DeactivateWeaponsIconSystem();
                break;
            case PlayerState.Mech:
                //handle player visibility
                _playerObjects[0].gameObject.SetActive(false);
                _playerObjects[1].gameObject.SetActive(false);
                _playerObjects[2].gameObject.SetActive(false);
                _playerObjects[3].gameObject.SetActive(false);
                _playerObjects[4].gameObject.SetActive(true);
                UIManager.Instance.ActivateShieldSystemAndChangeSpriteColor(true);
                UIManager.Instance.AnimateHealthSystem(UIManager.Instance.MechHealthSystem);      
                UIManager.Instance.DeactivateWeaponsIconSystem();
                break;
            default:
                Debug.Log("There is no case for this Player State");
                break;
        }
    }

    private void StorePosition(Vector3 position, Quaternion rotation)
    {
        //store previous position and rotation
        _previousPosition = position;
        _previousRotation = rotation;
    }

    private void SetPosition()
    {
        //set position and rotation to next player
        //Debug.Log("Player ID is " + _playerID);
        _playerTransforms[_playerID].position = _previousPosition;
        _playerTransforms[_playerID].rotation = _previousRotation;
    }

    public Vector3 GetPosition()
    {
        return _previousPosition;
    }

    public PlayerState GetPlayerState()
    {
        return _playerState;
    }
    #endregion

    public bool ArmorHealed()
    {
        HealedArmor = true;
        return HealedArmor;
    }

    public Transform GetSpawnPosition()
    {
        int roomCompleted = GameManager.Instance.RoomsCompleted();
        //Debug.Log("Room completed is " + roomCompleted);
        return _restartSpawnPosition[roomCompleted];
    }
}
