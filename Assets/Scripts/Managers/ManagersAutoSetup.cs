using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Automatically sets up required manager GameObjects at runtime if they don't exist.
/// This ensures the game can run without manual scene setup.
/// </summary>
public static class ManagersAutoSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoSetupManagers()
    {
        SetupGameInput();
        SetupAudioManager();
        SetupEffectPoolManager();
    }

    private static void SetupGameInput()
    {
        if (GameInput.Instance != null) return;

        // Try to find existing GameInput in scene
        var existing = Object.FindFirstObjectByType<GameInput>();
        if (existing != null) return;

        // Create GameInput
        var gameInputObj = new GameObject("GameInput");
        var gameInput = gameInputObj.AddComponent<GameInput>();
        Object.DontDestroyOnLoad(gameInputObj);

        // Try to load the InputActionAsset from Resources or default location
        var inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        if (inputActions == null)
        {
            // Try to find it in the project
            inputActions = Resources.Load<InputActionAsset>("InputActions");
        }

        if (inputActions != null)
        {
            // Use reflection to set the private field since we can't access it directly
            var field = typeof(GameInput).GetField("inputActions",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(gameInput, inputActions);
        }
        else
        {
            Debug.LogWarning("ManagersAutoSetup: Could not find InputActionAsset. " +
                "Please assign it manually to the GameInput component or place it in a Resources folder.");
        }

        Debug.Log("ManagersAutoSetup: Created GameInput");
    }

    private static void SetupAudioManager()
    {
        if (AudioManager.Instance != null) return;

        var existing = Object.FindFirstObjectByType<AudioManager>();
        if (existing != null) return;

        var audioManagerObj = new GameObject("AudioManager");
        audioManagerObj.AddComponent<AudioManager>();
        Object.DontDestroyOnLoad(audioManagerObj);

        Debug.Log("ManagersAutoSetup: Created AudioManager (audio clips need to be assigned)");
    }

    private static void SetupEffectPoolManager()
    {
        if (EffectPoolManager.Instance != null) return;

        var existing = Object.FindFirstObjectByType<EffectPoolManager>();
        if (existing != null) return;

        var effectPoolObj = new GameObject("EffectPoolManager");
        effectPoolObj.AddComponent<EffectPoolManager>();
        Object.DontDestroyOnLoad(effectPoolObj);

        Debug.Log("ManagersAutoSetup: Created EffectPoolManager (effect prefabs need to be assigned)");
    }
}
