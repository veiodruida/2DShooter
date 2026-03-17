# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

2D top-down space shooter built in Unity. 4 scenes: MainMenu + 3 levels. Wave-based gameplay with a multi-stage boss (MotherShip) as the final encounter in each level.

## Development Commands

This is a Unity project — there is no CLI build command. Use the Unity Editor directly.

- **Open project:** Unity Hub → Open → `D:\Unity Projects\2DShooter`
- **Run tests:** Unity Editor → Window → General → Test Runner
- **Build:** File → Build Settings → Build

## Architecture

### Singletons / Global State

Three persistent singletons coordinate all systems:

- **`GameManager`** (`Assets/Scripts/Utility/GameManager.cs`) — score, timer, enemy count, win/lose conditions, PlayerPrefs save/load. All systems call into this for game state changes.
- **`GameSettings`** (`Assets/Scripts/Utility/GameSettings.cs`) — loads `DifficultyData` ScriptableObject from PlayerPrefs selection. Persists across scenes via `DontDestroyOnLoad`.
- **`UIManager`** (`Assets/Scripts/UI/UIManager.cs`) — page-based UI navigation (Main Menu, Pause, Victory, GameOver). Auto-spawns from `Resources/UIManager` if not in scene.

### Difficulty System

Difficulty is a `DifficultyData` ScriptableObject (assets in `Assets/Dificuldade/`). Every tunable value lives here: player speed/fire rate, enemy speed/health/spawn timing, boss behavior, item drop rates, damage multipliers. All spawners and entities read from `GameSettings.dadosDeDificuldade` at start.

Difficulties: `Facil` (0.7×), `Medio` (1.0×), `Dificil` (1.4×), `Furia` (2.0×). Multipliers are applied by `GameManager.CalcularVidaInimigo()` and `GameManager.GetDificuldadeMultiplier()`.

### Entity Component Design

Player, enemies, and boss all share the same components:
- **`Health`** (`Assets/Scripts/Health&Damage/Health.cs`) — universal health/lives system. `teamId == 0` = player/shield; `teamId != 0` = enemies. Handles invincibility frames, death effects, optional lives system.
- **`ShootingController`** (`Assets/Scripts/ShootingProjectiles/ShootingController.cs`) — unified weapon for all entities. `isPlayerControlled` flag switches behavior. 3 weapon levels; higher levels fire more projectiles.

### Boss Flow (MotherShip)

Stage 1: Boss spawns 10 enemy waves; its shield is invincible. Stage 2: After all waves die, shield becomes vulnerable. Stage 3: After shield is destroyed, boss itself is vulnerable. Fury modes trigger at 60% and 25% health (thresholds from `DifficultyData`).

### Persistence (PlayerPrefs keys)

| Key | Value |
|-----|-------|
| `DificuldadeSelecionada` | int (0–3) |
| `highscore` | int |
| `score` | int (current run) |
| `melhor_tempo` | float |
| `historico_partidas` | string — last 10 records as `"points\|time,..."` |

### Scene Build Order

1. `MainMenu`
2. `Level1`
3. `Level2`
4. `Level3`

## Key File Locations

| System | Path |
|--------|------|
| Player controller | `Assets/Scripts/Player/Controller.cs` |
| Shooting (all entities) | `Assets/Scripts/ShootingProjectiles/ShootingController.cs` |
| Enemy AI | `Assets/Scripts/Enemies/Enemy.cs` |
| Boss | `Assets/Scripts/Enemies/MotherShip.cs` |
| Health/damage | `Assets/Scripts/Health&Damage/Health.cs` |
| Power-ups | `Assets/Scripts/Items/PowerUpItem.cs` |
| Difficulty config | `Assets/Scripts/Utility/DifficultyData.cs` + `Assets/Dificuldade/` |
| UI | `Assets/Scripts/UI/UIManager.cs` |
| Scenes | `Assets/_Scenes/` |

## Input System

Uses the **new Unity Input System** (not the legacy Input class). All player input goes through the Input System asset; `Controller.cs` and `UIManager.cs` use `InputAction` callbacks.
