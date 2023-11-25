using UnityEngine;
using UnityEngine.InputSystem;
using CCS.Player;

public class InputManager : MonoSingleton<InputManager>
{
    #region Variables

    [Header("Player Scripts")]
    //handle to Player
    [SerializeField] private Player _player;

    //handle to Mech
    [SerializeField] private Mech _mech;

    //handle to Input Asset
    private PlayerInputActions _playerInput;

    //Cache Vector Zero
    private Vector2 _vectorZero = Vector2.zero;

    //change when mouse is being used
    private bool _isMouseActive = false;

    private bool _initialMouseCheck = false;

    #endregion

    #region Start, OnDisable and Initialize
    // Start is called before the first frame update
    void Start()
    {
        //initialize input asset
        InputInitialization();

        CheckForInput();
    }

    private void OnDisable()
    {
        PlayerUnsibscribe();
        MechUnsubscribe();
        UIUnsubscribe();

        PlayerManager.onPlayerChange -= PlayerManager_onPlayerChange;
        GameManager.OnRestartRoom -= GameManager_OnRestartRoom;
    }
    private void InputInitialization()
    {
        //initialize instance of input asset
        _playerInput = new PlayerInputActions();

        //null check input asset
        if (_playerInput != null)
        {
            //enable player action map
            _playerInput.Player.Enable();
        }
        else Debug.LogError("The Player Input Asset is NULL.");

        //get active Player script
        _player = PlayerManager.Instance.GetPlayer();

        //subscribe to inputs from Action Maps
        Subscribe();
    }

    private void CheckForInput()
    {
        //check for input
        var mouse = Mouse.current;
        var gamepad = Gamepad.current;
        var keyboard = Keyboard.current;

        //check for mouse
        if (mouse == null)
        {
            print("No moused detected");
            _isMouseActive = false;
        }
        else
        {
            print("Mouse is active");
            _isMouseActive = true;
        }

        //check for gamepad
        if (gamepad == null)
        {
            print("No gamepad detected");
        }
        else
        {
            print("Gamepad is active");
        }

        //check for keyboard
        if (keyboard == null)
        {
            print("No keyboard detected");
        }
        else
        {
            print("Keyboard is active");
        }
    }

    #endregion

    #region Event Methods

    private void PlayerManager_onPlayerChange()
    {
        //on player change, set active player
        _player = PlayerManager.Instance.GetPlayer();
    }

    private void GameManager_onGameOver()
    {
        _playerInput.Player.Disable();
        _playerInput.Mech.Disable();

        //enable UI action map
        print("enable UI action map here");
    }

    #endregion

    #region Subscribe and Unsubscribe

    private void Subscribe()
    {
        //Player Action Map
        PlayerSubscribe();

        //Mech Action Map
        MechSubscribe();

        //UI Action Map
        UISubscribe();

        //onPlayerChange Event
        PlayerManager.onPlayerChange += PlayerManager_onPlayerChange;

        GameManager.OnGameOver += GameManager_onGameOver;

        GameManager.OnRestartRoom += GameManager_OnRestartRoom;
    }

    private void GameManager_OnRestartRoom()
    {
        _playerInput.Player.Enable();
    }

    private void PlayerSubscribe()
    {
        //Player Action Map Movement Action (WASD, Gamepad direction, Left Joystick)
        _playerInput.Player.Movement.performed += Player_Movement_performed;
        _playerInput.Player.Movement.canceled += Player_Movement_canceled;

        //Player Action Map Rotation Action (Arrows, Right Joystick) NO MOUSE
        _playerInput.Player.Rotation.performed += Player_Rotation_performed;
        _playerInput.Player.Rotation.canceled += Player_Rotation_canceled;

        //Player Action Map Mouse Rotation Action NO ARROWS OR JOYSTICK
        _playerInput.Player.MouseRotation.performed += Player_MouseRotation_performed;
        _playerInput.Player.MouseRotation.canceled += Player_MouseRotation_canceled;

        //mouse input for shooting
        _playerInput.Player.Shoot.started += Shoot_started;
        _playerInput.Player.Shoot.canceled += Shoot_canceled;

        //dash input
        _playerInput.Player.Dash.started += Dash_started;

        //camera zoom in
        _playerInput.Player.CameraIn.started += CameraIn_started;

        //camera zoom out
        _playerInput.Player.CameraOut.started += CameraOut_started;

        //pause function
        _playerInput.Player.Pause.performed += Pause_performed;

        //skip director
        _playerInput.Player.Resume.performed += Resume_performed;
    }

    private void Resume_performed(InputAction.CallbackContext context)
    {
        //call skip director on director manager
        DirectorManager.Instance.SkipDirectorResume();
    }

    private void UISubscribe()
    {
        //UI input actions
        _playerInput.UI.Navigate.performed += Navigate_performed;
        _playerInput.UI.UnPause.performed += UnPause_performed;
        _playerInput.UI.MouseAction.performed += MouseAction_performed;
    }

    private void MouseAction_performed(InputAction.CallbackContext context)
    {
        if (_initialMouseCheck)
        {
            //using mouse
            print("Mouse Action Performed");

            if (_isMouseActive == false)
            {
                //activate mouse rotation
                _isMouseActive = true;
                
                UIManager.Instance.SetMouse(_isMouseActive);
                PauseMenu.Instance.SetMouse(_isMouseActive);
                ShopManager.Instance.SetMouse(_isMouseActive);
            }
        }
        else
        {
            _initialMouseCheck = true;
        }
    }

    private void UnPause_performed(InputAction.CallbackContext context)
    {
        //unpause the game.
        UnPause();
    }

    private void Navigate_performed(InputAction.CallbackContext context)
    {
        //navigate
        if (_isMouseActive == true)
        {
            //deactivate mouse rotation
            _isMouseActive = false;
           
            UIManager.Instance.SetMouse(_isMouseActive);
            PauseMenu.Instance.SetMouse(_isMouseActive);
            ShopManager.Instance.SetMouse(_isMouseActive);
        }
    }

    private void Pause_performed(InputAction.CallbackContext context)
    {
        //pause game
        Pause();
    }

    private void CameraOut_started(InputAction.CallbackContext context)
    {
        //tell camera manager to zoom out
        CameraManager.Instance.ZoomOut();
    }

    private void CameraIn_started(InputAction.CallbackContext context)
    {
        //tell camera manager to zoom in
        CameraManager.Instance.ZoomIn();
    }

    private void Dash_started(InputAction.CallbackContext context)
    {
        //start player dash routine
        _player.StartDashRoutine();
    }

    private void MechSubscribe()
    {
        //Mech Movement Action (WASD, Gamepad direction, Left Joystick)
        _playerInput.Mech.Movement.performed += Mech_Movement_performed;
        _playerInput.Mech.Movement.canceled += Mech_Movement_canceled;

        //Mech Rotation Action (Arrows, Right Joystick) NO MOUSE
        _playerInput.Mech.Rotation.performed += Mech_Rotation_performed;
        _playerInput.Mech.Rotation.canceled += Mech_Rotation_canceled;

        //Mech Action Map Mouse Rotation Action NO ARROWS OR JOYSTICK
        _playerInput.Mech.MouseRotation.performed += Mech_MouseRotation_performed;
        _playerInput.Mech.MouseRotation.canceled += Mech_MouseRotation_canceled;

        //mouse input for shooting
        _playerInput.Mech.Shoot.started += Mech_Shoot_started;
        _playerInput.Mech.Shoot.canceled += Mech_Shoot_canceled;
    }

    private void PlayerUnsibscribe()
    {
        //Player Action Map Movement Action (WASD, Gamepad direction, Left Joystick)
        _playerInput.Player.Movement.performed -= Player_Movement_performed;
        _playerInput.Player.Movement.canceled -= Player_Movement_canceled;

        //Player Action Map Rotation Action (Arrows, Right Joystick) NO MOUSE
        _playerInput.Player.Rotation.performed -= Player_Rotation_performed;
        _playerInput.Player.Rotation.canceled -= Player_Rotation_canceled;

        //Player Action Map Mouse Rotation Action NO ARROWS OR JOYSTICK
        _playerInput.Player.MouseRotation.performed -= Player_MouseRotation_performed;
        _playerInput.Player.MouseRotation.canceled -= Player_MouseRotation_canceled;

        //mouse input for shooting
        _playerInput.Player.Shoot.started -= Shoot_started;
        _playerInput.Player.Shoot.canceled -= Shoot_canceled;

        //dash input
        _playerInput.Player.Dash.started -= Dash_started;

        //camera zoom in
        _playerInput.Player.CameraIn.started -= CameraIn_started;

        //camera zoom out
        _playerInput.Player.CameraOut.started -= CameraOut_started;

        //pause
        _playerInput.Player.Pause.performed -= Pause_performed;

        //skip director
        _playerInput.Player.Resume.performed -= Resume_performed;
    }

    private void MechUnsubscribe()
    {
        //Mech Movement Action (WASD, Gamepad direction, Left Joystick)
        _playerInput.Mech.Movement.performed -= Mech_Movement_performed;
        _playerInput.Mech.Movement.canceled -= Mech_Movement_canceled;

        //Mech Rotation Action (Arrows, Right Joystick) NO MOUSE
        _playerInput.Mech.Rotation.performed -= Mech_Rotation_performed;
        _playerInput.Mech.Rotation.canceled -= Mech_Rotation_canceled;

        //Mech Action Map Mouse Rotation Action NO ARROWS OR JOYSTICK
        _playerInput.Mech.MouseRotation.performed -= Mech_MouseRotation_performed;
        _playerInput.Mech.MouseRotation.canceled -= Mech_MouseRotation_canceled;
    }

    private void UIUnsubscribe()
    {
        //UI input actions
        _playerInput.UI.Navigate.performed -= Navigate_performed;
        _playerInput.UI.UnPause.performed -= UnPause_performed;
        _playerInput.UI.MouseAction.performed -= MouseAction_performed;
    }

    #endregion
   
    #region Activate and Deactivate Action Maps
    public void ActivateMech()
    {
        _playerInput.Player.Disable();
        _playerInput.Mech.Enable();
    }

    public void DeactivateMech()
    {
        _playerInput.Mech.Disable();
        _playerInput.Player.Enable();
    }

    public void ActivateUI()
    {
        _playerInput.Player.Disable();
        _playerInput.UI.Enable();
    }

    public void DeactivateUI()
    {
        _playerInput.UI.Disable();
        _playerInput.Player.Enable();
    }

    public void Pause()
    {
        _initialMouseCheck = false;

        ActivateUI();

        //tell pause menu to pause
        PauseMenu.Instance.Pause();
        PauseMenu.Instance.SetMouse(_isMouseActive);
    }

    public void UnPause()
    {
        _initialMouseCheck = false;

        DeactivateUI();

        _player.SetMouse(_isMouseActive);
        UIManager.Instance.SetMouse(_isMouseActive);
    }

    #endregion

    #region Movement

    //Player Action Map
    //set Movement Performed in Player
    private void Player_Movement_performed(InputAction.CallbackContext context)
    {
        _player.SetMovement(context.ReadValue<Vector2>());
    }

    //set Movement Cancelled in Player
    private void Player_Movement_canceled(InputAction.CallbackContext context)
    {
        _player.SetMovement(_vectorZero);
    }

    #endregion

    #region Rotation

    //set Rotation Performed in Player
    private void Player_Rotation_performed(InputAction.CallbackContext context)
    {
        _player.SetRotation(context.ReadValue<Vector2>());

        if (_isMouseActive == true)
        {
            //deactivate mouse rotation
            _isMouseActive = false;
      
            _player.SetMouse(_isMouseActive);
            UIManager.Instance.SetMouse(_isMouseActive);
        }
    }

    //set Rotation Cancelled in Player
    private void Player_Rotation_canceled(InputAction.CallbackContext context)
    {
        _player.SetRotation(_vectorZero);
    }

    //set Mouse rotation Performed in Player
    private void Player_MouseRotation_performed(InputAction.CallbackContext context)
    {
        _player.SetMouseRotation(context.ReadValue<Vector2>());

        if (_initialMouseCheck)
        {
            if (_isMouseActive == false)
            {
                //activate mouse rotation
                _isMouseActive = true;
                _player.SetMouse(_isMouseActive);
                UIManager.Instance.SetMouse(_isMouseActive);
            }
        }
        else
        {
            _initialMouseCheck = true;
        }
    }

    //set Mouse rotation Cancelled in Player
    private void Player_MouseRotation_canceled(InputAction.CallbackContext context)
    {
        _player.SetMouseRotation(_vectorZero);
    }

    #endregion

    #region MechMovement

    //Mech Action Map
    //set Movement Performed in Mech
    private void Mech_Movement_performed(InputAction.CallbackContext context)
    {
        _mech.SetMovement(context.ReadValue<Vector2>());
    }

    //set Movement Cancelled in Mech
    private void Mech_Movement_canceled(InputAction.CallbackContext context)
    {
        _mech.SetMovement(_vectorZero);
    }

    #endregion

    #region MechRotation

    //set Rotation Performed in Mech
    private void Mech_Rotation_performed(InputAction.CallbackContext context)
    {
        _mech.SetRotation(context.ReadValue<Vector2>());
    }

    //set Rotation Cancelled in Mech
    private void Mech_Rotation_canceled(InputAction.CallbackContext context)
    {
        _mech.SetRotation(_vectorZero);
    }

    //set Mouse rotation Performed in Mech
    private void Mech_MouseRotation_performed(InputAction.CallbackContext context)
    {
        _mech.SetMouseRotation(context.ReadValue<Vector2>());
    }

    //set Mouse rotation Cancelled in Mech
    private void Mech_MouseRotation_canceled(InputAction.CallbackContext context)
    {
        _mech.SetMouseRotation(_vectorZero);
    }

    #endregion

    #region Shooting Inputs

    private void Shoot_started(InputAction.CallbackContext context)
    {
        _player.MouseFiring();
    }

    private void Shoot_canceled(InputAction.CallbackContext context)
    {
        _player.StopMouseFireRoutine();
    }

    private void Mech_Shoot_started(InputAction.CallbackContext context)
    {
        _mech.StartFireRoutine();
    }

    private void Mech_Shoot_canceled(InputAction.CallbackContext context)
    {
        _mech.StopFireRoutine();
    }

    #endregion
}
