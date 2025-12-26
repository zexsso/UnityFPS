# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a multiplayer first-person shooter (FPS) game built with Unity 6 using **PurrNet** for networking. The project name is "MyFPS_PurrNet".

## Key Technologies

- **Unity 6** with Universal Render Pipeline (URP)
- **PurrNet** - Networking framework (installed via git: `https://github.com/PurrNet/PurrNet.git?path=/Assets/PurrNet#release`)
- **Cinemachine** - Camera system
- **Unity Input System** - Input handling
- **Unity MCP** - Editor integration for AI assistance

## Architecture

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
- Views: MainGameView, ScoreboardView, WaitingForPlayersView, EndGameView, SettingsView

## Project Structure

```
Assets/
├── Scripts/
│   ├── GameStates/      # State machine nodes (game flow)
│   ├── Managers/        # ScoreManager
│   ├── PlayerScripts/   # PlayerController, PlayerHealth
│   ├── UI/              # View classes and GameViewManager
│   └── WeaponScripts/   # Weapon (raycast shooting)
├── Prefabs/
│   ├── Player.prefab           # Networked player character
│   ├── NetworkPrefabs.asset    # PurrNet prefab registry
│   └── *Impact.prefab          # VFX prefabs
└── Scenes/
    └── DefaultScene.unity      # Main game scene
```

## Key Scripts

- **PlayerController.cs**: First-person movement, uses CharacterController. Only enabled for owner (`enabled = isOwner`)
- **PlayerHealth.cs**: Uses `SyncVar<int>` for health. Death triggers `OnDeath_Server` action
- **Weapon.cs**: Raycast shooting with recoil animation. Damage dealt via `PlayerHealth.ChangeHealt()` ServerRpc
- **ScoreManager.cs**: Tracks kills/deaths per PlayerID using `SyncDictionary`

## Development Notes

- Player layers: Separate layers for self vs other players (set in PlayerHealth.OnSpawned)
- Headshot detection: Uses collider tag "Head" for increased damage
- Scoreboard toggle: Tab key shows/hides ScoreboardView
- Settings: Mouse sensitivity stored in PlayerPrefs ("MouseSensitivity")
