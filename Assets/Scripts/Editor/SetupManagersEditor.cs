using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;

/// <summary>
/// Editor utility to automatically set up manager GameObjects in the scene.
/// Access via Tools > Setup Game Managers menu.
/// </summary>
public class SetupManagersEditor : Editor
{
    [MenuItem("Tools/Setup Game Managers")]
    public static void SetupManagers()
    {
        SetupGameInput();
        SetupAudioManager();
        SetupEffectPoolManager();

        Debug.Log("Game Managers setup complete! Don't forget to assign references in the Inspector.");
    }

    [MenuItem("Tools/Setup Game Managers/GameInput Only")]
    public static void SetupGameInput()
    {
        if (Object.FindFirstObjectByType<GameInput>() != null)
        {
            Debug.LogWarning("GameInput already exists in the scene.");
            return;
        }

        GameObject gameInputObj = new GameObject("GameInput");
        gameInputObj.AddComponent<GameInput>();

        Undo.RegisterCreatedObjectUndo(gameInputObj, "Create GameInput");
        Selection.activeGameObject = gameInputObj;

        Debug.Log("GameInput created. Assign the InputActionAsset in the Inspector.");
    }

    [MenuItem("Tools/Setup Game Managers/AudioManager Only")]
    public static void SetupAudioManager()
    {
        if (AudioManager.Instance != null || Object.FindFirstObjectByType<AudioManager>() != null)
        {
            Debug.LogWarning("AudioManager already exists in the scene.");
            return;
        }

        GameObject audioManagerObj = new GameObject("AudioManager");
        audioManagerObj.AddComponent<AudioManager>();

        Undo.RegisterCreatedObjectUndo(audioManagerObj, "Create AudioManager");
        Selection.activeGameObject = audioManagerObj;

        Debug.Log("AudioManager created. Assign audio clips in the Inspector.");
    }

    [MenuItem("Tools/Setup Game Managers/EffectPoolManager Only")]
    public static void SetupEffectPoolManager()
    {
        if (EffectPoolManager.Instance != null || Object.FindFirstObjectByType<EffectPoolManager>() != null)
        {
            Debug.LogWarning("EffectPoolManager already exists in the scene.");
            return;
        }

        GameObject effectPoolObj = new GameObject("EffectPoolManager");
        effectPoolObj.AddComponent<EffectPoolManager>();

        Undo.RegisterCreatedObjectUndo(effectPoolObj, "Create EffectPoolManager");
        Selection.activeGameObject = effectPoolObj;

        Debug.Log("EffectPoolManager created. Assign effect prefabs in the Inspector.");
    }

    [MenuItem("Tools/Setup UI/Create Kill Feed")]
    public static void CreateKillFeed()
    {
        // Find the Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene. Create a Canvas first.");
            return;
        }

        // Check if KillFeedView already exists
        if (Object.FindFirstObjectByType<KillFeedView>() != null)
        {
            Debug.LogWarning("KillFeedView already exists in the scene.");
            return;
        }

        // Create KillFeed container
        GameObject killFeedObj = new GameObject("KillFeedView");
        killFeedObj.transform.SetParent(canvas.transform, false);

        RectTransform killFeedRect = killFeedObj.AddComponent<RectTransform>();
        killFeedRect.anchorMin = new Vector2(1, 1);
        killFeedRect.anchorMax = new Vector2(1, 1);
        killFeedRect.pivot = new Vector2(1, 1);
        killFeedRect.anchoredPosition = new Vector2(-20, -20);
        killFeedRect.sizeDelta = new Vector2(300, 200);

        CanvasGroup canvasGroup = killFeedObj.AddComponent<CanvasGroup>();
        KillFeedView killFeedView = killFeedObj.AddComponent<KillFeedView>();

        // Create entries parent
        GameObject entriesParent = new GameObject("Entries");
        entriesParent.transform.SetParent(killFeedObj.transform, false);

        RectTransform entriesRect = entriesParent.AddComponent<RectTransform>();
        entriesRect.anchorMin = Vector2.zero;
        entriesRect.anchorMax = Vector2.one;
        entriesRect.offsetMin = Vector2.zero;
        entriesRect.offsetMax = Vector2.zero;

        // Add vertical layout group
        var layoutGroup = entriesParent.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperRight;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.spacing = 5;

        // Set serialized field reference using reflection
        var entriesField = typeof(KillFeedView).GetField("entriesParent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        entriesField?.SetValue(killFeedView, entriesParent.transform);

        Undo.RegisterCreatedObjectUndo(killFeedObj, "Create KillFeed");
        Selection.activeGameObject = killFeedObj;

        Debug.Log("KillFeedView created. Now run Tools > Setup UI > Create Kill Feed Entry Prefab to create the entry prefab.");
    }

    [MenuItem("Tools/Setup UI/Create Kill Feed Entry Prefab")]
    public static void CreateKillFeedEntryPrefab()
    {
        // Ensure the prefab folder exists
        string prefabFolder = "Assets/Prefabs/UI";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }

        // Create the entry GameObject
        GameObject entryObj = new GameObject("KillFeedEntry");

        // Add RectTransform
        RectTransform rect = entryObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, 30);

        // Add CanvasGroup for fading
        entryObj.AddComponent<CanvasGroup>();

        // Add horizontal layout
        var layout = entryObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleRight;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 5;
        layout.padding = new RectOffset(5, 5, 2, 2);

        // Add background image
        var bg = entryObj.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 0.5f);

        // Create killer name text
        GameObject killerTextObj = new GameObject("KillerName");
        killerTextObj.transform.SetParent(entryObj.transform, false);
        var killerText = killerTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        killerText.text = "Killer";
        killerText.fontSize = 14;
        killerText.alignment = TMPro.TextAlignmentOptions.MidlineRight;
        var killerRect = killerTextObj.GetComponent<RectTransform>();
        killerRect.sizeDelta = new Vector2(100, 30);

        // Create headshot icon placeholder
        GameObject headshotIconObj = new GameObject("HeadshotIcon");
        headshotIconObj.transform.SetParent(entryObj.transform, false);
        var headshotIcon = headshotIconObj.AddComponent<UnityEngine.UI.Image>();
        headshotIcon.color = Color.red;
        var hsRect = headshotIconObj.GetComponent<RectTransform>();
        hsRect.sizeDelta = new Vector2(20, 20);
        headshotIconObj.SetActive(false);

        // Create weapon icon placeholder
        GameObject weaponIconObj = new GameObject("WeaponIcon");
        weaponIconObj.transform.SetParent(entryObj.transform, false);
        var weaponIcon = weaponIconObj.AddComponent<UnityEngine.UI.Image>();
        weaponIcon.color = Color.white;
        var wRect = weaponIconObj.GetComponent<RectTransform>();
        wRect.sizeDelta = new Vector2(30, 20);

        // Create victim name text
        GameObject victimTextObj = new GameObject("VictimName");
        victimTextObj.transform.SetParent(entryObj.transform, false);
        var victimText = victimTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        victimText.text = "Victim";
        victimText.fontSize = 14;
        victimText.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        var victimRect = victimTextObj.GetComponent<RectTransform>();
        victimRect.sizeDelta = new Vector2(100, 30);

        // Add KillFeedEntry component
        var entry = entryObj.AddComponent<KillFeedEntry>();

        // Set serialized field references using SerializedObject
        var so = new SerializedObject(entry);
        so.FindProperty("killerNameText").objectReferenceValue = killerText;
        so.FindProperty("victimNameText").objectReferenceValue = victimText;
        so.FindProperty("weaponIcon").objectReferenceValue = weaponIcon;
        so.FindProperty("headshotIcon").objectReferenceValue = headshotIcon;
        so.ApplyModifiedProperties();

        // Save as prefab
        string prefabPath = $"{prefabFolder}/KillFeedEntry.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(entryObj, prefabPath);
        Object.DestroyImmediate(entryObj);

        Debug.Log($"KillFeedEntry prefab created at: {prefabPath}");
        Debug.Log("Assign this prefab to the KillFeedView's entryPrefab field.");

        // Select the created prefab
        Selection.activeObject = prefab;

        // Try to assign to existing KillFeedView
        var killFeedView = Object.FindFirstObjectByType<KillFeedView>();
        if (killFeedView != null && prefab != null)
        {
            var prefabEntry = prefab.GetComponent<KillFeedEntry>();
            var kfvSo = new SerializedObject(killFeedView);
            kfvSo.FindProperty("entryPrefab").objectReferenceValue = prefabEntry;
            kfvSo.ApplyModifiedProperties();
            Debug.Log("Automatically assigned prefab to KillFeedView in scene.");
        }
    }

    [MenuItem("Tools/Setup UI/Create MainGameView Timer")]
    public static void CreateMainGameViewTimer()
    {
        var mainGameView = Object.FindFirstObjectByType<MainGameView>();
        if (mainGameView == null)
        {
            Debug.LogError("MainGameView not found in scene.");
            return;
        }

        Transform viewTransform = mainGameView.transform;

        // Create Timer Container
        GameObject timerContainer = new GameObject("TimerContainer");
        timerContainer.transform.SetParent(viewTransform, false);

        RectTransform timerRect = timerContainer.AddComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.5f, 1f);
        timerRect.anchorMax = new Vector2(0.5f, 1f);
        timerRect.pivot = new Vector2(0.5f, 1f);
        timerRect.anchoredPosition = new Vector2(0, -10);
        timerRect.sizeDelta = new Vector2(150, 50);

        // Add background
        var bg = timerContainer.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 0.5f);

        // Create timer text
        GameObject timerTextObj = new GameObject("RoundTimerText");
        timerTextObj.transform.SetParent(timerContainer.transform, false);
        var timerText = timerTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        timerText.text = "03:00";
        timerText.fontSize = 32;
        timerText.fontStyle = TMPro.FontStyles.Bold;
        timerText.alignment = TMPro.TextAlignmentOptions.Center;

        RectTransform textRect = timerTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Create Respawn Container
        GameObject respawnContainer = new GameObject("RespawnContainer");
        respawnContainer.transform.SetParent(viewTransform, false);
        respawnContainer.SetActive(false);

        RectTransform respawnRect = respawnContainer.AddComponent<RectTransform>();
        respawnRect.anchorMin = new Vector2(0.5f, 0.5f);
        respawnRect.anchorMax = new Vector2(0.5f, 0.5f);
        respawnRect.pivot = new Vector2(0.5f, 0.5f);
        respawnRect.anchoredPosition = Vector2.zero;
        respawnRect.sizeDelta = new Vector2(300, 60);

        var respawnBg = respawnContainer.AddComponent<UnityEngine.UI.Image>();
        respawnBg.color = new Color(0, 0, 0, 0.7f);

        // Create respawn text
        GameObject respawnTextObj = new GameObject("RespawnTimerText");
        respawnTextObj.transform.SetParent(respawnContainer.transform, false);
        var respawnText = respawnTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        respawnText.text = "Respawning in 3...";
        respawnText.fontSize = 24;
        respawnText.alignment = TMPro.TextAlignmentOptions.Center;

        RectTransform respawnTextRect = respawnTextObj.GetComponent<RectTransform>();
        respawnTextRect.anchorMin = Vector2.zero;
        respawnTextRect.anchorMax = Vector2.one;
        respawnTextRect.offsetMin = Vector2.zero;
        respawnTextRect.offsetMax = Vector2.zero;

        // Set serialized field references
        var so = new SerializedObject(mainGameView);
        so.FindProperty("roundTimerText").objectReferenceValue = timerText;
        so.FindProperty("timerContainer").objectReferenceValue = timerContainer;
        so.FindProperty("respawnContainer").objectReferenceValue = respawnContainer;
        so.FindProperty("respawnTimerText").objectReferenceValue = respawnText;
        so.ApplyModifiedProperties();

        Undo.RegisterCreatedObjectUndo(timerContainer, "Create MainGameView Timer");
        Undo.RegisterCreatedObjectUndo(respawnContainer, "Create MainGameView Respawn");

        Debug.Log("MainGameView timer and respawn UI created and assigned.");
    }
}
