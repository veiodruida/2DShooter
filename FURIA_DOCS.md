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

#### Sistema de Movimento do Jogador
- **Movimento Absoluto**: WASD/joystick movem sempre para frente/atrás/esquerda/direita no espaço mundial, **independente da rotação da nave**.
- **Física de Motor**: Usa `Vector2.SmoothDamp` para fornecer aceleração/desaceleração suave.
- **Implementação**:
  - `HandleInput()`: Coleta input, aplica `SmoothDamp` para suavização com `accelerationTime = 0.3f`.
  - `MovePlayer()`: Aplica movimento direto em `transform.position` (modo padrão) ou `AddForce` (modo Asteroides).
  - `movementMode`: Enum que controla o tipo de movimento (`FreeRoam`, `MoveHorizontally`, `MoveVertically`, `Astroids`).
  - `lockXCoordinate`/`lockYCoordinate`: Bloqueiam eixos quando necessário (ex: movimento apenas vertical/horizontal).
- **Colisão com Paredes**: Verifica `Physics2D.OverlapCircleAll` antes de aplicar movimento para evitar atravessar boundaries.
- **Knockback**: Força de repulsão aplicada via `ApplyKnockback()` que atenua o controle do jogador.
- **Som de Dano**: `hitSound` só executado uma vez durante o tempo de invincibilidade (`isInvincible`).
- **Nota Importante**: O movimento do jogador foi corrigido para garantir que WASD/joystick movem sempre para frente/atrás/esquerda/direita no espaço mundial, não relativo à rotação da nave. O sistema de física de motor foi adaptado para este movimento absoluto.

#### Sistema de Saúde e Dano
- **`Health.cs`**: Gerencia `currentLives`, I-Frames cooldown (`invencibilidadeTimer`), e executa a rotina `Die()`.
  - **Métodos Principais**:
    - `TakeDamage(int damageAmount)`: Aplica dano, verifica invencibilidade, executa efeitos visuais e sonoros.
    - `Die()`: Executa lógica de morte (divisão de asteroides, finalização de boss, respawn).
    - `HandleDeathWithLives()`: Gerencia respawn quando `useLives = true`.
    - `HandleDeathWithoutLives()`: Executa `GameOver()` quando `useLives = false`.
    - `ForceDeathAnimation()`: Força animação de morte mesmo para escudo.
    - `ReceiveHealing(int healingAmount)`: Aplica cura ao objeto.
    - `SetRespawnPoint(Vector3 newPos)`: Define ponto de respawn.
  - **Variáveis Principais**:
    - `currentHealth`: Health atual do objeto.
    - `currentLives`: Vidas restantes (se `useLives = true`).
    - `isInvincible`: Estado de invencibilidade (I-Frames).
    - `timeToBecomeDamagableAgain`: Tempo até o objeto voltar a ser vulnerável.
    - `deathEffect`: Efeito visual de morte.
    - `hitEffect`: Efeito visual de dano.
    - `hitSound`: Som de dano (executado uma vez).
    - `deathSound`: Som de morte.
    - `characterSprite`: SpriteRenderer para efeitos de piscar.
    - `useLives`: Se o objeto usa sistema de vidas.
    - `invincibilityTime`: Tempo de invencibilidade (I-Frames).
    - `teamId`: ID da equipa (0 = Player, 1 = Enemy).

- **`Damage.cs`**: Lida com a aplicação de dano em objetos com `Health`.
  - **Métodos Principais**:
    - `OnTriggerEnter2D(Collider2D collision)`: Aplica dano ao entrar num trigger.
    - `OnTriggerStay2D(Collider2D collision)`: Aplica dano enquanto estiver num trigger (DoT).
    - `OnCollisionEnter2D(Collision2D collision)`: Aplica dano ao colidir.
    - `DealDamage(GameObject collisionGameObject)`: Função interna que aplica dano.
  - **Variáveis Principais**:
    - `damageAmount`: Quantidade de dano a aplicar.
    - `repulsionForce`: Força de repulsão (knockback) aplicada ao alvo.
    - `hitEffect`: Efeito visual de dano.
    - `destroyAfterDamage`: Se destruir o objeto após aplicar dano.
    - `dealDamageOnTriggerEnter`: Se aplicar dano em triggers.
    - `dealDamageOnTriggerStay`: Se aplicar dano em triggers (DoT).
    - `dealDamageOnCollision`: Se aplicar dano em colisões.
    - `teamId`: ID da equipa (0 = Player, 1 = Enemy).
  - **Regras Absolutas**:
    - Nunca hardcode dano explicitamente. Sempre use `other.GetComponent<Damage>()?.damageAmount`.
    - Força de repulsão é aplicada independentemente de estar invencível ou não.
    - Som de dano só executado uma vez durante o tempo de invincibilidade.

- **`ShieldController.cs`**: Camada defensiva orbital que absorve gatilhos físicos nativamente antes de atingir a nave.
  - **Métodos Principais**:
    - `ActivarEscudo()`: Ativa o escudo.
    - `DesativarEscudo()`: Desativa o escudo.
    - `TomarDano(int damageAmount)`: Aplica dano ao escudo.
  - **Variáveis Principais**:
    - `shieldHealth`: Health do escudo.
    - `shieldDuration`: Duração do escudo.
    - `shieldCooldown`: Cooldown entre ativações.
    - `shieldVisual`: SpriteRenderer do escudo.
    - `shieldColor`: Cor do escudo.

#### Sistema de Movimento do Jogador
- **`Controller.cs`**: O avatar físico. Impulsiona a velocidade do Rigidbody/Transform com base nos eixos de input e delega o escudo visualmente ao `ShieldController.cs`.
  - **Métodos Principais**:
    - `HandleInput()`: Coleta input, aplica `SmoothDamp` para suavização, executa movimento.
    - `MovePlayer(Vector2 movement)`: Aplica movimento absoluto no espaço mundial.
    - `ApplyKnockback(Vector2 force)`: Aplica força de repulsão externa.
    - `LookAtPoint(Vector2 lookPoint)`: Faz a nave olhar para um ponto.
    - `GanharEscudo(int vidas)`: Adiciona vidas ao escudo.
    - `GanharBomba(int quantidade)`: Adiciona bombas ao jogador.
  - **Variáveis Principais**:
    - `myRigidbody`: Rigidbody2D do jogador.
    - `moveSpeed`: Velocidade de movimento.
    - `rotationSpeed`: Velocidade de rotação.
    - `accelerationTime`: Tempo de aceleração do motor (0.3f).
    - `currentInputVector`: Input atual.
    - `smoothInputVelocity`: Input suavizado.
    - `knockbackVelocity`: Velocidade de repulsão acumulada.
    - `aimMode`: Modo de mira (Mouse, Forwards, DualStickMobile).
    - `movementMode`: Modo de movimento (FreeRoam, MoveHorizontally, MoveVertically, Astroids).
    - `shieldObject`: Objeto do escudo.
    - `startEngineSound`: Som de partida do motor.
    - `engineSound`: Som do motor.
    - `turbineAnimator`: Animator do turbina.
    - `moveAction`: InputAction de movimento.
    - `lookAction`: InputAction de mira.
    - `mobileMoveJoystick`: Joystick de movimento mobile.
    - `mobileLookJoystick`: Joystick de mira mobile.
  - **Sistema de Movimento**:
    - **Movimento Absoluto**: WASD/joystick movem sempre para frente/atrás/esquerda/direita no espaço mundial, **independente da rotação da nave**.
    - **Física de Motor**: Usa `Vector2.SmoothDamp` para fornecer aceleração/desaceleração suave.
    - **Colisão com Paredes**: Verifica `Physics2D.OverlapCircleAll` antes de aplicar movimento para evitar atravessar boundaries.
    - **Knockback**: Força de repulsão aplicada via `ApplyKnockback()` que atenua o controle do jogador.
    - **Som de Dano**: `hitSound` só executado uma vez durante o tempo de invincibilidade (`isInvincible`).

#### Sistema de Armas e Projéteis
- **`ShootingController.cs`**: O canhão. Gera instâncias de balas com base em cooldowns personalizáveis e pontos de tiro.
  - **Métodos Principais**:
    - `Fire()`: Dispara projéteis.
    - `AdicionarProjétil(int quantidade)`: Adiciona projéteis extras.
    - `ResetarArma()`: Reseta a arma.
  - **Variáveis Principais**:
    - `weaponLevel`: Nível da arma (1, 2, 3).
    - `cooldown`: Cooldown entre tiros.
    - `firePoint`: Ponto de tiro.
    - `projectilePrefab`: Prefab do projétil.
    - `isPlayerControlled`: Se é controlado pelo jogador.
    - `teamId`: ID da equipa.

#### Sistema de Inimigos e Perigos
- **`Enemy.cs`**: Os combatentes de IA que orquestram padrões variados de tiro e manobras evasivas.
  - **Métodos Principais**:
    - `DoBeforeDestroy()`: Executa antes da destruição.
    - `MoverInimigo()`: Move o inimigo.
    - `Atirar()`: Atira projéteis.
  - **Variáveis Principais**:
    - `health`: Health do inimigo.
    - `speed`: Velocidade do inimigo.
    - `rotationSpeed`: Velocidade de rotação.
    - `fireRate`: Taxa de tiro.
    - `projectilePrefab`: Prefab do projétil.
    - `teamId`: ID da equipa.

- **`Asteroid.cs`**: Característica com mecânica de clonagem fractal. Ao atingir `Die()`, invoca `DividirOuExplodir()`.
  - **Métodos Principais**:
    - `DividirOuExplodir()`: Divide o asteroide em clones.
    - `MoverAsteroide()`: Move o asteroide.
  - **Variáveis Principais**:
    - `health`: Health do asteroide.
    - `speed`: Velocidade do asteroide.
    - `cloneCount`: Número de clones.
    - `clonePrefab`: Prefab do clone.
    - `estaDestruindo`: Se está a ser destruído.

- **`MotherShip.cs`**: O chefe que orquestra padrões de combate complexos.
  - **Métodos Principais**:
    - `FinalizarBoss()`: Finaliza o boss.
    - `AtivarEstagio2()`: Ativa o estágio 2.
    - `MoverBoss()`: Move o boss.
    - `AtirarBoss()`: Atira projéteis.
  - **Variáveis Principais**:
    - `health`: Health do boss.
    - `shieldHealth`: Health do escudo.
    - `stage`: Estágio atual.
    - `fireRate`: Taxa de tiro.
    - `projectilePrefab`: Prefab do projétil.
    - `spriteEscudoEstagio2`: Sprite do escudo do estágio 2.

- **`Bomb.cs`**: Bomba do boss que rastreia o jogador usando `Slerp`.
  - **Métodos Principais**:
    - `MoverBomba()`: Move a bomba.
    - `Explodir()`: Explode a bomba.
  - **Variáveis Principais**:
    - `health`: Health da bomba.
    - `speed`: Velocidade da bomba.
    - `target`: Alvo da bomba.
    - `explosionRadius`: Raio da explosão.

#### Sistema de Câmera
- **`CameraController.cs`**: Limita a visão do jogador matematicamente.
  - **Métodos Principais**:
    - `AtualizarCamera()`: Atualiza a câmera.
    - `ClampCamera()`: Clamping da câmera.
  - **Variáveis Principais**:
    - `camera`: Câmera principal.
    - `orthoSize`: Tamanho ortográfico.
    - `limiteX`: Limite X da câmera.
    - `limiteY`: Limite Y da câmera.

#### Sistema de Interface do Usuário
- **`UIManager.cs`**: Sincronização de UI.
  - **Métodos Principais**:
    - `UpdateUI()`: Atualiza a UI.
    - `MostrarMenu()`: Mostra o menu.
    - `MostrarGameOver()`: Mostra o game over.
  - **Variáveis Principais**:
    - `playerHealth`: Health do jogador.
    - `scoreText`: Texto de pontuação.
    - `healthText`: Texto de health.
    - `livesText`: Texto de vidas.
    - `timerText`: Texto de tempo.
    - `difficultyText`: Texto de dificuldade.

- **`UiShieldDisplay.cs`**: Display do escudo.
  - **Métodos Principais**:
    - `AtualizarEscudo()`: Atualiza o escudo.
  - **Variáveis Principais**:
    - `shieldHealth`: Health do escudo.
    - `shieldMaxHealth`: Health máximo do escudo.
    - `shieldBar`: Barra do escudo.

- **`UIHealthDisplay.cs`**: Display de health.
  - **Métodos Principais**:
    - `AtualizarHealth()`: Atualiza o health.
  - **Variáveis Principais**:
    - `currentHealth`: Health atual.
    - `maxHealth`: Health máximo.
    - `healthBar`: Barra de health.

#### Sistema de Items e Power-ups
- **`PowerUpItem.cs`**: Item de power-up.
  - **Métodos Principais**:
    - `AtivarPowerUp()`: Ativa o power-up.
    - `Explodir()`: Explode o power-up.
  - **Variáveis Principais**:
    - `powerUpType`: Tipo de power-up.
    - `duration`: Duração do power-up.
    - `effect`: Efeito do power-up.

#### Sistema de Spawners
- **`EnemySpawner.cs`**: Spawner de inimigos.
  - **Métodos Principais**:
    - `SpawnInimigo()`: Spawna inimigos.
    - `PararSpawning()`: Para de spawna.
  - **Variáveis Principais**:
    - `spawnRate`: Taxa de spawn.
    - `enemyPrefab`: Prefab do inimigo.
    - `spawnCount`: Número de inimigos.

- **`AsteroidSpawner.cs`**: Spawner de asteroides.
  - **Métodos Principais**:
    - `SpawnAsteroide()`: Spawna asteroides.
    - `PararSpawning()`: Para de spawna.
  - **Variáveis Principais**:
    - `spawnRate`: Taxa de spawn.
    - `asteroidPrefab`: Prefab do asteroide.
    - `spawnCount`: Número de asteroides.
    - `limiteX`: Limite X de spawn.
    - `limiteY`: Limite Y de spawn.

#### Sistema de Utilitários
- **`GameManager.cs`**: Gerenciador do jogo.
  - **Métodos Principais**:
    - `GameOver()`: Game over.
    - `NivelConcluido()`: Nivel concluído.
    - `CalcularVidaInimigo()`: Calcula vida do inimigo.
    - `GetDificuldadeMultiplier()`: Obtém multiplicador de dificuldade.
  - **Variáveis Principais**:
    - `score`: Pontuação.
    - `timer`: Tempo.
    - `enemyCount`: Número de inimigos.
    - `nivelAtual`: Nivel atual.
    - `dificuldadeSelecionada`: Dificuldade selecionada.
    - `highscore`: Melhor pontuação.
    - `melhor_tempo`: Melhor tempo.
    - `historico_partidas`: Histórico de partidas.

- **`GameSettings.cs`**: Configurações do jogo.
  - **Métodos Principais**:
    - `CarregarConfiguracoes()`: Carrega configurações.
    - `SalvarConfiguracoes()`: Salva configurações.
  - **Variáveis Principais**:
    - `configAtual`: Configuração atual.
    - `dificuldadeSelecionada`: Dificuldade selecionada.
    - `velocidadePlayer`: Velocidade do jogador.
    - `vidasIniciais`: Vidas iniciais.
    - `multiplicadorDanoRecebido`: Multiplicador de dano recebido.

#### Sistema de Input
- **`VirtualJoystick.cs`**: Joystick virtual.
  - **Métodos Principais**:
    - `AtualizarJoystick()`: Atualiza o joystick.
  - **Variáveis Principais**:
    - `ID`: ID único.
    - `controlStick`: Control stick.
    - `deadZone`: Zona morta.
    - `sensitivity`: Sensibilidade.

- **`InputAction.cs`**: Ação de input.
  - **Métodos Principais**:
    - `Enable()`: Habilita a ação.
    - `Disable()`: Desabilita a ação.
  - **Variáveis Principais**:
    - `name`: Nome da ação.
    - `type`: Tipo de ação.
    - `binding`: Binding.

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
- **`UIManager.cs` & Modules (`UiShieldDisplay.cs`, `UIHealthDisplay.cs`)**: For bulletproof synchronization, all UI components must inherit from `UIelement.cs` and implement auto-location in `UpdateUI()`:
  ```csharp
  if (playerHealth == null) {
      GameObject player = GameObject.FindGameObjectWithTag("Player");
      if (player != null) playerHealth = player.GetComponent<Health>();
  }
  ```
  **RULE:** Never trust Inspector references for UI components in dynamic levels (1, 2, 3). Always use Tag-based recovery as a fallback.

### 2.7 Boss Phases & Asset Swapping
- **`MotherShip.cs` Event:** When transitioning to Stage 2 (`AtivarEstagio2()`), do NOT just change `SpriteRenderer.color`. 
- **RULE:** Use physical Asset swapping. Assign the High-Res variant (e.g. `retro_buble_boss_red`) to the `spriteEscudoEstagio2` field and swap the `sr.sprite` dynamically. This preserves pixel density and the intended "Boss Mode" visual identity.

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
