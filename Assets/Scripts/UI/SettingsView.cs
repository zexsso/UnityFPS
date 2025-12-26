using UnityEngine;
using PurrNet;
using TMPro;

public class SettingsView : View
{

    [SerializeField] private TMP_InputField sensitivityInput;
    private GameViewManager _gameViewManager;

    private bool isVisible = false;



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
        }
        else Debug.LogWarning("Invalid sensitivity value");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isVisible) _gameViewManager.ShowView<SettingsView>(false);
            else _gameViewManager.HideView<SettingsView>();
            isVisible = !isVisible;
        }


    }

    public override void OnShow()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (PlayerController.LocalPlayer != null) sensitivityInput.text = PlayerController.LocalPlayer.lookSensitivity.ToString("F2");
    }

    public override void OnHide()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
