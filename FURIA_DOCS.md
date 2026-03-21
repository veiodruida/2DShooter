# 2D SHOOTER - MASTER ENCYCLOPEDIA & SYSTEM PROMPT (FOR LLM/CLAUDE CONTEXT)

**ROLE & PURPOSE:** This document is the absolute source of truth for the 2DShooter Unity Project. As an AI Agent modifying this codebase, you must ingest this document. It outlines EVERY core module, dependency, mechanic, and the exact constraints required to avoid regressions.

---

## 1. GAMEPLAY & MACRO ARCHITECTURE
- **Core Loop:** The Player controls a ship (`Controller.cs`), shooting dynamically (`ShootingController.cs`) against a continuous wave of enemies (`EnemySpawner.cs`) and mathematically-guided asteroids (`AsteroidSpawner.cs`).
- **Logic Decoupling:** Health is completely isolated from Damage. An object never directly depletes its own health through collision code; it always defers to interacting with the target's `Health.cs` script while propagating data from its own `Damage.cs` script.

---

## 2. THE C# SCRIPT TAXONOMY (MODULE INDEX)

### 2.1 Health & Damage (The Foundational Tier)
- **`Health.cs`**: The Universal Receiver. Exists on Player, MotherShip(Boss), Asteroids, and regular Enemies. Manages `currentLives`, I-Frames cooldown (`invencibilidadeTimer`), and executes the `Die()` routine (summoning `deathEffect`).
- **`Damage.cs`**: The Universal Dealer. Exists on Projectiles, Kamikaze Enemies, Asteroids. Deals integer damage on impact and spawns `hitEffect`. **ABSOLUTE RULE:** Never, under any circumstance, hardcode damage explicitly (like `TakeDamage(999)`). Always query the component: `other.GetComponent<Damage>()?.damageAmount`.

### 2.2 Player & Movement
- **`Controller.cs`**: The physical avatar. Drives Rigidbody/Transform velocity based on input axes and delegates shielding visually to `ShieldController.cs`.
- **`ScreenClearBomb.cs` & `PlayerBomb.cs`**: AoE tactical countermeasures protecting the player.
- **`ShieldController.cs`**: Defensive orbital layer. While linked to the Player, it absorbs raw physical triggers natively before they hit the ship.

### 2.3 Weapon Systems (Projectiles)
- **`ShootingController.cs`**: The gun barrel. Generates instances of bullets relying on customizable cooldowns and fire points.
- **`Projectile.cs`**: The physical Payload. Translates strictly forward. Automatically flags self-destruction upon hitting a target with an opposing `teamId` (0 = Caster is Player, 1 = Caster is Enemy).

### 2.4 Enemies & Hazards
- **`Asteroid.cs`**: Feature a fractal cloning mechanic. Upon reaching `Die()`, it invokes `DividirOuExplodir()`. **RULE:** Clones initiated via `Instantiate(gameObject)` inherit the dead parent's internal states. Private variables like `estaDestruindo=true` MUST be manually scrubbed to `false` in the child immediately. Clones are given temporary 0.35s I-Frames (`Collider.enabled=false`), spawned horizontally offset, and launched at 1.8x velocity to guarantee evasion from the player's core explosion AoE.
- **`AsteroidSpawner.cs`**: Balistic Spawning Tracker. Spawns Asteroids in an invisible outer-bounds margin native to its variables (`limiteX/Y`), but dynamically measures the inner dimensions of `CameraController.cs` to generate an intercept vector that crosses right through the observable player window.
- **`Enemy.cs` & `MotherShip.cs`**: The AI Combatants orchestrating varying patterns of shooting and evasive maneuvering.
- **`Bomb.cs` (Boss Bomb)**: Tracks the player using spherical interpolation (`Slerp`). Because it moves entirely via `Transform` without Rigidbody mass, it employs the Fúria Override (see 3.1) to avoid passing harmlessly through asteroids.

### 2.5 Camera Extents
- **`CameraController.cs`**: Limits player vision mathematically. Do not use raw Center-clamping on the X/Y axes. Clamp utilizing the lens geometry to block the screen edge perfectly against the abyss:
  `camHalfWidth = aspect * orthoSize`

### 2.6 User Interface (Sync Layer)
- **`UIManager.cs` & Modules (`UiShieldDisplay.cs`, `UIHealthDisplay.cs`)**: For bulletproof synchronization, no UI updates manually via standard `Update()`. They all inherit from our base class `UIelement.cs`. On `Start()`, they auto-locate their Scene target (`GameObject.FindGameObjectWithTag("Player")`) and cache references. `UIManager.instance.UpdateUI()` broadcasts changes passively.

---

## 3. "FÚRIA MODE" HACKS & BUG PREVENTION

Whenever generating or modifying logic in this ecosystem, YOU MUST strictly obey these native overrides:

### 3.1 The "OverlapCircle" Force Trigger
- Unity's native `OnTriggerEnter2D` drops collisions on scaled GameObjects translating without Rigidbodies, or when Layer matrices desync natively.
- **Protocol:** If a Projectile or Bomb fails to detect a hit, inject a manual radar sweep in the `Update()`:
  ```csharp
  Collider2D[] radar = Physics2D.OverlapCircleAll(transform.position, 0.7f);
  foreach (var obj in radar) { OnTriggerEnter2D(obj); } // Força a execução nativa
  ```

### 3.2 Race Condition Immunity
- When two highly-volatile entities (Bomb vs Asteroid) touch, processing destruction asynchronously across scripts drops colliders before the second object registers the hit (making Asteroids immune).
- **Protocol:** The dominant script (e.g. `Bomb.cs`) must process both deaths in local scope before terminating:
  ```csharp
  int targetDmg = GetComponent<Damage>() != null ? GetComponent<Damage>().damageAmount : 1;
  other.GetComponent<Health>()?.TakeDamage(targetDmg); // Fere o Asteroide ativamente
  GetComponent<Health>()?.Die(); // Auto-destrói a Bomba na mesma Frame
  ```

### 3.3 The "Ghost Bullet" Audit Rule
- Missing/invalid setups in the Unity Inspector often mistakenly assign `Projectile` prefabs into `deathEffect` or `particulasExplosao` fields, making enemies shoot backwards upon taking damage.
- **Protocol:** ALWAYS audit visual effects right before instantiation in scripts like `Health.cs` or `Asteroid.cs`:
  ```csharp
  if (fx.GetComponent<Projectile>() != null) { return; } // Bloqueio cirúrgico do Bug Fantasma
  ```

## 4. DIFFICULTY SCALING EXPERTISE (GAME SETTINGS)

O ritmo de jogo e agressividade matemática dos inimigos são brutalmente geridos através dos singletons `GameSettings.cs` e `GameManager.cs`. 
- **O Enum de Dificuldade (`GameSettings.Dificuldade`)**: Contém os tiers standard (Easy, Medium, Hard) e o tier supremo `Furia` (Cuba Mode).
- **Protocolo de Implementação (Como a IA local deve injetar Dificuldade):**
  - **Standard Mode Scaling:** Multiplicar as speeds e forcas base dos objectos combinando `GameSettings.instance.nivelAtual` e um cast da própria dificuldade base `(int)dificuldadeSelecionada`. 
    *Exemplo (`Bomb.cs`)*: `velocidade *= (1f + (nivelAtual * 0.1f))` ou `forcaCurva *= (1f + ((int)dificuldade * 0.2f))`
  - **Fúria Mode Scaling:** Hard-override para estatísticas sádicas que ignoram matemática normal se `dificuldadeSelecionada == Furia`. 
    *Exemplo (`AsteroidSpawner.cs`)*: Se `Furia`, o `tempoParaProximoSpawn *= 0.4f` (criando 2.5x mais spawns) ou no caso das `Bomb`, a forca de Steering (Curva de Perseguição) vira `* 2.5f`, impossibilitando fuga padrão do jogador sem uso de items.
- Nunca usar randomizadores flat isolados para dificuldade. O código deve interpelar sempre os singletons no `Awake()`/`Start()` de cada Prefab Spawnado.

**[END OF SYSTEM PROMPT]** Read and internalize this taxonomy map and constraint blueprint before suggesting fixes or writing new scripts for the 2DShooter.
