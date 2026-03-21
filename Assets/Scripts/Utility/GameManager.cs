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
    public string nomePaginaGameOver = "GameOverPage"; // Mudado para string
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

        // Seção Player
        sb.AppendLine($"<color=lime><b>[PLAYER]</b></color>");
        sb.AppendLine($" - Vidas: {cfg.vidasIniciais}");
        sb.AppendLine($" - Velocidade: {cfg.velocidadePlayer}");
        sb.AppendLine($" - Cadência Tiro: {cfg.taxaDeTiro}s");

        // Seção Inimigos e Spawns
        sb.AppendLine($"<color=orange><b>[SISTEMA & SPAWNS]</b></color>");
        sb.AppendLine($" - Spawn Inimigos: {cfg.tempoSpawnInimigos}s");
        sb.AppendLine($" - Spawn Itens: {cfg.tempoSpawnItens}s");
        sb.AppendLine($" - Vel. Inimigo Base: {cfg.velocidadeInimigoComum}");
        sb.AppendLine($" - Recarga Tiro Inimigo: {cfg.intervaloTiroInimigo}s");

        // Seção Boss
        sb.AppendLine($"<color=magenta><b>[BOSS MOTHERSHIP]</b></color>");
        sb.AppendLine($" - Escudo abre após: {cfg.navesParaAbrirEscudo} naves");
        sb.AppendLine($" - Vida Escudo (E2): {cfg.vidaDoEscudoEstagio2}");
        sb.AppendLine($" - Fúria 1 (Tiro/Vel): {cfg.intervaloFuria1}s / {cfg.velocidadeFuria1}");
        sb.AppendLine($" - Fúria 2 (Tiro/Vel): {cfg.intervaloFuria2}s / {cfg.velocidadeFuria2}");

        // Seção Global
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
            Cursor.lockState = CursorLockMode.Confined; // Mantém o mouse dentro da janela
        }

        // 1. RODAR O STARTUP (Carrega PlayerPrefs e Reseta Arma)
        HandleStartUp();
        Time.timeScale = 1f;
        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            var cfg = GameSettings.instance.configAtual;
            GerarLogDiagnostico();
        }
        else
        {
            Debug.LogError("CUIDADO: Level 1 iniciado sem GameSettings ou sem arquivo DifficultyData!");
        }
    }

    private void Update()
    {
        if (!gameIsOver)
        {
            tempoDaFase += Time.deltaTime;
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
                // Ativa o shield com o máximo de vidas
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
        // Garantir que a variável local esteja sincronizada com o GameSettings antes do switch
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
        // 1. Tenta buscar o multiplicador do arquivo de dificuldade
        if (GameSettings.instance != null && GameSettings.instance.configAtual != null)
        {
            float multiplicador = GameSettings.instance.configAtual.multiplicadorVidaInimigo;

            // Se a vida base for 10 e o multiplicador for 1.5, retorna 15
            return Mathf.RoundToInt(vidaBase * multiplicador);
        }

        // 2. Fallback caso o GameSettings não esteja pronto
        return vidaBase;
    }

    public int CalcularPontuacaoFinalBoss()
    {
        int pontosBase = 500;
        if (GameSettings.instance != null && GameSettings.instance.dificuldadeSelecionada == GameSettings.Dificuldade.Furia)
        {
            return pontosBase * 2; // Dobro de pontos no modo Fúria
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
        UpdateUIElements();
    }

    public static void SalvarDadosPartida()
    {
        if (instance == null) return;

        // --- 1. ATUALIZAR RECORDES ABSOLUTOS ---
        int recordePontos = PlayerPrefs.GetInt("highscore", 0);
        float melhorTempo = PlayerPrefs.GetFloat("melhor_tempo", 9999f);

        if (score > recordePontos)
        {
            PlayerPrefs.SetInt("highscore", score);
            instance.highScore = score;
        }

        if (instance.tempoDaFase < melhorTempo && instance.tempoDaFase > 1f)
        {
            PlayerPrefs.SetFloat("melhor_tempo", instance.tempoDaFase);
            instance.tempoRecordeAnterior = instance.tempoDaFase;
        }

        // --- 2. ADICIONAR AO HISTÓRICO (Para o HistoricoDisplay) ---
        string historicoAtual = PlayerPrefs.GetString("historico_partidas", "");
        // Formato: Pontos|Tempo
        string novaEntrada = $"{score}|{instance.tempoDaFase:F2}";

        if (string.IsNullOrEmpty(historicoAtual))
        {
            historicoAtual = novaEntrada;
        }
        else
        {
            // Mantém apenas as últimas 10 partidas para não sobrecarregar
            List<string> listaEntradas = historicoAtual.Split(',').ToList();
            listaEntradas.Add(novaEntrada);

            if (listaEntradas.Count > 10)
            {
                listaEntradas.RemoveAt(0); // Remove a mais antiga
            }

            historicoAtual = string.Join(",", listaEntradas);
        }

        PlayerPrefs.SetString("historico_partidas", historicoAtual);

        // --- 3. LIMPEZA FINAL ---
        instance.LimparObjetosDaCena();

        // Salva fisicamente no disco
        PlayerPrefs.Save();
        Debug.Log("<color=green>GameManager: Recordes e Histórico salvos com sucesso!</color>");
    }

    public static void SalvarNoHistorico(int pontos, float tempo)
    {
        // 1. Pega o que já existe
        string historicoAtual = PlayerPrefs.GetString("historico_partidas", "");

        // 2. Cria a nova entrada formatada como o Display espera (PONTOS|TEMPO)
        string novaEntrada = $"{pontos}|{tempo:F2}";

        // 3. Junta ao histórico (separando por vírgula se já houver algo)
        if (string.IsNullOrEmpty(historicoAtual))
        {
            historicoAtual = novaEntrada;
        }
        else
        {
            // Limita o histórico para não crescer infinitamente (ex: as últimas 10)
            string[] todas = historicoAtual.Split(',');
            if (todas.Length >= 10)
                historicoAtual = string.Join(",", todas.Skip(1).ToArray()) + "," + novaEntrada;
            else
                historicoAtual += "," + novaEntrada;
        }

        // 4. Salva de volta
        PlayerPrefs.SetString("historico_partidas", historicoAtual);
        PlayerPrefs.Save();
    }

    public static void UpdateUIElements()
    {
        if (UIManager.instance != null) UIManager.instance.UpdateUI();
    }

    public void LimparObjetosDaCena()
    {
        // 1. Criamos uma lista com as Tags que queremos remover
        string[] tagsParaLimpar = { "EnemyProjectile", "Items", "PlayerProjectile", "Enemy" };

        foreach (string tag in tagsParaLimpar)
        {
            // 2. Buscamos todos os objetos com essa Tag
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
        if (gameIsOver) return;

        score = CalcularPontuacaoFinalDaFase();
        gameIsOver = true;
        PlayerPrefs.SetInt("score", score);

        if (score > highScore) SalvarDadosPartida();

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
        Time.timeScale = 0f; // TRAVA O JOGO
        // Paramos o BGM imediatamente para o som de Game Over brilhar
        if (bgmSource != null) StartCoroutine(TransicaoMusicaVitoria());

        if (gameOverEffect != null) Instantiate(gameOverEffect, transform.position, transform.rotation);
        if (gameOverSound != null)
        {
            // Usamos PlayClipAtPoint porque o Time.timeScale 0 pode afetar AudioSources comuns em alguns casos,
            // e também garante que o som toque mesmo se o GameManager for mexido.
            AudioSource.PlayClipAtPoint(gameOverSound.clip, Camera.main.transform.position, gameOverSound.volume);
        }

        if (UIManager.instance != null)
        {
            UIManager.instance.allowPause = false;
            UIManager.instance.ConfigurarCursor(true);
            UIManager.instance.GoToPageByName(nomePaginaGameOver);
        }
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
            bgmSource.volume = volumeInicial; // Deixa o volume pronto para o próximo restart
        }

        // Toca o som de fim de jogo se for o último nível, senão toca o de vitória normal
        if (nivelAtual == 3 && gameOverSound != null) gameOverSound.Play();
        else if (victorySound != null) victorySound.Play();
    }

    private void OnApplicationQuit()
    {
        SalvarDadosPartida();
        ResetScore();
    }
}