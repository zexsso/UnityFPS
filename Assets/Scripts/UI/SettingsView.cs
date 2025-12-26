using UnityEngine;
using PurrNet;
using TMPro;

public class SettingsView : View
{
    private const string SENSITIVITY_KEY = "MouseSensitivity";
    private const float DEFAULT_SENSITIVITY = 1f;

    [SerializeField] private TMP_InputField sensitivityInput;

    private GameViewManager _gameViewManager;
    private GameInput _gameInput;
    private bool _isVisible = false;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<SettingsView>();
    }

    private void Start()
    {
        _gameViewManager = InstanceHandler.GetInstance<GameViewManager>();
        sensitivityInput.onEndEdit.AddListener(OnSensitivityChanged);

        // Load saved sensitivity
        LoadSettings();
    }

    private void LoadSettings()
    {
        float savedSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, DEFAULT_SENSITIVITY);
        if (PlayerController.LocalPlayer != null)
        {
            PlayerController.LocalPlayer.SetLookSensitivity(savedSensitivity);
        }
    }

    private void OnSensitivityChanged(string newValue)
    {
        if (float.TryParse(newValue, out float newSens))
        {
            newSens = Mathf.Clamp(newSens, 0.1f, 20f);
            if (PlayerController.LocalPlayer != null)
            {
                PlayerController.LocalPlayer.SetLookSensitivity(newSens);
            }

            // Save to PlayerPrefs
            PlayerPrefs.SetFloat(SENSITIVITY_KEY, newSens);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning("Invalid sensitivity value");
        }
    }

    private void Update()
    {
        // Use new Input System for cancel/escape
        if (_gameInput == null)
        {
            _gameInput = GameInput.Instance;
        }

        if (_gameInput != null && _gameInput.CancelPressed)
        {
            if (!_isVisible)
                _gameViewManager.ShowView<SettingsView>(false);
            else
                _gameViewManager.HideView<SettingsView>();

            _isVisible = !_isVisible;
        }
    }

    public override void OnShow()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (PlayerController.LocalPlayer != null)
        {
            sensitivityInput.text = PlayerController.LocalPlayer.LookSensitivity.ToString("F2");
        }
    }

    public override void OnHide()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
