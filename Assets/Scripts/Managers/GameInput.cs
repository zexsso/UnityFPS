using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Centralized input manager using the new Input System.
/// Handles all player input and provides clean access to input values.
/// </summary>
public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActions;

    // Player action map
    private InputActionMap _playerActionMap;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _attackAction;
    private InputAction _jumpAction;
    private InputAction _crouchAction;
    private InputAction _sprintAction;

    // UI action map
    private InputActionMap _uiActionMap;
    private InputAction _cancelAction;

    // Input values
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool AttackPressed { get; private set; }
    public bool AttackHeld { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool CrouchPressed { get; private set; }
    public bool CrouchHeld { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CancelPressed { get; private set; }
    public bool ScoreboardHeld { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeInputActions();
    }

    private void InitializeInputActions()
    {
        // If no asset assigned, try to find one in project
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        // If still no asset, create default actions programmatically
        if (inputActions == null)
        {
            Debug.LogWarning("GameInput: InputActionAsset not assigned. Creating default actions.");
            CreateDefaultInputActions();
            return;
        }

        // Get Player action map
        _playerActionMap = inputActions.FindActionMap("Player");
        if (_playerActionMap != null)
        {
            _moveAction = _playerActionMap.FindAction("Move");
            _lookAction = _playerActionMap.FindAction("Look");
            _attackAction = _playerActionMap.FindAction("Attack");
            _jumpAction = _playerActionMap.FindAction("Jump");
            _crouchAction = _playerActionMap.FindAction("Crouch");
            _sprintAction = _playerActionMap.FindAction("Sprint");
        }

        // Get UI action map
        _uiActionMap = inputActions.FindActionMap("UI");
        if (_uiActionMap != null)
        {
            _cancelAction = _uiActionMap.FindAction("Cancel");
        }
    }

    private void CreateDefaultInputActions()
    {
        // Create a runtime InputActionAsset with default FPS controls
        inputActions = ScriptableObject.CreateInstance<InputActionAsset>();

        // Create Player action map
        _playerActionMap = new InputActionMap("Player");

        // Move - WASD composite
        _moveAction = _playerActionMap.AddAction("Move", InputActionType.Value);
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // Look - Mouse delta
        _lookAction = _playerActionMap.AddAction("Look", InputActionType.Value);
        _lookAction.AddBinding("<Mouse>/delta");

        // Attack - Left mouse
        _attackAction = _playerActionMap.AddAction("Attack", InputActionType.Button);
        _attackAction.AddBinding("<Mouse>/leftButton");

        // Jump - Space
        _jumpAction = _playerActionMap.AddAction("Jump", InputActionType.Button);
        _jumpAction.AddBinding("<Keyboard>/space");

        // Crouch - C or Ctrl
        _crouchAction = _playerActionMap.AddAction("Crouch", InputActionType.Button);
        _crouchAction.AddBinding("<Keyboard>/c");
        _crouchAction.AddBinding("<Keyboard>/leftCtrl");

        // Sprint - Shift
        _sprintAction = _playerActionMap.AddAction("Sprint", InputActionType.Button);
        _sprintAction.AddBinding("<Keyboard>/leftShift");

        inputActions.AddActionMap(_playerActionMap);

        // Create UI action map
        _uiActionMap = new InputActionMap("UI");

        // Cancel - Escape
        _cancelAction = _uiActionMap.AddAction("Cancel", InputActionType.Button);
        _cancelAction.AddBinding("<Keyboard>/escape");

        inputActions.AddActionMap(_uiActionMap);
    }

    private void OnEnable()
    {
        _playerActionMap?.Enable();
        _uiActionMap?.Enable();
    }

    private void OnDisable()
    {
        _playerActionMap?.Disable();
        _uiActionMap?.Disable();
    }

    private void Update()
    {
        // Read continuous input values
        MoveInput = _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        LookInput = _lookAction?.ReadValue<Vector2>() ?? Vector2.zero;

        // Read button states
        AttackPressed = _attackAction?.WasPressedThisFrame() ?? false;
        AttackHeld = _attackAction?.IsPressed() ?? false;
        JumpPressed = _jumpAction?.WasPressedThisFrame() ?? false;
        CrouchPressed = _crouchAction?.WasPressedThisFrame() ?? false;
        CrouchHeld = _crouchAction?.IsPressed() ?? false;
        SprintHeld = _sprintAction?.IsPressed() ?? false;
        CancelPressed = _cancelAction?.WasPressedThisFrame() ?? false;

        // Scoreboard is Tab key - using Keyboard directly for simplicity
        ScoreboardHeld = Keyboard.current?.tabKey.isPressed ?? false;
    }

    /// <summary>
    /// Enables player input (for gameplay)
    /// </summary>
    public void EnablePlayerInput()
    {
        _playerActionMap?.Enable();
    }

    /// <summary>
    /// Disables player input (for menus, death, etc.)
    /// </summary>
    public void DisablePlayerInput()
    {
        _playerActionMap?.Disable();
    }

    /// <summary>
    /// Checks if player input is currently enabled
    /// </summary>
    public bool IsPlayerInputEnabled => _playerActionMap?.enabled ?? false;
}
