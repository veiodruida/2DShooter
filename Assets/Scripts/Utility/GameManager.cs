using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [Tooltip("Referência ao objeto do jogador na cena")]
    public GameObject player = null;

    [Header("Pontuação e Cronómetro")]
    [SerializeField] private int gameManagerScore = 0;
    public int bonusPerfect = 1000;
    public float tempoRecordeAnterior = 0f;

    public static int score
    {
        get { return instance.gameManagerScore; }
        set { instance.gameManagerScore = value; }
    }

    [Header("Configurações do Boss e Recordes")]
    public int highScore = 0;
    public float tempoDaFase = 0f;
    public int scoreBaseBoss = 5000;
    public int penalidadePorSegundo = 50;

    [Header("Progresso e Vitória")]
    public bool gameIsWinnable = true;
    public int enemiesToDefeat = 10;
    public int enemiesDefeated = 0; 
    public bool printDebugOfWinnableStatus = true;
    public string nomePaginaVitoria = "VictoryPage";
    public GameObject victoryEffect;
    public AudioSource victorySound;

     [Header("Configurações de Game Over")]
    public string nomePaginaGameOver = "GameOverPage";
    public GameObject gameOverEffect;
    public AudioSource gameOverSound;

    [Header("Configurações de Musica Fundo")]
    public AudioSource bgmSource;
    public float tempoDeFade = 1.5f;

    [Header("Configurações de Nível de Dificuldade")]
    public int nivelAtual = 1;
    public GameSettings.Dificuldade dificuldadeSelecionada;

    [Header("Cursor Settings")]
    public Texture2D cursorCustomizado;

    private int numberOfEnemiesFoundAtStart;

    [HideInInspector]
    public bool gameIsOver = false;

    // Flag estática — garante salvamento único por cena de jogo
    private static bool hasSavedThisMatch = false;

    // NOVO: Garante apenas um registro no Records (historico_partidas) por jornada inteira
    private static bool hasSavedToHistoryThisRun = false;
    
    // NOVO: Track do tempo total em todos os níveis
    public static float tempoTotalPartida = 0f;

    private void Awake()
    {
        Time.timeScale = 1f;
        if (instance == null) instance = this;
        else { DestroyImmediate(this); return; }

        if ((player == null) && (Object.FindFirstObjectByType<Controller>() != null))
        {
            player = Object.FindFirstObjectByType<Controller>().gameObject;
        }
    }

    void GerarLogDiagnostico()
    {
        if (GameSettings.instance == null || GameSettings.instance.configAtual == null)
        {
            Debug.LogError("<color=red>[ERRO CRÍTICO]</color> GameManager: Arquivo de configuração não encontrado!");
            return;
        }

        DifficultyData cfg = GameSettings.instance.configAtual;
        string dificuldadeNome = GameSettings.instance.dificuldadeSelecionada.ToString();

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"<color=white>==========================================</color>");
        sb.AppendLine($"<color=cyan><b>DIAGNÓSTICO DE DIFICULDADE: {dificuldadeNome.ToUpper()}</b></color>");
        sb.AppendLine($"<color=white>==========================================</color>");
        sb.AppendLine($"<color=lime><b>[PLAYER]</b></color>");
        sb.AppendLine($" - Vidas: {cfg.vidasIniciais}");
        sb.AppendLine($" - Velocidade: {cfg.velocidadePlayer}");
        sb.AppendLine($" - Cadência Tiro: {cfg.taxaDeTiro}s");
        sb.AppendLine($"<color=orange><b>[SISTEMA & SPAWNS]</b></color>");
        sb.AppendLine($" - Spawn Inimigos: {cfg.tempoSpawnInimigos}s");
        sb.AppendLine($" - Spawn Itens: {cfg.tempoSpawnItens}s");
        sb.AppendLine($" - Vel. Inimigo Base: {cfg.velocidadeInimigoComum}");
        sb.AppendLine($" - Recarga Tiro Inimigo: {cfg.intervaloTiroInimigo}s");
        sb.AppendLine($"<color=magenta><b>[BOSS MOTHERSHIP]</b></color>");
        sb.AppendLine($" - Escudo abre após: {cfg.navesParaAbrirEscudo} naves");
        sb.AppendLine($" - Vida Escudo (E2): {cfg.vidaDoEscudoEstagio2}");
        sb.AppendLine($" - Fúria 1 (Tiro/Vel): {cfg.intervaloFuria1}s / {cfg.velocidadeFuria1}");
        sb.AppendLine($" - Fúria 2 (Tiro/Vel): {cfg.intervaloFuria2}s / {cfg.velocidadeFuria2}");
        sb.AppendLine($"<color=yellow><b>[GLOBAL]</b></color>");
        sb.AppendLine($" - Multiplicador de Dano: {cfg.multiplicadorDanoRecebido}x");
        sb.AppendLine($"<color=white>==========================================</color>");
        Debug.Log(sb.ToString());
    }
    
    void Start()
    {
        if (cursorCustomizado != null)
        {
            Vector2 hotSpot = new Vector2(cursorCustomizado.width / 2f, cursorCustomizado.height / 2f);
            Cursor.SetCursor(cursorCustomizado, hotSpot, CursorMode.Auto);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        HandleStartUp();
        
        // NOVO: Detecta o nível automaticamente pelo nome da cena
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Level") && int.TryParse(sceneName.Substring(5), out int levelNum))
        {
            nivelAtual = levelNum;
            Debug.Log($"<color=cyan>GameManager: Nível detectado -> {nivelAtual}</color>");
        }

        // CRÍTICO: Reseta a flag no início de cada cena de jogo
        hasSavedThisMatch = false;
        Time.timeScale = 1f;

        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            GerarLogDiagnostico();
        }
        else
        {
            Debug.LogError("CUIDADO: Level 1 iniciado sem GameSettings ou sem arquivo DifficultyData!");
        }
    }

    /// <summary>Mapeia o enum de dificuldade para o nome da nação exibido no jogo.</summary>
    public static string GetDificuldadeNome()
    {
        if (GameSettings.instance == null) return "N/A";
        switch (GameSettings.instance.dificuldadeSelecionada)
        {
            case GameSettings.Dificuldade.Facil:   return "USA";
            case GameSettings.Dificuldade.Medio:   return "RUSSIA";
            case GameSettings.Dificuldade.Dificil: return "N.KOREA";
            case GameSettings.Dificuldade.Furia:   return "CUBA";
            default:                               return "???";
        }
    }

    private void Update()
    {
        if (!gameIsOver)
        {
            tempoDaFase += Time.deltaTime;
            tempoTotalPartida += Time.deltaTime; // Acumula tempo total da jogada
        }
    }

    void HandleStartUp()
    {
        ResetPlayerWeapon();
        AtivarShieldDoPlayer();

        if (PlayerPrefs.HasKey("highscore")) highScore = PlayerPrefs.GetInt("highscore");
        if (PlayerPrefs.HasKey("score")) score = PlayerPrefs.GetInt("score");
        if (PlayerPrefs.HasKey("melhor_tempo")) tempoRecordeAnterior = PlayerPrefs.GetFloat("melhor_tempo");

        UpdateUIElements();

        if (printDebugOfWinnableStatus) FigureOutHowManyEnemiesExist();
    }

    private void ResetPlayerWeapon()
    {
        if (player != null)
        {
            ShootingController shooter = player.GetComponent<ShootingController>();
            if (shooter != null) shooter.weaponLevel = 1;
        }
    }

    private void AtivarShieldDoPlayer()
    {
        if (player != null)
        {
            Controller playerController = player.GetComponent<Controller>();
            if (playerController != null)
            {
                playerController.GanharEscudo(playerController.shieldObject != null
                    ? playerController.shieldObject.GetComponent<Health>().maximumLives
                    : 3);
            }
        }
    }

    private void FigureOutHowManyEnemiesExist()
    {
        List<EnemySpawner> enemySpawners = Object.FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None).ToList();
        List<Enemy> staticEnemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList();

        int enemiesFromSpawners = 0;
        foreach (EnemySpawner spawner in enemySpawners)
        {
            if (!spawner.spawnInfinite) enemiesFromSpawners += spawner.maxSpawn;
        }
        numberOfEnemiesFoundAtStart = enemiesFromSpawners + staticEnemies.Count;
    }

    public void IncrementEnemiesDefeated()
    {
        enemiesDefeated++;
        if (enemiesDefeated >= enemiesToDefeat && gameIsWinnable && Object.FindFirstObjectByType<MotherShip>() == null)
        {
            LevelCleared();
        }
    }

    public float GetDificuldadeMultiplier()
    {
        if (GameSettings.instance != null)
        {
            dificuldadeSelecionada = GameSettings.instance.dificuldadeSelecionada;
        }

        switch (dificuldadeSelecionada)
        {
            case GameSettings.Dificuldade.Facil: return 0.7f;
            case GameSettings.Dificuldade.Medio: return 1.0f;
            case GameSettings.Dificuldade.Dificil: return 1.4f;
            case GameSettings.Dificuldade.Furia: return 2.0f;
            default: return 1.0f;
        }
    }

    public int CalcularVidaInimigo(int vidaBase)
    {
        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            float multiplicador = GameSettings.instance.configAtual.multiplicadorVidaInimigo;
            return Mathf.RoundToInt(vidaBase * multiplicador);
        }
        return vidaBase;
    }

    public int CalcularPontuacaoFinalBoss()
    {
        int pontosBase = 500;
        if (GameSettings.instance != null && GameSettings.instance.dificuldadeSelecionada == GameSettings.Dificuldade.Furia)
        {
            return pontosBase * 2;
        }
        return pontosBase;
    }

    public int CalcularPontuacaoFinalDaFase()
    {
        int pontuacaoFinal = gameManagerScore;
        int pontosDeTempo = Mathf.Max(0, scoreBaseBoss - Mathf.FloorToInt(tempoDaFase * penalidadePorSegundo));
        pontuacaoFinal += pontosDeTempo;

        Health pHealth = (player != null) ? player.GetComponent<Health>() : null;
        if (pHealth != null && pHealth.currentHealth >= pHealth.maximumHealth)
        {
            pontuacaoFinal += bonusPerfect;
        }
        return pontuacaoFinal;
    }

    public static void AddScore(int scoreAmount)
    {
        score += scoreAmount;
        if (score > instance.highScore)
        {
            instance.highScore = score;
            PlayerPrefs.SetInt("highscore", score);
        }
        UpdateUIElements();
    }

    public static void ResetScore()
    {
        PlayerPrefs.SetInt("score", 0);
        score = 0;
        tempoTotalPartida = 0f; // Reseta o tempo total ao iniciar nova jornada
        hasSavedToHistoryThisRun = false; // Permite um novo registro ao histórico
        UpdateUIElements();
    }

    /// <summary>
    /// Salva o resultado da partida atual no histórico.
    /// Protegida por flag estática para garantir chamada única por cena.
    /// @param appendToHistory: Adiciona um novo registro na lista formatada (Records).
    /// </summary>
    public static void SalvarDadosPartida(bool appendToHistory = false)
    {
        if (instance == null) return;

        // GUARD: Uma única execução por cena de jogo para evitar repetições
        if (hasSavedThisMatch)
        {
            Debug.Log("<color=orange>GameManager: SalvarDadosPartida ignorado (já salvo nesta partida).</color>");
            return;
        }
        hasSavedThisMatch = true;
        Debug.Log("<color=green>GameManager: Salvando partida...</color>");

        // --- 1. ATUALIZAR RECORDES ABSOLUTOS ---
        int recordePontos = PlayerPrefs.GetInt("highscore", 0);
        float melhorTempo = PlayerPrefs.GetFloat("melhor_tempo", 9999f);

        if (score > recordePontos)
        {
            PlayerPrefs.SetInt("highscore", score);
            instance.highScore = score;
        }

        // Para o recorde de 'melhor tempo', consideramos apenas a fase ou o total? 
        // Aqui mantemos a lógica original de verificar se este tempo foi o menor registrado.
        if (instance.tempoDaFase < melhorTempo && instance.tempoDaFase > 1f)
        {
            PlayerPrefs.SetFloat("melhor_tempo", instance.tempoDaFase);
            instance.tempoRecordeAnterior = instance.tempoDaFase;
        }

        // --- 2. ADICIONAR AO HISTÓRICO (SOMENTE AO FINALIZAR A JORNADA) ---
        if (appendToHistory && !hasSavedToHistoryThisRun)
        {
            hasSavedToHistoryThisRun = true;
            // CRÍTICO: InvariantCulture força '.' como decimal
            string difNome = GetDificuldadeNome();
            // USAMOS tempoTotalPartida para representar a soma de todos os níveis
            string tempoFormatado = tempoTotalPartida.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            string novaEntrada = $"{score};{tempoFormatado};{difNome}";

            string historicoAtual = PlayerPrefs.GetString("historico_partidas", "");

            List<string> listaEntradas = string.IsNullOrEmpty(historicoAtual)
                ? new List<string>()
                : historicoAtual.Split(',').Where(e => !string.IsNullOrEmpty(e.Trim())).ToList();

            listaEntradas.Add(novaEntrada);

            // Mantém apenas as últimas 50 entradas
            if (listaEntradas.Count > 50)
                listaEntradas = listaEntradas.Skip(listaEntradas.Count - 50).ToList();

            PlayerPrefs.SetString("historico_partidas", string.Join(",", listaEntradas));
            Debug.Log($"<color=cyan>GameManager: Novo registro adicionado ao Records: {novaEntrada}</color>");
        }

        // --- 3. LIMPEZA E PERSISTÊNCIA ---
        PlayerPrefs.SetInt("score", score);
        PlayerPrefs.Save();

        instance.LimparObjetosDaCena();
    }

    public static void UpdateUIElements()
    {
        if (UIManager.instance != null) UIManager.instance.UpdateUI();
    }

    public void LimparObjetosDaCena()
    {
        string[] tagsParaLimpar = { "EnemyProjectile", "Items", "PlayerProjectile", "Enemy" };

        foreach (string tag in tagsParaLimpar)
        {
            GameObject[] objetos = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objetos)
            {
                Destroy(obj);
            }
        }

        Debug.Log("<color=cyan>GameManager: Cena limpa de entidades temporárias.</color>");
    }

    public void LevelCleared()
    {
        // Restaurar timeScale (pode ter sido travado pelo GameOver simultâneo)
        Time.timeScale = 1f;

        score = CalcularPontuacaoFinalDaFase();
        gameIsOver = true;
        
        // Só salva no Records se for o level final (L3)
        SalvarDadosPartida(nivelAtual >= 3); 

        if (nivelAtual == 3 && GameSettings.instance != null &&
            GameSettings.instance.dificuldadeSelecionada == GameSettings.Dificuldade.Dificil)
        {
            PlayerPrefs.SetInt("JogoFinalizado", 1);
            PlayerPrefs.Save();
        }

        if (UIManager.instance != null)
        {
            if (player != null) player.SetActive(false);
            UIManager.instance.allowPause = false;
            UIManager.instance.ConfigurarCursor(true);
            UIManager.instance.UpdateUI();
            UIManager.instance.GoToPageByName(nomePaginaVitoria);
            if (victoryEffect != null) Instantiate(victoryEffect, transform.position, transform.rotation);
            if (victorySound != null) StartCoroutine(TransicaoMusicaVitoria());
        }
        LimparObjetosDaCena();
    }

    public void GameOver()
    {
        if (gameIsOver) return;
        gameIsOver = true;
        Time.timeScale = 0f;

        if (bgmSource != null) StartCoroutine(TransicaoMusicaVitoria());

        if (gameOverEffect != null) Instantiate(gameOverEffect, transform.position, transform.rotation);
        if (gameOverSound != null)
        {
            AudioSource.PlayClipAtPoint(gameOverSound.clip, Camera.main.transform.position, gameOverSound.volume);
        }

        if (UIManager.instance != null)
        {
            UIManager.instance.allowPause = false;
            UIManager.instance.ConfigurarCursor(true);
            UIManager.instance.GoToPageByName(nomePaginaGameOver);
        }
        
        // Morte do jogador encerra a jornada — registra no Records
        SalvarDadosPartida(true); 
        LimparObjetosDaCena();
    }

    public void ReiniciarFase()
    {
        ResetScore();
        tempoDaFase = 0f;
        ResetPlayerWeapon();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator TransicaoMusicaVitoria()
    {
        if (bgmSource != null)
        {
            float volumeInicial = bgmSource.volume;
            for (float t = 0; t < tempoDeFade; t += Time.deltaTime)
            {
                bgmSource.volume = Mathf.Lerp(volumeInicial, 0, t / tempoDeFade);
                yield return null;
            }
            bgmSource.Stop();
            bgmSource.volume = volumeInicial;
        }

        if (nivelAtual == 3 && gameOverSound != null) gameOverSound.Play();
        else if (victorySound != null) victorySound.Play();
    }

    private void OnApplicationQuit()
    {
        // Ao fechar o jogo voluntariamente, salvamos o progresso atual
        SalvarDadosPartida(true);
        ResetScore();
    }
}
