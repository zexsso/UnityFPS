# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a multiplayer first-person shooter (FPS) game built with Unity 6 using **PurrNet** for networking. The project name is "MyFPS_PurrNet".

## Key Technologies

- **Unity 6** (6000.3.2f1) with Universal Render Pipeline (URP)
- **PurrNet** - Networking framework (via git: `https://github.com/PurrNet/PurrNet.git?path=/Assets/PurrNet#release`)
- **Cinemachine 3.1.3** - Camera system
- **Unity Input System 1.17.0** - Modern input handling via `GameInput` singleton
- **Unity MCP** - Editor integration for AI assistance

## Architecture

### Input System (New)

All input is handled through `GameInput.cs` singleton using the new Unity Input System:

```csharp
// Access input values
GameInput.Instance.MoveInput      // Vector2 for movement
GameInput.Instance.LookInput      // Vector2 for camera
GameInput.Instance.AttackPressed  // Single shot
GameInput.Instance.AttackHeld     // Automatic fire
GameInput.Instance.JumpPressed
GameInput.Instance.CrouchPressed
GameInput.Instance.SprintHeld
GameInput.Instance.CancelPressed  // ESC key
GameInput.Instance.ScoreboardHeld // Tab key
```

Input actions are defined in `Assets/InputSystem_Actions.inputactions`.

### Networking with PurrNet

All networked scripts inherit from `NetworkBehaviour`. Key patterns:

- **SyncVar<T>**: Synchronized variables (e.g., `SyncVar<int> playerHealth`)
- **SyncDictionary<K,V>**: Synchronized collections (e.g., scores)
- **[ServerRpc]**: Client-to-server calls. Use `requireOwnership: false` when any client can call
- **[ObserversRpc]**: Server-to-all-clients calls. Use `runLocally: true` to also run on caller
- **NetworkAnimator**: Synchronizes animator parameters across network
- **isOwner**: Check if local client owns this NetworkBehaviour
- **owner.Value / owner.HasValue**: Get/check the PlayerID owner
- **GiveOwnership(PlayerID)**: Transfer ownership to a player

### State Machine (PurrNet.StateMachine)

Game flow is managed through `StateNode` classes:

```
WaitForPlayersState → PlayerSpawningState → RoundRunningState → RoundEndState → GameEndState
                              ↑___________________________________|
```

- `StateNode`: Base state class with `Enter(bool asServer)` and `Exit(bool asServer)`
- `StateNode<T>`: State that receives data from previous state
- `machine.Next()`: Advance to next state
- `machine.SetState(stateNode)`: Jump to specific state

### InstanceHandler Pattern

Global singleton access for managers and views:
```csharp
InstanceHandler.RegisterInstance(this);      // In Awake
InstanceHandler.UnregisterInstance<T>();     // In OnDestroy
InstanceHandler.GetInstance<T>();            // Get instance
InstanceHandler.TryGetInstance<T>(out var x); // Safe get
```

### View System

UI views extend abstract `View` class with `CanvasGroup` visibility control:
- `GameViewManager.ShowView<T>()` / `HideView<T>()`
- Views automatically handle `interactable` and `blocksRaycasts`
- Views: MainGameView, ScoreboardView, WaitingForPlayersView, EndGameView, SettingsView, KillFeedView

### Audio System

Centralized audio via `AudioManager` singleton:
```csharp
AudioManager.Instance.PlayWeaponFire(position);
AudioManager.Instance.PlayHitMarker();
AudioManager.Instance.PlayHeadshot();
AudioManager.Instance.PlayDeath(position);
```

Supports 3D spatial audio with object pooling for efficiency.

### Object Pooling

`EffectPoolManager` provides pooled particle effects:
```csharp
EffectPoolManager.Instance.GetEnvironmentHitEffect(position, rotation);
EffectPoolManager.Instance.GetPlayerHitEffect(position, rotation);
```

## Project Structure

```
Assets/
├── Scripts/
│   ├── GameStates/      # State machine nodes (game flow)
│   ├── Managers/        # ScoreManager, GameInput, AudioManager, ObjectPool
│   ├── PlayerScripts/   # PlayerController, PlayerHealth
│   ├── UI/              # View classes, KillFeed, GameViewManager
│   └── WeaponScripts/   # Weapon (raycast shooting)
├── Prefabs/
│   ├── Player.prefab           # Networked player character
│   ├── NetworkPrefabs.asset    # PurrNet prefab registry
│   └── *Impact.prefab          # VFX prefabs
└── Scenes/
    └── DefaultScene.unity      # Main game scene
```

## Key Scripts

- **PlayerController.cs**: First-person movement with sprint. Uses new Input System. Only enabled for owner.
- **PlayerHealth.cs**: Uses `SyncVar<int>` for health. Death triggers `OnDeath_Server` action. Supports headshot tracking.
- **Weapon.cs**: Raycast shooting with recoil animation. Uses object pooling for effects. Audio integration.
- **ScoreManager.cs**: Tracks kills/deaths per PlayerID using `SyncDictionary`. Broadcasts kills to KillFeed.
- **GameInput.cs**: Singleton wrapper for new Input System. Provides clean access to all input values.
- **AudioManager.cs**: Centralized audio with 3D spatial sound and object pooling.
- **EffectPoolManager.cs**: Object pooling for particle effects to reduce GC.

## Development Notes

- Player layers: Separate layers for self (`PlayerSelf`) vs other players (`PlayerOther`)
- Headshot detection: Uses collider tag "Head" for increased damage
- Scoreboard toggle: Tab key shows/hides ScoreboardView
- Settings: Mouse sensitivity stored in PlayerPrefs ("MouseSensitivity")
- Volume settings: MasterVolume, SFXVolume, MusicVolume in PlayerPrefs
- Round timer: Configurable duration in RoundRunningState (default 3 minutes)

## Scene Setup Requirements

For a new scene to work properly, ensure:
1. `GameInput` prefab/object with InputActionAsset assigned
2. `AudioManager` for audio (optional but recommended)
3. `EffectPoolManager` for effect pooling (optional but recommended)
4. `GameViewManager` with all View references
5. PurrNet NetworkManager configured
6. State machine with all game states linked

## Editor Setup Tools

Use Unity menu items to quickly set up components:

- **Tools > Setup Game Managers**: Creates GameInput, AudioManager, EffectPoolManager
- **Tools > Setup UI > Create Kill Feed**: Creates KillFeedView on Canvas
- **Tools > Setup UI > Create Kill Feed Entry Prefab**: Creates prefab for kill feed entries
- **Tools > Setup UI > Create MainGameView Timer**: Adds timer and respawn UI to MainGameView

Note: `ManagersAutoSetup.cs` automatically creates managers at runtime if they don't exist, but audio clips and effect prefabs need to be assigned in the Inspector.

## Common Patterns

### Adding a New Synced Variable
```csharp
[SerializeField] private SyncVar<float> myValue = new(0f);

protected override void OnSpawned()
{
    base.OnSpawned();
    if (isOwner)
        myValue.onChanged += OnMyValueChanged;
}

protected override void OnDestroy()
{
    base.OnDestroy();
    myValue.onChanged -= OnMyValueChanged;
}
```

### Adding Kill Feed Entry
```csharp
if (InstanceHandler.TryGetInstance(out KillFeedView killFeed))
{
    killFeed.AddKillEntry("KillerName", "VictimName", isHeadshot: true);
}
```

### Playing Audio
```csharp
// 3D positional sound
AudioManager.Instance?.PlayWeaponFire(transform.position);

// UI sound (2D)
AudioManager.Instance?.PlayHitMarker();
```
