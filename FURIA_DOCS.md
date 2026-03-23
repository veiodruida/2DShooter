# FURIA 2DShooter - Handoff Técnico para Claude/LLM

Este documento é a referência operacional do projeto `D:\Unity_Projects\2DShooter`.
Use-o como contexto base antes de alterar gameplay, prefabs, cenas ou física.

Objetivo deste handoff:
- reduzir reexploração desnecessária
- registrar o estado real do código em 2026-03-22
- listar regras de segurança para evitar regressões
- apontar problemas já corrigidos e pendências reais

---

## 0. Atualizações recentes (2026-03-23)

### Câmera e limites

Arquivos envolvidos:
- `Assets/Scripts/Camera/CameraController.cs`
- `Assets/_Scenes/Level1.unity`
- `Assets/_Scenes/Level2.unity`
- `Assets/_Scenes/Level3.unity`
- `Assets/Prefabs/Camera/Large Camera.prefab`
- `Assets/Prefabs/Camera/Small Camera.prefab`

Estado atual:
- a câmera voltou ao uso de clamp manual simples em `X/Y`
- em `Level1`, os limites manuais salvos atualmente são:
  - `minBounds = (-5.65, -16)`
  - `maxBounds = (-4.45, 5)`
- `detectarLimitesAutomaticamente` está desligado na cena para evitar regressões no `X`
- foi corrigido um problema grave em que a `MainCamera` estava com `Camera.rect` reduzido, fazendo o jogo parecer pequeno mesmo com espaço no ecrã
- o `rect` da câmera foi resetado para tela cheia (`0,0,1,1`) em:
  - `MainMenu`
  - `Level1`
  - `Level2`
  - `Level3`

Observação importante:
- houve várias tentativas de cálculo automático por `Boundary`/`Background`, mas o estado que voltou a funcionar foi o clamp manual
- não reativar detecção automática no `X` sem validar visualmente em runtime

### Escudo do jogador

Arquivo envolvido:
- `Assets/Scripts/Player/ShieldController.cs`

Mudança aplicada:
- o escudo do jogador agora aplica a mesma repulsão usada em `Boundary` também quando colide com:
  - `Boss`
  - `BossShield`

Como funciona agora:
- `ShieldController` procura `Damage` no collider atingido ou no parent
- lê `Damage.repulsionForce`
- chama `Controller.ApplyImmediateRepulsion(...)`

Regra:
- para a repulsão funcionar, os objetos do boss e do escudo do boss precisam de `Damage.repulsionForce > 0` no Inspector

### Records / sessão completa

Arquivos envolvidos:
- `Assets/Scripts/Utility/GameManager.cs`
- `Assets/Scripts/UI/RecordDisplay.cs`

Mudanças aplicadas:
- o score da run continua acumulando entre níveis
- o histórico final (`historico_partidas`) deve registrar uma única entrada por jornada
- o tempo recorde passou a representar a jornada inteira, usando `tempoTotalPartida`
- `RecordDisplay` agora mostra `BEST RUN TIME` em vez de `BEST TIME`

Fluxo pretendido:
- `MainMenu` inicia nova run e chama `ResetScore()`
- `Level1 -> Level2 -> Level3` mantém score acumulado
- só no fim da jornada ou em `GameOver` é que a entrada final é gravada no histórico

Observação importante:
- o botão de vitória no prefab base `LevelVictoryScreen.prefab` ainda aponta para `MainMenu`
- o fluxo correto parece vir de override em `inGameUI.prefab`, onde a instância da página de vitória injeta `Level2`
- se houver bug de progressão de fase, verificar primeiro os overrides do `inGameUI`

### Main Menu / Instructions

Arquivo envolvido:
- `Assets/_Scenes/MainMenu.unity`

Mudança aplicada:
- o texto `InstructionsText` recebeu uma secção `Score System` com os valores reais do jogo

Valores documentados no texto:
- `Straight Shooter`: 5 pontos
- `Straight Chaser`: 5 pontos
- `Diagonal Shooter`: 10 pontos
- `Diagonal Chaser`: 10 pontos
- `Chaser Enemy`: 20 pontos
- `Mothership`: 500 pontos
- `Mothership` em `Cuba/Fury`: 1000 pontos
- bónus de boss clear: até 5000 pontos com penalidade de 50 por segundo
- bónus perfect clear: 10000 pontos
- recorde final usa score total da run

### Multi-monitor / aspect ratio

Arquivos envolvidos:
- `Assets/Scripts/Camera/AspectRatioEnforcer.cs` (criado e depois removido da solução ativa)
- `Assets/Scripts/Camera/CameraController.cs`
- `Assets/Prefabs/UI/inGameUI.prefab`
- `Assets/Prefabs/UI/MainMenu.prefab`

Estado atual:
- foi testada uma abordagem com barras (`AspectRatioEnforcer`), mas ela foi abandonada
- o componente não deve ser considerado parte da solução final ativa
- a solução de barras foi removida das câmeras
- `CanvasScaler` dos prefabs principais foi ajustado para:
  - `Scale With Screen Size`
  - `ReferenceResolution = 1920x1080`
  - `MatchWidthOrHeight = 0.5`

Observação:
- o comportamento multi-monitor ainda precisa de validação visual real em formatos extremos
- qualquer trabalho futuro em aspect ratio deve tomar cuidado para não alterar `Camera.rect` novamente

### WebGL mobile / joysticks

Arquivos envolvidos:
- `Assets/Scripts/Player/Controller.cs`
- `Assets/Scripts/UI/MobileOnlyVisibility.cs`
- `Assets/VirtualJoystick/Scripts/VirtualJoystick.cs`

Problema corrigido:
- em WebGL aberto no telemóvel, os joysticks não apareciam

Causa:
- a lógica antiga dependia de `Application.isMobilePlatform`
- em browser mobile/WebGL isso frequentemente retorna como desktop

Correção aplicada:
- a deteção de mobile/touch agora usa runtime checks:
  - `Application.isMobilePlatform`
  - `SystemInfo.deviceType == DeviceType.Handheld`
  - `Input.touchSupported`
  - `Touchscreen.current != null`

Locais corrigidos:
- `Controller.cs` para ativar modo `DualStickMobile`
- `MobileOnlyVisibility.cs` para não esconder UI touch em WebGL mobile
- `VirtualJoystick.cs` do asset third-party para não se auto-desativar erroneamente quando `onlyOnMobile` está ativo

Regra importante:
- se os joysticks ainda não aparecerem após isso, o próximo suspeito não é mais detecção de plataforma; é layout/âncoras fora da área visível do ecrã

---

## 1. Resumo do projeto

- Engine: Unity 2D
- Linguagem principal: C#
- Plataforma principal atual: desktop, com suporte parcial a mobile/joystick virtual
- Cenas jogáveis no build:
  - `Assets/_Scenes/Level1.unity`
  - `Assets/_Scenes/Level2.unity`
  - `Assets/_Scenes/Level3.unity`

Loop principal:
- jogador controla uma nave
- mira com mouse no desktop
- move com WASD
- atira projéteis com mouse esquerdo
- usa bomba/tela limpa com outro sistema
- enfrenta inimigos, asteroides e boss

---

## 2. Estado atual importante

O repositório está com alterações locais não relacionadas. Não reverta nada por padrão.

Arquivos com mudanças locais visíveis quando este handoff foi atualizado:
- `Assets/Scripts/Health&Damage/Damage.cs`
- `Assets/Scripts/Player/Controller.cs`
- `Assets/Scripts/Player/ShieldController.cs`
- `Assets/Scripts/ShootingProjectiles/Projectile.cs`
- `Assets/Scripts/ShootingProjectiles/ShootingController.cs`
- `Assets/Prefabs/Enemies/...` vários prefabs
- `Assets/_Scenes/Level1.unity`
- `ProjectSettings/Physics2DSettings.asset`
- `ProjectSettings/TagManager.asset`
- outros assets de fonte/UI

Regra:
- trate a worktree como suja
- antes de editar um arquivo, leia o estado atual dele
- não faça rollback de mudanças do usuário

Limitação do ambiente atual:
- `dotnet build` não funciona aqui porque não há .NET SDK instalado
- validação local foi feita por inspeção de código/diff, não por compilação CLI

---

## 3. Mapa dos scripts centrais

### Player

#### `Assets/Scripts/Player/Controller.cs`
Responsável por:
- movimento do jogador
- leitura de input de movimento e mira
- rotação para mouse no desktop
- rotação e tiro por joystick de mira no mobile
- som do motor
- integração com escudo e bomba
- aplicação de knockback

Pontos relevantes:
- `aimMode` muda em `Start()` com `#if UNITY_IOS || UNITY_ANDROID || UNITY_TVOS`
- no desktop, usa `AimTowardsMouse`
- `HandleInput()` chama `shootingController.Fire()` no mobile quando o joystick direito está ativo
- movimento padrão é absoluto no mundo, não relativo à rotação da nave

Risco atual:
- a lógica de bloqueio por boundary usa `targetPosition = transform.position * movement * Time.deltaTime * moveSpeed;`
- isso parece incorreto matematicamente; o esperado provavelmente seria soma, não multiplicação
- não corrigi isso nesta rodada porque o foco foi o sistema de tiro

#### `Assets/Scripts/Player/ShieldController.cs`
Responsável por:
- ativar e resetar o escudo
- detectar contato com `Boundary`
- aplicar repulsão imediata no player com base no `Damage.repulsionForce`

Observação:
- o escudo usa collider trigger grande
- isso influencia diretamente bugs de spawn de projétil

#### `Assets/Scripts/Player/ScreenClearBomb.cs`
Responsável por:
- bomba especial do jogador
- lançamento do projétil de bomba
- explosão e limpeza de ameaças

### Tiro e projéteis

#### `Assets/Scripts/ShootingProjectiles/ShootingController.cs`
Responsável por:
- cooldown de tiro
- leitura do `fireAction`
- suporte a tiro mobile via `SetMobileFiring(bool)`
- spawn do projétil
- upgrade de arma (`weaponLevel`)

Estado atual importante:
- tem `projectilePrefab`
- tem `projectileHolder`
- tem `projectileSpawnPoint`
- em `Start()`, tenta localizar `ProjectileHolder` e `PontoDisparo`
- usa `weaponLevel` 1..3

Mudanças recentes já aplicadas:
- spawn agora usa `projectileSpawnPoint` quando existir
- se não existir, cai para `transform`
- projétil nasce com offset à frente via `projectileSpawnForwardOffset = 0.3f`
- após instanciar, o script ignora colisão entre o projétil e todos os colliders do dono (`Physics2D.IgnoreCollision`)

Motivo da mudança:
- havia bug intermitente onde o som do tiro tocava mas o projétil às vezes “não saía”
- causa provável: projétil nascia dentro/encostando no player ou escudo e morria imediatamente

#### `Assets/Scripts/ShootingProjectiles/Projectile.cs`
Responsável por:
- mover o projétil para frente com `transform.up * projectileSpeed`
- destruir ao bater em `Boundary`
- aplicar dano baseado em tag do projétil e `teamId` do alvo

Estado atual:
- ainda existe lógica de dano aqui
- ao mesmo tempo, o prefab do projétil do player também possui `Damage.cs`

Conclusão:
- há duplicidade de responsabilidade entre `Projectile.cs` e `Damage.cs`
- isso é dívida técnica real
- se o próximo agente tocar no sistema de dano de projéteis, o ideal é consolidar a responsabilidade em um único lugar

### Vida e dano

#### `Assets/Scripts/Health&Damage/Health.cs`
Responsável por:
- vida/vidas
- i-frames/invencibilidade
- efeitos de hit/death
- lógica de morte do player, escudo, inimigos, boss etc.

Observação:
- há proteções contra “ghost bullet” em efeitos, verificando se o prefab de efeito tem `Projectile`

#### `Assets/Scripts/Health&Damage/Damage.cs`
Responsável por:
- aplicar dano em trigger/stay/collision
- aplicar repulsão/knockback
- opcionalmente destruir o objeto causador de dano

Mudança recente já aplicada:
- a destruição do objeto (`destroyAfterDamage`) agora só ocorre quando realmente acertou um alvo de time inimigo
- antes, podia destruir mesmo em contato com aliado/mesmo time

Motivo:
- isso contribuía para sumiço do projétil logo após o spawn quando ele encostava no próprio player/escudo

### Inimigos e boss

#### `Assets/Scripts/Enemies/Enemy.cs`
Responsável por:
- lista de armas (`List<ShootingController> guns`)
- tiro dos inimigos
- score e morte

#### `Assets/Scripts/Enemies/EnemySpawner.cs`
Responsável por:
- spawn de inimigos
- após spawn, injeta `projectileHolder` em cada `ShootingController` encontrado nos filhos

#### `Assets/Scripts/Enemies/Asteroid.cs`
Responsável por:
- comportamento dos asteroides
- divisão/explosão
- lógica de colisão específica

#### `Assets/Scripts/Enemies/Bomb.cs`
Responsável por:
- bomba do boss
- usa `Physics2D.OverlapCircleAll` para reforçar detecção

#### `Assets/Scripts/Enemies/MotherShip.cs`
Responsável por:
- boss
- escudo do boss
- estágio 2
- tiro e fim do nível

Observação importante:
- este arquivo ainda contém alguns fallbacks de dano hardcoded, por exemplo `999` em um trecho de colisão
- isso viola a filosofia geral do sistema e merece revisão futura

### Gestão e UI

#### `Assets/Scripts/Utility/GameManager.cs`
Responsável por:
- singleton principal do jogo
- score
- estado de game over
- identificação do nível atual
- ativação inicial do player/escudo/arma
- limpeza da cena ao finalizar

#### `Assets/Scripts/Utility/GameSettings.cs`
Responsável por:
- configuração atual da dificuldade
- taxa de tiro do player/inimigos
- velocidade do player
- multiplicadores de dificuldade

#### `Assets/Scripts/UI/UIManager.cs` e derivados
Responsáveis por:
- HUD
- pause
- timer
- highscore
- UI do escudo e bombas

Regra prática:
- para níveis dinâmicos, prefira fallback por tag/find quando uma referência de Inspector puder faltar

---

## 4. Prefabs e objetos relevantes

### Player

#### `Assets/Prefabs/PLayer/Player.prefab`
Pontos importantes observados:
- tem `Controller`
- tem `ShootingController`
- tem `Health`
- tem `Rigidbody2D`
- tem `PolygonCollider2D`
- tem filho `PontoDisparo`
- referencia o prefab `Player_Projectile`
- `ShootingController.fireSound` no prefab está `null`

Observação importante:
- se o usuário relata “som de tiro toca”, esse som pode estar vindo de efeito/prefab auxiliar ou de estado de cena, não necessariamente do `fireSound` do `ShootingController` no prefab base

#### `Assets/Prefabs/PLayer/shield_buble.prefab`
Pontos importantes:
- tag `Shield`
- layer 6
- `CircleCollider2D` trigger grande
- `Health.teamId = 0`
- começa desativado

Implicação:
- qualquer projétil do player que nasça dentro dessa área pode gerar comportamento inesperado se a exclusão de colisão falhar

### Projétil do player

#### `Assets/Prefabs/Projectiles/Player_Projectiles/Player_Projectile.prefab`
Pontos importantes observados:
- tag `PlayerProjectile`
- layer 7
- possui `Projectile.cs`
- possui `Damage.cs`
- possui `Rigidbody2D` kinematic
- possui `PolygonCollider2D` trigger
- `projectileSpeed = 30`
- `Damage.teamId = 0`
- `Damage.destroyAfterDamage = 1`
- `Damage.dealDamageOnTriggerEnter = 1`

Conclusão:
- o prefab está funcional, mas com lógica de dano duplicada

### Holder de projéteis

Cada cena principal tem um objeto `ProjectileHolder`:
- `Assets/_Scenes/Level1.unity`
- `Assets/_Scenes/Level2.unity`
- `Assets/_Scenes/Level3.unity`

`ShootingController.Start()` tenta achar esse objeto por nome quando `projectileHolder` está `null`.

---

## 5. Tags, layers e convenções

Ver também:
- `ProjectSettings/TagManager.asset`
- `ProjectSettings/Physics2DSettings.asset`

Tags confirmadas importantes:
- `Player`
- `Shield`
- `EnemyProjectile`
- `PlayerProjectile`
- `Boundary`
- `Enemy`
- `Boss`
- `Bomb`
- `Items`

Conveção de times:
- `teamId = 0` => player/aliados do player
- `teamId != 0` => inimigos/ameaças

Conveção de spawn:
- tiros do player devem sair de `PontoDisparo` quando presente
- se adicionar nova arma, mantenha esse padrão

---

## 6. Bug recente de tiro intermitente

### Sintoma relatado
- quando o jogador atira, o som do tiro acontece
- o projétil às vezes sai, às vezes não

### Causa provável
Combinação de dois fatores:
- projétil podia nascer muito próximo ou dentro dos colliders do player/escudo
- `Damage.cs` podia destruir o projétil mesmo sem atingir um alvo inimigo

### Correções já feitas

#### Em `ShootingController.cs`
- spawn passou a usar `projectileSpawnPoint`
- projétil nasce um pouco à frente do ponto de disparo
- colisões entre projétil e colliders do próprio dono são ignoradas no spawn

#### Em `Damage.cs`
- destruição após dano agora só acontece quando o alvo atingido é de outro time

### Resultado esperado
- o tiro não deve mais sumir aleatoriamente ao nascer
- o escudo ativo não deve sabotar o projétil do player no frame de spawn

### Se o bug persistir
Investigar nesta ordem:
1. confirmar no Inspector/runtime se há múltiplos colliders/layers interferindo
2. confirmar matriz de colisão 2D em `ProjectSettings/Physics2DSettings.asset`
3. verificar se algum script de efeito está destruindo projéteis por tag
4. revisar a duplicidade `Projectile.cs` + `Damage.cs`
5. inspecionar se a cena override do prefab do player mudou `PontoDisparo` ou layers

---

## 7. Dívidas técnicas reais

Estas são as pendências mais importantes para continuidade:

### 7.1 Consolidar dano de projéteis
Hoje o projétil do player usa:
- `Projectile.cs`
- `Damage.cs`

Isso gera:
- duplicidade de regras
- risco de double hit
- comportamento difícil de prever

Direção recomendada:
- escolher um único responsável pelo dano do projétil
- idealmente manter `Projectile.cs` só para movimento/vida útil
- deixar `Damage.cs` cuidar do dano
- depois ajustar boss/inimigos que ainda leem `Projectile.damage`

### 7.2 Revisar `Controller.MovePlayer()`
Trecho suspeito:
- cálculo de `targetPosition` para boundary parece incorreto

Impacto possível:
- colisão de parede inconsistente
- falsa detecção ou bloqueio estranho de movimento

### 7.3 Revisar hardcodes de dano em `MotherShip.cs`
Há trechos com fallback agressivo como `999`.
Ideal:
- padronizar tudo para consumir `Damage.damageAmount`

### 7.4 Documentação antiga estava divergente
O arquivo anterior descrevia métodos/campos que não existem mais exatamente assim.
Ao continuar o desenvolvimento:
- sempre privilegie o código real sobre documentação antiga

---

## 8. Regras para qualquer agente que continuar

### Regras de edição
- leia o arquivo real antes de editar
- não assuma que o prefab/cena está limpo
- preserve mudanças locais não relacionadas
- prefira correções pequenas e verificáveis

### Regras de gameplay
- não introduza dano hardcoded se já existe `Damage`
- respeite `teamId`
- respeite `PontoDisparo`
- projéteis do player não devem colidir com o próprio player/escudo no spawn
- ao alterar tiro, testar com escudo ativo e escudo inativo

### Regras de cenas/prefabs
- verificar overrides em `Level1`, `Level2`, `Level3`
- se mudar prefab do player, checar impacto nas três cenas
- se mudar layers/tags, validar `TagManager` e `Physics2DSettings`

### Regras de investigação
- primeiro localizar o fluxo real no código
- depois confirmar prefab relevante
- por fim confirmar override em cena

---

## 9. Fluxos de referência

### Fluxo de tiro do player
1. `Controller.cs` e/ou `ShootingController.ProcessInput()` detecta input
2. `ShootingController.Fire()` valida cooldown
3. `ShootingController.SpawnProjectile()` instancia
4. projétil nasce em `PontoDisparo` com offset à frente
5. colisões com o dono são ignoradas
6. `Projectile.cs` move o projétil
7. `Damage.cs` e/ou `Projectile.cs` processam hit

### Fluxo de escudo
1. player ativa/recebe escudo
2. `ShieldController.ActivarEscudo()` liga o objeto e reseta vida
3. escudo absorve hits por `Health`
4. boundary pode aplicar repulsão via `ShieldController`

### Fluxo de inimigo armado
1. `EnemySpawner` instancia inimigo
2. coleta `ShootingController` nos filhos
3. injeta `projectileHolder`
4. `Enemy.cs` chama `gun.Fire()`

---

## 10. Arquivos prioritários para abrir primeiro em qualquer task de combate

Ordem recomendada:
- `Assets/Scripts/ShootingProjectiles/ShootingController.cs`
- `Assets/Scripts/ShootingProjectiles/Projectile.cs`
- `Assets/Scripts/Health&Damage/Damage.cs`
- `Assets/Scripts/Health&Damage/Health.cs`
- `Assets/Scripts/Player/Controller.cs`
- `Assets/Scripts/Player/ShieldController.cs`
- `Assets/Prefabs/PLayer/Player.prefab`
- `Assets/Prefabs/Projectiles/Player_Projectiles/Player_Projectile.prefab`
- `Assets/_Scenes/Level1.unity`

---

## 11. Próxima tarefa recomendada

Se houver continuidade imediata no sistema de combate, a melhor próxima tarefa é:

1. unificar a lógica de dano do projétil entre `Projectile.cs` e `Damage.cs`
2. validar colisões do player/escudo/projétil nas três cenas
3. revisar `Controller.MovePlayer()` para o cálculo de boundary

---

## 12. Resumo executivo para Claude

Se você é outro agente entrando agora:
- o bug principal recente era projétil do player sumindo no spawn
- já houve correção em `ShootingController.cs` e `Damage.cs`
- o projeto ainda tem dívida técnica por duplicidade entre `Projectile.cs` e `Damage.cs`
- o escudo grande do player influencia diretamente colisões de tiro
- a worktree está suja; não reverta mudanças alheias
- use o código real como fonte da verdade, não a documentação antiga
