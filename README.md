# 2D Shooter

Um jogo de tiro espacial 2D (space shooter) desenvolvido em Unity, com ação intensa, Ondas de inimigos, power-ups e sistema de escudo.

![Unity Version](https://img.shields.io/badge/Unity-6000.4.0f1-blue?logo=unity)
![Status](https://img.shields.io/badge/Status-Em%20Desenvolvimento-yellow)
![Genre](https://img.shields.io/badge/Gênero-Space%20Shooter-orange)
![License](https://img.shields.io/badge/License-MIT-green)

---

## 🎮 Sobre o Jogo

**2D Shooter** é um clássico jogo de tiro espacial (shoot 'em up) onde o jogador controla uma nave espacial em uma batalha contra forças alienígenas. Sobreviva às ondas de inimigos, colete power-ups e melhore sua pontuação em múltiplos níveis de dificuldade crescente.

### Características Principais
- Controles fluidos com suporte a mouse, teclado e controles mobile (joystick virtual)
- Sistema de escudo defensivo
- Power-ups variados (bombas, saúde, escudo)
- Inimigos diversificados (Asteroides, Naves Inimigas, Nave Mãe)
- Sistema de projéteis com física
- Camera shake para impacto visual
- Dificuldade progressiva dinâmica
- UI completa (HUD, high score, contadores)
- Sistema de high score persistente
- 3 níveis completos + Menu Principal
- Suporte a Mobile (controles touch)
- Console de desenvolvedor integrado

---

## 🛠️ Tecnologias Utilizadas

- **Unity**: 6000.4.0f1 (Unity 6.0)
- **Linguagem**: C#
- **Rendering**: Universal Render Pipeline (URP)
- **Text**: TextMesh Pro
- **Input System**: Nova API de Input da Unity
- **Física**: Physics2D
- **Sistema de Áudio**: Audio Source nativo da Unity

---

## 📁 Estrutura do Projeto

```
2DShooter/
├── Assets/
│   ├── Art/                          # Arte do jogo
│   │   ├── Animations/              # Animações de sprites
│   │   ├── Enemies/                 # Sprites de inimigos
│   │   ├── Environment/             # Cenários e fundos
│   │   ├── Icons/                   # Ícones UI
│   │   ├── Items/                   # Power-ups e itens
│   │   ├── Player/                  # Sprite da nave do jogador
│   │   ├── Projectiles/             # Projéteis e balas
│   │   ├── RenderTextures/          # Texturas de renderização (minimaps)
│   │   ├── Reticles/                | Mira/cursor
│   │   └── UI Elements/             # Elementos de interface
│   ├── Audio/                       # Arquivos de áudio
│   │   ├── Music/                   # Trilha sonora
│   │   └── Sound Effects/           # Efeitos sonoros
│   ├── Prefabs/                     # Prefabs do jogo
│   ├── Scripts/                     # Código fonte
│   │   ├── Camera/                  # Controle de câmera e camera shake
│   │   ├── Enemies/                 # Lógica de inimigos
│   │   │   ├── Asteroid.cs
│   │   │   ├── AsteroidSpawner.cs
│   │   │   ├── Bomb.cs
│   │   │   ├── Enemy.cs
│   │   │   ├── EnemySpawner.cs
│   │   │   └── MotherShip.cs
│   │   ├── Health&Damage/           # Sistema de dano
│   │   │   ├── Damage.cs
│   │   │   └── Health.cs
│   │   ├── Items/                   # Power-ups e itens coletáveis
│   │   │   ├── ItemSpawner.cs
│   │   │   └── PowerUpItem.cs
│   │   ├── Player/                  # Controles do jogador
│   │   │   ├── Controller.cs       # Movimentação principal
│   │   │   ├── ScreenClearBomb.cs  # Lógica da bomba
│   │   │   └── ShieldController.cs # Controle do escudo
│   │   ├── ShootingProjectiles/     # Sistema de tiros
│   │   │   ├── PlayerBomb.cs
│   │   │   ├── Projectile.cs
│   │   │   └── ShootingController.cs
│   │   ├── UI/                      # Interface do usuário (20+ scripts)
│   │   │   ├── BombButtonController.cs
│   │   │   ├── DifficultyHUDController.cs
│   │   │   ├── HighScoreDisplay.cs
│   │   │   ├── ScoreDisplay.cs
│   │   │   ├── UIBossHealthBar.cs
│   │   │   ├── UIHealthDisplay.cs
│   │   │   ├── UIManager.cs
│   │   │   └── ... (muito mais)
│   │   └── Utility/                 # Scripts utilitários
│   │       ├── Audio2DManager.cs
│   │       ├── CameraShake.cs
│   │       ├── DifficultyData.cs
│   │       ├── DirectionalMover.cs
│   │       ├── GameManager.cs
│   │       ├── GameSettings.cs
│   │       └── ScreenshotUtility.cs
│   ├── Input/                       # Input System actions
│   │   └── InputSystem_Actions.inputactions
│   ├── _Scenes/                     # Cenas do jogo
│   │   ├── MainMenu.unity           # Menu principal
│   │   ├── Level1.unity             # Nível 1
│   │   ├── Level2.unity             # Nível 2
│   │   └── Level3.unity             # Nível 3
│   ├── Resources/                   # Recursos dinâmicos
│   ├── TextMesh Pro/                # Fontes TMP
│   ├── VirtualJoystick/             # Controles mobile (joystick virtual)
│   └── Settings/                    # Configurações URP e cenas
├── Builds/                          # Builds compilados (não versionar)
├── Library/                         # Cache do Unity (não versionar)
├── Packages/                        # Pacotes Unity
├── ProjectSettings/                 # Configurações do projeto
├── UserSettings/                    # Configurações do usuário
├── *.sln                            # Solution do Visual Studio
└── README.md                        # Este arquivo

```

---

## 🎯 Mecânicas do Jogo

### Controles

#### Desktop (Teclado + Mouse)
- **Movimentação**: WASD ou setas direcionais
- **Mira**: Mouse (aponta para onde atira)
- **Atirar**: Botão esquerdo do mouse ou espaço
- **Bomba de Tela**: Botão direito do mouse ou tecla B
- **Pausar**: ESC ou Pause

#### Mobile (Touch)
- **Movimentação**: Joystick virtual esquerdo
- **Mira**: Joystick virtual direito (ou mira automática)
- **Atirar**: Botão de tela
- **Bomba**: Botão dedicado na UI

### Sistemas de Jogo

| Sistema | Descrição | Scripts |
|---------|-----------|---------|
| **Player Controller** | Nave com física, rotação e modos de movimento | `Controller.cs` |
| **Shooting** | Sistema de projéteis com cooldown e pooling | `ShootingController.cs`, `Projectile.cs` |
| **Screen Bomb** | Bomba que limpa a tela de inimigos | `ScreenClearBomb.cs`, `PlayerBomb.cs` |
| **Shield** | Escudo defensivo que absorve danos | `ShieldController.cs` |
| **Inimigos** | 3 tipos: Asteroides (físicos), Inimigos voadores, Nave Mãe | `Enemy.cs`, `Asteroid.cs`, `MotherShip.cs` |
| **Spawning** | Sistema de spawn de inimigos e itens | `EnemySpawner.cs`, `AsteroidSpawner.cs`, `ItemSpawner.cs` |
| **Power-ups** | Itens coletáveis (vida, escudo, bomba) | `PowerUpItem.cs` |
| **Dano/Vida** | Sistema modular de health por GameObject | `Health.cs`, `Damage.cs` |
| **UI/HUD** | Interface completa responsiva | `UIManager.cs`, `ScoreDisplay.cs`, `UIHealthDisplay.cs`, `UIBombDisplay.cs`, `ShieldVulnerabilityDisplay.cs` |
| **Camera** | Segue jogador + efeito de shake | `CameraController.cs`, `CameraShake.cs` |
| **Difficulty** | Sistema de dificuldade progressiva | `DifficultyData.cs`, `DifficultyHUDController.cs` |
| **Audio** | Gerenciamento de sons 2D | `Audio2DManager.cs` |

---

## 🎨 Características Técnicas

### Modes de Movimento
O jogo suporta múltiplos modos de movimento configuráveis:
- **FreeRoam**: Movimentação livre em todas as direções (estilo asteroids)
- **MoveHorizontally**: Movimento apenas horizontal
- **MoveVertically**: Movimento apenas vertical
- **Astroids**: Física de rotação com impulso (estiloclássico asteroids)

### Aim Modes (Mira)
- **AimTowardsMouse**: Mira aponta para a posição do mouse
- **AimForwards**: Mira sempre para frente (movimentação + direção)
- **DualStickMobile**: Mira com joystick direito (controle mobile)

### Sistema de Escudo
- Efeito visual de vulnerabilidade quando o escudo está ativo
- Sem atualização - você precisa gerenciar sua sobrevivência
- Display visual no HUD mostra status do escudo

### Dificuldade Dinâmica
- Escala automática de dificuldade baseada no desempenho
- Parâmetros configuráveis (spawn rate, velocidade inimiga, etc.)
- Indicador visual de nível de dificuldade no HUD

### Bomba de Tela
- Elimina todos os inimigos na tela
- Tempo de recarga (cooldown)
- Uso limitado (consumo de recursos)
- Botão dedicado na UI e no teclado

---

## 📊 Progresso do Projeto

✅ **Implementado:**
- [x] Sistema de movimento fluido (teclado + mouse + mobile)
- [x] Sistema de projéteis com física
- [x] Bomba de tela funcional com cooldown
- [x] Sistema de escudo defensivo
- [x] 3 tipos de inimigos (Asteroid, Enemy, MotherShip)
- [x] Sistema de spawning de inimigos e itens
- [x] Power-ups (Saúde, Escudo, Bomba)
- [x] Sistema de health e dano
- [x] Camera shake em impactos
- [x] UI completa (HUD com score, saúde, escudo, bombas)
- [x] Display de high score
- [x] Sistema de dificuldade progressiva
- [x] 3 níveis completos
- [x] Menu principal funcional
- [x] Sons e efeitos sonoros
- [x] Suporte a Mobile (joystick virtual)
- [x] Console de desenvolvedor
- [x] Sistema de GameManager persistente
- [x] Screenshot utility

🔄 **Em Desenvolvimento:**
- [ ] Mais tipos de power-ups
- [ ] Mais padrões de inimigos
- [ ] Cutscenes e transições
- [ ] Achievements Sistema
- [ ] Mais efeitos visuais (VFX)
- [ ] Configurações de jogo (som, gráficos)
- [ ] Tutorial/Opcional level
- [ ] Suporte a Gamepad melhorado

---

## 🚀 Como Executar

### Requisitos do Sistema
- **Unity**: 6000.4.0f1 (Unity 6.0) ou superior
- **Sistema Operacional**: Windows, macOS ou Linux
- **GPU**: Qualquer GPU com suporte a OpenGL 3.0+ / DirectX 11+
- **RAM**: Mínimo de 4GB (recomendado 8GB+)
- **Mobile**: iOS 12+, Android 8+

### Passos para Execução

1. **Abrir o Projeto**
   ```bash
   - Abra o Unity Hub
   - Clique em "Add Project"
   - Selecione a pasta D:/Unity_Projects/2DShooter
   - Clique em "Open"
   ```

2. **Aguardar Compilação**
   - O Unity vai importar e compilar todos os assets e pacotes
   - Primeira execução pode levar alguns minutos

3. **Executar o Jogo**
   - Abra a cena `Assets/_Scenes/MainMenu.unity`
   - Clique no botão **Play** no topo do editor Unity
   - Ou pressione **Ctrl+P** (Windows/Linux) / **Cmd+P** (macOS)

4. **Build para Executável**
   ```
   File → Build Settings
   - Adicione as cenas desejadas (MainMenu, Level1, Level2, Level3)
   - Selecione plataforma (Windows, macOS, Linux, Android, iOS)
   - Clique em "Build and Run"
   ```

5. **Build Mobile** (Android/iOS)
   - Certifique-se de ter o SDK da plataforma instalado
   - Configure as configurações de player (Company Name, Product Name, icons)
   - Build e deploy via USB

---

## 🎮 Guia Rápido de Gameplay

### Objetivo
Sobreviva o máximo tempo possível, destrua inimigos e colete power-ups para aumentar sua pontuação.

### Power-ups Disponíveis
| Power-up | Efeito | Icone |
|----------|--------|-------|
| **Saúde (Health)** | +1 vida | [coração] |
| **Escudo (Shield)** | Ativa escudo protetor | [escudo] |
| **Bomba (Bomb)** | Adiciona bomba de tela | [bomba] |

### Escore (Score)
- Inimigos normais: 100 pontos
- Inimigos especiais: 250 pontos
- Power-ups: 50 pontos
- Combo kills: multiplicador por kills rápidos

### Dicas
- **Use o escudo** quando cercado por inimigos - ele protege contra danos
- **Bomba de tela** limpa todos os inimigos - use em situações de emergência
- **Mantenha-se em movimento** constante para evitar ser atingido
- **Priorize power-ups de escudo** em ondas difíceis

---

## 🧪 Testes & Debug

### Console de Desenvolvedor
Pressione a tecla **`~`** ( til) durante o jogo para abrir o console.

Comandos disponíveis:
```bash
loadscene <nome>    # Carrega uma cena (ex: loadscene Level1)
sethhealth <valor>  # Define a saúde do jogador
listscenes          # Lista todas as cenas disponíveis
```

### Configurações de Debug
- `RuntimeDeviceProfile.cs` - Detecta se está rodando em mobile ou desktop
- `GameSettings.cs` - Configurações de jogo editáveis
- `DifficultyData.cs` - Ajuste de parâmetros de dificuldade

---

## 📝 Estrutura de Scripts

### Player
- **Controller.cs** - Movimentação principal da nave, entrada de input, rotação
- **ShieldController.cs** - Gerenciamento do escudo defensivo
- **ScreenClearBomb.cs** - Lógica da bomba que limpa a tela

### Shooting
- **ShootingController.cs** - Controle de disparo, cooldown, pool de projéteis
- **Projectile.cs** - Comportamento das balas
- **PlayerBomb.cs** - Projétil da bomba de tela

### Enemies
- **Enemy.cs** - Classe base de inimigos (movimento básico)
- **Asteroid.cs** - Inimigo com física (rotação, colisão)
- **Bomb.cs** - Inimigo que solta projéteis
- **MotherShip.cs** - Boss/Nave mãe ( inimigo especial )
- **EnemySpawner.cs** - Gerencia spawn de inimigos ao longo do tempo
- **AsteroidSpawner.cs** - Spawn específico de asteroides com padrão

### UI (20+ scripts)
- **UIManager.cs** - Gerenciador principal da interface
- **ScoreDisplay.cs** - Mostra pontuação atual
- **HighScoreDisplay.cs** - Exibe high score
- **UIHealthDisplay.cs** - Barra de vida/corações
- **UIBombDisplay.cs** - Indicador de bombas disponíveis
- **ShieldVulnerabilityDisplay.cs** - Status do escudo
- **DifficultyHUDController.cs** - Indicador de dificuldade
- **UIBossHealthBar.cs** - Barra de vida de boss

### Utility
- **GameManager.cs** - Estado global do jogo (singleton)
- **CameraShake.cs** - Efeito de tremer a câmera
- **Audio2DManager.cs** - Gerenciador de sons 2D
- **GameSettings.cs** - Configurações editáveis via inspector
- **ScreenshotUtility.cs** - Ferramenta de captura de tela

---

## ⚙️ Configuração do Input

### Input Actions ( Desktop )
```
Player:
├── Move (WASD/Arrows) - Vector2
├── Look (Mouse) - Vector2
├── Shoot (Mouse Left/Space) - Button
└── Bomb (Mouse Right/B) - Button
```

### Mobile Controls
- **VirtualJoystick** - Sistema de joystick virtual disponível em `VirtualJoystick/`
- Configuração automática via `Controller.cs` detecta plataforma
- Joystick esquerdo: movimento
- Joystick direito: mira (dual-stick mode)

---

## 🎵 Áudio

O jogo inclui:
- **Música de fundo** (Assets/Audio/Music/)
- **Efeitos sonoros** (Assets/Audio/Sound Effects/):
  - Tiros
  - Explosões
  - Power-ups coletados
  - Dano recebido
  - Escudo ativado
  - Bombalançada

Gerenciado por `Audio2DManager.cs` com音量音量controle centralizado.

---

## 🏗️ Arquitetura do Projeto

### Padrões Utilizados
- **Singleton**: GameManager para estado global
- **Object Pooling**: Projéteis e partículas (implicito em sistema de spawners)
- **Observer**: Eventos via UnityEvents (danos, coleta)
- **Component-based**: Física e lógica separadas em Components
- **ScriptableObjects**: Configurações de dificuldade (DifficultyData)

### Camadas (Layers)
- Default
- UI
- Player
- Enemy
- Projectile
- Environment

### Tags
- Player
- Enemy
- Projectile
- PowerUp
- Boss

---

## 📈 Otimizações

- **Object pooling** implícito em EnemySpawner/ItemSpawner
- **Camera shake** com duração mínima e smoothing
- **Input buffering** paratiros
- **Shape-based collision** otimizada para 2D
- **Resource loading** async em cenas
- **Mobile only visibility** para UI mobile

---

## 🤝 Contribuindo

1. Faça fork do repositório
2. Crie uma branch: `git checkout -b feature/NovaFeature`
3. Commit: `git commit -m 'Adiciona nova feature'`
4. Push: `git push origin feature/NovaFeature`
5. Abra um Pull Request

---

## 📜 Licença

Projeto licenciado sob MIT License. Veja arquivo [LICENSE](LICENSE) para detalhes.

---

## 👨‍💻 Autor

Desenvolvido por: **Jhonni Vieceli**

---

## 🔗 Recursos Úteis

- [Documentação Unity](https://docs.unity3d.com/Manual/index.html)
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/manual/)
- [TextMesh Pro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/)
- [Physics2D](https://docs.unity3d.com/Manual/Physics2D.html)
- [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/)

---

## 📌 Notas

- **Versão Unity**: 6000.4.0f1 (Unity 6.0)
- **Target Platforms**: Windows, macOS, Linux, Android, iOS
- **Resolution**: 1920x1080 (upscaling adaptativo)
- **FPS Target**: 60 FPS
- **Source Control**: Git (.gitignore configurado para Unity)
- **Mobile Ready**: Suporte completo a touch com joystick virtual

---

**Boas jogatinas! 🚀👾**
