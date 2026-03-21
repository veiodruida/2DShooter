# 2D SHOOTER - SYSTEM PROMPT & FÚRIA ARCHITECTURE (FOR CLAUDE/LLM CONTEXT)

**ROLE & PURPOSE:** This document is the absolute source of truth for the 2DShooter Unity Project. As an AI Agent modifying this codebase, you must ingest this document to understand the global structure, fundamental mechanics, and the "Fúria Mode" constraints. Failure to adhere to these exact implementations will recreate solved bugs (Ghost projectiles, Race conditions, Vector failures).

---

## 1. PROJECT OVERVIEW & GAMEPLAY LOOP
- **Genre:** 2D Space Shooter (Top-Down/Arcade).
- **Core Loop:** The Player controls a ship, survives waves of enemies and Asteroids, uses Bombs, and manages a Shield system. 
- **Difficulty/Pacing:** Governed by `GameManager.cs` e `GameSettings.cs` (which dictate speed, frequency, and curvature of hazards like Bombs and Asteroids in "Fúria" mode vs normal).

---

## 2. CORE ARCHITECTURE & SCRIPTS

### 2.1 The Health & Damage Ecosystem (CRITICAL)
This project uses a decoupled Health/Damage system. Never mix them.
- **`Health.cs`**: attached to ALL destroyable entities (Player, Asteroid, Boss, Enemy, Bomb).
  - Handles `currentLives`, `invencibilidadeTimer` (I-Frames), and instantiates `hitEffect` / `deathEffect`.
  - **Constraint:** Cloned objects (like Asteroid children generated via `Instantiate(gameObject)`) MUST have their private states (like `estaDestruindo`) explicitly reset upon birth.
- **`Damage.cs`**: attached to ANY entity that deals damage (Projectiles, Asteroids, Kamikazes).
  - Uses triggers to push its `damageAmount` value into the target's `Health.cs` (`DealDamage`).
  - **Constraint:** NEVER hardcode damage values like `TakeDamage(1)`. Always read `other.GetComponent<Damage>()?.damageAmount`.

### 2.2 Player & Controllers
- **`Controller.cs`**: Exists on the Player spaceship. Handles physical velocity, boosting, and links to the `shieldObject` hierarchy.
- **`ShootingController.cs` & `Projectile.cs`**: Manages weapon arrays, instantiating `Projectile` prefabs. Projectiles are driven by their own forward velocity and manage their own destruction upon hitting different `teamId` targets via `Damage.cs`.

### 2.3 The Enemy & Hazard Systems
- **`AsteroidSpawner.cs`**: Governs off-screen spawning. 
  - **Mechanic:** Must read the Camera's Orthographic boundaries. Asteroids spawn on an invisible outer ring and their `direcaoMovimento` vectors are mathematically forced to intercept the inner playable screen area via calculated target points.
- **`Asteroid.cs`**: Splitting/Fractal logic.
  - When reaching 0 Health, an asteroid calls `DividirOuExplodir()`. If generations remain, it instantiates children.
  - **Constraint - Evasion:** Children are born with horizontal offsets, 1.8x speed multipliers, aggressive diagonal vectors, and a 0.35s I-Frame shield (`Collider2D.enabled = false`) to escape the explosion radius of the parent without dying instantly.
- **`Bomb.cs` (MotherShip Bomb / Screen Hazard)**: 
  - Employs a Spherical `Slerp` Steering logic to curve towards the Player. Uses manual collision overrides to guarantee explosive delivery against asteroids.

### 2.4 Interface & Syncing
- **`UIManager.cs` / `UIelement.cs`**: All health bars or dynamic UI panels (e.g. `UiShieldDisplay.cs` or `UIHealthDisplay.cs`) MUST inherit from `UIelement`. 
  - **Sync Rule:** They must auto-locate their targets (`FindGameObjectWithTag("Player")`) in `Start()` to guarantee the UI populates on Frame 1, avoiding decoupled null references across Scene reloads.

---

## 3. "FÚRIA MODE" MANDATORY WORKAROUNDS

Whenever modifying collision or instantiation logic, you MUST obey these Unity Hacks established to preserve stability:

### 3.1 The "OverlapCircle" Physics Fix
Unity's `OnTriggerEnter2D` often fails silently on objects moved via `Transform.Translate` without `Rigidbody2D`, or when Layer Collision Matrices are corrupted by creating new Tags ("Bomb" tag bug).
- **Rule:** If a projectile or bomb is phasing through targets, inject a manual radar in `Update()`:
  ```csharp
  Collider2D[] colisoes = Physics2D.OverlapCircleAll(transform.position, 0.7f);
  foreach (var col in colisoes) { OnTriggerEnter2D(col); } // Força a execução do trigger native
  ```

### 3.2 Race Condition Extinction (Mutual Death)
When two highly volatile objects collide (e.g., Bomb vs Asteroid), processing destruction synchronously across scripts drops colliders before both objects register the hit, breaking logic (Asteroids surviving direct hits).
- **Rule:** A dominant object (like the Bomb) must force the target's damage explicitly inline before killing itself:
  ```csharp
  int danoReal = GetComponent<Damage>() != null ? GetComponent<Damage>().damageAmount : 1;
  other.GetComponent<Health>()?.TakeDamage(danoReal); // Applies calculated damage synchronously
  GetComponent<Health>()?.Die(); // Explodes the Bomb instantly + spawns effects
  ```

### 3.3 Visual Effect Auditing (Anti-Ghost Projectiles)
Designers often drag Projectile Prefabs into `particulasExplosao` or `hitEffect` fields out of error.
- **Rule:** Whenever instantiating an effect in `Asteroid.cs` or `Health.cs`, you must assert it is NOT a Projectile to prevent enemies from shooting backwards upon taking damage:
  ```csharp
  if (fx.GetComponent<Projectile>() != null) { return; } // Abort instantiation to prevent ghostly bugs
  ```

### 3.4 Camera Viewport True Clamping
- **Rule:** `Mathf.Clamp` applied strictly to `transform.position` bounds allows the edge of the screen to peek the void. Always modify the position limits using the true lens geometry (Orthographic Size * Aspect):
  ```csharp
  float camHalfHeight = playerCamera.orthographicSize;
  float camHalfWidth = playerCamera.aspect * camHalfHeight;
  pos.x = Mathf.Clamp(pos.x, minBounds.x + camHalfWidth, maxBounds.x - camHalfWidth);
  ```

**[END OF SYSTEM PROMPT]** Read and internalize this architecture before generating any scripts for the 2DShooter.
